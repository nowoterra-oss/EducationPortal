using EduPortal.Application.Interfaces;
using EduPortal.Application.Services.Interfaces;
using EduPortal.Infrastructure.Repositories;
using EduPortal.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EduPortal.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Register Repositories
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<ITeacherRepository, TeacherRepository>();
        services.AddScoped<IHomeworkRepository, HomeworkRepository>();
        services.AddScoped<IInternalExamRepository, InternalExamRepository>();
        services.AddScoped<IAttendanceRepository, AttendanceRepository>();
        services.AddScoped<IAgpPeriodRepository, AgpPeriodRepository>();

        // Register Infrastructure Services
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IParentService, ParentService>();
        services.AddScoped<ICounselorService, CounselorService>();
        services.AddScoped<IClassService, ClassService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ICalendarService, CalendarService>();
        services.AddScoped<IWeeklyScheduleService, WeeklyScheduleService>();
        services.AddScoped<IAGPService, AGPService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IUniversityApplicationService, UniversityApplicationService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IStudentClassAssignmentService, StudentClassAssignmentService>();
        services.AddScoped<IAnnouncementService, AnnouncementService>();
        services.AddScoped<IAcademicTermService, AcademicTermService>();
        services.AddScoped<IClassroomService, ClassroomService>();
        services.AddScoped<IClubService, ClubService>();
        services.AddScoped<IHobbyService, HobbyService>();
        services.AddScoped<ICompetitionService, CompetitionService>();
        services.AddScoped<IScheduleService, ScheduleService>();

        // Register Application Services (using DbContext)
        services.AddScoped<IStudyAbroadService, StudyAbroadService>();
        services.AddScoped<IApplicationDocumentService, ApplicationDocumentService>();
        services.AddScoped<IVisaProcessService, VisaProcessService>();
        services.AddScoped<IAccommodationArrangementService, AccommodationArrangementService>();
        services.AddScoped<ICareerAssessmentService, CareerAssessmentService>();
        services.AddScoped<ISchoolRecommendationService, SchoolRecommendationService>();
        services.AddScoped<ISportsAssessmentService, SportsAssessmentService>();
        services.AddScoped<IServicePackageService, ServicePackageService>();
        services.AddScoped<IStudentPackagePurchaseService, StudentPackagePurchaseService>();
        services.AddScoped<IStudentTeacherAssignmentService, StudentTeacherAssignmentService>();

        // Payment Plan Services
        services.AddScoped<IPaymentPlanService, PaymentPlanService>();
        services.AddScoped<IStudentPaymentPlanService, StudentPaymentPlanService>();
        services.AddScoped<IPaymentInstallmentService, PaymentInstallmentService>();
        services.AddScoped<IPaymentService, PaymentService>();

        // Scheduling Services
        services.AddScoped<ISchedulingService, SchedulingService>();

        // Core Domain Services (moved from Application layer)
        services.AddScoped<IHomeworkService, HomeworkService>();
        services.AddScoped<IHomeworkAssignmentService, HomeworkAssignmentService>();
        services.AddScoped<IHomeworkDraftService, HomeworkDraftService>();
        services.AddScoped<IInternalExamService, InternalExamService>();
        services.AddScoped<IInternationalExamService, InternationalExamService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IAuditService, AuditService>();

        // File Storage Service
        services.AddScoped<IFileStorageService, FileStorageService>();

        // Student Activity Services
        services.AddScoped<IStudentActivityService, StudentActivityService>();

        // Student Group Services
        services.AddScoped<IStudentGroupService, StudentGroupService>();

        // Admin Calendar Service
        services.AddScoped<IAdminCalendarService, AdminCalendarService>();

        // Student Certificate Service
        services.AddScoped<IStudentCertificateService, StudentCertificateService>();

        // Course Resource Service
        services.AddScoped<ICourseResourceService, CourseResourceService>();

        // Student Extended Info Service (Foreign Languages, Hobbies, Activities, Readiness Exams)
        services.AddScoped<IStudentExtendedInfoService, StudentExtendedInfoService>();

        // Permission Service
        services.AddScoped<IPermissionService, PermissionService>();

        // Advisor Access Service (Danışman erişim kontrolü)
        services.AddScoped<IAdvisorAccessService, AdvisorAccessService>();

        // Counselor Dashboard Service
        services.AddScoped<ICounselorDashboardService, CounselorDashboardService>();

        // AGP Period Service
        services.AddScoped<IAgpPeriodService, Application.Services.Implementations.AgpPeriodService>();

        // Curriculum Progress Service
        services.AddScoped<ICurriculumProgressService, CurriculumProgressService>();

        return services;
    }
}
