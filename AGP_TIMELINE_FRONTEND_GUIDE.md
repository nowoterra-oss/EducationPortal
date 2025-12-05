# AGP Timeline Frontend Entegrasyon Rehberi

## 1. API Endpoint'leri

| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | `/api/agp` | Tüm AGP'leri listele (sayfalı) |
| GET | `/api/agp/{id}` | Tek AGP detayı |
| POST | `/api/agp` | Yeni AGP oluştur |
| PUT | `/api/agp/{id}` | AGP güncelle |
| DELETE | `/api/agp/{id}` | AGP sil (soft delete) |
| GET | `/api/agp/student/{studentId}` | Öğrenciye ait AGP'ler |

---

## 2. TypeScript Interface'leri

### AGP Response DTO (GET)

```typescript
interface AGPDto {
  id: number;
  studentId: number;
  studentName: string;
  academicYear: string;
  startDate: string;        // ISO format
  endDate: string;          // ISO format
  planDocumentUrl?: string;
  status: number;           // 0: Taslak, 1: Onaylandı, 2: Tamamlandı
  statusName: string;
  milestoneCount: number;
  completedMilestoneCount: number;
  overallProgress: number;
  milestones: AGPGoalDto[];
  periods: AgpPeriodDto[];  // Timeline dönemleri
}
```

### Timeline DTO Yapıları

```typescript
// Dönem (Period)
interface AgpPeriodDto {
  id?: number;              // GET'te döner, POST/PUT'ta opsiyonel
  title: string;            // Örn: "1. Dönem", "Yaz Dönemi"
  startDate: string;        // Format: "yyyy-MM-dd" (örn: "2025-09-01")
  endDate: string;          // Format: "yyyy-MM-dd" (örn: "2025-12-31")
  color?: string;           // Hex renk kodu (örn: "#FF5733")
  order: number;            // Sıralama (0, 1, 2...)
  milestones: AgpMilestoneDto[];
  activities: AgpActivityDto[];
}

// Sınav/Hedef (Milestone)
interface AgpMilestoneDto {
  id?: number;
  title: string;            // Örn: "SAT Sınavı", "IELTS 29 Ağu."
  date: string;             // Format: "yyyy-MM-dd"
  color?: string;           // Hex renk kodu
  type: string;             // "exam" | "goal" | "event" (default: "exam")
}

// Aktivite
interface AgpActivityDto {
  id?: number;
  title: string;            // Örn: "SAT Hazırlık", "IELTS Çalışma"
  hoursPerWeek: number;     // Haftalık saat (örn: 6)
  notes?: string;           // Notlar
}
```

---

## 3. Create/Update Request DTO'ları

```typescript
// POST /api/agp
interface CreateAGPDto {
  studentId: number;
  academicYear: string;
  startDate: string;
  endDate: string;
  planDocumentUrl?: string;
  status: number;
  periods?: AgpPeriodDto[];  // Timeline dönemleri
}

// PUT /api/agp/{id}
interface UpdateAGPDto {
  academicYear: string;
  startDate: string;
  endDate: string;
  planDocumentUrl?: string;
  status: number;
  periods?: AgpPeriodDto[];  // Timeline dönemleri
}
```

---

## 4. Örnek Request Body

### Create (POST /api/agp)

```json
{
  "studentId": 5,
  "academicYear": "2025-2026",
  "startDate": "2025-09-01T00:00:00",
  "endDate": "2026-06-30T00:00:00",
  "status": 0,
  "periods": [
    {
      "title": "1. Dönem",
      "startDate": "2025-09-01",
      "endDate": "2025-12-31",
      "color": "#ef4444",
      "order": 0,
      "milestones": [
        {
          "title": "SAT Sınavı",
          "date": "2025-12-15",
          "color": "#3b82f6",
          "type": "exam"
        }
      ],
      "activities": [
        {
          "title": "SAT Hazırlık",
          "hoursPerWeek": 6,
          "notes": "Matematik ağırlıklı"
        }
      ]
    }
  ]
}
```

### Update (PUT /api/agp/{id})

