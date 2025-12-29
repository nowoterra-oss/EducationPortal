# AGP (Akademik Gelisim Plani) Periods Backend Guide

Bu dokuman, AGP Periods (Donemler) modulu icin backend API'larini ve kullanim kilavuzunu icerir.

## Genel Bakis

AGP Periods modulu, ogrencilerin akademik gelisim planlarini zaman cizelgesi (Gantt Chart) olarak yonetmelerini saglar. Her AGP birden fazla donem (period) icerebilir ve her donem icinde sinav/hedefler (milestones) ve aktiviteler (activities) tanimlanabilir.

## Veri Modeli

### Entity'ler

```
AcademicDevelopmentPlan (AGP)
├── AgpPeriod (Donemler)
│   ├── AgpTimelineMilestone (Sinavlar/Hedefler)
│   └── AgpActivity (Aktiviteler)
└── AGPMilestone (Eski hedef sistemi - ayri)
```

### AgpPeriod Entity

| Alan | Tip | Aciklama |
|------|-----|----------|
| Id | int | Primary key |
| AgpId | int | Bagli AGP'nin ID'si |
| PeriodName | string? | Donem adi (opsiyonel) |
| Title | string | Baslik |
| StartDate | DateTime | Baslangic tarihi |
| EndDate | DateTime | Bitis tarihi |
| Color | string? | Renk kodu (#RRGGBB) |
| Order | int | Siralama |
| IsDeleted | bool | Soft delete flagi |

### AgpTimelineMilestone Entity

| Alan | Tip | Aciklama |
|------|-----|----------|
| Id | int | Primary key |
| AgpPeriodId | int | Bagli donem ID'si |
| Title | string | Sinav/hedef adi |
| Date | DateTime | Tarih |
| Color | string? | Renk kodu |
| Type | string | Tip (exam, goal, event vb.) |
| IsMilestone | bool | Milestone olarak goster |
| IsDeleted | bool | Soft delete flagi |

### AgpActivity Entity

| Alan | Tip | Aciklama |
|------|-----|----------|
| Id | int | Primary key |
| AgpPeriodId | int | Bagli donem ID'si |
| Title | string | Aktivite adi |
| StartDate | DateTime | Baslangic tarihi |
| EndDate | DateTime | Bitis tarihi |
| HoursPerWeek | int? | Haftalik saat |
| OwnerType | int | Sahip tipi (1-8 arasi renk kodu) |
| Status | ActivityStatus | Durum (Planned, InProgress, Completed, NeedsReview, Overdue) |
| NeedsReview | bool | Inceleme gerekli mi |
| Notes | string? | Notlar |
| IsDeleted | bool | Soft delete flagi |

## API Endpoints

Base URL: `/api/agp`

### AGP CRUD Islemleri

| Method | Endpoint | Aciklama | Permission |
|--------|----------|----------|------------|
| GET | `/` | Tum AGP'leri listele (sayfalama ile) | AgpView |
| GET | `/{id}` | Tek bir AGP getir | AgpView |
| POST | `/` | Yeni AGP olustur | AgpCreate |
| PUT | `/{id}` | AGP guncelle | AgpEdit |
| DELETE | `/{id}` | AGP sil (soft delete) | AgpEdit |
| GET | `/student/{studentId}` | Ogrencinin AGP'lerini getir | AgpView |

### Period (Donem) Islemleri

| Method | Endpoint | Aciklama | Permission |
|--------|----------|----------|------------|
| GET | `/{agpId}/periods` | AGP'nin tum donemlerini getir | AgpView |
| GET | `/{agpId}/timeline` | Timeline view (Gantt Chart) icin veri | AgpView |
| GET | `/periods/{periodId}` | Tek bir donemi getir | AgpView |
| POST | `/periods` | Yeni donem olustur | AgpCreate |
| PUT | `/periods/{periodId}` | Donem guncelle | AgpEdit |
| DELETE | `/periods/{periodId}` | Donem sil (soft delete) | AgpEdit |
| GET | `/student/{studentId}/periods` | Ogrencinin tum donemlerini getir | AgpView |

### Goal/Milestone Islemleri (Eski sistem)

| Method | Endpoint | Aciklama | Permission |
|--------|----------|----------|------------|
| GET | `/{id}/goals` | AGP hedeflerini getir | AgpView |
| POST | `/{id}/goals` | Yeni hedef ekle | AgpCreate |
| PUT | `/{id}/goals/{goalId}` | Hedef guncelle | AgpEdit |
| DELETE | `/{id}/goals/{goalId}` | Hedef sil | AgpEdit |

## DTO'lar

### AgpPeriodCreateDto

```json
{
  "agpId": 1,
  "periodName": "1. Donem",
  "title": "Hazirlik Donemi",
  "startDate": "2025-01-01",
  "endDate": "2025-03-31",
  "color": "#4CAF50",
  "order": 1,
  "milestones": [
    {
      "title": "SAT Sinavi",
      "date": "2025-02-15",
      "color": "#FF5722",
      "type": "exam",
      "isMilestone": true
    }
  ],
  "activities": [
    {
      "title": "SAT Matematik Calismasi",
      "startDate": "2025-01-01",
      "endDate": "2025-02-14",
      "hoursPerWeek": 10,
      "ownerType": 1,
      "status": "InProgress",
      "needsReview": false,
      "notes": "Khan Academy kullanilacak"
    }
  ]
}
```

### AgpPeriodUpdateDto

```json
{
  "periodName": "1. Donem (Guncellendi)",
  "title": "Hazirlik Donemi",
  "startDate": "2025-01-01",
  "endDate": "2025-04-30",
  "color": "#2196F3",
  "order": 1,
  "milestones": [...],
  "activities": [...]
}
```

Not: `milestones` ve `activities` gonderilirse, mevcut olanlar soft delete edilir ve yenileri eklenir.

### AgpPeriodResponseDto

```json
{
  "id": 1,
  "periodName": "1. Donem",
  "title": "Hazirlik Donemi",
  "startDate": "2025-01-01T00:00:00",
  "endDate": "2025-03-31T00:00:00",
  "color": "#4CAF50",
  "order": 1,
  "agpId": 1,
  "studentId": 54,
  "studentName": "Ali Veli",
  "totalDays": 90,
  "elapsedDays": 30,
  "progressPercentage": 33.33,
  "milestones": [...],
  "activities": [...]
}
```

### AgpTimelineViewDto (Gantt Chart icin)

```json
{
  "timelineStart": "2025-01-01T00:00:00",
  "timelineEnd": "2025-06-30T00:00:00",
  "today": "2025-01-15T00:00:00",
  "months": ["MONTH 1", "MONTH 2", "MONTH 3", "MONTH 4", "MONTH 5", "MONTH 6"],
  "periods": [...],
  "studentId": 54,
  "studentName": "Ali Veli",
  "agpId": 1
}
```

## Ornek Kullanim

### 1. Timeline Verisini Getirme (Gantt Chart)

```http
GET /api/agp/1/timeline?months=12
Authorization: Bearer {token}
```

Response:
```json
{
  "success": true,
  "message": "Islem basarili",
  "data": {
    "timelineStart": "2025-01-01T00:00:00",
    "timelineEnd": "2025-12-31T00:00:00",
    "today": "2025-01-15T00:00:00",
    "months": ["MONTH 1", "MONTH 2", ...],
    "periods": [
      {
        "id": 1,
        "title": "Hazirlik Donemi",
        "startDate": "2025-01-01T00:00:00",
        "endDate": "2025-03-31T00:00:00",
        "color": "#4CAF50",
        "progressPercentage": 33.33,
        "milestones": [
          {
            "id": 1,
            "title": "SAT Sinavi",
            "date": "2025-02-15T00:00:00",
            "type": "exam",
            "isMilestone": true
          }
        ],
        "activities": [...]
      }
    ],
    "studentId": 54,
    "studentName": "Ali Veli",
    "agpId": 1
  }
}
```

### 2. Yeni Donem Olusturma

```http
POST /api/agp/periods
Authorization: Bearer {token}
Content-Type: application/json

{
  "agpId": 1,
  "title": "Uygulama Donemi",
  "startDate": "2025-04-01",
  "endDate": "2025-06-30",
  "color": "#2196F3",
  "order": 2,
  "milestones": [
    {
      "title": "TOEFL Sinavi",
      "date": "2025-05-15",
      "type": "exam",
      "isMilestone": true
    }
  ],
  "activities": [
    {
      "title": "TOEFL Writing Calismasi",
      "startDate": "2025-04-01",
      "endDate": "2025-05-14",
      "hoursPerWeek": 8,
      "ownerType": 2,
      "status": "Planned"
    }
  ]
}
```

### 3. Donem Guncelleme

```http
PUT /api/agp/periods/1
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "Hazirlik Donemi (Revize)",
  "endDate": "2025-04-15",
  "milestones": [
    {
      "title": "SAT Sinavi (Ertelendi)",
      "date": "2025-03-01",
      "type": "exam",
      "isMilestone": true
    }
  ]
}
```

### 4. Ogrencinin Tum Donemlerini Getirme

```http
GET /api/agp/student/54/periods
Authorization: Bearer {token}
```

## Onemli Notlar

### Soft Delete
Tum silme islemleri soft delete olarak gerceklestirilir. `IsDeleted = true` yapilir, kayit veritabanindan silinmez.

### Global Query Filter
`AcademicDevelopmentPlan` entity'si icin global query filter tanimlanmistir. Bu nedenle sorgularda otomatik olarak `IsDeleted = false` filtresi uygulanir.

### Danisman Erisim Kontrolu
`IAdvisorAccessService` kullanilarak danisman erisimleri kontrol edilir. Danismanlar sadece kendilerine atanmis ogrencilerin AGP kayitlarina erisebilir.

### Tarih Cakismasi Kontrolu
Ayni AGP icinde cakisan tarih araliginda donem olusturulamaz. `FindOverlappingPeriodsAsync` metodu ile kontrol edilir.

### OwnerType Renk Kodlari (Aktiviteler)
| Kod | Renk | Aciklama |
|-----|------|----------|
| 1 | Mavi | Akademik |
| 2 | Yesil | Spor |
| 3 | Turuncu | Sanat |
| 4 | Mor | Sosyal |
| 5 | Kirmizi | Test Prep |
| 6 | Cyan | Dil |
| 7 | Kahverengi | Staj |
| 8 | Gri | Diger |

### ActivityStatus Enum
```csharp
public enum ActivityStatus
{
    Planned,      // Planlanmis
    InProgress,   // Devam ediyor
    Completed,    // Tamamlandi
    NeedsReview,  // Inceleme gerekli
    Overdue       // Gecmis
}
```

## Servis Kayitlari

`DependencyInjection.cs` dosyasindaki kayitlar:

```csharp
// Repository
services.AddScoped<IAgpPeriodRepository, AgpPeriodRepository>();

// Service
services.AddScoped<IAgpPeriodService, Application.Services.Implementations.AgpPeriodService>();
```

## Dosya Yapisi

```
src/
├── EduPortal.Domain/
│   ├── Entities/
│   │   ├── AcademicDevelopmentPlan.cs
│   │   ├── AgpPeriod.cs
│   │   ├── AgpTimelineMilestone.cs
│   │   └── AgpActivity.cs
│   └── Enums/
│       └── ActivityStatus.cs
├── EduPortal.Application/
│   ├── DTOs/AGP/
│   │   └── AgpPeriodDto.cs (tum DTO'lar)
│   ├── Interfaces/
│   │   └── IAgpPeriodRepository.cs
│   └── Services/
│       ├── Interfaces/
│       │   └── IAgpPeriodService.cs
│       └── Implementations/
│           └── AgpPeriodService.cs
├── EduPortal.Infrastructure/
│   ├── Repositories/
│   │   └── AgpPeriodRepository.cs
│   └── Services/
│       └── AGPService.cs
└── EduPortal.API/
    └── Controllers/
        └── AGPController.cs
```

## Hata Ayiklama

### Sorun: Yeni AGP kaydedildiginde gorunmuyor

1. **IsDeleted kontrolu**: `AGPService` sorgularinda `!IsDeleted` filtresi var mi kontrol edin
2. **Global Query Filter**: `ApplicationDbContext` icinde `HasQueryFilter` tanimli mi kontrol edin
3. **Veritabani kontrolu**: SQL ile kaydin `IsDeleted` degerini kontrol edin:
   ```sql
   SELECT Id, StudentId, IsDeleted FROM AcademicDevelopmentPlans
   WHERE Id = {id}
   ```

### Sorun: Donem bilgileri gelmiyor

1. **Include kontrolu**: Repository sorgularinda `Include(p => p.Milestones)` ve `Include(p => p.Activities)` var mi
2. **IsDeleted filtresi**: Alt entity'lerde de `!IsDeleted` filtresi uygulanmali

### Sorun: Timeline verileri eksik

1. **Tarih araligi**: `GetTimelineViewAsync` metodundaki `monthsToShow` parametresini kontrol edin
2. **Order**: Donemler `Order` alanina gore siralanmali

## Son Guncellemeler

- **2025-12-28**: `AGPService` icindeki tum sorgulara `IsDeleted` filtreleri eklendi
- **2025-12-28**: `DeleteAsync` ve `DeleteGoalAsync` metodlari soft delete kullanacak sekilde guncellendi
- **2025-12-28**: Alt entity'ler (Periods, Milestones, Activities) icin Include sorgularinda IsDeleted filtresi eklendi
- **2025-12-28**: `CreateAsync` ve `UpdateAsync` metodlarinda `AgpActivity` icin `StartDate`, `EndDate`, `OwnerType`, `Status`, `NeedsReview` alanlari duzeltildi (500 hata duzeltmesi)
- **2025-12-28**: `AgpTimelineMilestone` icin `IsMilestone` alani mapping'e eklendi
- **2025-12-28**: `MapToPeriodDto` metodunda tum yeni alanlar response'a eklendi
- **2025-12-28**: EF Core Include filtre hatasi duzeltildi (ayni navigation icin birden fazla Where filtresi sorunu)
- **2025-12-28**: `PeriodName` ve varsayilan `Color` degerler `CreateAsync`'e eklendi
- **2025-12-28**: `AGP_FRONTEND_GUIDE.md` dokumani olusturuldu
