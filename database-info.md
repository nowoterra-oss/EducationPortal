# EduPortal Database Bilgileri

## Database Bağlantı Detayları

**Database Tipi**: Microsoft SQL Server (SQL Express)
**Server**: `localhost\SQLEXPRESS`
**Database Adı**: `EduPortalDb`
**Authentication**: Windows Authentication (Trusted_Connection)
**Connection String**: `Server=localhost\SQLEXPRESS;Database=EduPortalDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true`

---

## Tablolar (50+ Tablo)

### 🔐 Identity / Authentication Tabloları
- `AspNetUsers` - Kullanıcılar (Admin, Öğrenci, Öğretmen, vb.)
- `AspNetRoles` - Roller (7 rol: Admin, Ogrenci, Ogretmen, Danışman, Muhasebe, Veli, Kayitci)
- `AspNetUserRoles` - Kullanıcı-Rol ilişkileri
- `AspNetUserClaims` - Kullanıcı claim'leri
- `AspNetRoleClaims` - Rol claim'leri
- `AspNetUserLogins` - Kullanıcı login'leri
- `AspNetUserTokens` - Kullanıcı token'ları

### 👨‍🎓 Öğrenci Tabloları
- `Students` - Öğrenci bilgileri (StudentNo, SchoolName, CurrentGrade, Gender, etc.)
- `StudentSiblings` - Kardeş bilgileri
- `StudentHobbies` - Öğrenci hobileri
- `StudentClubMemberships` - Kulüp üyelikleri
- `StudentDocuments` - Öğrenci belgeleri
- `StudentTeacherAssignments` - Öğrenci-Öğretmen atamaları
- `StudentCounselorAssignments` - Öğrenci-Danışman atamaları

### 👨‍🏫 Öğretmen ve Danışman Tabloları
- `Teachers` - Öğretmen bilgileri
- `Counselors` - Danışman bilgileri
- `CounselingMeetings` - Danışmanlık görüşmeleri

### 👪 Veli Tabloları
- `Parents` - Veli bilgileri
- `ParentContacts` - Veli iletişim bilgileri

### 📚 Eğitim Tabloları
- `Courses` - Dersler
- `CourseResources` - Ders kaynakları
- `Curriculum` - Müfredat
- `ClassPerformance` - Sınıf performansı

### 📝 Ödev Tabloları
- `Homework` - Ödevler (Title, Description, AssignedDate, DueDate, MaxScore)
- `StudentHomeworkSubmissions` - Ödev teslim/gönderimleri (Status, Score, TeacherFeedback)

### ✅ Devam/Yoklama Tabloları
- `Attendance` - Devamsızlık kayıtları (Date, Status: Geldi, GecGeldi, Gelmedi_Mazeretli, etc.)

### 📊 Sınav Tabloları
- `InternalExams` - İç sınavlar
- `ExamResults` - Sınav sonuçları
- `InternationalExams` - Uluslararası sınavlar (SAT, TOEFL, IELTS, etc.)

### 🏆 Yarışma ve Ödüller
- `CompetitionAndAwards` - Yarışmalar ve ödüller

### 🎓 Üniversite Başvuruları
- `UniversityApplications` - Üniversite başvuruları
- `AcademicDevelopmentPlans` - Akademik gelişim planları (AGP)

### 📅 Takvim ve Program
- `CalendarEvents` - Takvim etkinlikleri
- `WeeklySchedules` - Haftalık programlar

### 💬 Mesajlaşma
- `Messages` - Mesajlar
- `MessageAttachments` - Mesaj ekleri
- `MessageRecipients` - Mesaj alıcıları

### 🔔 Bildirimler
- `Notifications` - Bildirimler

### 💰 Ödeme Tabloları
- `PaymentPlans` - Ödeme planları
- `Payments` - Ödemeler
- `Installments` - Taksitler

### 📄 Dosya ve Doküman
- `Files` - Dosyalar
- `Documents` - Dokümanlar

### 🎯 Diğer Tablolar
- `Clubs` - Kulüpler
- `Hobbies` - Hobiler
- `__EFMigrationsHistory` - Entity Framework migration geçmişi

---

## Varsayılan Veriler (Seed Data)

### Roller (7 Adet)
1. **Admin** - Yönetici
2. **Ogrenci** - Öğrenci
3. **Ogretmen** - Öğretmen
4. **Danışman** - Psikolojik Danışman
5. **Muhasebe** - Muhasebe/Finans
6. **Veli** - Veli/Aile
7. **Kayitci** - Kayıt Sorumlusu

### Admin Kullanıcı
- **Email**: admin@eduportal.com
- **Şifre**: Admin@123
- **Rol**: Admin
- **Durum**: Aktif

---

## Önemli Kolonlar

### Students Tablosu Örnek Kolonlar:
- `Id` (int) - Primary Key
- `UserId` (string) - AspNetUsers FK
- `StudentNo` (string) - Öğrenci numarası
- `SchoolName` (string) - Okul adı
- `CurrentGrade` (int) - Mevcut sınıf (1-12)
- `Gender` (int) - Cinsiyet (0: Erkek, 1: Kız)
- `DateOfBirth` (datetime) - Doğum tarihi
- `LGSPercentile` (decimal) - LGS yüzdelik dilim
- `IsBilsem` (bool) - BİLSEM öğrencisi mi?
- `LanguageLevel` (string) - Dil seviyesi (A1, A2, B1, B2, C1, C2)
- `TargetMajor` (string) - Hedef bölüm
- `TargetCountry` (string) - Hedef ülke
- `EnrollmentDate` (datetime) - Kayıt tarihi
- `IsDeleted` (bool) - Soft delete flag
- `CreatedAt` (datetime) - Oluşturulma tarihi
- `UpdatedAt` (datetime) - Güncellenme tarihi

