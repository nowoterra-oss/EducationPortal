using EduPortal.Application.Common;
using EduPortal.Application.DTOs.StudentActivity;

namespace EduPortal.Application.Services.Interfaces;

public interface IStudentActivityService
{
    // Summer Activities
    Task<ApiResponse<List<StudentSummerActivityDto>>> GetSummerActivitiesByStudentAsync(int studentId);
    Task<ApiResponse<StudentSummerActivityDto>> GetSummerActivityByIdAsync(int id);
    Task<ApiResponse<StudentSummerActivityDto>> CreateSummerActivityAsync(CreateStudentSummerActivityDto dto);
    Task<ApiResponse<StudentSummerActivityDto>> UpdateSummerActivityAsync(UpdateStudentSummerActivityDto dto);
    Task<ApiResponse<bool>> DeleteSummerActivityAsync(int id);

    // Internships
    Task<ApiResponse<List<StudentInternshipDto>>> GetInternshipsByStudentAsync(int studentId);
    Task<ApiResponse<StudentInternshipDto>> GetInternshipByIdAsync(int id);
    Task<ApiResponse<StudentInternshipDto>> CreateInternshipAsync(CreateStudentInternshipDto dto);
    Task<ApiResponse<StudentInternshipDto>> UpdateInternshipAsync(UpdateStudentInternshipDto dto);
    Task<ApiResponse<bool>> DeleteInternshipAsync(int id);

    // Social Projects
    Task<ApiResponse<List<StudentSocialProjectDto>>> GetSocialProjectsByStudentAsync(int studentId);
    Task<ApiResponse<StudentSocialProjectDto>> GetSocialProjectByIdAsync(int id);
    Task<ApiResponse<StudentSocialProjectDto>> CreateSocialProjectAsync(CreateStudentSocialProjectDto dto);
    Task<ApiResponse<StudentSocialProjectDto>> UpdateSocialProjectAsync(UpdateStudentSocialProjectDto dto);
    Task<ApiResponse<bool>> DeleteSocialProjectAsync(int id);
}
