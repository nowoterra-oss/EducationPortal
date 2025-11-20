# SQL Server Setup Guide

## Database Bilgileri

**Database Tipi**: Microsoft SQL Server (SQL Express)
**Server**: `localhost\SQLEXPRESS`
**Database Adı**: `EduPortalDb`
**Authentication**: Windows Authentication (Trusted_Connection)

---

## Kurulum Adımları

### 1. SQL Server Kontrolü

SSMS'de SQL Server'ınızın çalıştığından emin olun:

```sql
-- SSMS'de çalıştırın
SELECT @@VERSION;
SELECT @@SERVERNAME;
```

### 2. Uygulamayı Çalıştırın

Windows makinenizde, proje klasöründe:

```bash
cd src/EduPortal.API
dotnet run
```

veya Visual Studio'da **F5** tuşuna basın.

### 3. Otomatik Database Oluşturma

Uygulama başladığında:
- ✅ `EduPortalDb` veritabanı otomatik oluşturulacak
- ✅ Tüm tablolar migration'lardan oluşturulacak (50+ tablo)
- ✅ Seed data eklenecek (7 rol + admin kullanıcı)

### 4. Database Kontrolü

SSMS'de yeni veritabanını kontrol edin:

```sql
-- Database'in oluştuğunu kontrol et
USE EduPortalDb;
GO

-- Tabloları listele
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

-- Rolleri kontrol et
SELECT * FROM AspNetRoles;

-- Admin kullanıcıyı kontrol et
SELECT * FROM AspNetUsers WHERE Email = 'admin@eduportal.com';
```

---

## Connection String Detayları

### Mevcut Ayar (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=EduPortalDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

### Alternatif Connection String Örnekleri

#### SQL Authentication (Kullanıcı adı/şifre ile)
```json
"DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=EduPortalDb;User Id=sa;Password=YourPassword123;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

#### LocalDB Kullanımı
```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EduPortalDb;Trusted_Connection=True;MultipleActiveResultSets=true"
```

#### Remote SQL Server
```json
"DefaultConnection": "Server=192.168.1.100;Database=EduPortalDb;User Id=sa;Password=YourPassword123;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

---

## Veritabanı Yapısı

### Identity Tabloları (7 tablo)
- `AspNetUsers` - Kullanıcılar
- `AspNetRoles` - Roller (Admin, Ogrenci, Ogretmen, Danışman, Muhasebe, Veli, Kayitci)
- `AspNetUserRoles` - Kullanıcı-Rol ilişkileri
- `AspNetUserClaims`
- `AspNetRoleClaims`
- `AspNetUserLogins`
- `AspNetUserTokens`

### Uygulama Tabloları (40+ tablo)
- **Öğrenci**: Students, StudentSiblings, StudentHobbies, StudentClubMemberships, StudentDocuments
- **Eğitim**: Courses, CourseResources, Curriculum, ClassPerformance
- **Ödev**: Homework, StudentHomeworkSubmissions
- **Devam**: Attendance
- **Sınav**: InternalExams, ExamResults, InternationalExams
- **Üniversite**: UniversityApplications, AcademicDevelopmentPlans
- **Diğer**: Teachers, Counselors, Parents, Payments, Messages, Notifications, vb.

---

## Seed Data

### Varsayılan Roller (7 adet)
1. **Admin** - Sistem yöneticisi
2. **Ogrenci** - Öğrenci
3. **Ogretmen** - Öğretmen
4. **Danışman** - Psikolojik danışman
5. **Muhasebe** - Muhasebe/Finans
6. **Veli** - Veli/Aile
7. **Kayitci** - Kayıt sorumlusu

### Admin Kullanıcı
- **Email**: admin@eduportal.com
- **Şifre**: Admin@123
- **Rol**: Admin
- **Durum**: Aktif

---

## Swagger Kullanımı

Uygulama başladıktan sonra:

1. Tarayıcıda açın: `http://localhost:5129/swagger`
2. **Authorize** butonuna tıklayın
3. Login endpoint'i ile giriş yapın:

```json
POST /api/auth/login
{
  "email": "admin@eduportal.com",
  "password": "Admin@123"
}
```

4. Dönen `token`'ı kopyalayın
5. Authorize penceresinde: `Bearer {token}` formatında girin

