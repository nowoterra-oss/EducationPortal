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

        return services;
    }
}
