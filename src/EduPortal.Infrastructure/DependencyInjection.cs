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

        // Register Infrastructure Services
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<ICoachService, CoachService>();
        services.AddScoped<IParentService, ParentService>();
        services.AddScoped<ICounselorService, CounselorService>();

        // Register Application Services (using DbContext)
        services.AddScoped<ICoachingSessionService, CoachingSessionService>();
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
        services.AddScoped<IInternalExamService, InternalExamService>();
        services.AddScoped<IInternationalExamService, InternationalExamService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IAuditService, AuditService>();

        return services;
    }
}
