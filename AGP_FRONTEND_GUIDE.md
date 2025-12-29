# AGP Frontend Duzenleme Talimatlari

Bu dokuman, AGP (Akademik Gelisim Plani) modulunun frontend tarafinda yapilmasi gereken duzeltmeleri ve gelistirmeleri icerir.

## 1. API Type Duzeltmeleri

### 1.1 AgpMilestoneDto Duzeltmesi

**Dosya:** `src/core/api/agp.api.ts`

Frontend'deki `AgpMilestoneDto` ile backend'deki uyumsuzluk var. Backend su alanlari bekliyor:

```typescript
// MEVCUT (yanlis)
export interface AgpMilestoneDto {
  id?: number;
  title: string;
  category: MilestoneCategory;  // Backend'de yok!
  examDate: string;             // Backend 'date' bekliyor
  applicationStartDate?: string; // Backend'de yok!
  applicationEndDate?: string;   // Backend'de yok!
  color?: string;
  date?: string;                // Bu dogru
}

// OLMASI GEREKEN (Backend artik hem 'type' hem 'category' destekliyor)
export interface AgpMilestoneDto {
  id?: number;
  title: string;
  date: string;                    // yyyy-MM-dd formati (zorunlu)
  color?: string;
  type?: string;                   // 'exam', 'goal', 'event' (varsayilan: 'exam')
  category?: string;               // type ile ayni deger - Frontend uyumlulugu icin eklendi
  isMilestone?: boolean;           // Timeline'da elmas seklinde goster (varsayilan: false)
  applicationStartDate?: string;   // yyyy-MM-dd (opsiyonel) - Basvuru baslangic tarihi
  applicationEndDate?: string;     // yyyy-MM-dd (opsiyonel) - Basvuru bitis tarihi
}

// NOT: Backend'de Category = Type alias'i olarak tanimli
// Yani category gonderirseniz type'a yazilir, type okursaniz category'den de okunabilir
// applicationStartDate ve applicationEndDate alanlari artik backend'de destekleniyor
```

### 1.2 AgpActivityDto Duzeltmesi

```typescript
// MEVCUT
export interface AgpActivityDto {
  id?: number;
  title: string;
  hoursPerWeek: number;     // Backend'de opsiyonel
  notes?: string;
  startDate?: string;
  endDate?: string;
  ownerType?: number;
  status?: ActivityStatus;
  needsReview?: boolean;
}

// OLMASI GEREKEN (Backend ile uyumlu)
export interface AgpActivityDto {
  id?: number;
  title: string;
  startDate?: string;        // yyyy-MM-dd (opsiyonel - yoksa period tarihleri kullanilir)
  endDate?: string;          // yyyy-MM-dd (opsiyonel - yoksa period tarihleri kullanilir)
  hoursPerWeek?: number;     // Opsiyonel
  ownerType?: number;        // 1-8 arasi (varsayilan: 1)
  status?: string;           // 'Planned', 'InProgress', 'Completed', 'NeedsReview', 'Overdue'
  needsReview?: boolean;     // (varsayilan: false)
  notes?: string;
}
```

### 1.3 AgpPeriodDto Duzeltmesi

```typescript
// OLMASI GEREKEN
export interface AgpPeriodDto {
  id?: number;
  title: string;              // Zorunlu
  periodName?: string;        // Opsiyonel - bos birakilirsa tarihlerden otomatik olusturulur
  startDate: string;          // yyyy-MM-dd (zorunlu)
  endDate: string;            // yyyy-MM-dd (zorunlu)
  color?: string;             // Hex renk kodu (varsayilan: '#3B82F6')
  order?: number;             // Siralama (varsayilan: otomatik)
  milestones?: AgpMilestoneDto[];  // Bos array gonderilebilir
  activities?: AgpActivityDto[];   // Bos array gonderilebilir
}
```

## 2. AgpForm.tsx Duzeltmeleri

### 2.1 Period Formu Varsayilan Degerleri

Period eklerken `order` ve `color` alanlarinin varsayilan degerlerle gonderildiginden emin olun:

```typescript
const openAddPeriodModal = () => {
  setPeriodForm({
    title: '',
    startDate: '',
    endDate: '',
    color: '#3B82F6',         // Varsayilan renk ekle
    milestones: [],
    activities: [],
    order: formData.periods?.length || 0  // Otomatik siralama
  });
  setEditingPeriodIndex(null);
  setShowPeriodModal(true);
};
```

### 2.2 Milestone Ekleme Formu

Milestone eklerken `date` alaninin dogru formatta gonderildiginden emin olun:

```typescript
// Milestone eklerken
const addMilestone = (milestone: {
  title: string;
  date: string;                    // examDate yerine date kullanin!
  type?: string;
  color?: string;
  isMilestone?: boolean;
  applicationStartDate?: string;   // Basvuru baslangic tarihi (opsiyonel)
  applicationEndDate?: string;     // Basvuru bitis tarihi (opsiyonel)
}) => {
  // ...
};
```

### 2.3 Activity Ekleme Formu

Activity eklerken `status` alaninin string olarak gonderildiginden emin olun:

```typescript
// Activity status degerleri
type ActivityStatusValue = 'Planned' | 'InProgress' | 'Completed' | 'NeedsReview' | 'Overdue';

const addActivity = (activity: {
  title: string;
  startDate?: string;
  endDate?: string;
  hoursPerWeek?: number;
  ownerType?: number;
  status?: ActivityStatusValue;  // Enum yerine string
  needsReview?: boolean;
  notes?: string;
}) => {
  // ...
};
```

## 3. Backend API Endpoint'leri

### 3.1 AGP CRUD

