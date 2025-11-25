# EduPortal - Production Database Kurulum Rehberi

Bu dokümantasyon production ortamı için harici MSSQL sunucusunun nasıl kurulup yapılandırılacağını açıklar.

## Gereksinimler

- Ubuntu 20.04+ veya Windows Server 2019+
- Minimum 4GB RAM (önerilen: 8GB+)
- Minimum 20GB disk alanı
- Network erişimi (API sunucusundan erişilebilir)

## Seçenek 1: Linux'ta MSSQL Server

### 1.1 MSSQL Server Kurulumu (Ubuntu)

```bash
# Microsoft GPG anahtarını ekle
curl https://packages.microsoft.com/keys/microsoft.asc | sudo tee /etc/apt/trusted.gpg.d/microsoft.asc

# Repository ekle
sudo add-apt-repository "$(wget -qO- https://packages.microsoft.com/config/ubuntu/22.04/mssql-server-2022.list)"

# Paket listesini güncelle
sudo apt update

# MSSQL Server kur
sudo apt install mssql-server -y

# MSSQL'i yapılandır
sudo /opt/mssql/bin/mssql-conf setup
```

Yapılandırma sırasında:
1. Edition seçin: `3` (Express - ücretsiz) veya lisanslı sürüm
2. SA şifresini girin (güçlü şifre!)
3. Şartları kabul edin

### 1.2 MSSQL Tools Kurulumu

```bash
# Tools repository ekle
curl https://packages.microsoft.com/config/ubuntu/22.04/prod.list | sudo tee /etc/apt/sources.list.d/mssql-release.list

sudo apt update
sudo ACCEPT_EULA=Y apt install mssql-tools18 unixodbc-dev -y

# PATH'e ekle
echo 'export PATH="$PATH:/opt/mssql-tools18/bin"' >> ~/.bashrc
source ~/.bashrc
```

### 1.3 Bağlantı Testi

```bash
# Lokalde test
sqlcmd -S localhost -U sa -P 'YourPassword' -C -Q "SELECT @@VERSION"
```

### 1.4 Database Oluştur

```bash
sqlcmd -S localhost -U sa -P 'YourPassword' -C -Q "
CREATE DATABASE EduPortalDb_Prod
GO
"
```

### 1.5 Firewall Ayarları

```bash
# UFW
sudo ufw allow from <API_SERVER_IP> to any port 1433

# veya iptables
sudo iptables -A INPUT -p tcp -s <API_SERVER_IP> --dport 1433 -j ACCEPT
```

### 1.6 Uzak Bağlantıları Etkinleştir

```bash
# TCP/IP'yi etkinleştir
sudo /opt/mssql/bin/mssql-conf set network.tcpport 1433
sudo /opt/mssql/bin/mssql-conf set network.ipaddress 0.0.0.0

# Servisi yeniden başlat
sudo systemctl restart mssql-server
```

---

## Seçenek 2: Docker'da MSSQL (Ayrı Sunucu)

### 2.1 Docker Kurulumu

```bash
curl -fsSL https://get.docker.com | sh
sudo usermod -aG docker $USER
```

### 2.2 MSSQL Container

```bash
# Volume oluştur
docker volume create mssql-prod-data

# Container çalıştır
docker run -d \
  --name mssql-prod \
  --restart unless-stopped \
  -e 'ACCEPT_EULA=Y' \
  -e 'MSSQL_SA_PASSWORD=YourStrongPassword123!' \
  -e 'MSSQL_PID=Developer' \
  -p 1433:1433 \
  -v mssql-prod-data:/var/opt/mssql \
  mcr.microsoft.com/mssql/server:2022-latest
```

### 2.3 Database Oluştur

```bash
docker exec -it mssql-prod /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P 'YourStrongPassword123!' -C \
  -Q "CREATE DATABASE EduPortalDb_Prod"
```

---

## Seçenek 3: Windows Server'da MSSQL

### 3.1 SQL Server Kurulumu

