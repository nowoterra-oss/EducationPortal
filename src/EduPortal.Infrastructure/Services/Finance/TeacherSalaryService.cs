using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Finance;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace EduPortal.Infrastructure.Services.Finance;

public class TeacherSalaryService : ITeacherSalaryService
{
    private readonly ApplicationDbContext _context;
    private readonly IPaymentNotificationService _notificationService;

    public TeacherSalaryService(ApplicationDbContext context, IPaymentNotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<PagedResult<TeacherSalaryDto>> GetAllPagedAsync(
        int pageNumber, int pageSize, int? year = null, int? month = null, SalaryStatus? status = null)
    {
        var query = _context.TeacherSalaries
            .Include(s => s.Teacher)
                .ThenInclude(t => t.User)
            .Where(s => !s.IsDeleted)
            .AsQueryable();

        if (year.HasValue)
            query = query.Where(s => s.Year == year.Value);

        if (month.HasValue)
            query = query.Where(s => s.Month == month.Value);

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);

        query = query.OrderByDescending(s => s.Year).ThenByDescending(s => s.Month);

        var totalCount = await query.CountAsync();

        var salaries = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = salaries.Select(MapToDto);

        return new PagedResult<TeacherSalaryDto>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<TeacherSalaryDto?> GetByIdAsync(int id)
    {
        var salary = await _context.TeacherSalaries
            .Include(s => s.Teacher)
                .ThenInclude(t => t.User)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

        return salary != null ? MapToDto(salary) : null;
    }

    public async Task<List<TeacherSalaryDto>> GetByTeacherAsync(int teacherId)
    {
        var salaries = await _context.TeacherSalaries
            .Include(s => s.Teacher)
                .ThenInclude(t => t.User)
            .Where(s => s.TeacherId == teacherId && !s.IsDeleted)
            .OrderByDescending(s => s.Year)
            .ThenByDescending(s => s.Month)
            .ToListAsync();

        return salaries.Select(MapToDto).ToList();
    }

    public async Task<TeacherSalaryDto> CreateAsync(TeacherSalaryCreateDto dto)
    {
        var existingSalary = await _context.TeacherSalaries
            .FirstOrDefaultAsync(s => s.TeacherId == dto.TeacherId
                && s.Year == dto.Year
                && s.Month == dto.Month
                && !s.IsDeleted);

        if (existingSalary != null)
            throw new InvalidOperationException($"Bu öğretmen için {dto.Year}/{dto.Month} dönemine ait maaş kaydı zaten mevcut.");

        var salary = new TeacherSalary
        {
            TeacherId = dto.TeacherId,
            BaseSalary = dto.BaseSalary,
            Bonus = dto.Bonus,
            Deduction = dto.Deduction,
            Year = dto.Year,
            Month = dto.Month,
            DueDate = dto.DueDate,
            Status = SalaryStatus.Bekliyor,
            Description = dto.Description,
            Notes = dto.Notes
        };

        _context.TeacherSalaries.Add(salary);
        await _context.SaveChangesAsync();

        await _notificationService.SendSalaryCreatedAsync(dto.TeacherId, salary.NetSalary, dto.Month, dto.Year);

        return (await GetByIdAsync(salary.Id))!;
    }

    public async Task<List<TeacherSalaryDto>> CreateBulkAsync(TeacherSalaryBulkCreateDto dto)
    {
        var results = new List<TeacherSalaryDto>();
        var createdSalaryIds = new List<int>();

        // Yeni yöntem: TeacherSalaries listesi varsa her öğretmen için özel maaş kullan
        if (dto.TeacherSalaries != null && dto.TeacherSalaries.Any())
        {
            foreach (var item in dto.TeacherSalaries)
            {
                // Öğretmeni getir
                var teacher = await _context.Teachers.FindAsync(item.TeacherId);
                if (teacher == null)
                    continue;

                // Maaş tipini belirle (item'dan gelen veya öğretmenin tanımlı tipi)
                var isMonthly = item.SalaryType.HasValue
                    ? item.SalaryType.Value == 0
                    : teacher.SalaryType == SalaryType.Monthly;

                // Süreyi belirle (aylık için DurationMonths, saatlik için 1)
                var duration = isMonthly ? (item.DurationMonths ?? 1) : 1;

                // Başlangıç yılı ve ayını belirle
                var startYear = item.Year ?? dto.Year;
                var startMonth = item.Month ?? dto.Month;

                // Her ay için maaş kaydı oluştur
                for (int i = 0; i < duration; i++)
                {
                    // Ay hesapla (12'yi geçerse yılı artır)
                    var currentMonth = startMonth + i;
                    var currentYear = startYear;
                    while (currentMonth > 12)
                    {
                        currentMonth -= 12;
                        currentYear++;
                    }

                    // Bu ay için zaten kayıt var mı kontrol et
                    var existingSalary = await _context.TeacherSalaries
                        .FirstOrDefaultAsync(s => s.TeacherId == item.TeacherId
                            && s.Year == currentYear
                            && s.Month == currentMonth
                            && !s.IsDeleted);

                    if (existingSalary != null)
                        continue;

                    // Maaş hesapla
                    decimal baseSalary = item.BaseSalary;

                    // Eğer baseSalary 0 ise öğretmenin tanımlı maaşını kullan
                    if (baseSalary == 0)
                    {
                        if (isMonthly && teacher.MonthlySalary.HasValue)
                        {
                            baseSalary = teacher.MonthlySalary.Value;
                        }
                        else if (!isMonthly)
                        {
                            // Saatlik ücret hesapla
                            var hourlyRate = item.HourlyRate ?? teacher.HourlyRate ?? 0;
                            var workedHours = item.WorkedHours ?? 0;
                            baseSalary = hourlyRate * workedHours;
                        }
                    }

                    // İlgili ay için DueDate hesapla (ayın son günü)
                    var dueDate = new DateTime(currentYear, currentMonth, DateTime.DaysInMonth(currentYear, currentMonth));

                    var salary = new TeacherSalary
                    {
                        TeacherId = item.TeacherId,
                        BaseSalary = baseSalary,
                        Bonus = item.Bonus ?? 0,
                        Deduction = item.Deductions ?? 0,
                        Year = currentYear,
                        Month = currentMonth,
                        DueDate = dueDate,
                        Status = SalaryStatus.Bekliyor,
                        Description = dto.Description,
                        Notes = item.Notes
                    };

                    _context.TeacherSalaries.Add(salary);
                }
            }
        }
        // Eski yöntem: Sadece TeacherIds varsa
        else if (dto.TeacherIds != null && dto.TeacherIds.Any())
        {
            foreach (var teacherId in dto.TeacherIds)
            {
                var existingSalary = await _context.TeacherSalaries
                    .FirstOrDefaultAsync(s => s.TeacherId == teacherId
                        && s.Year == dto.Year
                        && s.Month == dto.Month
                        && !s.IsDeleted);

                if (existingSalary != null)
                    continue;

                // Öğretmeni getir - tanımlı maaşını kullanmak için
                var teacher = await _context.Teachers.FindAsync(teacherId);
                if (teacher == null)
                    continue;

                // Öğretmenin tanımlı maaşını kullan, yoksa default
                decimal baseSalary = teacher.MonthlySalary ?? dto.DefaultBaseSalary ?? 0;

                var salary = new TeacherSalary
                {
                    TeacherId = teacherId,
                    BaseSalary = baseSalary,
                    Bonus = 0,
                    Deduction = 0,
                    Year = dto.Year,
                    Month = dto.Month,
                    DueDate = dto.DueDate,
                    Status = SalaryStatus.Bekliyor,
                    Description = dto.Description
                };

                _context.TeacherSalaries.Add(salary);
            }
        }

        await _context.SaveChangesAsync();

        // Oluşturulan tüm maaşları getir
        // Yeni yöntemde birden fazla ay olabileceği için tüm yeni kayıtları al
        if (dto.TeacherSalaries != null && dto.TeacherSalaries.Any())
        {
            var teacherIds = dto.TeacherSalaries.Select(t => t.TeacherId).Distinct().ToList();

            // Her öğretmen için oluşturulan maaşları getir
            foreach (var item in dto.TeacherSalaries)
            {
                var startYear = item.Year ?? dto.Year;
                var startMonth = item.Month ?? dto.Month;
                var duration = item.DurationMonths ?? 1;

                for (int i = 0; i < duration; i++)
                {
                    var currentMonth = startMonth + i;
                    var currentYear = startYear;
                    while (currentMonth > 12)
                    {
                        currentMonth -= 12;
                        currentYear++;
                    }

                    var salary = await _context.TeacherSalaries
                        .Include(s => s.Teacher)
                            .ThenInclude(t => t.User)
                        .FirstOrDefaultAsync(s => s.TeacherId == item.TeacherId
                            && s.Year == currentYear
                            && s.Month == currentMonth
                            && !s.IsDeleted);

                    if (salary != null && !createdSalaryIds.Contains(salary.Id))
                    {
                        createdSalaryIds.Add(salary.Id);
                        results.Add(MapToDto(salary));
                        await _notificationService.SendSalaryCreatedAsync(salary.TeacherId, salary.NetSalary, salary.Month, salary.Year);
                    }
                }
            }
        }
        else if (dto.TeacherIds != null && dto.TeacherIds.Any())
        {
            foreach (var teacherId in dto.TeacherIds)
            {
                var salary = await _context.TeacherSalaries
                    .Include(s => s.Teacher)
                        .ThenInclude(t => t.User)
                    .FirstOrDefaultAsync(s => s.TeacherId == teacherId
                        && s.Year == dto.Year
                        && s.Month == dto.Month
                        && !s.IsDeleted);

                if (salary != null)
                {
                    results.Add(MapToDto(salary));
                    await _notificationService.SendSalaryCreatedAsync(teacherId, salary.NetSalary, dto.Month, dto.Year);
                }
            }
        }

        return results;
    }

    public async Task<TeacherSalaryDto> UpdateAsync(int id, TeacherSalaryCreateDto dto)
    {
        var salary = await _context.TeacherSalaries.FindAsync(id);
        if (salary == null || salary.IsDeleted)
            throw new KeyNotFoundException("Maaş kaydı bulunamadı.");

        if (salary.Status == SalaryStatus.Odendi)
            throw new InvalidOperationException("Ödenmiş maaş kaydı güncellenemez.");

        salary.BaseSalary = dto.BaseSalary;
        salary.Bonus = dto.Bonus;
        salary.Deduction = dto.Deduction;
        salary.DueDate = dto.DueDate;
        salary.Description = dto.Description;
        salary.Notes = dto.Notes;
        salary.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(id))!;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var salary = await _context.TeacherSalaries.FindAsync(id);
        if (salary == null || salary.IsDeleted)
            return false;

        if (salary.Status == SalaryStatus.Odendi)
            throw new InvalidOperationException("Ödenmiş maaş kaydı silinemez.");

        salary.IsDeleted = true;
        salary.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<TeacherSalaryDto> PaySalaryAsync(int id, TeacherSalaryPayDto dto)
    {
        var salary = await _context.TeacherSalaries
            .Include(s => s.Teacher)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

        if (salary == null)
            throw new KeyNotFoundException("Maaş kaydı bulunamadı.");

        if (salary.Status == SalaryStatus.Odendi)
            throw new InvalidOperationException("Bu maaş zaten ödenmiş.");

        salary.Status = SalaryStatus.Odendi;
        salary.PaidDate = DateTime.UtcNow;
        salary.PaymentMethod = dto.PaymentMethod;
        salary.TransactionId = dto.TransactionId;
        salary.Notes = dto.Notes ?? salary.Notes;
        salary.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _notificationService.SendSalaryPaidAsync(salary.TeacherId, salary.NetSalary, salary.Month, salary.Year);

        return (await GetByIdAsync(id))!;
    }

    public async Task<List<TeacherSalaryDto>> GetPendingAsync()
    {
        var salaries = await _context.TeacherSalaries
            .Include(s => s.Teacher)
                .ThenInclude(t => t.User)
            .Where(s => s.Status == SalaryStatus.Bekliyor && !s.IsDeleted)
            .OrderBy(s => s.DueDate)
            .ToListAsync();

        return salaries.Select(MapToDto).ToList();
    }

    public async Task<List<TeacherSalaryDto>> GetOverdueAsync()
    {
        var today = DateTime.Today;
        var salaries = await _context.TeacherSalaries
            .Include(s => s.Teacher)
                .ThenInclude(t => t.User)
            .Where(s => s.Status == SalaryStatus.Bekliyor && s.DueDate < today && !s.IsDeleted)
            .OrderBy(s => s.DueDate)
            .ToListAsync();

        foreach (var salary in salaries.Where(s => s.Status != SalaryStatus.Gecikti))
        {
            salary.Status = SalaryStatus.Gecikti;
        }
        await _context.SaveChangesAsync();

        return salaries.Select(MapToDto).ToList();
    }

    public async Task<decimal> GetTotalPendingAsync()
    {
        return await _context.TeacherSalaries
            .Where(s => (s.Status == SalaryStatus.Bekliyor || s.Status == SalaryStatus.Gecikti) && !s.IsDeleted)
            .SumAsync(s => s.BaseSalary + s.Bonus - s.Deduction);
    }

    public async Task<decimal> GetTotalPaidForMonthAsync(int year, int month)
    {
        return await _context.TeacherSalaries
            .Where(s => s.Year == year && s.Month == month && s.Status == SalaryStatus.Odendi && !s.IsDeleted)
            .SumAsync(s => s.BaseSalary + s.Bonus - s.Deduction);
    }

    private static TeacherSalaryDto MapToDto(TeacherSalary salary)
    {
        var culture = new CultureInfo("tr-TR");
        return new TeacherSalaryDto
        {
            Id = salary.Id,
            TeacherId = salary.TeacherId,
            TeacherName = salary.Teacher?.User != null
                ? $"{salary.Teacher.User.FirstName} {salary.Teacher.User.LastName}"
                : string.Empty,
            TeacherEmail = salary.Teacher?.User?.Email,
            BaseSalary = salary.BaseSalary,
            Bonus = salary.Bonus,
            Deduction = salary.Deduction,
            NetSalary = salary.NetSalary,
            Year = salary.Year,
            Month = salary.Month,
            MonthName = culture.DateTimeFormat.GetMonthName(salary.Month),
            DueDate = salary.DueDate,
            PaidDate = salary.PaidDate,
            Status = salary.Status,
            PaymentMethod = salary.PaymentMethod,
            TransactionId = salary.TransactionId,
            Description = salary.Description,
            Notes = salary.Notes,
            CreatedAt = salary.CreatedAt
        };
    }
}
