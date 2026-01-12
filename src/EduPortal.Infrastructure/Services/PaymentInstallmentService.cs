using EduPortal.Application.Common;
using EduPortal.Application.DTOs.PaymentPlan;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using EduPortal.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class PaymentInstallmentService : IPaymentInstallmentService
{
    private readonly ApplicationDbContext _context;
    private readonly string _uploadPath;
    private readonly string[] _allowedExtensions = { ".png", ".jpg", ".jpeg", ".pdf" };
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public PaymentInstallmentService(ApplicationDbContext context)
    {
        _context = context;
        _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "receipts");
    }

    public async Task<IEnumerable<PaymentInstallmentDto>> GetByStudentPaymentPlanAsync(int planId)
    {
        return await _context.Set<PaymentInstallment>()
            .Include(i => i.StudentPaymentPlan)
                .ThenInclude(p => p.Student)
                .ThenInclude(s => s.User)
            .Where(i => i.StudentPaymentPlanId == planId)
            .OrderBy(i => i.InstallmentNumber)
            .Select(i => MapToDto(i))
            .ToListAsync();
    }

    public async Task<IEnumerable<PaymentInstallmentDto>> GetByStudentIdAsync(int studentId)
    {
        return await _context.Set<PaymentInstallment>()
            .Include(i => i.StudentPaymentPlan)
                .ThenInclude(p => p.Student)
                .ThenInclude(s => s.User)
            .Where(i => i.StudentPaymentPlan.StudentId == studentId)
            .OrderBy(i => i.DueDate)
            .Select(i => MapToDto(i))
            .ToListAsync();
    }

    public async Task<IEnumerable<PaymentInstallmentDto>> GetByParentIdAsync(int parentId)
    {
        // Velinin çocuklarının ID'lerini bul
        var studentIds = await _context.Set<StudentParent>()
            .Where(sp => sp.ParentId == parentId)
            .Select(sp => sp.StudentId)
            .ToListAsync();

        if (!studentIds.Any())
            return Enumerable.Empty<PaymentInstallmentDto>();

        // Çocukların taksitlerini getir
        return await _context.Set<PaymentInstallment>()
            .Include(i => i.StudentPaymentPlan)
                .ThenInclude(p => p.Student)
                .ThenInclude(s => s.User)
            .Where(i => studentIds.Contains(i.StudentPaymentPlan.StudentId))
            .OrderBy(i => i.DueDate)
            .Select(i => MapToDto(i))
            .ToListAsync();
    }

    public async Task<PaymentInstallmentDto?> GetByIdAsync(int id)
    {
        var installment = await _context.Set<PaymentInstallment>()
            .Include(i => i.StudentPaymentPlan)
                .ThenInclude(p => p.Student)
                .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(i => i.Id == id);

        return installment != null ? MapToDto(installment) : null;
    }

    public async Task<IEnumerable<PaymentInstallmentDto>> GetOverdueAsync()
    {
        var today = DateTime.Today;

        // Önce pending olanları bul ve overdue yap
        var pendingInstallments = await _context.Set<PaymentInstallment>()
            .Where(i => i.Status == InstallmentStatus.Pending && i.DueDate < today)
            .ToListAsync();

        foreach (var inst in pendingInstallments)
        {
            inst.Status = InstallmentStatus.Overdue;
        }

        if (pendingInstallments.Any())
            await _context.SaveChangesAsync();

        // Tüm overdue'ları getir
        return await _context.Set<PaymentInstallment>()
            .Include(i => i.StudentPaymentPlan)
                .ThenInclude(p => p.Student)
                .ThenInclude(s => s.User)
            .Where(i => i.Status == InstallmentStatus.Overdue)
            .OrderBy(i => i.DueDate)
            .Select(i => MapToDto(i))
            .ToListAsync();
    }

    public async Task<IEnumerable<PaymentInstallmentDto>> GetUpcomingAsync(int days = 7)
    {
        var today = DateTime.Today;
        var futureDate = today.AddDays(days);

        return await _context.Set<PaymentInstallment>()
            .Include(i => i.StudentPaymentPlan)
                .ThenInclude(p => p.Student)
                .ThenInclude(s => s.User)
            .Where(i => i.Status == InstallmentStatus.Pending &&
                       i.DueDate >= today &&
                       i.DueDate <= futureDate)
            .OrderBy(i => i.DueDate)
            .Select(i => MapToDto(i))
            .ToListAsync();
    }

    public async Task<PaymentInstallmentDto> PayInstallmentAsync(int installmentId, PayInstallmentDto dto)
    {
        var installment = await _context.Set<PaymentInstallment>()
            .Include(i => i.StudentPaymentPlan)
            .FirstOrDefaultAsync(i => i.Id == installmentId);

        if (installment == null)
            throw new Exception($"Installment with ID {installmentId} not found");

        if (installment.Status == InstallmentStatus.Paid)
            throw new Exception("This installment is already paid");

        // Payment kaydı oluştur
        var paymentMethod = PaymentMethod.Nakit; // Varsayılan
        if (!string.IsNullOrEmpty(dto.PaymentMethod))
        {
            Enum.TryParse<PaymentMethod>(dto.PaymentMethod, true, out paymentMethod);
        }

        var payment = new Payment
        {
            StudentId = installment.StudentPaymentPlan.StudentId,
            Amount = dto.Amount,
            PaymentDate = dto.PaymentDate,
            PaymentMethod = paymentMethod,
            TransactionId = dto.TransactionReference,
            Status = PaymentStatus.Odendi,
            Notes = dto.Notes
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        // Taksiti güncelle
        installment.PaidAmount += dto.Amount;
        installment.PaidDate = dto.PaymentDate;
        installment.PaymentId = payment.Id;

        if (installment.PaidAmount >= installment.Amount)
        {
            installment.Status = InstallmentStatus.Paid;
        }
        else
        {
            installment.Status = InstallmentStatus.PartiallyPaid;
        }

        // Student payment planı güncelle
        var plan = installment.StudentPaymentPlan;
        plan.PaidAmount += dto.Amount;
        plan.RemainingAmount = plan.TotalAmount - plan.PaidAmount;

        // Tüm taksitler ödendiyse planı tamamla
        var allInstallments = await _context.Set<PaymentInstallment>()
            .Where(i => i.StudentPaymentPlanId == plan.Id)
            .ToListAsync();

        if (allInstallments.All(i => i.Status == InstallmentStatus.Paid))
        {
            plan.Status = PaymentPlanStatus.Completed;
            plan.EndDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(installmentId))!;
    }

    public async Task<object> GetStatisticsAsync()
    {
        var installments = await _context.Set<PaymentInstallment>()
            .Include(i => i.StudentPaymentPlan)
            .ToListAsync();

        var today = DateTime.Today;

        return new
        {
            TotalInstallments = installments.Count,
            PaidInstallments = installments.Count(i => i.Status == InstallmentStatus.Paid),
            PendingInstallments = installments.Count(i => i.Status == InstallmentStatus.Pending),
            OverdueInstallments = installments.Count(i => i.Status == InstallmentStatus.Overdue),
            PartiallyPaidInstallments = installments.Count(i => i.Status == InstallmentStatus.PartiallyPaid),
            TotalAmount = installments.Sum(i => i.Amount),
            TotalPaid = installments.Sum(i => i.PaidAmount),
            TotalRemaining = installments.Sum(i => i.Amount - i.PaidAmount),
            UpcomingThisWeek = installments.Count(i =>
                i.Status == InstallmentStatus.Pending &&
                i.DueDate >= today &&
                i.DueDate <= today.AddDays(7)),
            UpcomingThisMonth = installments.Count(i =>
                i.Status == InstallmentStatus.Pending &&
                i.DueDate >= today &&
                i.DueDate <= today.AddDays(30))
        };
    }

    public async Task<PaymentInstallmentDto> UploadReceiptAsync(int installmentId, IFormFile file, string? notes, int studentId)
    {
        var installment = await _context.Set<PaymentInstallment>()
            .Include(i => i.StudentPaymentPlan)
                .ThenInclude(p => p.Student)
                .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(i => i.Id == installmentId);

        if (installment == null)
            throw new KeyNotFoundException("Taksit bulunamadı.");

        // Yetkilendirme: Sadece kendi öğrencisinin taksiti için dekont yükleyebilir
        if (installment.StudentPaymentPlan.StudentId != studentId)
            throw new UnauthorizedAccessException("Bu taksit için dekont yükleme yetkiniz yok.");

        // Zaten ödendi mi kontrol et
        if (installment.Status == InstallmentStatus.Paid)
            throw new InvalidOperationException("Bu taksit zaten ödenmiş.");

        // Dosya validasyonu
        if (file == null || file.Length == 0)
            throw new ArgumentException("Dosya seçilmedi.");

        if (file.Length > MaxFileSize)
            throw new ArgumentException("Dosya boyutu 10MB'dan büyük olamaz.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
            throw new ArgumentException("Sadece PNG, JPG, JPEG ve PDF dosyaları kabul edilmektedir.");

        // Klasör yapısı oluştur
        var now = DateTime.Now;
        var yearMonth = now.ToString("yyyy/MM");
        var directory = Path.Combine(_uploadPath, yearMonth);
        Directory.CreateDirectory(directory);

        // Dosya adı oluştur
        var fileName = $"receipt_{studentId}_{installmentId}_{now:yyyyMMddHHmmss}{extension}";
        var filePath = Path.Combine(directory, fileName);
        var relativePath = $"/uploads/receipts/{yearMonth}/{fileName}";

        // Dosyayı kaydet
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Taksiti güncelle
        installment.ReceiptPath = relativePath;
        installment.ReceiptUploadDate = DateTime.UtcNow;
        installment.Status = InstallmentStatus.AwaitingApproval;
        installment.RejectionReason = null; // Önceki red sebebini temizle
        if (!string.IsNullOrEmpty(notes))
            installment.Notes = notes;

        await _context.SaveChangesAsync();

        return MapToDto(installment);
    }

    public async Task<(byte[] FileContent, string ContentType, string FileName)?> GetReceiptAsync(int installmentId)
    {
        var installment = await _context.Set<PaymentInstallment>()
            .FirstOrDefaultAsync(i => i.Id == installmentId);

        if (installment == null || string.IsNullOrEmpty(installment.ReceiptPath))
            return null;

        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", installment.ReceiptPath.TrimStart('/'));

        if (!File.Exists(fullPath))
            return null;

        var fileContent = await File.ReadAllBytesAsync(fullPath);
        var extension = Path.GetExtension(fullPath).ToLowerInvariant();
        var contentType = extension switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            _ => "application/octet-stream"
        };

        var fileName = Path.GetFileName(fullPath);

        return (fileContent, contentType, fileName);
    }

    public async Task<PagedResult<PaymentInstallmentDto>> GetPendingApprovalAsync(int pageNumber, int pageSize)
    {
        var query = _context.Set<PaymentInstallment>()
            .Include(i => i.StudentPaymentPlan)
                .ThenInclude(p => p.Student)
                .ThenInclude(s => s.User)
            .Where(i => i.Status == InstallmentStatus.AwaitingApproval)
            .OrderBy(i => i.ReceiptUploadDate);

        var totalCount = await query.CountAsync();

        var installments = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = installments.Select(MapToDto);

        return new PagedResult<PaymentInstallmentDto>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<PaymentInstallmentDto> ApproveInstallmentAsync(int installmentId, string approvedByUserId, string? notes)
    {
        var installment = await _context.Set<PaymentInstallment>()
            .Include(i => i.StudentPaymentPlan)
                .ThenInclude(p => p.Student)
                .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(i => i.Id == installmentId);

        if (installment == null)
            throw new KeyNotFoundException("Taksit bulunamadı.");

        if (installment.Status != InstallmentStatus.AwaitingApproval)
            throw new InvalidOperationException("Bu taksit onay bekliyor durumunda değil.");

        // Taksiti onayla
        installment.Status = InstallmentStatus.Paid;
        installment.PaidAmount = installment.Amount;
        installment.PaidDate = DateTime.UtcNow;
        installment.ApprovedBy = approvedByUserId;
        installment.ApprovalDate = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(notes))
            installment.Notes = notes;

        // Ödeme planını güncelle
        var plan = installment.StudentPaymentPlan;
        plan.PaidAmount += installment.Amount;
        plan.RemainingAmount = plan.TotalAmount - plan.PaidAmount;

        // Tüm taksitler ödendiyse planı tamamla
        var allInstallments = await _context.Set<PaymentInstallment>()
            .Where(i => i.StudentPaymentPlanId == plan.Id)
            .ToListAsync();

        if (allInstallments.All(i => i.Status == InstallmentStatus.Paid))
        {
            plan.Status = PaymentPlanStatus.Completed;
            plan.EndDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // ApprovedByUser bilgisini ekle
        var approver = await _context.Set<ApplicationUser>().FindAsync(approvedByUserId);
        var dto = MapToDto(installment);
        dto.ApprovedByName = approver != null ? $"{approver.FirstName} {approver.LastName}" : null;

        return dto;
    }

    public async Task<PaymentInstallmentDto> RejectInstallmentAsync(int installmentId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Red sebebi belirtilmelidir.");

        var installment = await _context.Set<PaymentInstallment>()
            .Include(i => i.StudentPaymentPlan)
                .ThenInclude(p => p.Student)
                .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(i => i.Id == installmentId);

        if (installment == null)
            throw new KeyNotFoundException("Taksit bulunamadı.");

        if (installment.Status != InstallmentStatus.AwaitingApproval)
            throw new InvalidOperationException("Bu taksit onay bekliyor durumunda değil.");

        // Taksiti reddet
        installment.Status = InstallmentStatus.Rejected;
        installment.RejectionReason = reason;

        await _context.SaveChangesAsync();

        return MapToDto(installment);
    }

    public async Task<object> GetDebugStatusAsync()
    {
        var all = await _context.Set<PaymentInstallment>()
            .Include(i => i.StudentPaymentPlan)
                .ThenInclude(p => p.Student)
                .ThenInclude(s => s.User)
            .ToListAsync();

        var statusCounts = all.GroupBy(i => i.Status)
            .Select(g => new { Status = g.Key.ToString(), StatusInt = (int)g.Key, Count = g.Count() })
            .ToList();

        var awaitingApproval = all
            .Where(i => i.Status == InstallmentStatus.AwaitingApproval)
            .Select(i => new
            {
                i.Id,
                Status = i.Status.ToString(),
                StatusInt = (int)i.Status,
                i.ReceiptPath,
                i.ReceiptUploadDate,
                StudentName = $"{i.StudentPaymentPlan.Student?.User?.FirstName} {i.StudentPaymentPlan.Student?.User?.LastName}"
            })
            .ToList();

        return new
        {
            Total = all.Count,
            StatusCounts = statusCounts,
            AwaitingApprovalCount = awaitingApproval.Count,
            AwaitingApprovalList = awaitingApproval,
            EnumValues = Enum.GetValues<InstallmentStatus>().Select(e => new { Name = e.ToString(), Value = (int)e })
        };
    }

    public async Task<PaymentInstallmentDto> MarkAsCashPaymentAsync(int installmentId, string approvedByUserId, string? notes, string? paymentMethod)
    {
        var installment = await _context.Set<PaymentInstallment>()
            .Include(i => i.StudentPaymentPlan)
                .ThenInclude(p => p.Student)
                .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(i => i.Id == installmentId);

        if (installment == null)
            throw new KeyNotFoundException("Taksit bulunamadı.");

        // Zaten ödenmiş mi kontrol et
        if (installment.Status == InstallmentStatus.Paid)
            throw new InvalidOperationException("Bu taksit zaten ödenmiş.");

        // İptal edilmiş mi kontrol et
        if (installment.Status == InstallmentStatus.Cancelled)
            throw new InvalidOperationException("İptal edilmiş taksit için ödeme kaydedilemez.");

        // Taksiti ödenmiş olarak işaretle
        installment.Status = InstallmentStatus.Paid;
        installment.PaidAmount = installment.Amount;
        installment.PaidDate = DateTime.UtcNow;
        installment.ApprovedBy = approvedByUserId;
        installment.ApprovalDate = DateTime.UtcNow;
        installment.PaymentMethod = paymentMethod ?? "cash";

        // Not ekle
        var paymentMethodLabel = paymentMethod switch
        {
            "cash" => "Nakit",
            "bank_transfer" => "Banka Transferi",
            "pos" => "POS",
            "other" => "Diğer",
            _ => "Elden Ödeme"
        };
        var existingNotes = string.IsNullOrEmpty(installment.Notes) ? "" : installment.Notes + " - ";
        installment.Notes = existingNotes + $"{paymentMethodLabel} olarak alındı. {notes ?? ""}".Trim();

        // Ödeme planını güncelle
        var plan = installment.StudentPaymentPlan;
        plan.PaidAmount += installment.Amount;
        plan.RemainingAmount = plan.TotalAmount - plan.PaidAmount;

        // Tüm taksitler ödendiyse planı tamamla
        var allInstallments = await _context.Set<PaymentInstallment>()
            .Where(i => i.StudentPaymentPlanId == plan.Id)
            .ToListAsync();

        if (allInstallments.All(i => i.Status == InstallmentStatus.Paid))
        {
            plan.Status = PaymentPlanStatus.Completed;
            plan.EndDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // ApprovedByUser bilgisini ekle
        var approver = await _context.Set<ApplicationUser>().FindAsync(approvedByUserId);
        var dto = MapToDto(installment);
        dto.ApprovedByName = approver != null ? $"{approver.FirstName} {approver.LastName}" : null;

        return dto;
    }

    private static PaymentInstallmentDto MapToDto(PaymentInstallment installment)
    {
        var today = DateTime.Today;
        var daysOverdue = installment.Status == InstallmentStatus.Overdue && installment.DueDate < today
            ? (today - installment.DueDate).Days
            : 0;

        return new PaymentInstallmentDto
        {
            Id = installment.Id,
            StudentPaymentPlanId = installment.StudentPaymentPlanId,
            StudentId = installment.StudentPaymentPlan.StudentId,
            StudentName = $"{installment.StudentPaymentPlan.Student?.User?.FirstName} {installment.StudentPaymentPlan.Student?.User?.LastName}",
            StudentNo = installment.StudentPaymentPlan.Student?.StudentNo,
            InstallmentNumber = installment.InstallmentNumber,
            Amount = installment.Amount,
            PaidAmount = installment.PaidAmount,
            DueDate = installment.DueDate,
            PaidDate = installment.PaidDate,
            Status = installment.Status.ToString(),
            PaymentId = installment.PaymentId,
            Notes = installment.Notes,
            DaysOverdue = daysOverdue,
            // Yeni dekont alanları
            ReceiptPath = installment.ReceiptPath,
            ReceiptUploadDate = installment.ReceiptUploadDate,
            ApprovedBy = installment.ApprovedBy,
            ApprovedByName = installment.ApprovedByUser != null
                ? $"{installment.ApprovedByUser.FirstName} {installment.ApprovedByUser.LastName}"
                : null,
            ApprovalDate = installment.ApprovalDate,
            RejectionReason = installment.RejectionReason,
            PaymentMethod = installment.PaymentMethod
        };
    }
}