1. [SQL Server İndirme](https://www.microsoft.com/sql-server/sql-server-downloads)
2. Setup wizard'ı çalıştır
3. "New SQL Server stand-alone installation" seç
4. Mixed Mode authentication seç
5. SA şifresi belirle

### 3.2 Firewall

```powershell
# PowerShell
New-NetFirewallRule -DisplayName "SQL Server" -Direction Inbound -Protocol TCP -LocalPort 1433 -Action Allow
```

### 3.3 TCP/IP Etkinleştir

1. SQL Server Configuration Manager aç
2. SQL Server Network Configuration → Protocols
3. TCP/IP → Enable
4. TCP/IP Properties → IP Addresses → IPAll → TCP Port: 1433
5. SQL Server servisini yeniden başlat

---

## API Sunucusundan Bağlantı Testi

### Telnet ile

```bash
# API sunucusunda
telnet <DB_SERVER_IP> 1433
```

### sqlcmd ile

```bash
# mssql-tools kurulu ise
sqlcmd -S <DB_SERVER_IP>,1433 -U sa -P 'Password' -C -Q "SELECT 1"
```

### Docker içinden

```bash
docker run --rm mcr.microsoft.com/mssql-tools18 \
  /opt/mssql-tools18/bin/sqlcmd \
  -S <DB_SERVER_IP>,1433 -U sa -P 'Password' -C \
  -Q "SELECT @@VERSION"
```

---

## .env.prod Güncelleme

Bağlantı başarılı olduktan sonra API sunucusunda `.env.prod` dosyasını güncelleyin:

```env
# .env.prod
MSSQL_SA_PASSWORD=YourProductionPassword123!
DB_HOST=192.168.1.100  # veya db.yourdomain.com
DB_PORT=1433
DB_NAME=EduPortalDb_Prod
DB_USER=sa
```

---

## Güvenlik Önerileri

### 1. Dedicated Database User

SA yerine özel bir kullanıcı oluşturun:

```sql
-- SA ile bağlanarak çalıştır
USE EduPortalDb_Prod
GO

-- Login oluştur
CREATE LOGIN eduportal_app WITH PASSWORD = 'StrongAppPassword123!'
GO

-- User oluştur
CREATE USER eduportal_app FOR LOGIN eduportal_app
GO

-- İzinler ver
ALTER ROLE db_owner ADD MEMBER eduportal_app
GO
```

Sonra `.env.prod`'u güncelleyin:
```env
DB_USER=eduportal_app
MSSQL_SA_PASSWORD=StrongAppPassword123!
```

### 2. Network Segmentation

- DB sunucusunu private network'e koy
- Sadece API sunucusundan erişim izni ver
- VPN veya private IP kullan

### 3. SSL/TLS Encryption

Connection string'e ekleyin:
```
Encrypt=True;TrustServerCertificate=False
```

### 4. Backup Strategy

```bash
# Günlük backup cron job
0 2 * * * /opt/scripts/backup-prod-db.sh
```

Örnek backup scripti:
```bash
#!/bin/bash
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR=/backups/mssql

sqlcmd -S localhost -U sa -P 'Password' -C -Q "
BACKUP DATABASE EduPortalDb_Prod
TO DISK = '$BACKUP_DIR/EduPortalDb_Prod_$TIMESTAMP.bak'
WITH COMPRESSION, STATS = 10
"

# Eski backupları sil (30 günden eski)
find $BACKUP_DIR -name "*.bak" -mtime +30 -delete
```

---

## Checklist

Üretim ortamına geçmeden önce:

- [ ] MSSQL Server kuruldu ve çalışıyor
- [ ] Database oluşturuldu (EduPortalDb_Prod)
- [ ] Firewall kuralları yapılandırıldı
- [ ] API sunucusundan bağlantı test edildi
- [ ] .env.prod güncellendi
- [ ] Backup stratejisi belirlendi
- [ ] Monitoring kuruldu
- [ ] Dedicated user oluşturuldu (opsiyonel)
- [ ] SSL/TLS yapılandırıldı (opsiyonel)

---

## Troubleshooting

### Bağlantı Timeout

```bash
# Network bağlantısını kontrol et
ping <DB_SERVER_IP>
traceroute <DB_SERVER_IP>

# Port açık mı?
nc -zv <DB_SERVER_IP> 1433
```

### Login Failed

```sql
-- SA ile bağlan ve kontrol et
SELECT name, is_disabled
FROM sys.server_principals
WHERE type = 'S'
```

### Database Mevcut Değil

```sql
-- Database listele
SELECT name FROM sys.databases
```
