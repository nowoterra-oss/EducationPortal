# EduPortal Database Bilgileri

## Database Bağlantı Detayları

**Database Tipi**: SQLite
**Dosya Yolu**: `/home/user/EducationPortal/src/EduPortal.API/eduportal.db`
**Dosya Boyutu**: 476 KB
**Connection String**: `Data Source=/home/user/EducationPortal/src/EduPortal.API/eduportal.db`

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

### .NET/C#
```csharp
var connectionString = "Data Source=/home/user/EducationPortal/src/EduPortal.API/eduportal.db";
```

### Python
```python
import sqlite3
conn = sqlite3.connect('/home/user/EducationPortal/src/EduPortal.API/eduportal.db')
```

### Node.js
```javascript
const sqlite3 = require('sqlite3');
const db = new sqlite3.Database('/home/user/EducationPortal/src/EduPortal.API/eduportal.db');
```

---

## Örnek SQL Sorguları

### Tüm Tabloları Listele
```sql
SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;
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

## VS Code SQLite Extension Kullanımı

1. **Extension Kur**: SQLite (alexcvzz.vscode-sqlite)
2. **Aç**: Ctrl+Shift+P → "SQLite: Open Database"
3. **Dosya Seç**: eduportal.db
4. **Kullan**:
   - Sol panelde SQLITE EXPLORER görünür
   - Tabloları genişlet
   - Sağ tık → "Show Table" ile verileri gör
   - SQL sorguları çalıştır

---

## Güvenlik Notları

- ⚠️ **Production'da SQLite kullanmayın** - SQL Server / PostgreSQL kullanın
- 🔒 `.db` dosyasını `.gitignore`'a ekleyin (zaten ekli)
- 🔐 Connection string'leri environment variable'larda tutun
- 📊 Backup alın: `cp eduportal.db eduportal_backup_$(date +%Y%m%d).db`

---

## Yararlı Komutlar

### Backup Al
```bash
cp /home/user/EducationPortal/src/EduPortal.API/eduportal.db ~/eduportal_backup.db
```

### Database Boyutunu Kontrol Et
```bash
ls -lh /home/user/EducationPortal/src/EduPortal.API/eduportal.db
```

### Database'i Sıfırla (DİKKAT!)
```bash
rm /home/user/EducationPortal/src/EduPortal.API/eduportal.db
dotnet run  # Otomatik yeniden oluşturulur
```
