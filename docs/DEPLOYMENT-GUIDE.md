# EduPortal - Deployment Guide

Bu kılavuz EduPortal'ın sunuculara nasıl deploy edileceğini açıklar.

## İçindekiler

1. [Sunucu Gereksinimleri](#sunucu-gereksinimleri)
2. [İlk Kurulum](#ilk-kurulum)
3. [CI/CD ile Deployment](#cicd-ile-deployment)
4. [Manuel Deployment](#manuel-deployment)
5. [Monitoring](#monitoring)
6. [Troubleshooting](#troubleshooting)

---

## Sunucu Gereksinimleri

### Minimum Gereksinimler

| Bileşen | TEST | PRODUCTION |
|---------|------|------------|
| CPU | 2 core | 4 core |
| RAM | 4 GB | 8 GB |
| Disk | 40 GB | 100 GB |
| OS | Ubuntu 22.04 | Ubuntu 22.04 |

### Yazılım Gereksinimleri

- Docker 20.10+
- Docker Compose 2.0+
- Git
- curl
- (Opsiyonel) NGINX

---

## İlk Kurulum

### 1. Sunucu Hazırlığı

```bash
# Sistem güncelleme
sudo apt update && sudo apt upgrade -y

# Docker kurulumu
curl -fsSL https://get.docker.com | sh

# Docker Compose kurulumu
sudo apt install docker-compose-plugin -y

# Kullanıcı ayarları
sudo usermod -aG docker $USER
newgrp docker

# Firewall
sudo ufw allow 8080/tcp  # TEST API
sudo ufw allow 8081/tcp  # PROD API
sudo ufw allow 22/tcp    # SSH
sudo ufw enable
```

### 2. Proje Klonlama

```bash
# Proje dizini
sudo mkdir -p /opt/eduportal
sudo chown $USER:$USER /opt/eduportal

# Klonla
git clone <repository-url> /opt/eduportal
cd /opt/eduportal
```

### 3. Environment Dosyaları

```bash
# Test ortamı
cp .env.example .env.test
nano .env.test
```

```env
# .env.test
MSSQL_SA_PASSWORD=TestPass123!
DB_HOST=mssql-test
DB_PORT=1433
DB_NAME=EduPortalDb_Test
DB_USER=sa
JWT_KEY=TestEnvironmentJwtKey32CharsLong!!
JWT_ISSUER=EduPortalAPI
JWT_AUDIENCE=EduPortalClient
```

```bash
# Production ortamı
cp .env.example .env.prod
nano .env.prod
```

```env
# .env.prod
MSSQL_SA_PASSWORD=<production-password>
DB_HOST=<production-db-server>
DB_PORT=1433
DB_NAME=EduPortalDb_Prod
DB_USER=<db-user>
JWT_KEY=<secure-production-key-32-chars>
JWT_ISSUER=EduPortalAPI
JWT_AUDIENCE=EduPortalClient
```

### 4. Deploy Kullanıcısı (CI/CD için)

```bash
# Deploy kullanıcısı oluştur
sudo adduser deploy --disabled-password
sudo usermod -aG docker deploy

# SSH key ayarla
sudo mkdir -p /home/deploy/.ssh
sudo nano /home/deploy/.ssh/authorized_keys
# GitHub Actions'tan public key'i yapıştır

sudo chown -R deploy:deploy /home/deploy/.ssh
sudo chmod 700 /home/deploy/.ssh
sudo chmod 600 /home/deploy/.ssh/authorized_keys

# Proje dizini izinleri
sudo chown -R deploy:deploy /opt/eduportal
```

### 5. İlk Deploy

```bash
# TEST ortamı
cd /opt/eduportal
./scripts/start-test.sh

# Kontrol
curl http://localhost:8080/health
```

---

## CI/CD ile Deployment

CI/CD yapılandırıldıktan sonra deployment otomatiktir.

### TEST Ortamı

1. Feature branch'i `develop`'a merge et
2. GitHub Actions otomatik deploy eder
3. Slack bildirimi gelir (yapılandırıldıysa)

### PRODUCTION Ortamı

1. `develop`'u `main`'e merge et (PR ile)
2. GitHub Actions workflow başlar
3. **Approval gerekli** - GitHub'da approve et
4. Deploy başlar
5. Health check otomatik yapılır
6. Başarılı ise git tag oluşturulur

### Manuel Tetikleme

1. GitHub → Actions → "CD - Deploy to Production"
2. "Run workflow"
3. Version gir (opsiyonel)
4. "Run workflow"
5. Approval ver

---

## Manuel Deployment

CI/CD kullanılamıyorsa manuel deployment:

### Script ile

```bash
# TEST
./scripts/deploy-test.sh

# PRODUCTION
./scripts/deploy-prod.sh
```

### Adım Adım

```bash
cd /opt/eduportal

# 1. Kodu güncelle
git pull origin main

# 2. Docker image build
docker-compose -f docker-compose.test.yml --env-file .env.test build

# 3. Mevcut container'ları durdur
docker-compose -f docker-compose.test.yml --env-file .env.test down

# 4. Yeni container'ları başlat
docker-compose -f docker-compose.test.yml --env-file .env.test up -d

# 5. Health check
sleep 15
curl http://localhost:8080/health

# 6. Logları kontrol et
docker-compose -f docker-compose.test.yml --env-file .env.test logs -f
```

### Docker Hub'dan Pull

```bash
# Login
docker login

# Pull latest
docker pull yourusername/eduportal-api:latest

# veya specific version
docker pull yourusername/eduportal-api:v1.0.0

# Restart
docker-compose -f docker-compose.test.yml --env-file .env.test up -d
```

---

## Monitoring

### Container Durumu

```bash
# Çalışan container'lar
docker ps

# Tüm container'lar
docker ps -a

# Resource kullanımı
docker stats
```

### Loglar

```bash
# Tüm loglar
./scripts/logs-test.sh

# API logları
docker-compose -f docker-compose.test.yml logs -f api-test

# Son 100 satır
docker logs --tail=100 eduportal-api-test
```

### Health Check

```bash
# Manuel health check
curl http://localhost:8080/health

# Continuous monitoring
watch -n 5 'curl -s http://localhost:8080/health'

# Script ile
./scripts/deploy/health-check.sh http://localhost:8080/health
```

### Disk Kullanımı

```bash
# Docker disk kullanımı
docker system df

# Temizlik
docker system prune -f
docker image prune -f
```

---

## Troubleshooting

### Container Başlamıyor

```bash
# Logları kontrol et
docker logs eduportal-api-test

# Container detayları
docker inspect eduportal-api-test

# Yeniden başlat
docker-compose -f docker-compose.test.yml --env-file .env.test restart
```

### Database Bağlantı Hatası

```bash
# MSSQL container durumu
docker logs eduportal-mssql-test

# Bağlantı testi
docker exec -it eduportal-api-test sh -c 'curl -v telnet://mssql-test:1433'

# Connection string kontrolü
docker exec -it eduportal-api-test printenv | grep Connection
```

### Port Çakışması

```bash
# Port kullanan servisi bul
sudo lsof -i :8080
sudo netstat -tlnp | grep 8080

# Container'ı durdur
docker stop $(docker ps -q --filter publish=8080)
```

### Bellek Yetersizliği

```bash
# Memory kullanımı
docker stats --no-stream

# Container limit
docker inspect eduportal-api-test --format='{{.HostConfig.Memory}}'

# Sistemi kontrol et
free -h
```

### Rollback

```bash
# Önceki image'a dön
./scripts/deploy/rollback-prod.sh

# Specific versiyona dön
./scripts/deploy/rollback-prod.sh v1.0.0

# Manuel rollback
docker tag yourusername/eduportal-api:v1.0.0 yourusername/eduportal-api:prod-latest
docker-compose -f docker-compose.prod.yml --env-file .env.prod up -d
```

---

## Checklist

### Deploy Öncesi

- [ ] Kod review yapıldı
- [ ] Testler geçti
- [ ] Environment variables güncellendi
- [ ] Database migration hazır (varsa)
- [ ] Backup alındı (production)
- [ ] Team bilgilendirildi

### Deploy Sonrası

- [ ] Health check başarılı
- [ ] API'ler çalışıyor
- [ ] Loglar normal
- [ ] Performans OK
- [ ] Monitoring aktif

### Sorun Durumunda

- [ ] Logları topla
- [ ] Rollback planı hazır
- [ ] Team'i bilgilendir
- [ ] Incident ticket aç

---

## Yardımcı Komutlar

```bash
# Durum kontrolü
./scripts/status.sh

# Test ortamı
./scripts/start-test.sh
./scripts/stop-test.sh
./scripts/restart-test.sh
./scripts/logs-test.sh

# Production ortamı
./scripts/start-prod.sh
./scripts/stop-prod.sh
./scripts/restart-prod.sh
./scripts/logs-prod.sh

# Deployment
./scripts/deploy/deploy-to-test.sh
./scripts/deploy/deploy-to-prod.sh
./scripts/deploy/rollback-prod.sh
./scripts/deploy/health-check.sh <url>

# Backup
./scripts/backup-db-test.sh
```
