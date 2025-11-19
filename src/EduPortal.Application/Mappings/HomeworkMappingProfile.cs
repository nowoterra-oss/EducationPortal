using AutoMapper;
using EduPortal.Application.DTOs.Homework;
using EduPortal.Domain.Entities;

namespace EduPortal.Application.Mappings;

public class HomeworkMappingProfile : Profile
{
    public HomeworkMappingProfile()
    {
        CreateMap<Homework, HomeworkDto>()
            .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.CourseName))
            .ForMember(dest => dest.TotalSubmissions, opt => opt.MapFrom(src => src.Submissions.Count));

        CreateMap<HomeworkCreateDto, Homework>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<StudentHomeworkSubmission, HomeworkSubmissionDto>()
            .ForMember(dest => dest.HomeworkTitle, opt => opt.MapFrom(src => src.Homework.Title))
            .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.User.FirstName + " " + src.Student.User.LastName));

        CreateMap<HomeworkSubmitDto, StudentHomeworkSubmission>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
    }
}
