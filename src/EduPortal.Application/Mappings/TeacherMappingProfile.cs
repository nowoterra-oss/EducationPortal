using AutoMapper;
using EduPortal.Application.DTOs.Teacher;
using EduPortal.Domain.Entities;

namespace EduPortal.Application.Mappings;

public class TeacherMappingProfile : Profile
{
    public TeacherMappingProfile()
    {
        CreateMap<Teacher, TeacherDto>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : null));

        CreateMap<TeacherCreateDto, Teacher>()
            .ForMember(dest => dest.User, opt => opt.Ignore()) // User will be created separately
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

        CreateMap<TeacherUpdateDto, Teacher>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}
