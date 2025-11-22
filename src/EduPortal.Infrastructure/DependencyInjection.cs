using EduPortal.Application.Interfaces;
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
        services.AddScoped<IHomeworkRepository, HomeworkRepository>();
        services.AddScoped<IAttendanceRepository, AttendanceRepository>();

        // Register Infrastructure Services
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<ICoachService, CoachService>();

        // Register Application Services (using DbContext)
        services.AddScoped<ICoachingSessionService, CoachingSessionService>();
        services.AddScoped<IStudyAbroadService, StudyAbroadService>();
        services.AddScoped<IApplicationDocumentService, ApplicationDocumentService>();
        services.AddScoped<IVisaProcessService, VisaProcessService>();
        services.AddScoped<IAccommodationArrangementService, AccommodationArrangementService>();
        services.AddScoped<ICareerAssessmentService, CareerAssessmentService>();
        services.AddScoped<ISchoolRecommendationService, SchoolRecommendationService>();
        services.AddScoped<ISportsAssessmentService, SportsAssessmentService>();

        return services;
    }
}
