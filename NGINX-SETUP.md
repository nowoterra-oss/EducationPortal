# EduPortal - NGINX Kurulum Rehberi

Bu dokümantasyon NGINX'in host sunucuda nasıl kurulup yapılandırılacağını açıklar.

## Gereksinimler

- Ubuntu 20.04+ / Debian 10+
- Root veya sudo erişimi
- Domain adları (DNS kayıtları yapılmış)

## 1. NGINX Kurulumu

```bash
# Paket listesini güncelle
sudo apt update

# NGINX'i kur
sudo apt install nginx -y

# Durumu kontrol et
sudo systemctl status nginx

# Enable et (sunucu başlangıcında çalışsın)
sudo systemctl enable nginx
```

## 2. Firewall Ayarları

```bash
# UFW kullanıyorsanız
sudo ufw allow 'Nginx Full'
sudo ufw status

# veya iptables
sudo iptables -A INPUT -p tcp --dport 80 -j ACCEPT
sudo iptables -A INPUT -p tcp --dport 443 -j ACCEPT
```

## 3. Config Dosyalarını Kopyala

```bash
cd /opt/eduportal

# Proxy params snippet'i kopyala
sudo cp nginx/proxy_params.conf /etc/nginx/snippets/

# Site config'lerini kopyala
sudo cp nginx/test-api.conf /etc/nginx/sites-available/
sudo cp nginx/api.conf /etc/nginx/sites-available/
```

## 4. Domain Adlarını Güncelle

```bash
# TEST API config'ini düzenle
sudo nano /etc/nginx/sites-available/test-api.conf
# "test-api.yourdomain.com" → "test-api.gercekdomain.com"

# PRODUCTION API config'ini düzenle
sudo nano /etc/nginx/sites-available/api.conf
# "api.yourdomain.com" → "api.gercekdomain.com"
```

## 5. Site'ları Aktif Et

```bash
# Symbolic linkler oluştur
sudo ln -s /etc/nginx/sites-available/test-api.conf /etc/nginx/sites-enabled/
sudo ln -s /etc/nginx/sites-available/api.conf /etc/nginx/sites-enabled/

# Default site'ı kaldır (opsiyonel)
sudo rm /etc/nginx/sites-enabled/default

# Dosyaları listele
ls -la /etc/nginx/sites-enabled/
```

## 6. Config'i Test Et

```bash
sudo nginx -t
```

Beklenen çıktı:
```
nginx: the configuration file /etc/nginx/nginx.conf syntax is ok
nginx: configuration file /etc/nginx/nginx.conf test is successful
```

## 7. NGINX'i Yeniden Yükle

```bash
sudo systemctl reload nginx
```

## 8. SSL Sertifikası (Let's Encrypt)

### Certbot Kurulumu

```bash
# Certbot ve NGINX plugin'i kur
sudo apt install certbot python3-certbot-nginx -y
```

### SSL Sertifikası Al

```bash
# TEST API için
sudo certbot --nginx -d test-api.gercekdomain.com

# PRODUCTION API için
sudo certbot --nginx -d api.gercekdomain.com
```

Certbot size şunları soracak:
1. E-posta adresi (yenileme bildirimleri için)
2. Şartları kabul edin
3. HTTP→HTTPS yönlendirmesi (Evet seçin)

### Otomatik Yenileme Test

```bash
sudo certbot renew --dry-run
```

### Yenileme Cron'u (Otomatik eklenir)

```bash
# Kontrol et
sudo systemctl status certbot.timer
```

## 9. Test

```bash
# HTTP üzerinden test (SSL'den önce)
curl http://test-api.gercekdomain.com/health
curl http://api.gercekdomain.com/health

# HTTPS üzerinden test (SSL'den sonra)
curl https://test-api.gercekdomain.com/health
curl https://api.gercekdomain.com/health
```

## Komple Kurulum Scripti

```bash
#!/bin/bash
# nginx-setup.sh

set -e

DOMAIN_TEST="test-api.yourdomain.com"  # DEĞİŞTİR
DOMAIN_PROD="api.yourdomain.com"        # DEĞİŞTİR
PROJECT_DIR="/opt/eduportal"

echo "=== NGINX Kurulumu ==="

# NGINX kur
sudo apt update
sudo apt install nginx certbot python3-certbot-nginx -y

# Config dosyalarını kopyala
sudo cp $PROJECT_DIR/nginx/proxy_params.conf /etc/nginx/snippets/
sudo cp $PROJECT_DIR/nginx/test-api.conf /etc/nginx/sites-available/
sudo cp $PROJECT_DIR/nginx/api.conf /etc/nginx/sites-available/

# Domain adlarını güncelle
sudo sed -i "s/test-api.yourdomain.com/$DOMAIN_TEST/g" /etc/nginx/sites-available/test-api.conf
sudo sed -i "s/api.yourdomain.com/$DOMAIN_PROD/g" /etc/nginx/sites-available/api.conf

# Site'ları aktif et
sudo ln -sf /etc/nginx/sites-available/test-api.conf /etc/nginx/sites-enabled/
sudo ln -sf /etc/nginx/sites-available/api.conf /etc/nginx/sites-enabled/
sudo rm -f /etc/nginx/sites-enabled/default

# Test ve reload
sudo nginx -t
sudo systemctl reload nginx

echo "=== NGINX kurulumu tamamlandı ==="
echo ""
echo "SSL sertifikası almak için:"
echo "  sudo certbot --nginx -d $DOMAIN_TEST"
echo "  sudo certbot --nginx -d $DOMAIN_PROD"
```

## Yapılandırma Özeti

| Dosya | Konum |
|-------|-------|
| proxy_params.conf | /etc/nginx/snippets/ |
| test-api.conf | /etc/nginx/sites-available/ |
| api.conf | /etc/nginx/sites-available/ |

| URL | Hedef |
|-----|-------|
| test-api.domain.com | 127.0.0.1:8080 |
| api.domain.com | 127.0.0.1:8081 |

## Troubleshooting

### 502 Bad Gateway

```bash
# API container çalışıyor mu?
docker ps | grep eduportal

# Port açık mı?
curl http://127.0.0.1:8080/health
curl http://127.0.0.1:8081/health

# NGINX error log
sudo tail -f /var/log/nginx/error.log
```

### SSL Sertifika Sorunu

```bash
# Sertifika durumu
sudo certbot certificates

# Manuel yenileme
sudo certbot renew

# Sertifikayı sil ve yeniden al
sudo certbot delete --cert-name test-api.domain.com
sudo certbot --nginx -d test-api.domain.com
```

### Permission Denied

```bash
# Socket izinleri
ls -la /var/run/docker.sock

# SELinux (CentOS/RHEL)
sudo setsebool -P httpd_can_network_connect 1
```

### Rate Limit Sorunu

```bash
# Rate limit zone'ları kontrol et
sudo grep -r "limit_req_zone" /etc/nginx/

# Geçici olarak devre dışı bırak (test için)
# limit_req satırını yorum satırı yap
```

## Log Dosyaları

| Log | Konum |
|-----|-------|
| NGINX Access | /var/log/nginx/access.log |
| NGINX Error | /var/log/nginx/error.log |
| TEST API Access | /var/log/nginx/eduportal-test-api.access.log |
| TEST API Error | /var/log/nginx/eduportal-test-api.error.log |
| PROD API Access | /var/log/nginx/eduportal-api.access.log |
| PROD API Error | /var/log/nginx/eduportal-api.error.log |

```bash
# Log takibi
sudo tail -f /var/log/nginx/eduportal-api.access.log
sudo tail -f /var/log/nginx/eduportal-api.error.log
```
