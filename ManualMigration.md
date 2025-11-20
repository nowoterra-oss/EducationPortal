# Manuel Migration Uygulama Rehberi

Eğer tablolar otomatik oluşmadıysa, bu adımları takip edin:

---

## Yöntem 1: .NET CLI ile Migration (ÖNERİLEN)

### Adım 1: Komut İstemi'ni Aç (CMD veya PowerShell)

Proje klasörüne gidin:
```cmd
cd C:\path\to\EducationPortal
```

### Adım 2: Migration'ı Uygula

```cmd
dotnet ef database update --project src/EduPortal.Infrastructure --startup-project src/EduPortal.API --verbose
```

`--verbose` parametresi detaylı log gösterir. Eğer hata varsa görebilirsiniz.

### Adım 3: Sonucu Kontrol Edin

Başarılı olursa şöyle bir mesaj görürsünüz:
```
Applying migration '20251120183040_InitialCreate'.
Done.
```

---

## Yöntem 2: Package Manager Console (Visual Studio)

### Adım 1: Visual Studio'da Package Manager Console'u Açın

- Tools → NuGet Package Manager → Package Manager Console

### Adım 2: Default Project'i Seçin

Console'un üstünde "Default project" dropdown'ından:
- **EduPortal.Infrastructure** seçin

### Adım 3: Komutu Çalıştırın

```powershell
Update-Database -Verbose
```

---

## Yöntem 3: SQL Script ile Manuel Oluşturma

### Adım 1: Migration SQL Script Oluştur

Komut İstemi'nde:
```cmd
cd C:\path\to\EducationPortal
dotnet ef migrations script --project src/EduPortal.Infrastructure --startup-project src/EduPortal.API --output migration.sql
```

Bu komut `migration.sql` dosyası oluşturur.

### Adım 2: SSMS'de SQL Script'i Çalıştır

1. SSMS'i aç
2. `localhost\SQLEXPRESS` sunucusuna bağlan
3. File → Open → File → `migration.sql` seçin
4. F5 tuşu ile çalıştır

---

## Sorun Giderme

### Hata: "A network-related error occurred"

**Çözüm**: SQL Server servisi çalışmıyor olabilir

1. Windows Search → "Services"
2. "SQL Server (SQLEXPRESS)" servisini bulun
3. Sağ tık → Start

### Hata: "Login failed for user"

**Çözüm**: Windows Authentication problemi

**Seçenek A**: SQL Authentication kullanın

`appsettings.json` dosyasını güncelleyin:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=EduPortalDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

**Seçenek B**: Windows kullanıcınızı SQL Server'a ekleyin

SSMS'de:
```sql
USE master;
GO

CREATE LOGIN [YourDomain\YourUsername] FROM WINDOWS;
GO

ALTER SERVER ROLE sysadmin ADD MEMBER [YourDomain\YourUsername];
GO
```

### Hata: "Cannot open database"

**Çözüm**: Database oluşturulmamış

Önce database'i manuel oluşturun:

SSMS'de:
```sql
CREATE DATABASE EduPortalDb;
GO
```

Sonra migration'ı tekrar çalıştırın.

### Hata: "dotnet-ef command not found"

**Çözüm**: EF Core tools kurulu değil

```cmd
dotnet tool install --global dotnet-ef
```

veya güncelleyin:
```cmd
dotnet tool update --global dotnet-ef
```

---

## Database Oluştuğunu Doğrulama

### SSMS'de Kontrol

```sql
-- Database'in var olduğunu kontrol et
SELECT name FROM sys.databases WHERE name = 'EduPortalDb';

-- Database'i kullan
USE EduPortalDb;
GO

-- Tabloları say
SELECT COUNT(*) AS TotalTables
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE';
-- Beklenen: 50+ tablo

-- Tablo isimlerini listele
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
```

### Beklenen Tablolar

En az şu tabloları görmelisiniz:
- AspNetUsers
- AspNetRoles
- AspNetUserRoles
- Students
- Teachers
- Courses
- Homework
- Attendance
- InternalExams
- Payments
- vb. (50+ tablo)

---

## Seed Data Ekleme

Migration başarılı olduktan sonra, seed data için uygulamayı çalıştırın:

```cmd
cd src/EduPortal.API
dotnet run
```

Veya Visual Studio'da F5 ile başlatın.

Seed data eklendiğinde console'da göreceksiniz:
```
info: Program[0]
      Database seeded successfully
```

Kontrol:
```sql
-- Rolleri kontrol et
SELECT * FROM AspNetRoles;
-- Beklenen: 7 rol

-- Admin kullanıcıyı kontrol et
SELECT * FROM AspNetUsers WHERE Email = 'admin@eduportal.com';
-- Beklenen: 1 kullanıcı
```

---

## Hala Sorun Varsa

### Debug Modu ile Çalıştırın

Visual Studio'da:
1. `DbInitializer.cs` dosyasını açın
2. Satır 17'ye breakpoint koyun (await context.Database.MigrateAsync())
3. F5 ile debug başlatın
4. Exception oluşursa detaylarını görün

### Connection String'i Test Edin

`Program.cs` dosyasına test kodu ekleyin:

```csharp
// Add DbContext SONRASINA ekleyin
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"Connection String: {connectionString}");

// Test connection
try
{
    using (var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString))
    {
        connection.Open();
        Console.WriteLine("✅ SQL Server bağlantısı başarılı!");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ SQL Server bağlantı hatası: {ex.Message}");
}
```

---

## Alternatif: Sıfırdan Başlama

Eğer hiçbir şey çalışmıyorsa, sıfırdan:

```cmd
# Migration'ları sil
Remove-Item -Recurse -Force src/EduPortal.Infrastructure/Migrations

# Yeni migration oluştur
dotnet ef migrations add InitialCreate --project src/EduPortal.Infrastructure --startup-project src/EduPortal.API

# Uygula
dotnet ef database update --project src/EduPortal.Infrastructure --startup-project src/EduPortal.API
```

---

## İletişim

Bu adımlardan sonra hala sorun varsa, lütfen şunları paylaşın:
1. Console'daki tam hata mesajı
2. SSMS'de `EduPortalDb` görünüyor mu?
3. Hangi yöntemi denediniz?
4. SQL Server versiyonu (SSMS'de `SELECT @@VERSION`)
