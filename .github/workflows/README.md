# EduPortal - GitHub Actions Workflows

Bu klasör EduPortal projesinin CI/CD pipeline'larını içerir.

## Workflow Dosyaları

| Workflow | Dosya | Trigger | Açıklama |
|----------|-------|---------|----------|
| CI Build & Test | `ci-build-test.yml` | PR, Push | Build, test, docker build testi |
| CD Deploy Test | `cd-deploy-test.yml` | Push to `develop` | Test ortamına deploy |
| CD Deploy Prod | `cd-deploy-prod.yml` | Push to `main` | Production'a deploy (onay gerekli) |
| Rollback | `rollback.yml` | Manual | Önceki versiyona geri dön |

## Pipeline Akışı

```
┌─────────────────────────────────────────────────────────────────┐
│                        DEVELOPMENT                              │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   feature/* branch                                              │
│        │                                                        │
│        ▼                                                        │
│   ┌─────────┐    ┌──────────────────────────────────────────┐  │
│   │   PR    │───►│  CI: Build → Test → Docker Build Test   │  │
│   └─────────┘    └──────────────────────────────────────────┘  │
│        │                           │                            │
│        │              ❌ Fail: PR merge blocked                 │
│        │              ✅ Pass: Ready for review                 │
│        ▼                                                        │
├─────────────────────────────────────────────────────────────────┤
│                          TEST                                   │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   develop branch                                                │
│        │                                                        │
│        ▼                                                        │
│   ┌──────────────────────────────────────────────────────────┐ │
│   │  CD Test: Build Image → Push → Deploy → Health Check    │ │
│   └──────────────────────────────────────────────────────────┘ │
│        │                                                        │
│        │              ❌ Fail: Slack notification               │
│        │              ✅ Pass: Test environment ready           │
│        ▼                                                        │
├─────────────────────────────────────────────────────────────────┤
│                       PRODUCTION                                │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   main branch                                                   │
│        │                                                        │
│        ▼                                                        │
│   ┌──────────────────────────────────────────────────────────┐ │
│   │            ⏸️  APPROVAL REQUIRED                          │ │
│   └──────────────────────────────────────────────────────────┘ │
│        │                                                        │
│        ▼                                                        │
│   ┌──────────────────────────────────────────────────────────┐ │
│   │  CD Prod: Backup → Build → Push → Deploy → Health Check │ │
│   └──────────────────────────────────────────────────────────┘ │
│        │                                                        │
│        │              ❌ Fail: Auto-rollback + Alert            │
│        │              ✅ Pass: Production live!                 │
│        ▼                                                        │
│   ┌──────────────────────────────────────────────────────────┐ │
│   │                   🎉 DEPLOYED                              │ │
│   └──────────────────────────────────────────────────────────┘ │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

## Gerekli GitHub Secrets

Repository Settings → Secrets and Variables → Actions

### Docker Hub
| Secret | Açıklama |
|--------|----------|
| `DOCKERHUB_USERNAME` | Docker Hub kullanıcı adı |
| `DOCKERHUB_TOKEN` | Docker Hub access token |

### Test Server
| Secret | Açıklama |
|--------|----------|
| `TEST_SERVER_HOST` | Test sunucusu IP/hostname |
| `TEST_SERVER_USER` | SSH kullanıcı adı |
| `TEST_SERVER_SSH_KEY` | SSH private key (ed25519) |

### Production Server
| Secret | Açıklama |
|--------|----------|
| `PROD_SERVER_HOST` | Production sunucusu IP/hostname |
| `PROD_SERVER_USER` | SSH kullanıcı adı |
| `PROD_SERVER_SSH_KEY` | SSH private key (ed25519) |

### Notifications
| Secret | Açıklama |
|--------|----------|
| `SLACK_WEBHOOK_URL` | Slack incoming webhook URL |

## Environment Ayarları

Repository Settings → Environments

### `test` Environment
- Auto-deploy (onay gerekmez)
- Secrets: Test server credentials

### `production` Environment
- Required reviewers: En az 1 onay
- Wait timer: 5 dakika (opsiyonel)
- Deployment branches: Sadece `main`

## SSH Key Oluşturma

```bash
# Ed25519 key oluştur (önerilen)
ssh-keygen -t ed25519 -C "github-actions-deploy" -f deploy_key -N ""

# Public key'i sunucuya ekle
cat deploy_key.pub >> ~/.ssh/authorized_keys

# Private key'i GitHub Secrets'a ekle
cat deploy_key
```

## Docker Hub Token Oluşturma

1. Docker Hub → Account Settings → Security
2. New Access Token
3. Token'a isim ver (örn: "github-actions")
4. Token'ı kopyala ve GitHub Secrets'a ekle

## Slack Webhook Oluşturma

1. Slack Workspace → Apps → Incoming Webhooks
2. Add to Slack
3. Kanal seç
4. Webhook URL'i kopyala ve GitHub Secrets'a ekle

## Manuel Workflow Çalıştırma

### Rollback
1. Actions → Rollback Deployment
2. "Run workflow" butonuna tıkla
3. Environment seç (test/production)
4. Version gir (opsiyonel, boş bırakılırsa önceki versiyona döner)
5. Rollback sebebini yaz
6. "Run workflow"

### Production Deploy (Manual)
1. Actions → CD - Deploy to Production
2. "Run workflow"
3. Version gir (opsiyonel)
4. "Run workflow"
5. Approval bekle

## Troubleshooting

### Build Hatası
```bash
# Lokal olarak test et
dotnet build EduPortal.sln
dotnet test
```

### Docker Build Hatası
```bash
# Lokal olarak test et
docker build -t test -f src/EduPortal.API/Dockerfile .
```

### Deploy Hatası
1. Actions loglarını kontrol et
2. SSH bağlantısını test et:
   ```bash
   ssh -i deploy_key user@server "echo 'Connection OK'"
   ```
3. Sunucudaki Docker durumunu kontrol et:
   ```bash
   docker ps -a
   docker logs eduportal-api-test
   ```

### Health Check Hatası
```bash
# Sunucuda kontrol et
curl -v http://localhost:8080/health
docker-compose logs api-test
```
