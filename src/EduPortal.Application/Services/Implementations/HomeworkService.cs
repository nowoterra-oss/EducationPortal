using AutoMapper;
using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Homework;
using EduPortal.Application.Interfaces;
using EduPortal.Application.Services.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;

namespace EduPortal.Application.Services.Implementations;

public class HomeworkService : IHomeworkService
{
    private readonly IHomeworkRepository _homeworkRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IMapper _mapper;

    public HomeworkService(
        IHomeworkRepository homeworkRepository,
        IStudentRepository studentRepository,
        IMapper mapper)
    {
        _homeworkRepository = homeworkRepository;
        _studentRepository = studentRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PagedResponse<HomeworkDto>>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var homeworks = await _homeworkRepository.GetAllAsync();
            var homeworksList = homeworks.ToList();

            var totalRecords = homeworksList.Count;
            var pagedHomeworks = homeworksList
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var homeworkDtos = _mapper.Map<List<HomeworkDto>>(pagedHomeworks);
            var pagedResponse = new PagedResponse<HomeworkDto>(homeworkDtos, totalRecords, pageNumber, pageSize);

            return ApiResponse<PagedResponse<HomeworkDto>>.SuccessResponse(pagedResponse);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResponse<HomeworkDto>>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<HomeworkDto>> GetByIdAsync(int id)
    {
        try
        {
            var homework = await _homeworkRepository.GetHomeworkWithSubmissionsAsync(id);
            if (homework == null)
            {
                return ApiResponse<HomeworkDto>.ErrorResponse("Ödev bulunamadı");
            }

            var dto = _mapper.Map<HomeworkDto>(homework);
            dto.TotalSubmissions = homework.Submissions?.Count ?? 0;

            return ApiResponse<HomeworkDto>.SuccessResponse(dto);
        }
        catch (Exception ex)
        {
            return ApiResponse<HomeworkDto>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<HomeworkDto>> CreateAsync(HomeworkCreateDto dto)
    {
        try
        {
            // Validate dates
            if (dto.DueDate < dto.AssignedDate)
            {
                return ApiResponse<HomeworkDto>.ErrorResponse("Son teslim tarihi, atanma tarihinden önce olamaz");
            }

            var homework = _mapper.Map<Homework>(dto);

            await _homeworkRepository.AddAsync(homework);

            var homeworkDto = _mapper.Map<HomeworkDto>(homework);
            return ApiResponse<HomeworkDto>.SuccessResponse(homeworkDto, "Ödev başarıyla oluşturuldu");
        }
        catch (Exception ex)
        {
            return ApiResponse<HomeworkDto>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<HomeworkSubmissionDto>> SubmitHomeworkAsync(HomeworkSubmitDto dto)
    {
        try
        {
            // Check if homework exists
            var homework = await _homeworkRepository.GetByIdAsync(dto.HomeworkId);
            if (homework == null)
            {
                return ApiResponse<HomeworkSubmissionDto>.ErrorResponse("Ödev bulunamadı");
            }

            // Check if student exists
            var student = await _studentRepository.GetByIdAsync(dto.StudentId);
            if (student == null)
            {
                return ApiResponse<HomeworkSubmissionDto>.ErrorResponse("Öğrenci bulunamadı");
            }

            // Check if already submitted
            var existingSubmission = await _homeworkRepository.GetSubmissionAsync(dto.HomeworkId, dto.StudentId);

            StudentHomeworkSubmission submission;

            if (existingSubmission != null)
            {
                // Update existing submission
                existingSubmission.SubmissionUrl = dto.SubmissionUrl;
                existingSubmission.SubmissionDate = dto.SubmissionDate ?? DateTime.UtcNow;
                existingSubmission.Status = dto.Status;
                existingSubmission.CompletionPercentage = 100;
                submission = existingSubmission;
            }
            else
            {
                // Create new submission
                submission = new StudentHomeworkSubmission
                {
                    HomeworkId = dto.HomeworkId,
                    StudentId = dto.StudentId,
                    SubmissionUrl = dto.SubmissionUrl,
                    SubmissionDate = dto.SubmissionDate ?? DateTime.UtcNow,
                    Status = dto.Status,
                    CompletionPercentage = 100
                };
            }

            var submissionDto = _mapper.Map<HomeworkSubmissionDto>(submission);
            return ApiResponse<HomeworkSubmissionDto>.SuccessResponse(submissionDto, "Ödev başarıyla teslim edildi");
        }
        catch (Exception ex)
        {
            return ApiResponse<HomeworkSubmissionDto>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<HomeworkSubmissionDto>> EvaluateSubmissionAsync(int submissionId, int score, string? feedback)
    {
        try
        {
            // Note: This would require a submission repository or accessing through homework repository
            // For now, we'll get the submission through the homework repository
            // In a real implementation, you might want to create a separate ISubmissionRepository

            var submission = await _homeworkRepository.GetSubmissionAsync(submissionId, 0);
            if (submission == null)
            {
                return ApiResponse<HomeworkSubmissionDto>.ErrorResponse("Teslim bulunamadı");
            }

            submission.Score = score;
            submission.TeacherFeedback = feedback;
            submission.Status = HomeworkStatus.Degerlendirildi;

            var submissionDto = _mapper.Map<HomeworkSubmissionDto>(submission);
            return ApiResponse<HomeworkSubmissionDto>.SuccessResponse(submissionDto, "Ödev başarıyla değerlendirildi");
        }
        catch (Exception ex)
        {
            return ApiResponse<HomeworkSubmissionDto>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<HomeworkSubmissionDto>>> GetHomeworkSubmissionsAsync(int homeworkId)
    {
        try
        {
            var homework = await _homeworkRepository.GetHomeworkWithSubmissionsAsync(homeworkId);
            if (homework == null)
            {
                return ApiResponse<List<HomeworkSubmissionDto>>.ErrorResponse("Ödev bulunamadı");
            }

            var submissionDtos = _mapper.Map<List<HomeworkSubmissionDto>>(homework.Submissions);
            return ApiResponse<List<HomeworkSubmissionDto>>.SuccessResponse(submissionDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<HomeworkSubmissionDto>>.ErrorResponse($"Hata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<HomeworkSubmissionDto>>> GetStudentSubmissionsAsync(int studentId)
    {
        try
        {
            var submissions = await _homeworkRepository.GetStudentSubmissionsAsync(studentId);
            var submissionDtos = _mapper.Map<List<HomeworkSubmissionDto>>(submissions);

            return ApiResponse<List<HomeworkSubmissionDto>>.SuccessResponse(submissionDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<HomeworkSubmissionDto>>.ErrorResponse($"Hata: {ex.Message}");
        }
    }
}
