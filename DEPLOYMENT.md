# EduPortal - Docker Deployment Guide

Bu dokümantasyon EduPortal API'nin Docker ile nasıl dağıtılacağını açıklar.

## Gereksinimler

- Docker 20.10+
- Docker Compose 2.0+
- Git
- curl (health check için)

## Mimari Genel Bakış

```
┌──────────────────────────────────────────────────────────────────────┐
│                           HOST SERVER                                 │
│                                                                       │
│  ┌─────────────────────────────────────────────────────────────────┐ │
│  │                        NGINX (Host)                              │ │
│  │  Port 80/443                                                     │ │
│  │  • test-api.domain.com → 127.0.0.1:8080                         │ │
│  │  • api.domain.com → 127.0.0.1:8081                              │ │
│  └─────────────────────────────────────────────────────────────────┘ │
│                              │                                        │
│           ┌──────────────────┴──────────────────┐                    │
│           ▼                                      ▼                    │
│  ┌─────────────────────┐              ┌─────────────────────┐        │
│  │   TEST ENVIRONMENT  │              │  PROD ENVIRONMENT   │        │
│  │   (Docker Network)  │              │   (Docker Network)  │        │
│  │                     │              │                     │        │
│  │  ┌───────────────┐  │              │  ┌───────────────┐  │        │
│  │  │  API :8080    │  │              │  │  API :8081    │  │        │
│  │  └───────────────┘  │              │  └───────────────┘  │        │
│  │         │           │              │         │           │        │
│  │         ▼           │              │         │           │        │
│  │  ┌───────────────┐  │              │         ▼           │        │
│  │  │ MSSQL :1433   │  │              │   External MSSQL    │        │
│  │  │ (Container)   │  │              │   (Ayrı Sunucu)    │        │
│  │  └───────────────┘  │              │                     │        │
│  └─────────────────────┘              └─────────────────────┘        │
└──────────────────────────────────────────────────────────────────────┘
```

## Hızlı Başlangıç

### TEST Ortamı

```bash
# 1. Repository'yi klonla
git clone <repo-url>
cd EducationPortal

# 2. Environment dosyasını kontrol et
cat .env.test

# 3. Başlat
./scripts/start-test.sh

# 4. Test et
curl http://localhost:8080/health
```

### PRODUCTION Ortamı

```bash
# 1. .env.prod dosyasını yapılandır
nano .env.prod  # Gerçek değerleri gir

# 2. Başlat
./scripts/start-prod.sh

# 3. Test et
curl http://localhost:8081/health
```

---

## Detaylı Kurulum

### 1. Docker Kurulumu

```bash
# Docker'ı kur
curl -fsSL https://get.docker.com | sh

# Kullanıcıyı docker grubuna ekle
sudo usermod -aG docker $USER

# Yeniden giriş yap
exit
# Tekrar bağlan

# Docker Compose kurulumu (v2)
sudo apt install docker-compose-plugin

# Versiyonları kontrol et
docker --version
docker compose version
```

### 2. Proje Dosyalarını Hazırla

```bash
# Projeyi klonla
git clone <repo-url> /opt/eduportal
cd /opt/eduportal

# Script'leri çalıştırılabilir yap
chmod +x scripts/*.sh
```

### 3. TEST Ortamı Kurulumu

TEST ortamı hem API hem de MSSQL'i Docker içinde çalıştırır.

```bash
# .env.test dosyasını kontrol et/düzenle
nano .env.test
```

```env
# .env.test içeriği
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
# Test ortamını başlat
./scripts/start-test.sh

# Logları izle
./scripts/logs-test.sh

# Health check
curl http://localhost:8080/health
```

### 4. PRODUCTION Ortamı Kurulumu

PRODUCTION ortamı sadece API'yi Docker'da çalıştırır. MSSQL harici sunucudadır.

```bash
# .env.prod dosyasını yapılandır
nano .env.prod
```

```env
# .env.prod içeriği - GERÇEK DEĞERLERİ GİR!
MSSQL_SA_PASSWORD=your_production_db_password
DB_HOST=your-prod-mssql-server.com
DB_PORT=1433
DB_NAME=EduPortalDb_Prod
DB_USER=sa
JWT_KEY=YourSecureProductionJwtKey32Chars!
JWT_ISSUER=EduPortalAPI
JWT_AUDIENCE=EduPortalClient
```