```json
{
  "academicYear": "2025-2026",
  "startDate": "2025-09-01T00:00:00",
  "endDate": "2026-06-30T00:00:00",
  "status": 0,
  "periods": [
    {
      "title": "1. Dönem",
      "startDate": "2025-12-04",
      "endDate": "2025-12-18",
      "color": "#ef4444",
      "order": 0,
      "milestones": [
        {
          "title": "SAT Sınavı",
          "date": "2025-12-15",
          "color": "#3b82f6",
          "type": "exam"
        }
      ],
      "activities": [
        {
          "title": "SAT Hazırlık",
          "hoursPerWeek": 6,
          "notes": "Matematik ağırlıklı"
        }
      ]
    },
    {
      "title": "2. Dönem",
      "startDate": "2025-12-19",
      "endDate": "2025-12-27",
      "color": "#22c55e",
      "order": 1,
      "milestones": [],
      "activities": []
    }
  ]
}
```

---

## 5. Önemli Kurallar

| Konu | Açıklama |
|------|----------|
| **Tarih Formatı** | Periods içindeki tarihler `"yyyy-MM-dd"` formatında string olmalı |
| **Type Değerleri** | Milestone type: `"exam"`, `"goal"`, `"event"` (boş bırakılırsa `"exam"` olur) |
| **Order Sıralaması** | Periods'lar `order` alanına göre sıralanır (0'dan başla) |
| **Update Davranışı** | Update'te gönderilen periods **mevcut olanların yerine geçer** (append değil, replace) |
| **Boş Arrays** | `milestones` ve `activities` boş array `[]` olarak gönderilebilir |
| **Null Periods** | `periods: null` gönderilirse mevcut periods korunur |

---

## 6. Frontend Validasyonları

```typescript
// Tarih formatı kontrolü
const isValidDateFormat = (date: string): boolean => {
  return /^\d{4}-\d{2}-\d{2}$/.test(date);
};

// Period validasyonu
const validatePeriod = (period: AgpPeriodDto): boolean => {
  return (
    period.title?.trim().length > 0 &&
    isValidDateFormat(period.startDate) &&
    isValidDateFormat(period.endDate) &&
    new Date(period.startDate) <= new Date(period.endDate)
  );
};

// Milestone validasyonu
const validateMilestone = (m: AgpMilestoneDto): boolean => {
  return (
    m.title?.trim().length > 0 &&
    isValidDateFormat(m.date) &&
    ["exam", "goal", "event"].includes(m.type || "exam")
  );
};

// Activity validasyonu
const validateActivity = (a: AgpActivityDto): boolean => {
  return (
    a.title?.trim().length > 0 &&
    a.hoursPerWeek >= 0 &&
    a.hoursPerWeek <= 168 // Haftada max 168 saat
  );
};
```

---

## 7. API Çağrı Örnekleri

```typescript
// AGP Getir
const fetchAGP = async (id: number): Promise<AGPDto> => {
  const response = await fetch(`/api/agp/${id}`);
  const data = await response.json();
  return data.data;
};

// AGP Güncelle
const updateAGP = async (id: number, dto: UpdateAGPDto): Promise<AGPDto> => {
  const response = await fetch(`/api/agp/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(dto)
  });

  if (!response.ok) {
    throw new Error('AGP güncellenirken hata oluştu');
  }

  const data = await response.json();
  return data.data;
};

// Öğrencinin AGP'lerini Getir
const fetchStudentAGPs = async (studentId: number): Promise<AGPDto[]> => {
  const response = await fetch(`/api/agp/student/${studentId}`);
  const data = await response.json();
  return data.data;
};
```

---

## 8. HTTP Status Kodları

| Status | Açıklama |
|--------|----------|
| 200 | Başarılı |
| 400 | Validasyon hatası (eksik veya hatalı alan) |
| 404 | AGP bulunamadı |
| 500 | Sunucu hatası |

---

## 9. Hata Yönetimi

```typescript
try {
  const updatedAGP = await updateAGP(agpId, formData);
  console.log('✅ AGP güncellendi:', updatedAGP);
} catch (error) {
  console.error('❌ Hata:', error.message);
  // Kullanıcıya hata mesajı göster
}
```

---

## 10. Kontrol Listesi

- [ ] Period tarih formatı `yyyy-MM-dd` mi?
- [ ] Milestone type değeri `exam`, `goal` veya `event` mi?
- [ ] Order değerleri 0'dan başlıyor mu?
- [ ] Boş milestone/activity için boş array `[]` gönderiliyor mu?
- [ ] Update öncesi mevcut AGP verisi yükleniyor mu?

---

## Sorular?

Backend ekibine Slack veya issue üzerinden ulaşabilirsiniz.
