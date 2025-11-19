using AutoMapper;
using EduPortal.Application.DTOs.Attendance;
using EduPortal.Domain.Entities;

namespace EduPortal.Application.Mappings;

public class AttendanceMappingProfile : Profile
{
    public AttendanceMappingProfile()
    {
        CreateMap<Attendance, AttendanceDto>()
            .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.User.FirstName + " " + src.Student.User.LastName))
            .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.CourseName));

        CreateMap<AttendanceCreateDto, Attendance>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
    }
}
