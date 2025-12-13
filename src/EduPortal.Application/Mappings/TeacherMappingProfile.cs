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
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.BranchName : null))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.Branches, opt => opt.MapFrom(src => src.TeacherBranches))
            .ForMember(dest => dest.Certificates, opt => opt.MapFrom(src => src.TeacherCertificates))
            .ForMember(dest => dest.References, opt => opt.MapFrom(src => src.TeacherReferences))
            .ForMember(dest => dest.WorkTypes, opt => opt.MapFrom(src => src.TeacherWorkTypes.Select(wt => (int)wt.WorkType).ToList()));

        CreateMap<TeacherCreateDto, Teacher>()
            .ForMember(dest => dest.User, opt => opt.Ignore()) // User will be created separately
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

        CreateMap<TeacherUpdateDto, Teacher>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // New entity mappings for extended teacher form
        CreateMap<TeacherAddress, TeacherAddressDto>();
        CreateMap<TeacherAddressDto, TeacherAddress>();

        CreateMap<TeacherBranch, TeacherBranchDto>()
            .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course != null ? src.Course.CourseName : null))
            .ForMember(dest => dest.CourseCode, opt => opt.MapFrom(src => src.Course != null ? src.Course.CourseCode : null));
        CreateMap<TeacherBranchDto, TeacherBranch>();

        CreateMap<TeacherCertificate, TeacherCertificateDto>();
        CreateMap<TeacherCertificateDto, TeacherCertificate>();

        CreateMap<TeacherReference, TeacherReferenceDto>();
        CreateMap<TeacherReferenceDto, TeacherReference>();
    }
}