---

## REST Client Kullanımı

VS Code'da `.http` dosyasıyla test:

1. VS Code'da `EduPortal.http` dosyasını açın
2. REST Client extension'ı yükleyin
3. Login isteğini çalıştırın:

```http
### Login
POST http://localhost:5129/api/auth/login
Content-Type: application/json

{
  "email": "admin@eduportal.com",
  "password": "Admin@123"
}
```

4. Dönen token'ı `@token` değişkenine yerleştirin
5. Diğer endpoint'leri test edin

---

## Sorun Giderme

### Hata: "Cannot open database EduPortalDb"

**Çözüm**: SQL Server servisinin çalıştığından emin olun:
- Windows Services → SQL Server (SQLEXPRESS) → Start

### Hata: "Login failed for user"

**Çözüm 1**: Windows Authentication yerine SQL Authentication kullanın
**Çözüm 2**: SQL Server'da Windows Authentication'ı etkinleştirin

### Hata: "A network-related or instance-specific error"

**Çözüm**:
1. SQL Server Configuration Manager açın
2. SQL Server Network Configuration → Protocols for SQLEXPRESS
3. TCP/IP'yi Enable edin
4. SQL Server servisini restart edin

### Migration Hatası

Eğer migration hataları alırsanız:

```bash
# Eski migration'ları temizle
dotnet ef database drop --force

# Yeniden çalıştır
dotnet run
```

---

## Database Backup

### Backup Al (SSMS)
```sql
BACKUP DATABASE EduPortalDb
TO DISK = 'C:\Backup\EduPortalDb.bak'
WITH FORMAT, INIT, NAME = 'Full Backup of EduPortalDb';
```

### Restore Et (SSMS)
```sql
RESTORE DATABASE EduPortalDb
FROM DISK = 'C:\Backup\EduPortalDb.bak'
WITH REPLACE;
```

### Script ile Backup
```bash
sqlcmd -S localhost\SQLEXPRESS -E -Q "BACKUP DATABASE EduPortalDb TO DISK='C:\Backup\EduPortalDb.bak'"
```

---

## Önemli Notlar

⚠️ **Güvenlik**:
- Production'da JWT Key'i değiştirin
- Connection string'i environment variable'da tutun
- SQL Authentication kullanıyorsanız güçlü şifre kullanın

🔒 **Connection String Güvenliği**:
```bash
# appsettings.json yerine User Secrets kullanın
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost\\SQLEXPRESS;Database=EduPortalDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

📊 **Performance**:
- Production'da SQL Server Express yerine SQL Server Standard/Enterprise kullanın
- Index'leri optimize edin
- Connection pooling aktif (MultipleActiveResultSets=true)

---

## Faydalı SQL Sorguları

### Tüm Tabloları ve Satır Sayılarını Göster
```sql
SELECT
    t.NAME AS TableName,
    p.rows AS RowCounts
FROM
    sys.tables t
INNER JOIN
    sys.partitions p ON t.object_id = p.object_id
WHERE
    t.is_ms_shipped = 0
    AND p.index_id IN (0,1)
GROUP BY
    t.Name, p.Rows
ORDER BY
    t.Name;
```

### Database Boyutunu Kontrol Et
```sql
EXEC sp_spaceused;
```

### Tüm Foreign Key'leri Göster
```sql
SELECT
    fk.name AS ForeignKey,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    COL_NAME(fc.parent_object_id, fc.parent_column_id) AS ColumnName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable,
    COL_NAME(fc.referenced_object_id, fc.referenced_column_id) AS ReferencedColumn
FROM
    sys.foreign_keys AS fk
INNER JOIN
    sys.foreign_key_columns AS fc ON fk.object_id = fc.constraint_object_id
ORDER BY
    TableName, ForeignKey;
```

---

## İletişim ve Destek

Herhangi bir sorun yaşarsanız:
- Database migration loglarını kontrol edin
- SQL Server Error Log'ları inceleyin
- Connection string'in doğruluğunu kontrol edin

**Başarıyla çalıştığında göreceğiniz log:**
```
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (123ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      CREATE DATABASE [EduPortalDb];
...
info: Program[0]
      Database seeded successfully
```
