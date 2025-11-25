# EduPortal - CI/CD Kılavuzu

Bu dokümantasyon EduPortal projesinin CI/CD pipeline'ını detaylı olarak açıklar.

## İçindekiler

1. [Genel Bakış](#genel-bakış)
2. [Branch Stratejisi](#branch-stratejisi)
3. [CI Pipeline](#ci-pipeline)
4. [CD Pipeline - Test](#cd-pipeline---test)
5. [CD Pipeline - Production](#cd-pipeline---production)
6. [Rollback](#rollback)
7. [Kurulum](#kurulum)
8. [Sorun Giderme](#sorun-giderme)

---

## Genel Bakış

EduPortal CI/CD sistemi GitHub Actions kullanır ve aşağıdaki özelliklere sahiptir:

- ✅ Otomatik build ve test
- ✅ Code coverage raporlama
- ✅ Docker image build ve push
- ✅ Otomatik test ortamı deployment
- ✅ Manuel onaylı production deployment
- ✅ Otomatik rollback
- ✅ Slack bildirimleri

### Teknoloji Stack

| Bileşen | Teknoloji |
|---------|-----------|
| CI/CD Platform | GitHub Actions |
| Container Registry | Docker Hub |
| Runtime | .NET 8.0 |
| Container | Docker |
| Orchestration | Docker Compose |

---

## Branch Stratejisi

```
main (production)
  │
  ├── Protected: PR required
  ├── Auto-deploy to production (with approval)
  └── Semantic versioning tags (v1.0.0)
      │
develop (test/staging)
  │
  ├── Protected: PR required
  ├── Auto-deploy to test environment
  └── Test tags (test-YYYYMMDD-SHA)
      │
feature/* (development)
  │
  ├── No protection
  ├── CI runs on push
  └── Create PR to develop when ready
```

### Workflow

1. **Yeni özellik**: `feature/my-feature` branch'i oluştur
2. **Geliştirme**: Kod yaz, commit et, push et
3. **CI Kontrolü**: Her push'ta otomatik CI çalışır
4. **Code Review**: `develop`'a PR aç
5. **Merge to Develop**: PR onaylandıktan sonra merge
6. **Test Deploy**: Otomatik test ortamına deploy
7. **Test Validation**: QA testleri
8. **Production PR**: `develop` → `main` PR aç
9. **Production Deploy**: PR merge + approval → production deploy

---

## CI Pipeline

**Dosya**: `.github/workflows/ci-build-test.yml`

**Tetikleyici**:
- Her push (tüm branch'ler)
- Pull request'ler (main, develop)

### Jobs

#### 1. Build
```yaml
- Checkout code
- Setup .NET 8.0
- Restore dependencies (cached)
- Build solution (Release mode)
- Upload build artifacts
```

#### 2. Unit Tests
```yaml
- Run tests with coverage
- Generate Cobertura coverage report
- Upload test results
- Comment coverage on PR
- Check coverage threshold (min 60%)
```

#### 3. Docker Build Test
```yaml
- Setup Docker Buildx
- Build image (without push)
- Run container smoke test
- Verify container starts
```

#### 4. Security Scan
```yaml
- Run dotnet-outdated (outdated packages)
- Run Trivy vulnerability scanner
- Upload security scan results
```

### Status Checks

PR'ların merge edilebilmesi için tüm CI job'larının başarılı olması gerekir:

- ✅ Build must pass
- ✅ Tests must pass
- ✅ Coverage threshold met
- ✅ Docker build successful
- ✅ No critical security vulnerabilities

---

## CD Pipeline - Test

**Dosya**: `.github/workflows/cd-deploy-test.yml`

**Tetikleyici**: Push to `develop` branch

### Akış

```
Push to develop
      │
      ▼
┌─────────────────┐
│ Build & Push    │
│ Docker Image    │
│                 │
│ Tags:           │
│ - latest        │
│ - test-DATE-SHA │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Deploy to Test  │
│                 │
│ 1. Pull image   │
│ 2. Stop old     │
│ 3. Start new    │
│ 4. Health check │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Notify Slack    │
│ (if configured) │
└─────────────────┘
```

### Environment

- **Name**: `test`
- **Approval**: Gerekli değil (otomatik)
- **URL**: `http://test-server:8080`

### Deployment Script

```bash
# Sunucuda çalışan komutlar
docker pull $IMAGE:latest
docker-compose -f docker-compose.test.yml down
docker-compose -f docker-compose.test.yml up -d
# Health check
curl http://localhost:8080/health
```

---

## CD Pipeline - Production

**Dosya**: `.github/workflows/cd-deploy-prod.yml`

**Tetikleyici**:
- Push to `main` branch
- Manual workflow dispatch

### Akış

```
Push to main
      │
      ▼
┌─────────────────┐
│ Build & Push    │
│ Docker Image    │
│                 │
│ Tags:           │
│ - prod-latest   │
│ - v1.0.0        │
│ - prod-SHA      │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ ⏸️ APPROVAL     │◄── Manual approval required
│   REQUIRED      │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Pre-Deploy      │
│ Backup          │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Deploy to Prod  │
│                 │
│ 1. Pull image   │
│ 2. Graceful stop│
│ 3. Start new    │
│ 4. Health check │
│    (10 retries) │
└────────┬────────┘
         │
    ┌────┴────┐
    │         │
    ▼         ▼
┌───────┐ ┌──────────┐
│ ✅    │ │ ❌ Auto   │
│ Done! │ │ Rollback │
└───────┘ └──────────┘
         │
         ▼
┌─────────────────┐
│ Create Git Tag  │
│ v1.0.0          │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Notify Slack    │
└─────────────────┘
```

### Environment

- **Name**: `production`
- **Required reviewers**: Minimum 1
- **Wait timer**: 5 dakika (opsiyonel)
- **URL**: `https://api.yourdomain.com`

### Zero-Downtime Deployment

```bash
# 1. Pull new image (arka planda)
docker pull $IMAGE:new-version

# 2. Graceful shutdown (mevcut container)
docker-compose stop api-prod

# 3. Start new container
docker-compose up -d api-prod

# 4. Health check with retries
for i in {1..10}; do
  if curl -s http://localhost:8081/health | grep -q "healthy"; then
    echo "Success!"
    break
  fi
  sleep 5
done
```

---

## Rollback

**Dosya**: `.github/workflows/rollback.yml`

**Tetikleyici**: Manual dispatch only

### Kullanım

1. GitHub → Actions → "Rollback Deployment"
2. "Run workflow" butonuna tıkla
3. Parametreleri doldur:
   - **Environment**: `test` veya `production`
   - **Version**: Geri dönülecek versiyon (örn: `v1.0.0`, boş = önceki)
   - **Reason**: Rollback sebebi
4. "Run workflow"
5. Production için approval bekle

### Rollback Stratejisi

```
Current: v1.0.2
Previous: v1.0.1
Target: v1.0.0 (manual seçim)

Rollback steps:
1. Pull target image
2. Stop current container
3. Tag target as latest
4. Start new container
5. Health check
6. Log rollback event
```

---

## Kurulum

### 1. GitHub Secrets

Repository → Settings → Secrets and Variables → Actions

```bash
# Docker Hub
DOCKERHUB_USERNAME=your-username
DOCKERHUB_TOKEN=dckr_pat_xxxxx

# Test Server
TEST_SERVER_HOST=192.168.1.100
TEST_SERVER_USER=deploy
TEST_SERVER_SSH_KEY=-----BEGIN OPENSSH PRIVATE KEY-----...

# Production Server
PROD_SERVER_HOST=api.yourdomain.com
PROD_SERVER_USER=deploy
PROD_SERVER_SSH_KEY=-----BEGIN OPENSSH PRIVATE KEY-----...

# Slack (optional)
SLACK_WEBHOOK_URL=https://hooks.slack.com/services/xxx
```

### 2. Environments

Repository → Settings → Environments

**test Environment:**
- Secrets: Test server credentials

**production Environment:**
- Required reviewers: Add team members
- Deployment branches: Only `main`
- Secrets: Production server credentials

### 3. SSH Key Setup

```bash
# Generate ed25519 key
ssh-keygen -t ed25519 -C "github-deploy" -f deploy_key -N ""

# Add public key to server
ssh-copy-id -i deploy_key.pub user@server

# Or manually
cat deploy_key.pub >> ~/.ssh/authorized_keys

# Private key goes to GitHub Secrets
cat deploy_key
```

### 4. Docker Hub Setup

1. Login to Docker Hub
2. Account Settings → Security → New Access Token
3. Name: "github-actions"
4. Copy token to GitHub Secrets

### 5. Server Preparation

```bash
# On each server
# Install Docker
curl -fsSL https://get.docker.com | sh
usermod -aG docker deploy

# Clone repo
git clone <repo> /opt/eduportal
cd /opt/eduportal

# Create env files
cp .env.example .env.test  # or .env.prod
nano .env.test
```

---

## Sorun Giderme

### CI Build Başarısız

```bash
# Lokal test
dotnet restore
dotnet build --configuration Release
dotnet test
```

### Docker Build Başarısız

```bash
# Lokal test
docker build -t test -f src/EduPortal.API/Dockerfile .
docker run -d -p 8080:8080 test
curl http://localhost:8080/health
```

### SSH Bağlantı Hatası

```bash
# Key formatını kontrol et
ssh -i deploy_key -o StrictHostKeyChecking=no user@server "echo OK"

# Debug mode
ssh -vvv -i deploy_key user@server
```

### Health Check Başarısız

```bash
# Sunucuda kontrol
docker ps -a
docker logs eduportal-api-test
curl -v http://localhost:8080/health

# Network kontrolü
netstat -tlnp | grep 8080
```

### Rollback Başarısız

```bash
# Manual rollback
cd /opt/eduportal
docker images | grep eduportal
docker tag eduportal-api:v1.0.0 eduportal-api:latest
docker-compose restart api-test
```

---

## Best Practices

1. **Commit Messages**: Conventional commits kullan
   ```
   feat: Add user authentication
   fix: Resolve login timeout issue
   docs: Update API documentation
   ```

2. **PR Size**: Küçük, odaklı PR'lar oluştur

3. **Testing**: Minimum %80 test coverage hedefle

4. **Security**: Secrets'ları asla commit etme

5. **Monitoring**: Production deploy sonrası logları izle

6. **Rollback Plan**: Her deploy için rollback planı hazır olsun

7. **Documentation**: Değişiklikleri dokümante et
