-- Önce taksitleri sil (foreign key constraint nedeniyle)
DELETE FROM PaymentInstallments;

-- Sonra öğrenci ödeme planlarını sil
DELETE FROM StudentPaymentPlans;

-- Son olarak ödeme planı şablonlarını sil (isteğe bağlı)
-- DELETE FROM PaymentPlans;

-- Sonuçları göster
SELECT 'PaymentInstallments silindi' AS Result;
SELECT 'StudentPaymentPlans silindi' AS Result;