### Homework Tablosu Örnek Kolonlar:
- `Id` (int) - Primary Key
- `CourseId` (int) - Ders FK
- `TeacherId` (int) - Öğretmen FK
- `Title` (string) - Ödev başlığı
- `Description` (string) - Açıklama
- `AssignedDate` (datetime) - Verilme tarihi
- `DueDate` (datetime) - Son teslim tarihi
- `MaxScore` (int) - Maksimum puan
- `ResourceUrl` (string) - Kaynak URL
- `IsDeleted` (bool) - Soft delete flag
- `CreatedAt` (datetime) - Oluşturulma tarihi

### Attendance Tablosu Status Enum:
- `0` - Geldi (Mevcut)
- `1` - GecGeldi (Geç geldi)
- `2` - Gelmedi_Mazeretli (Mazeret ile gelmedi)
- `3` - Gelmedi_Mazeretsiz (Mazeretsiz gelmedi)
- `4` - DersIptal (Ders iptal)

---

## Connection String Örnekleri

### .NET/C# (Entity Framework)
```csharp
var connectionString = "Server=localhost\\SQLEXPRESS;Database=EduPortalDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
```

### .NET/C# (ADO.NET)
```csharp
using (SqlConnection conn = new SqlConnection(connectionString))
{
    conn.Open();
    // SQL komutları...
}
```

### Python (pyodbc)
```python
import pyodbc
conn = pyodbc.connect('DRIVER={SQL Server};SERVER=localhost\\SQLEXPRESS;DATABASE=EduPortalDb;Trusted_Connection=yes;')
```

### Node.js (mssql)
```javascript
const sql = require('mssql');
const config = {
    server: 'localhost\\SQLEXPRESS',
    database: 'EduPortalDb',
    options: {
        trustedConnection: true,
        trustServerCertificate: true
    }
};
```

---

## Örnek SQL Sorguları

### Tüm Tabloları Listele
```sql
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
```

### Admin Kullanıcıyı Getir
```sql
SELECT * FROM AspNetUsers WHERE Email = 'admin@eduportal.com';
```

### Tüm Rolleri Listele
```sql
SELECT * FROM AspNetRoles;
```

### Öğrenci Sayısı
```sql
SELECT COUNT(*) FROM Students WHERE IsDeleted = 0;
```

### Ödevleri Listele
```sql
SELECT h.*, c.CourseName
FROM Homework h
INNER JOIN Courses c ON h.CourseId = c.Id
WHERE h.IsDeleted = 0;
```

### Yoklama İstatistikleri
```sql
SELECT Status, COUNT(*) as Count
FROM Attendance
GROUP BY Status;
```

---

## SQL Server'a Bağlanma Yöntemleri

### 1. SQL Server Management Studio (SSMS) - ÖNERİLEN
1. SSMS'i aç
2. Server name: `localhost\SQLEXPRESS`
3. Authentication: Windows Authentication
4. Connect'e tıkla
5. Databases → EduPortalDb

### 2. Visual Studio Server Explorer
1. View → Server Explorer
2. Add Connection
3. Server name: `localhost\SQLEXPRESS`
4. Database: EduPortalDb

### 3. Azure Data Studio
1. New Connection
2. Server: `localhost\SQLEXPRESS`
3. Database: EduPortalDb
4. Authentication: Windows Authentication

### 4. VS Code SQL Server Extension
1. Extension: "SQL Server (mssql)"
2. Connect to Server
3. Server: `localhost\SQLEXPRESS`

---

## Güvenlik Notları

- 🔐 Connection string'leri **User Secrets** veya **Environment Variables**'da tutun
- 🔒 Production'da SQL Authentication kullanıyorsanız güçlü şifre belirleyin
- ⚠️ `appsettings.json` dosyasını git'e commit etmeyin (hassas bilgi içeriyorsa)
- 📊 Düzenli backup alın (SSMS → Right Click Database → Tasks → Backup)
- 🛡️ SQL Injection'a karşı her zaman parametreli sorgular kullanın

---

## Yararlı Komutlar

### Backup Al (SSMS)
```sql
BACKUP DATABASE EduPortalDb
TO DISK = 'C:\Backup\EduPortalDb.bak'
WITH FORMAT, INIT;
```

### Database Boyutunu Kontrol Et
```sql
EXEC sp_spaceused;
```

### Database'i Sıfırla (DİKKAT!)
```bash
# .NET CLI ile
dotnet ef database drop --force --project src/EduPortal.Infrastructure --startup-project src/EduPortal.API
dotnet run --project src/EduPortal.API
```

```sql
-- SSMS ile
DROP DATABASE EduPortalDb;
-- Sonra uygulamayı çalıştırın, otomatik oluşturulur
```

### Migration Komutları
```bash
# Yeni migration oluştur
dotnet ef migrations add MigrationName --project src/EduPortal.Infrastructure --startup-project src/EduPortal.API

# Migration uygula
dotnet ef database update --project src/EduPortal.Infrastructure --startup-project src/EduPortal.API

# Son migration'ı geri al
dotnet ef migrations remove --project src/EduPortal.Infrastructure --startup-project src/EduPortal.API
```
