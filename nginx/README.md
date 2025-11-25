# EduPortal - NGINX Configuration

Bu klasör EduPortal API'leri için NGINX referans config dosyalarını içerir.

## Dosyalar

| Dosya | Açıklama |
|-------|----------|
| `proxy_params.conf` | Ortak proxy parametreleri |
| `test-api.conf` | TEST ortamı config (port 8080) |
| `api.conf` | PRODUCTION ortamı config (port 8081) |

## Kurulum Adımları

### 1. NGINX Kurulumu

```bash
# Ubuntu/Debian
sudo apt update
sudo apt install nginx

# Başlat ve enable et
sudo systemctl start nginx
sudo systemctl enable nginx
```

### 2. Config Dosyalarını Kopyala

```bash
# Proxy params snippet'i kopyala
sudo cp proxy_params.conf /etc/nginx/snippets/

# Site config'lerini kopyala
sudo cp test-api.conf /etc/nginx/sites-available/
sudo cp api.conf /etc/nginx/sites-available/

# Symbolic link oluştur (enable et)
sudo ln -s /etc/nginx/sites-available/test-api.conf /etc/nginx/sites-enabled/
sudo ln -s /etc/nginx/sites-available/api.conf /etc/nginx/sites-enabled/

# Default site'ı kaldır (opsiyonel)
sudo rm /etc/nginx/sites-enabled/default
```

### 3. Domain Adını Güncelle

Config dosyalarındaki `yourdomain.com` kısmını gerçek domain adınızla değiştirin:

```bash
# test-api.conf içinde
sudo nano /etc/nginx/sites-available/test-api.conf
# "test-api.yourdomain.com" -> "test-api.gercekdomain.com"

# api.conf içinde
sudo nano /etc/nginx/sites-available/api.conf
# "api.yourdomain.com" -> "api.gercekdomain.com"
```

### 4. Config'i Test Et

```bash
sudo nginx -t
```

### 5. NGINX'i Yeniden Yükle

```bash
sudo systemctl reload nginx
```

### 6. SSL Sertifikası (Let's Encrypt)

```bash
# Certbot kurulumu
sudo apt install certbot python3-certbot-nginx

# TEST ortamı için SSL
sudo certbot --nginx -d test-api.gercekdomain.com

# PRODUCTION ortamı için SSL
sudo certbot --nginx -d api.gercekdomain.com

# Otomatik yenileme testi
sudo certbot renew --dry-run
```

## Port Yapısı

```
┌─────────────────────────────────────────────────────────────┐
│                      NGINX (Host)                           │
│                                                             │
│  test-api.domain.com:80/443  ──────►  127.0.0.1:8080       │
│                                              │              │
│                                              ▼              │
│                                    ┌─────────────────┐      │
│                                    │  API Container  │      │
│                                    │  (TEST)         │      │
│                                    └─────────────────┘      │
│                                                             │
│  api.domain.com:80/443  ───────────►  127.0.0.1:8081       │
│                                              │              │
│                                              ▼              │
│                                    ┌─────────────────┐      │
│                                    │  API Container  │      │
│                                    │  (PRODUCTION)   │      │
│                                    └─────────────────┘      │
└─────────────────────────────────────────────────────────────┘
```

## Sorun Giderme

### NGINX Başlamıyorsa

```bash
# Hata loglarını kontrol et
sudo journalctl -u nginx -f
sudo tail -f /var/log/nginx/error.log

# Config syntax kontrolü
sudo nginx -t
```

### 502 Bad Gateway

```bash
# API container çalışıyor mu?
docker ps | grep eduportal

# API health check
curl http://localhost:8080/health  # TEST
curl http://localhost:8081/health  # PROD
```

### Permission Denied

```bash
# SELinux kontrolü (CentOS/RHEL)
sudo setsebool -P httpd_can_network_connect 1
```

## Güvenlik Notları

1. **HTTPS Zorunlu**: Production'da mutlaka SSL kullanın
2. **Rate Limiting**: DDoS koruması için aktif
3. **Security Headers**: XSS, clickjacking koruması
4. **IP Whitelist**: Health endpoint için opsiyonel
