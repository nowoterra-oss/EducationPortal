using EduPortal.Application.Services.Implementations;
using EduPortal.Application.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EduPortal.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Add AutoMapper
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // Add Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<IHomeworkService, HomeworkService>();
        services.AddScoped<IInternalExamService, InternalExamService>();
        services.AddScoped<IAttendanceService, AttendanceService>();

        return services;
    }
}