| Method | Endpoint | Aciklama |
|--------|----------|----------|
| GET | `/api/agp` | Tum AGP'leri listele (sayfalama ile) |
| GET | `/api/agp/{id}` | Tek AGP getir |
| POST | `/api/agp` | Yeni AGP olustur (periods dahil) |
| PUT | `/api/agp/{id}` | AGP guncelle (periods dahil) |
| DELETE | `/api/agp/{id}` | AGP sil (soft delete) |
| GET | `/api/agp/student/{studentId}` | Ogrencinin AGP'lerini getir |

### 3.2 Period Islemleri (Ayri Endpoint)

| Method | Endpoint | Aciklama |
|--------|----------|----------|
| GET | `/api/agp/{agpId}/periods` | AGP'nin donemlerini getir |
| GET | `/api/agp/{agpId}/timeline` | Gantt Chart verisi |
| POST | `/api/agp/periods` | Yeni donem ekle |
| PUT | `/api/agp/periods/{periodId}` | Donem guncelle |
| DELETE | `/api/agp/periods/{periodId}` | Donem sil |

## 4. Ornek Payload'lar

### 4.1 AGP Olusturma (POST /api/agp)

```json
{
  "studentId": 24,
  "academicYear": "2025-2026",
  "startDate": "2025-09-01",
  "endDate": "2026-06-30",
  "status": 0,
  "periods": [
    {
      "title": "1. Donem - Hazirlik",
      "startDate": "2025-09-01",
      "endDate": "2025-12-31",
      "color": "#3B82F6",
      "order": 0,
      "milestones": [
        {
          "title": "SAT Sinavi",
          "date": "2025-11-15",
          "type": "exam",
          "color": "#EF4444",
          "isMilestone": true,
          "applicationStartDate": "2025-09-01",
          "applicationEndDate": "2025-10-15"
        }
      ],
      "activities": [
        {
          "title": "SAT Matematik Calismasi",
          "startDate": "2025-09-01",
          "endDate": "2025-11-14",
          "hoursPerWeek": 10,
          "ownerType": 1,
          "status": "InProgress",
          "needsReview": false,
          "notes": "Khan Academy kullanilacak"
        }
      ]
    }
  ]
}
```

### 4.2 AGP Guncelleme (PUT /api/agp/{id})

```json
{
  "academicYear": "2025-2026",
  "startDate": "2025-09-01",
  "endDate": "2026-06-30",
  "status": 1,
  "periods": [
    {
      "title": "1. Donem - Hazirlik (Guncellendi)",
      "startDate": "2025-09-01",
      "endDate": "2026-01-15",
      "color": "#10B981",
      "order": 0,
      "milestones": [],
      "activities": []
    }
  ]
}
```

## 5. OwnerType Renk Kodlari

Activities icin `ownerType` alani renk kodu olarak kullanilir:

| Kod | Anlam | Onerilen Renk |
|-----|-------|---------------|
| 1 | Akademik | #3B82F6 (Mavi) |
| 2 | Spor | #10B981 (Yesil) |
| 3 | Sanat | #F59E0B (Turuncu) |
| 4 | Sosyal | #8B5CF6 (Mor) |
| 5 | Test Prep | #EF4444 (Kirmizi) |
| 6 | Dil | #06B6D4 (Cyan) |
| 7 | Staj | #78716C (Kahverengi) |
| 8 | Diger | #6B7280 (Gri) |

## 6. Hata Ayiklama

### 6.1 500 Internal Server Error

Bu hata genellikle su nedenlerden kaynaklanir:

1. **Eksik alanlar**: `title`, `startDate`, `endDate` zorunludur
2. **Yanlis tarih formati**: `yyyy-MM-dd` formati kullanin
3. **Type uyumsuzlugu**: `status` string olmali (enum degil)

### 6.2 Console'da Debug

```typescript
// AGP olusturmadan once payload'i kontrol edin
const handleSubmit = async () => {
  console.log('AGP Payload:', JSON.stringify(formData, null, 2));
  // API cagrisini yapin
};
```

## 7. Yapilacaklar Listesi

- [ ] `AgpMilestoneDto` interface'ini guncelle (`examDate` -> `date`)
- [ ] `AgpMilestoneDto` interface'ine `applicationStartDate` ve `applicationEndDate` ekle
- [ ] `AgpActivityDto` interface'ini guncelle (`status` string olmali)
- [ ] `AgpPeriodDto` interface'ine `order` ve `color` varsayilan degerleri ekle
- [ ] Period ekleme formunda varsayilan degerleri ayarla
- [ ] Milestone ekleme formunda `date` alanini kullan
- [ ] Milestone formunda `applicationStartDate` ve `applicationEndDate` alanlari ekle
- [ ] Activity status'u string olarak gonder

## 8. Notlar

1. Backend'de Global Query Filter aktif - `IsDeleted = true` olan kayitlar otomatik filtrelenir
2. `order` alani 0'dan baslar, otomatik atanir (gonderilmezse)
3. `color` alani opsiyonel - gonderilmezse varsayilan `#3B82F6` kullanilir
4. `milestones` ve `activities` bos array olarak gonderilebilir `[]`
5. Periods guncelleme sirasinda mevcut milestones/activities silinip yeniden olusturulur (replace semantics)
6. `applicationStartDate` ve `applicationEndDate` alanlari milestone'lar icin basvuru tarihlerini belirtir (opsiyonel)

## 9. Son Guncellemeler

- **2025-12-28**: `applicationStartDate` ve `applicationEndDate` alanlari milestone DTO'larina eklendi
- **2025-12-28**: Backend'de `AgpTimelineMilestone` entity'sine yeni kolonlar eklendi
- **2025-12-28**: EF Core migration olusturuldu ve uygulandi (`AddApplicationDatesToMilestone`)
