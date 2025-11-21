using EduPortal.Application.Interfaces;
using EduPortal.Application.Services;
using EduPortal.Infrastructure.Repositories;
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

        // Register Application Services
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<ICoachService, CoachService>();

        return services;
    }
}
