using EduPortal.Application.Common;
using EduPortal.Application.DTOs.CounselorDashboard;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class CounselorDashboardService : ICounselorDashboardService
{
    private readonly ApplicationDbContext _context;

    public CounselorDashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<StudentFullProfileDto>> GetStudentFullProfileAsync(int studentId)
    {
        try
        {
            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Parents)
                    .ThenInclude(sp => sp.Parent)
                        .ThenInclude(p => p.User)
                .Include(s => s.ReadinessExams)
                .Include(s => s.TeacherAssignments)
                    .ThenInclude(ta => ta.Teacher)
                        .ThenInclude(t => t.User)
                .Include(s => s.TeacherAssignments)
                    .ThenInclude(ta => ta.Course)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
            {
                return ApiResponse<StudentFullProfileDto>.ErrorResponse("Ogrenci bulunamadi");
            }

            var profile = new StudentFullProfileDto
            {
                Id = student.Id,
                StudentNo = student.StudentNo,
                FullName = $"{student.User.FirstName} {student.User.LastName}",
                Email = student.User.Email ?? "",
                PhoneNumber = student.User.PhoneNumber,
                ProfilePhotoUrl = student.ProfilePhotoUrl,
                CurrentGrade = student.CurrentGrade,
                SchoolName = student.SchoolName,
                DateOfBirth = student.DateOfBirth,
                LGSPercentile = student.LGSPercentile,
                TargetMajor = student.TargetMajor,
                TargetCountry = student.TargetCountry,
                IsBilsem = student.IsBilsem,
                BilsemField = student.BilsemField,
                LanguageLevel = student.LanguageLevel,
                Parents = student.Parents.Select(sp => new ParentInfoDto
                {
                    Id = sp.Parent.Id,
                    FullName = $"{sp.Parent.User.FirstName} {sp.Parent.User.LastName}",
                    Email = sp.Parent.User.Email,
                    PhoneNumber = sp.Parent.User.PhoneNumber,
                    Occupation = sp.Parent.Occupation,
                    Relationship = sp.Relationship
                }).ToList(),
                EntranceExams = student.ReadinessExams.Select(re => new ReadinessExamDto
                {
                    Id = re.Id,
                    ExamName = re.ExamName,
                    ExamDate = re.ExamDate ?? DateTime.MinValue,
                    Score = ParseScore(re.Score),
                    Subject = null,
                    MaxScore = null,
                    Percentage = null,
                    Analysis = re.Notes
                }).ToList(),
                AssignedTeachers = student.TeacherAssignments
                    .Where(ta => ta.IsActive)
                    .Select(ta => new AssignedTeacherDto
                    {
                        TeacherId = ta.TeacherId,
                        TeacherName = $"{ta.Teacher.User.FirstName} {ta.Teacher.User.LastName}",
                        Subject = ta.Course?.CourseName,
                        CourseName = ta.Course?.CourseName
                    }).ToList()
            };

            // Haftalik program
            var schedules = await _context.LessonSchedules
                .Include(ls => ls.Course)
                .Include(ls => ls.Teacher)
                    .ThenInclude(t => t.User)
                .Where(ls => ls.StudentId == studentId && ls.Status == LessonStatus.Scheduled)
                .ToListAsync();

            profile.WeeklySchedule = schedules.Select(ls => new WeeklyScheduleItemDto
            {
                DayOfWeek = (int)ls.DayOfWeek,
                DayName = GetDayName((int)ls.DayOfWeek),
                StartTime = ls.StartTime.ToString(@"hh\:mm"),
                EndTime = ls.EndTime.ToString(@"hh\:mm"),
                CourseName = ls.Course?.CourseName ?? "",
                TeacherName = $"{ls.Teacher.User.FirstName} {ls.Teacher.User.LastName}"
            }).OrderBy(s => s.DayOfWeek).ThenBy(s => s.StartTime).ToList();

            return ApiResponse<StudentFullProfileDto>.SuccessResponse(profile);
        }
        catch (Exception ex)
        {
            return ApiResponse<StudentFullProfileDto>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<StudentAcademicPerformanceDto>> GetStudentAcademicPerformanceAsync(int studentId)
    {
        try
        {
            var performance = new StudentAcademicPerformanceDto { StudentId = studentId };

            // Odev performansi
            var homeworkAssignments = await _context.HomeworkAssignments
                .Where(ha => ha.StudentId == studentId && !ha.IsDeleted)
                .ToListAsync();

            var totalAssigned = homeworkAssignments.Count;
            var completed = homeworkAssignments.Count(ha =>
                ha.Status == HomeworkAssignmentStatus.TeslimEdildi ||
                ha.Status == HomeworkAssignmentStatus.Degerlendirildi);
            var pending = homeworkAssignments.Count(ha =>
                ha.Status == HomeworkAssignmentStatus.Atandi ||
                ha.Status == HomeworkAssignmentStatus.Goruldu ||
                ha.Status == HomeworkAssignmentStatus.DevamEdiyor);
            var late = homeworkAssignments.Count(ha => ha.Status == HomeworkAssignmentStatus.Gecikti);
            var gradedSubmissions = homeworkAssignments.Where(ha => ha.Score.HasValue).ToList();

            performance.HomeworkPerformance = new HomeworkPerformanceDto
            {
                TotalAssigned = totalAssigned,
                Completed = completed,
                Pending = pending,
                Late = late,
                CompletionRate = totalAssigned > 0 ? (decimal)completed / totalAssigned * 100 : 0,
                AverageScore = gradedSubmissions.Any() ? (decimal)gradedSubmissions.Average(ha => ha.Score ?? 0) : 0
            };

            // Sinav sonuclari
            var examResults = await _context.ExamResults
                .Include(er => er.Exam)
                    .ThenInclude(e => e.Course)
                .Where(er => er.StudentId == studentId)
                .ToListAsync();

            performance.ExamResults = examResults
                .GroupBy(er => new { er.Exam.ExamType, Subject = er.Exam.Course?.CourseName ?? "Genel" })
                .Select(g => new ExamResultSummaryDto
                {
                    ExamType = g.Key.ExamType ?? "Genel",
                    Subject = g.Key.Subject,
                    ExamCount = g.Count(),
                    AverageScore = g.Average(er => er.Score),
                    ScoreHistory = g.Select(er => new ExamScoreDataPoint
                    {
                        ExamName = er.Exam.Title,
                        ExamDate = er.Exam.ExamDate,
                        Score = er.Score,
                        MaxScore = er.Exam.MaxScore
                    }).OrderBy(s => s.ExamDate).ToList()
                }).ToList();

            // Hazir bulunusluk sinavlari
            var readinessExams = await _context.StudentReadinessExams
                .Where(re => re.StudentId == studentId && !re.IsDeleted)
                .ToListAsync();

            performance.ReadinessExams = readinessExams.Select(re => new ReadinessExamDto
            {
                Id = re.Id,
                ExamName = re.ExamName,
                ExamDate = re.ExamDate ?? DateTime.MinValue,
                Score = ParseScore(re.Score),
                Subject = null,
                MaxScore = null,
                Percentage = null,
                Analysis = re.Notes
            }).ToList();

            // AGP Ozeti
            var agp = await _context.AcademicDevelopmentPlans
                .Include(a => a.Periods)
                    .ThenInclude(p => p.Milestones)
                .Include(a => a.Periods)
                    .ThenInclude(p => p.Activities)
                .Where(a => a.StudentId == studentId && !a.IsDeleted)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync();

            if (agp != null)
            {
                var allMilestones = agp.Periods.SelectMany(p => p.Milestones).ToList();
                // AgpTimelineMilestone doesn't have IsCompleted, so we use a different approach
                // We can track completion based on milestone date being in the past
                var completedMilestones = allMilestones.Count(m => m.Date < DateTime.UtcNow);

                performance.AgpSummary = new AgpSummaryDto
                {
                    AgpId = agp.Id,
                    AcademicYear = agp.AcademicYear ?? "",
                    TotalMilestones = allMilestones.Count,
                    CompletedMilestones = completedMilestones,
                    OverallProgress = allMilestones.Count > 0 ? (decimal)completedMilestones / allMilestones.Count * 100 : 0,
                    Periods = agp.Periods.Select(p => new AgpPeriodSummaryDto
                    {
                        Title = p.Title,
                        StartDate = p.StartDate,
                        EndDate = p.EndDate,
                        Color = p.Color,
                        Milestones = p.Milestones.Select(m => m.Title).ToList(),
                        Activities = p.Activities.Select(a => a.Title).ToList()
                    }).ToList()
                };
            }

            return ApiResponse<StudentAcademicPerformanceDto>.SuccessResponse(performance);
        }
        catch (Exception ex)
        {
            return ApiResponse<StudentAcademicPerformanceDto>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<StudentInternationalEducationDto>> GetStudentInternationalEducationAsync(int studentId)
    {
        try
        {
            var result = new StudentInternationalEducationDto { StudentId = studentId };

            // Yurtdisi sinavlar
            var internationalExams = await _context.InternationalExams
                .Where(ie => ie.StudentId == studentId && !ie.IsDeleted)
                .ToListAsync();

            result.InternationalExams = internationalExams.Select(ie => new InternationalExamDto
            {
                Id = ie.Id,
                ExamName = ie.ExamName,
                ExamType = ie.ExamType.ToString(),
                RegistrationDeadline = ie.ApplicationEndDate,
                ExamDate = ie.ExamDate,
                Score = ParseScore(ie.Score),
                MaxScore = ParseScore(ie.MaxScore),
                Status = null // InternationalExam entity doesn't have Status
            }).ToList();

            // Sinav takvimi
            var examCalendar = await _context.StudentExamCalendars
                .Where(sec => sec.StudentId == studentId && !sec.IsDeleted)
                .ToListAsync();

            result.ExamCalendar = examCalendar.Select(ec => new ExamCalendarItemDto
            {
                Id = ec.Id,
                ExamName = ec.ExamName,
                ExamType = ec.ExamType,
                RegistrationDeadline = ec.RegistrationDeadline,
                ExamDate = ec.ExamDate,
                Status = ec.Status,
                DaysUntilExam = ec.ExamDate.HasValue ? (int)(ec.ExamDate.Value - DateTime.UtcNow).TotalDays : 0,
                DaysUntilDeadline = ec.RegistrationDeadline.HasValue ? (int)(ec.RegistrationDeadline.Value - DateTime.UtcNow).TotalDays : 0,
                IsUrgent = ec.ExamDate.HasValue && (ec.ExamDate.Value - DateTime.UtcNow).TotalDays <= 7
            }).ToList();

            return ApiResponse<StudentInternationalEducationDto>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<StudentInternationalEducationDto>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<StudentActivitiesAndAwardsDto>> GetStudentActivitiesAndAwardsAsync(int studentId)
    {
        try
        {
            var result = new StudentActivitiesAndAwardsDto { StudentId = studentId };

            // Aktiviteler
            var activities = await _context.StudentActivities
                .Where(a => a.StudentId == studentId && !a.IsDeleted)
                .ToListAsync();

            result.Activities = activities.Select(a => new ActivityDto
            {
                Id = a.Id,
                Name = a.Name,
                Type = a.Type ?? "",
                Level = null,
                Description = a.Description,
                IsActive = a.IsOngoing
            }).ToList();

            // Yaz aktiviteleri
            var summerActivities = await _context.StudentSummerActivities
                .Where(sa => sa.StudentId == studentId && !sa.IsDeleted)
                .ToListAsync();

            result.SummerActivities = summerActivities.Select(sa => new SummerActivityDto
            {
                Id = sa.Id,
                ActivityName = sa.ActivityName,
                ActivityType = sa.ActivityType,
                Organization = sa.OrganizingInstitution,
                Year = sa.StartDate.Year.ToString(),
                StartDate = sa.StartDate,
                EndDate = sa.EndDate,
                Description = sa.Description
            }).ToList();

            // Stajlar
            var internships = await _context.SimpleInternships
                .Where(i => i.StudentId == studentId && !i.IsDeleted)
                .ToListAsync();

            result.Internships = internships.Select(i => new InternshipDto
            {
                Id = i.Id,
                CompanyName = i.CompanyName,
                Position = i.Position,
                StartDate = i.StartDate,
                EndDate = i.EndDate,
                Description = i.Description
            }).ToList();

            // Yarisma sonuclari
            var competitions = await _context.CompetitionsAndAwards
                .Where(c => c.StudentId == studentId && !c.IsDeleted)
                .ToListAsync();

            result.Competitions = competitions.Select(c => new CompetitionDto
            {
                Id = c.Id,
                CompetitionName = c.Name,
                Category = c.Category,
                Result = c.Achievement,
                Date = c.Date,
                Description = c.Description
            }).ToList();

            // Sosyal projeler
            var socialProjects = await _context.StudentSocialProjects
                .Where(sp => sp.StudentId == studentId && !sp.IsDeleted)
                .ToListAsync();

            result.SocialProjects = socialProjects.Select(sp => new SocialProjectDto
            {
                Id = sp.Id,
                ProjectName = sp.ProjectName,
                Role = sp.Role,
                StartDate = sp.StartDate,
                EndDate = sp.EndDate,
                Description = sp.Description,
                Impact = sp.Outcomes
            }).ToList();

            // Kulup uyelikleri
            var clubMemberships = await _context.StudentClubMemberships
                .Where(cm => cm.StudentId == studentId && !cm.IsDeleted)
                .ToListAsync();

            result.ClubMemberships = clubMemberships.Select(cm => new ClubMembershipDto
            {
                Id = cm.Id,
                ClubName = cm.ClubName,
                Role = cm.Role,
                JoinDate = cm.StartDate,
                IsActive = !cm.EndDate.HasValue || cm.EndDate > DateTime.UtcNow
            }).ToList();

            // Oduller
            var awards = await _context.StudentAwards
                .Where(sa => sa.StudentId == studentId && !sa.IsDeleted)
                .ToListAsync();

            result.Awards = new AwardsSummaryDto
            {
                International = awards.Where(a => a.Scope == "International").Select(MapAward).ToList(),
                National = awards.Where(a => a.Scope == "National").Select(MapAward).ToList(),
                Local = awards.Where(a => a.Scope == "Local").Select(MapAward).ToList(),
                TotalCount = awards.Count
            };

            // Characterix
            var characterix = await _context.CharacterixResults
                .Where(cr => cr.StudentId == studentId && !cr.IsDeleted)
                .OrderByDescending(cr => cr.AssessmentDate)
                .FirstOrDefaultAsync();

            if (characterix != null)
            {
                result.Characterix = new CharacterixSummaryDto
                {
                    Id = characterix.Id,
                    AssessmentDate = characterix.AssessmentDate,
                    Analysis = characterix.Analysis,
                    Interpretation = characterix.Interpretation,
                    Recommendations = characterix.Recommendations,
                    ReportUrl = characterix.ReportUrl
                };
            }

            return ApiResponse<StudentActivitiesAndAwardsDto>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<StudentActivitiesAndAwardsDto>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<StudentUniversityTrackingDto>> GetStudentUniversityTrackingAsync(int studentId)
    {
        try
        {
            var result = new StudentUniversityTrackingDto { StudentId = studentId };

            var applications = await _context.UniversityApplications
                .Where(ua => ua.StudentId == studentId && !ua.IsDeleted)
                .ToListAsync();

            var applicationIds = applications.Select(a => a.Id).ToList();
            var requirements = await _context.UniversityRequirements
                .Where(ur => applicationIds.Contains(ur.UniversityApplicationId) && !ur.IsDeleted)
                .ToListAsync();

            result.Applications = applications.Select(ua =>
            {
                var appRequirements = requirements.Where(r => r.UniversityApplicationId == ua.Id).ToList();
                var completedCount = appRequirements.Count(r => r.IsCompleted);
                var totalCount = appRequirements.Count;

                return new UniversityApplicationDto
                {
                    Id = ua.Id,
                    UniversityName = ua.UniversityName,
                    Country = ua.Country,
                    Department = ua.Department,
                    Status = ua.Status.ToString(),
                    ApplicationDeadline = ua.ApplicationDeadline,
                    ApplicationDate = null, // UniversityApplication doesn't have SubmittedDate
                    ResultDate = ua.DecisionDate,
                    Result = null, // UniversityApplication doesn't have Result, Status covers it
                    Requirements = appRequirements.Select(r => new RequirementDto
                    {
                        Id = r.Id,
                        RequirementName = r.RequirementName,
                        RequirementType = r.RequirementType,
                        IsCompleted = r.IsCompleted,
                        Deadline = r.Deadline,
                        CompletedDate = r.CompletedDate,
                        Notes = r.Notes,
                        IsOverdue = r.Deadline.HasValue && !r.IsCompleted && r.Deadline.Value < DateTime.UtcNow,
                        DaysUntilDeadline = r.Deadline.HasValue ? (int)(r.Deadline.Value - DateTime.UtcNow).TotalDays : 0
                    }).ToList(),
                    CompletedRequirements = completedCount,
                    TotalRequirements = totalCount,
                    CompletionPercentage = totalCount > 0 ? (decimal)completedCount / totalCount * 100 : 0
                };
            }).ToList();

            // Ozet istatistikler
            result.Summary = new UniversitySummaryDto
            {
                TotalApplications = applications.Count,
                PendingApplications = applications.Count(a =>
                    a.Status == ApplicationStatus.Planlaniyor ||
                    a.Status == ApplicationStatus.BasvuruYapildi ||
                    a.Status == ApplicationStatus.Beklemede),
                AcceptedApplications = applications.Count(a => a.Status == ApplicationStatus.Kabul),
                RejectedApplications = applications.Count(a => a.Status == ApplicationStatus.Red),
                UpcomingDeadlines = applications
                    .Where(a => a.ApplicationDeadline > DateTime.UtcNow)
                    .OrderBy(a => a.ApplicationDeadline)
                    .Take(5)
                    .Select(a => new UpcomingDeadlineDto
                    {
                        DeadlineType = "UniversityApplication",
                        Title = a.UniversityName,
                        Deadline = a.ApplicationDeadline,
                        DaysRemaining = (int)(a.ApplicationDeadline - DateTime.UtcNow).TotalDays,
                        IsUrgent = (a.ApplicationDeadline - DateTime.UtcNow).TotalDays <= 7,
                        RelatedEntityType = "UniversityApplication",
                        RelatedEntityId = a.Id
                    }).ToList()
            };

            return ApiResponse<StudentUniversityTrackingDto>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<StudentUniversityTrackingDto>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<ExamCalendarItemDto>>> GetStudentExamCalendarAsync(int studentId)
    {
        try
        {
            var examCalendar = await _context.StudentExamCalendars
                .Where(sec => sec.StudentId == studentId && !sec.IsDeleted)
                .OrderBy(sec => sec.ExamDate)
                .ToListAsync();

            var result = examCalendar.Select(ec => new ExamCalendarItemDto
            {
                Id = ec.Id,
                ExamName = ec.ExamName,
                ExamType = ec.ExamType,
                RegistrationDeadline = ec.RegistrationDeadline,
                ExamDate = ec.ExamDate,
                Status = ec.Status,
                DaysUntilExam = ec.ExamDate.HasValue ? (int)(ec.ExamDate.Value - DateTime.UtcNow).TotalDays : 0,
                DaysUntilDeadline = ec.RegistrationDeadline.HasValue ? (int)(ec.RegistrationDeadline.Value - DateTime.UtcNow).TotalDays : 0,
                IsUrgent = ec.ExamDate.HasValue && (ec.ExamDate.Value - DateTime.UtcNow).TotalDays <= 7
            }).ToList();

            return ApiResponse<List<ExamCalendarItemDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ExamCalendarItemDto>>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<CounselorNoteDto>>> GetStudentCounselorNotesAsync(int studentId)
    {
        try
        {
            var notes = await _context.CounselorNotes
                .Include(cn => cn.Student)
                    .ThenInclude(s => s.User)
                .Include(cn => cn.Counselor)
                    .ThenInclude(c => c.User)
                .Where(cn => cn.StudentId == studentId && !cn.IsDeleted)
                .OrderByDescending(cn => cn.NoteDate)
                .ToListAsync();

            var result = notes.Select(cn => new CounselorNoteDto
            {
                Id = cn.Id,
                StudentId = cn.StudentId,
                StudentName = $"{cn.Student.User.FirstName} {cn.Student.User.LastName}",
                CounselorId = cn.CounselorId,
                CounselorName = $"{cn.Counselor.User.FirstName} {cn.Counselor.User.LastName}",
                CounselingMeetingId = cn.CounselingMeetingId,
                NoteDate = cn.NoteDate,
                Subject = cn.Subject,
                NoteContent = cn.NoteContent,
                AssignedTasks = cn.AssignedTasks,
                NextMeetingDate = cn.NextMeetingDate,
                SendEmailToParent = cn.SendEmailToParent,
                SendSmsToParent = cn.SendSmsToParent,
                EmailSent = cn.EmailSent,
                SmsSent = cn.SmsSent,
                EmailSentAt = cn.EmailSentAt,
                SmsSentAt = cn.SmsSentAt,
                CreatedAt = cn.CreatedAt,
                UpdatedAt = cn.UpdatedAt
            }).ToList();

            return ApiResponse<List<CounselorNoteDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<CounselorNoteDto>>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<CounselorNoteDto>> CreateCounselorNoteAsync(int counselorId, CreateCounselorNoteDto dto)
    {
        try
        {
            var note = new CounselorNote
            {
                StudentId = dto.StudentId,
                CounselorId = counselorId,
                CounselingMeetingId = dto.CounselingMeetingId,
                NoteDate = dto.NoteDate,
                Subject = dto.Subject,
                NoteContent = dto.NoteContent,
                AssignedTasks = dto.AssignedTasks,
                NextMeetingDate = dto.NextMeetingDate,
                SendEmailToParent = dto.SendEmailToParent,
                SendSmsToParent = dto.SendSmsToParent,
                CreatedAt = DateTime.UtcNow
            };

            _context.CounselorNotes.Add(note);
            await _context.SaveChangesAsync();

            // TODO: Email/SMS gonderimi burada yapilabilir

            return await GetCounselorNoteByIdAsync(note.Id);
        }
        catch (Exception ex)
        {
            return ApiResponse<CounselorNoteDto>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<CounselorNoteDto>> UpdateCounselorNoteAsync(int counselorId, UpdateCounselorNoteDto dto)
    {
        try
        {
            var note = await _context.CounselorNotes
                .FirstOrDefaultAsync(cn => cn.Id == dto.Id && !cn.IsDeleted);

            if (note == null)
            {
                return ApiResponse<CounselorNoteDto>.ErrorResponse("Not bulunamadi");
            }

            note.Subject = dto.Subject;
            note.NoteContent = dto.NoteContent;
            note.AssignedTasks = dto.AssignedTasks;
            note.NextMeetingDate = dto.NextMeetingDate;
            note.SendEmailToParent = dto.SendEmailToParent;
            note.SendSmsToParent = dto.SendSmsToParent;
            note.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetCounselorNoteByIdAsync(note.Id);
        }
        catch (Exception ex)
        {
            return ApiResponse<CounselorNoteDto>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteCounselorNoteAsync(int noteId)
    {
        try
        {
            var note = await _context.CounselorNotes
                .FirstOrDefaultAsync(cn => cn.Id == noteId && !cn.IsDeleted);

            if (note == null)
            {
                return ApiResponse<bool>.ErrorResponse("Not bulunamadi");
            }

            note.IsDeleted = true;
            note.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Not silindi");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<UpcomingDeadlineDto>>> GetUpcomingDeadlinesAsync(int studentId)
    {
        try
        {
            var deadlines = new List<UpcomingDeadlineDto>();
            var now = DateTime.UtcNow;
            var sevenDaysFromNow = now.AddDays(7);

            // Universite basvuru deadline'lari
            var universityDeadlines = await _context.UniversityApplications
                .Where(ua => ua.StudentId == studentId &&
                            !ua.IsDeleted &&
                            ua.ApplicationDeadline > now &&
                            ua.ApplicationDeadline <= sevenDaysFromNow)
                .Select(ua => new UpcomingDeadlineDto
                {
                    DeadlineType = "UniversityApplication",
                    Title = ua.UniversityName,
                    Deadline = ua.ApplicationDeadline,
                    DaysRemaining = (int)(ua.ApplicationDeadline - now).TotalDays,
                    IsUrgent = true,
                    RelatedEntityType = "UniversityApplication",
                    RelatedEntityId = ua.Id
                })
                .ToListAsync();

            deadlines.AddRange(universityDeadlines);

            // Sinav kayit deadline'lari
            var examDeadlines = await _context.StudentExamCalendars
                .Where(sec => sec.StudentId == studentId &&
                             !sec.IsDeleted &&
                             sec.RegistrationDeadline.HasValue &&
                             sec.RegistrationDeadline.Value > now &&
                             sec.RegistrationDeadline.Value <= sevenDaysFromNow)
                .Select(sec => new UpcomingDeadlineDto
                {
                    DeadlineType = "ExamRegistration",
                    Title = $"{sec.ExamName} - Kayit",
                    Deadline = sec.RegistrationDeadline!.Value,
                    DaysRemaining = (int)(sec.RegistrationDeadline.Value - now).TotalDays,
                    IsUrgent = true,
                    RelatedEntityType = "StudentExamCalendar",
                    RelatedEntityId = sec.Id
                })
                .ToListAsync();

            deadlines.AddRange(examDeadlines);

            // Sinav tarihleri
            var upcomingExams = await _context.StudentExamCalendars
                .Where(sec => sec.StudentId == studentId &&
                             !sec.IsDeleted &&
                             sec.ExamDate.HasValue &&
                             sec.ExamDate.Value > now &&
                             sec.ExamDate.Value <= sevenDaysFromNow)
                .Select(sec => new UpcomingDeadlineDto
                {
                    DeadlineType = "Exam",
                    Title = sec.ExamName,
                    Deadline = sec.ExamDate!.Value,
                    DaysRemaining = (int)(sec.ExamDate.Value - now).TotalDays,
                    IsUrgent = true,
                    RelatedEntityType = "StudentExamCalendar",
                    RelatedEntityId = sec.Id
                })
                .ToListAsync();

            deadlines.AddRange(upcomingExams);

            return ApiResponse<List<UpcomingDeadlineDto>>.SuccessResponse(
                deadlines.OrderBy(d => d.Deadline).ToList());
        }
        catch (Exception ex)
        {
            return ApiResponse<List<UpcomingDeadlineDto>>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<CounselorDashboardSummaryDto>> GetDashboardSummaryAsync(int studentId)
    {
        try
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
            {
                return ApiResponse<CounselorDashboardSummaryDto>.ErrorResponse("Ogrenci bulunamadi");
            }

            var summary = new CounselorDashboardSummaryDto
            {
                StudentId = studentId,
                StudentName = $"{student.User.FirstName} {student.User.LastName}"
            };

            // Son gorusme
            var lastMeeting = await _context.CounselingMeetings
                .Where(cm => cm.StudentId == studentId && !cm.IsDeleted)
                .OrderByDescending(cm => cm.MeetingDate)
                .FirstOrDefaultAsync();

            if (lastMeeting != null)
            {
                summary.LastMeetingDate = lastMeeting.MeetingDate;
                summary.LastMeetingNotes = lastMeeting.Notes;
            }

            // Sonraki gorusme
            var nextMeeting = await _context.CounselingMeetings
                .Where(cm => cm.StudentId == studentId &&
                            !cm.IsDeleted &&
                            cm.MeetingDate > DateTime.UtcNow)
                .OrderBy(cm => cm.MeetingDate)
                .FirstOrDefaultAsync();

            summary.NextMeetingDate = nextMeeting?.MeetingDate;

            // Odev durumu
            var homeworkAssignments = await _context.HomeworkAssignments
                .Include(ha => ha.Homework)
                .Where(ha => ha.StudentId == studentId && !ha.IsDeleted)
                .ToListAsync();

            summary.PendingHomeworks = homeworkAssignments.Count(ha =>
                ha.Status == HomeworkAssignmentStatus.Atandi ||
                ha.Status == HomeworkAssignmentStatus.Goruldu ||
                ha.Status == HomeworkAssignmentStatus.DevamEdiyor);
            summary.OverdueHomeworks = homeworkAssignments.Count(ha =>
                ha.Status == HomeworkAssignmentStatus.Gecikti ||
                (ha.Status == HomeworkAssignmentStatus.Atandi &&
                 ha.DueDate < DateTime.UtcNow));

            // AGP durumu
            var agp = await _context.AcademicDevelopmentPlans
                .Include(a => a.Periods)
                    .ThenInclude(p => p.Milestones)
                .Where(a => a.StudentId == studentId && !a.IsDeleted)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync();

            if (agp != null)
            {
                var allMilestones = agp.Periods.SelectMany(p => p.Milestones).ToList();
                var completedMilestones = allMilestones.Count(m => m.Date < DateTime.UtcNow);
                summary.AgpProgress = allMilestones.Count > 0
                    ? (decimal)completedMilestones / allMilestones.Count * 100
                    : 0;
            }

            // Basvuru durumu
            summary.PendingApplications = await _context.UniversityApplications
                .CountAsync(ua => ua.StudentId == studentId &&
                                 !ua.IsDeleted &&
                                 (ua.Status == ApplicationStatus.Planlaniyor ||
                                  ua.Status == ApplicationStatus.BasvuruYapildi ||
                                  ua.Status == ApplicationStatus.Beklemede));

            // Yaklasan sinavlar
            summary.UpcomingExams = await _context.StudentExamCalendars
                .CountAsync(sec => sec.StudentId == studentId &&
                                  !sec.IsDeleted &&
                                  sec.ExamDate.HasValue &&
                                  sec.ExamDate.Value > DateTime.UtcNow &&
                                  sec.ExamDate.Value <= DateTime.UtcNow.AddDays(30));

            // Yaklasan deadline'lar
            var deadlinesResponse = await GetUpcomingDeadlinesAsync(studentId);
            if (deadlinesResponse.Success)
            {
                summary.UpcomingDeadlines = deadlinesResponse.Data ?? new List<UpcomingDeadlineDto>();
            }

            return ApiResponse<CounselorDashboardSummaryDto>.SuccessResponse(summary);
        }
        catch (Exception ex)
        {
            return ApiResponse<CounselorDashboardSummaryDto>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<CounselorDashboardSummaryDto>>> GetCounselorStudentsSummaryAsync(int counselorId)
    {
        try
        {
            var assignments = await _context.StudentCounselorAssignments
                .Where(sca => sca.CounselorId == counselorId &&
                             sca.IsActive &&
                             !sca.IsDeleted)
                .Select(sca => sca.StudentId)
                .ToListAsync();

            var summaries = new List<CounselorDashboardSummaryDto>();

            foreach (var studentId in assignments)
            {
                var summaryResponse = await GetDashboardSummaryAsync(studentId);
                if (summaryResponse.Success && summaryResponse.Data != null)
                {
                    summaries.Add(summaryResponse.Data);
                }
            }

            return ApiResponse<List<CounselorDashboardSummaryDto>>.SuccessResponse(summaries);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<CounselorDashboardSummaryDto>>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    #region Helper Methods

    private async Task<ApiResponse<CounselorNoteDto>> GetCounselorNoteByIdAsync(int noteId)
    {
        var note = await _context.CounselorNotes
            .Include(cn => cn.Student)
                .ThenInclude(s => s.User)
            .Include(cn => cn.Counselor)
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(cn => cn.Id == noteId && !cn.IsDeleted);

        if (note == null)
        {
            return ApiResponse<CounselorNoteDto>.ErrorResponse("Not bulunamadi");
        }

        return ApiResponse<CounselorNoteDto>.SuccessResponse(new CounselorNoteDto
        {
            Id = note.Id,
            StudentId = note.StudentId,
            StudentName = $"{note.Student.User.FirstName} {note.Student.User.LastName}",
            CounselorId = note.CounselorId,
            CounselorName = $"{note.Counselor.User.FirstName} {note.Counselor.User.LastName}",
            CounselingMeetingId = note.CounselingMeetingId,
            NoteDate = note.NoteDate,
            Subject = note.Subject,
            NoteContent = note.NoteContent,
            AssignedTasks = note.AssignedTasks,
            NextMeetingDate = note.NextMeetingDate,
            SendEmailToParent = note.SendEmailToParent,
            SendSmsToParent = note.SendSmsToParent,
            EmailSent = note.EmailSent,
            SmsSent = note.SmsSent,
            EmailSentAt = note.EmailSentAt,
            SmsSentAt = note.SmsSentAt,
            CreatedAt = note.CreatedAt,
            UpdatedAt = note.UpdatedAt
        });
    }

    private static string GetDayName(int dayOfWeek)
    {
        return dayOfWeek switch
        {
            0 => "Pazar",
            1 => "Pazartesi",
            2 => "Sali",
            3 => "Carsamba",
            4 => "Persembe",
            5 => "Cuma",
            6 => "Cumartesi",
            _ => ""
        };
    }

    private static AwardDto MapAward(StudentAward a)
    {
        return new AwardDto
        {
            Id = a.Id,
            AwardName = a.AwardName,
            Category = a.Category,
            Rank = a.Rank,
            IssuingOrganization = a.IssuingOrganization,
            AwardDate = a.AwardDate
        };
    }

    private static decimal? ParseScore(string? score)
    {
        if (string.IsNullOrWhiteSpace(score))
            return null;

        if (decimal.TryParse(score, out var result))
            return result;

        return null;
    }

    #endregion
}
