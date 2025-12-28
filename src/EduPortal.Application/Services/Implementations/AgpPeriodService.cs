using AutoMapper;
using EduPortal.Application.Common;
using EduPortal.Application.DTOs.AGP;
using EduPortal.Application.Interfaces;
using EduPortal.Application.Services.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;

namespace EduPortal.Application.Services.Implementations;

public class AgpPeriodService : IAgpPeriodService
{
    private readonly IAgpPeriodRepository _periodRepository;
    private readonly IGenericRepository<AcademicDevelopmentPlan> _agpRepository;
    private readonly IMapper _mapper;

    public AgpPeriodService(
        IAgpPeriodRepository periodRepository,
        IGenericRepository<AcademicDevelopmentPlan> agpRepository,
        IMapper mapper)
    {
        _periodRepository = periodRepository;
        _agpRepository = agpRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<AgpPeriodResponseDto>> CreateAsync(AgpPeriodCreateDto dto)
    {
        try
        {
            // AGP kontrolü
            var agp = await _agpRepository.GetByIdAsync(dto.AgpId);
            if (agp == null || agp.IsDeleted)
            {
                return ApiResponse<AgpPeriodResponseDto>.ErrorResponse("AGP bulunamadı");
            }

            // Tarih validasyonu
            if (dto.EndDate < dto.StartDate)
            {
                return ApiResponse<AgpPeriodResponseDto>.ErrorResponse("Bitiş tarihi başlangıç tarihinden önce olamaz");
            }

            // Çakışma kontrolü
            var overlapping = await _periodRepository.FindOverlappingPeriodsAsync(
                dto.AgpId, dto.StartDate, dto.EndDate);
            if (overlapping.Any())
            {
                return ApiResponse<AgpPeriodResponseDto>.ErrorResponse("Bu tarih aralığında başka bir dönem mevcut");
            }

            // Dönem adı otomatik oluştur
            var periodName = dto.PeriodName;
            if (string.IsNullOrWhiteSpace(periodName))
            {
                periodName = $"{dto.StartDate:dd.MM.yyyy} - {dto.EndDate:dd.MM.yyyy}";
            }

            // Entity oluştur
            var period = new AgpPeriod
            {
                AgpId = dto.AgpId,
                PeriodName = periodName,
                Title = string.IsNullOrWhiteSpace(dto.Title) ? periodName : dto.Title,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Color = dto.Color,
                Order = dto.Order
            };

            // Sınavları/Milestone'ları ekle
            foreach (var milestoneDto in dto.Milestones)
            {
                var milestone = new AgpTimelineMilestone
                {
                    Title = milestoneDto.Title,
                    Date = milestoneDto.Date,
                    Color = milestoneDto.Color,
                    Type = milestoneDto.Type,
                    IsMilestone = milestoneDto.IsMilestone
                };
                period.Milestones.Add(milestone);
            }

            // Aktiviteleri ekle
            foreach (var activityDto in dto.Activities)
            {
                var activity = new AgpActivity
                {
                    Title = activityDto.Title,
                    StartDate = activityDto.StartDate,
                    EndDate = activityDto.EndDate,
                    HoursPerWeek = activityDto.HoursPerWeek,
                    OwnerType = activityDto.OwnerType,
                    Status = activityDto.Status,
                    NeedsReview = activityDto.NeedsReview,
                    Notes = activityDto.Notes
                };
                period.Activities.Add(activity);
            }

            var savedPeriod = await _periodRepository.AddAsync(period);

            // Detaylı veriyi getir
            var periodWithDetails = await _periodRepository.GetWithDetailsAsync(savedPeriod.Id);
            var responseDto = MapToResponseDto(periodWithDetails!);

            return ApiResponse<AgpPeriodResponseDto>.SuccessResponse(responseDto, "Dönem başarıyla oluşturuldu");
        }
        catch (Exception ex)
        {
            return ApiResponse<AgpPeriodResponseDto>.ErrorResponse($"Dönem oluşturulurken hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<AgpPeriodResponseDto>> UpdateAsync(int id, AgpPeriodUpdateDto dto)
    {
        try
        {
            var period = await _periodRepository.GetWithDetailsAsync(id);
            if (period == null || period.IsDeleted)
            {
                return ApiResponse<AgpPeriodResponseDto>.ErrorResponse("Dönem bulunamadı");
            }

            // Tarih validasyonu
            var startDate = dto.StartDate ?? period.StartDate;
            var endDate = dto.EndDate ?? period.EndDate;
            if (endDate < startDate)
            {
                return ApiResponse<AgpPeriodResponseDto>.ErrorResponse("Bitiş tarihi başlangıç tarihinden önce olamaz");
            }

            // Çakışma kontrolü
            var overlapping = await _periodRepository.FindOverlappingPeriodsAsync(
                period.AgpId, startDate, endDate, id);
            if (overlapping.Any())
            {
                return ApiResponse<AgpPeriodResponseDto>.ErrorResponse("Bu tarih aralığında başka bir dönem mevcut");
            }

            // Güncelle
            if (!string.IsNullOrEmpty(dto.PeriodName))
                period.PeriodName = dto.PeriodName;
            if (!string.IsNullOrEmpty(dto.Title))
                period.Title = dto.Title;
            if (dto.StartDate.HasValue)
                period.StartDate = dto.StartDate.Value;
            if (dto.EndDate.HasValue)
                period.EndDate = dto.EndDate.Value;
            if (dto.Color != null)
                period.Color = dto.Color;
            if (dto.Order.HasValue)
                period.Order = dto.Order.Value;

            period.UpdatedAt = DateTime.UtcNow;

            // Eğer milestones gönderildiyse, mevcut olanları sil ve yenilerini ekle
            if (dto.Milestones != null)
            {
                // Mevcut milestone'ları soft delete yap
                foreach (var milestone in period.Milestones)
                {
                    milestone.IsDeleted = true;
                }

                // Yenilerini ekle
                foreach (var milestoneDto in dto.Milestones)
                {
                    var milestone = new AgpTimelineMilestone
                    {
                        AgpPeriodId = period.Id,
                        Title = milestoneDto.Title,
                        Date = milestoneDto.Date,
                        Color = milestoneDto.Color,
                        Type = milestoneDto.Type,
                        IsMilestone = milestoneDto.IsMilestone
                    };
                    period.Milestones.Add(milestone);
                }
            }

            // Eğer activities gönderildiyse, mevcut olanları sil ve yenilerini ekle
            if (dto.Activities != null)
            {
                // Mevcut aktiviteleri soft delete yap
                foreach (var activity in period.Activities)
                {
                    activity.IsDeleted = true;
                }

                // Yenilerini ekle
                foreach (var activityDto in dto.Activities)
                {
                    var activity = new AgpActivity
                    {
                        AgpPeriodId = period.Id,
                        Title = activityDto.Title,
                        StartDate = activityDto.StartDate,
                        EndDate = activityDto.EndDate,
                        HoursPerWeek = activityDto.HoursPerWeek,
                        OwnerType = activityDto.OwnerType,
                        Status = activityDto.Status,
                        NeedsReview = activityDto.NeedsReview,
                        Notes = activityDto.Notes
                    };
                    period.Activities.Add(activity);
                }
            }

            await _periodRepository.UpdateAsync(period);

            // Güncel veriyi getir
            var updatedPeriod = await _periodRepository.GetWithDetailsAsync(id);
            var responseDto = MapToResponseDto(updatedPeriod!);

            return ApiResponse<AgpPeriodResponseDto>.SuccessResponse(responseDto, "Dönem başarıyla güncellendi");
        }
        catch (Exception ex)
        {
            return ApiResponse<AgpPeriodResponseDto>.ErrorResponse($"Dönem güncellenirken hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        try
        {
            var period = await _periodRepository.GetByIdAsync(id);
            if (period == null || period.IsDeleted)
            {
                return ApiResponse<bool>.ErrorResponse("Dönem bulunamadı");
            }

            await _periodRepository.DeleteAsync(period);
            return ApiResponse<bool>.SuccessResponse(true, "Dönem başarıyla silindi");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResponse($"Dönem silinirken hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<AgpPeriodResponseDto>> GetByIdAsync(int id)
    {
        try
        {
            var period = await _periodRepository.GetWithDetailsAsync(id);
            if (period == null || period.IsDeleted)
            {
                return ApiResponse<AgpPeriodResponseDto>.ErrorResponse("Dönem bulunamadı");
            }

            var responseDto = MapToResponseDto(period);
            return ApiResponse<AgpPeriodResponseDto>.SuccessResponse(responseDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<AgpPeriodResponseDto>.ErrorResponse($"Dönem getirilirken hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<AgpPeriodResponseDto>>> GetByAgpIdAsync(int agpId)
    {
        try
        {
            var periods = await _periodRepository.GetByAgpIdWithDetailsAsync(agpId);
            var responseDtos = periods.Select(MapToResponseDto).ToList();
            return ApiResponse<List<AgpPeriodResponseDto>>.SuccessResponse(responseDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<AgpPeriodResponseDto>>.ErrorResponse($"Dönemler getirilirken hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<AgpTimelineViewDto>> GetTimelineViewAsync(int agpId, int monthsToShow = 6)
    {
        try
        {
            var agp = await _agpRepository.GetByIdAsync(agpId);
            if (agp == null || agp.IsDeleted)
            {
                return ApiResponse<AgpTimelineViewDto>.ErrorResponse("AGP bulunamadı");
            }

            var periods = await _periodRepository.GetByAgpIdWithDetailsAsync(agpId);
            var periodsList = periods.ToList();

            var today = DateTime.Today;
            var timelineStart = await _periodRepository.GetEarliestStartDateAsync(agpId) ?? today.AddMonths(-1);
            var timelineEnd = await _periodRepository.GetLatestEndDateAsync(agpId) ?? today.AddMonths(monthsToShow);

            // En az monthsToShow kadar ay göster
            var monthsDiff = ((timelineEnd.Year - timelineStart.Year) * 12) + timelineEnd.Month - timelineStart.Month;
            if (monthsDiff < monthsToShow)
            {
                timelineEnd = timelineStart.AddMonths(monthsToShow);
            }

            // Ay isimlerini oluştur
            var months = new List<string>();
            var monthDate = new DateTime(timelineStart.Year, timelineStart.Month, 1);
            var monthCounter = 1;
            while (monthDate <= timelineEnd)
            {
                months.Add($"MONTH {monthCounter++}");
                monthDate = monthDate.AddMonths(1);
            }

            // Student bilgisini al (ilk period'dan)
            var studentId = 0;
            var studentName = "";
            if (periodsList.Any())
            {
                var firstPeriod = periodsList.First();
                if (firstPeriod.Agp?.Student?.User != null)
                {
                    studentId = firstPeriod.Agp.StudentId;
                    studentName = $"{firstPeriod.Agp.Student.User.FirstName} {firstPeriod.Agp.Student.User.LastName}";
                }
            }

            var timelineView = new AgpTimelineViewDto
            {
                TimelineStart = timelineStart,
                TimelineEnd = timelineEnd,
                Today = today,
                Months = months,
                Periods = periodsList.Select(MapToResponseDto).ToList(),
                StudentId = studentId,
                StudentName = studentName,
                AgpId = agpId
            };

            return ApiResponse<AgpTimelineViewDto>.SuccessResponse(timelineView);
        }
        catch (Exception ex)
        {
            return ApiResponse<AgpTimelineViewDto>.ErrorResponse($"Timeline verisi getirilirken hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<AgpPeriodResponseDto>>> GetByStudentIdAsync(int studentId)
    {
        try
        {
            var periods = await _periodRepository.GetByStudentIdAsync(studentId);
            var responseDtos = periods.Select(MapToResponseDto).ToList();
            return ApiResponse<List<AgpPeriodResponseDto>>.SuccessResponse(responseDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<AgpPeriodResponseDto>>.ErrorResponse($"Dönemler getirilirken hata: {ex.Message}");
        }
    }

    private AgpPeriodResponseDto MapToResponseDto(AgpPeriod period)
    {
        var today = DateTime.Today;
        var totalDays = (int)(period.EndDate - period.StartDate).TotalDays;
        var elapsedDays = (int)(today - period.StartDate).TotalDays;
        elapsedDays = Math.Max(0, Math.Min(elapsedDays, totalDays));
        var progressPercentage = totalDays > 0 ? (double)elapsedDays / totalDays * 100 : 0;

        var studentId = 0;
        var studentName = "";
        if (period.Agp?.Student?.User != null)
        {
            studentId = period.Agp.StudentId;
            studentName = $"{period.Agp.Student.User.FirstName} {period.Agp.Student.User.LastName}";
        }

        return new AgpPeriodResponseDto
        {
            Id = period.Id,
            PeriodName = period.PeriodName,
            Title = period.Title,
            StartDate = period.StartDate,
            EndDate = period.EndDate,
            Color = period.Color,
            Order = period.Order,
            AgpId = period.AgpId,
            StudentId = studentId,
            StudentName = studentName,
            TotalDays = totalDays,
            ElapsedDays = elapsedDays,
            ProgressPercentage = progressPercentage,
            Milestones = period.Milestones
                .Where(m => !m.IsDeleted)
                .Select(m => new AgpMilestoneResponseDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    Date = m.Date,
                    Color = m.Color,
                    Type = m.Type,
                    IsMilestone = m.IsMilestone
                })
                .OrderBy(m => m.Date)
                .ToList(),
            Activities = period.Activities
                .Where(a => !a.IsDeleted)
                .Select(a => new AgpActivityResponseDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    HoursPerWeek = a.HoursPerWeek,
                    OwnerType = a.OwnerType,
                    Status = a.Status,
                    NeedsReview = a.NeedsReview,
                    Notes = a.Notes
                })
                .OrderBy(a => a.StartDate)
                .ToList()
        };
    }
}