```bash
# Production ortamını başlat
./scripts/start-prod.sh

# Health check
curl http://localhost:8081/health
```

---

## Yönetim Komutları

### Temel Komutlar

| Komut | TEST | PRODUCTION |
|-------|------|------------|
| Başlat | `./scripts/start-test.sh` | `./scripts/start-prod.sh` |
| Durdur | `./scripts/stop-test.sh` | `./scripts/stop-prod.sh` |
| Yeniden Başlat | `./scripts/restart-test.sh` | `./scripts/restart-prod.sh` |
| Loglar | `./scripts/logs-test.sh` | `./scripts/logs-prod.sh` |
| Deploy | `./scripts/deploy-test.sh` | `./scripts/deploy-prod.sh` |
| Durum | `./scripts/status.sh` | `./scripts/status.sh` |

### DB Backup (Sadece TEST)

```bash
./scripts/backup-db-test.sh
# Backup dosyası: backups/EduPortalDb_Test_YYYYMMDD_HHMMSS.bak
```

### Docker Compose Komutları

```bash
# TEST ortamı
docker-compose -f docker-compose.test.yml --env-file .env.test ps
docker-compose -f docker-compose.test.yml --env-file .env.test logs -f api-test
docker-compose -f docker-compose.test.yml --env-file .env.test exec api-test /bin/bash

# PRODUCTION ortamı
docker-compose -f docker-compose.prod.yml --env-file .env.prod ps
docker-compose -f docker-compose.prod.yml --env-file .env.prod logs -f api-prod
```

---

## Systemd ile Otomatik Başlatma

```bash
# Service dosyalarını kopyala
sudo cp systemd/eduportal-test.service /etc/systemd/system/
sudo cp systemd/eduportal-prod.service /etc/systemd/system/

# WorkingDirectory'yi düzenle (gerekirse)
sudo nano /etc/systemd/system/eduportal-test.service
sudo nano /etc/systemd/system/eduportal-prod.service

# Daemon'ı yeniden yükle
sudo systemctl daemon-reload

# Enable et (sunucu başlangıcında otomatik başlar)
sudo systemctl enable eduportal-test
sudo systemctl enable eduportal-prod

# Yönetim komutları
sudo systemctl start eduportal-test
sudo systemctl status eduportal-test
sudo systemctl stop eduportal-test
sudo journalctl -u eduportal-test -f
```

---

## Port Özeti

| Servis | TEST | PRODUCTION |
|--------|------|------------|
| API | 127.0.0.1:8080 | 127.0.0.1:8081 |
| MSSQL | 127.0.0.1:1433 | External Server |
| Health Check | /health | /health |

---

## Troubleshooting

### Container Başlamıyorsa

```bash
# Logları kontrol et
docker-compose -f docker-compose.test.yml --env-file .env.test logs

# Container durumunu kontrol et
docker ps -a

# Network durumunu kontrol et
docker network ls
```

### Database Bağlantı Hatası

```bash
# MSSQL container'ın hazır olmasını bekle
docker-compose -f docker-compose.test.yml --env-file .env.test logs mssql-test

# Container içinden bağlantı test et
docker exec -it eduportal-api-test curl -v telnet://mssql-test:1433
```

### Port Çakışması

```bash
# Portu kullanan servisi bul
sudo lsof -i :8080
sudo lsof -i :1433

# Servisi durdur veya portu değiştir
```

### Image Yeniden Build

```bash
# Cache'siz build
docker-compose -f docker-compose.test.yml --env-file .env.test build --no-cache

# Eski image'ları temizle
docker image prune -f
```

---

## Güvenlik Notları

1. **.env dosyaları**: Git'e commit etmeyin!
2. **SA Password**: Production'da güçlü şifre kullanın
3. **JWT Key**: Her ortam için farklı, güçlü key
4. **Port Binding**: 127.0.0.1'e bind (sadece localhost)
5. **NGINX**: SSL/TLS mutlaka aktif olmalı

---

## Sonraki Adımlar

1. [NGINX-SETUP.md](NGINX-SETUP.md) - NGINX kurulumu
2. [PROD-DB-SETUP.md](PROD-DB-SETUP.md) - Production DB kurulumu
