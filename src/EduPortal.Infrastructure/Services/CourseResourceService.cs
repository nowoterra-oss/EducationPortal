using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Course;
using EduPortal.Application.Services.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduPortal.Infrastructure.Services;

public class CourseResourceService : ICourseResourceService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CourseResourceService> _logger;

    public CourseResourceService(ApplicationDbContext context, ILogger<CourseResourceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResponse<CourseResourceDto>>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var query = _context.CourseResources
                .Include(r => r.Course)
                .Include(r => r.Curriculum)
                .Where(r => !r.IsDeleted && r.IsVisible)
                .OrderByDescending(r => r.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = items.Select(MapToDto).ToList();
            var pagedResponse = new PagedResponse<CourseResourceDto>(dtos, totalCount, pageNumber, pageSize);

            return ApiResponse<PagedResponse<CourseResourceDto>>.SuccessResponse(pagedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all course resources");
            return ApiResponse<PagedResponse<CourseResourceDto>>.ErrorResponse($"Kaynaklar getirilirken hata olustu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PagedResponse<CourseResourceDto>>> GetByCourseAsync(int courseId, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var query = _context.CourseResources
                .Include(r => r.Course)
                .Include(r => r.Curriculum)
                .Where(r => r.CourseId == courseId && !r.IsDeleted && r.IsVisible)
                .OrderByDescending(r => r.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = items.Select(MapToDto).ToList();
            var pagedResponse = new PagedResponse<CourseResourceDto>(dtos, totalCount, pageNumber, pageSize);

            return ApiResponse<PagedResponse<CourseResourceDto>>.SuccessResponse(pagedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course resources for course {CourseId}", courseId);
            return ApiResponse<PagedResponse<CourseResourceDto>>.ErrorResponse($"Ders kaynaklari getirilirken hata olustu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<CourseResourceDto>> GetByIdAsync(int id)
    {
        try
        {
            var resource = await _context.CourseResources
                .Include(r => r.Course)
                .Include(r => r.Curriculum)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (resource == null)
            {
                return ApiResponse<CourseResourceDto>.ErrorResponse("Kaynak bulunamadi");
            }

            return ApiResponse<CourseResourceDto>.SuccessResponse(MapToDto(resource));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course resource by ID {Id}", id);
            return ApiResponse<CourseResourceDto>.ErrorResponse($"Kaynak getirilirken hata olustu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<CourseResourceDto>> CreateAsync(int courseId, CreateCourseResourceDto dto)
    {
        try
        {
            // Verify course exists
            var courseExists = await _context.Courses.AnyAsync(c => c.Id == courseId && !c.IsDeleted);
            if (!courseExists)
            {
                return ApiResponse<CourseResourceDto>.ErrorResponse("Ders bulunamadi");
            }

            // Verify curriculum exists if provided
            if (dto.CurriculumId.HasValue)
            {
                var curriculumExists = await _context.Curricula.AnyAsync(c => c.Id == dto.CurriculumId.Value && !c.IsDeleted);
                if (!curriculumExists)
                {
                    return ApiResponse<CourseResourceDto>.ErrorResponse("Mufredat bulunamadi");
                }
            }

            var resource = new CourseResource
            {
                CourseId = courseId,
                CurriculumId = dto.CurriculumId,
                Title = dto.Title,
                ResourceType = dto.ResourceType,
                ResourceUrl = dto.ResourceUrl,
                Description = dto.Description,
                IsVisible = dto.IsVisible,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _context.CourseResources.AddAsync(resource);
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            var createdResource = await _context.CourseResources
                .Include(r => r.Course)
                .Include(r => r.Curriculum)
                .FirstOrDefaultAsync(r => r.Id == resource.Id);

            return ApiResponse<CourseResourceDto>.SuccessResponse(MapToDto(createdResource!), "Kaynak basariyla olusturuldu");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating course resource for course {CourseId}", courseId);
            return ApiResponse<CourseResourceDto>.ErrorResponse($"Kaynak olusturulurken hata olustu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<CourseResourceDto>> UpdateAsync(int id, CreateCourseResourceDto dto)
    {
        try
        {
            var resource = await _context.CourseResources
                .Include(r => r.Course)
                .Include(r => r.Curriculum)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (resource == null)
            {
                return ApiResponse<CourseResourceDto>.ErrorResponse("Kaynak bulunamadi");
            }

            // Verify curriculum exists if provided
            if (dto.CurriculumId.HasValue)
            {
                var curriculumExists = await _context.Curricula.AnyAsync(c => c.Id == dto.CurriculumId.Value && !c.IsDeleted);
                if (!curriculumExists)
                {
                    return ApiResponse<CourseResourceDto>.ErrorResponse("Mufredat bulunamadi");
                }
            }

            resource.CurriculumId = dto.CurriculumId;
            resource.Title = dto.Title;
            resource.ResourceType = dto.ResourceType;
            resource.ResourceUrl = dto.ResourceUrl;
            resource.Description = dto.Description;
            resource.IsVisible = dto.IsVisible;
            resource.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ApiResponse<CourseResourceDto>.SuccessResponse(MapToDto(resource), "Kaynak basariyla guncellendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating course resource {Id}", id);
            return ApiResponse<CourseResourceDto>.ErrorResponse($"Kaynak guncellenirken hata olustu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        try
        {
            var resource = await _context.CourseResources
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (resource == null)
            {
                return ApiResponse<bool>.ErrorResponse("Kaynak bulunamadi");
            }

            // Soft delete
            resource.IsDeleted = true;
            resource.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Kaynak basariyla silindi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting course resource {Id}", id);
            return ApiResponse<bool>.ErrorResponse($"Kaynak silinirken hata olustu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<CourseResourceDto>> GetForDownloadAsync(int id)
    {
        try
        {
            var resource = await _context.CourseResources
                .Include(r => r.Course)
                .Include(r => r.Curriculum)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted && r.IsVisible);

            if (resource == null)
            {
                return ApiResponse<CourseResourceDto>.ErrorResponse("Kaynak bulunamadi veya erisime kapali");
            }

            return ApiResponse<CourseResourceDto>.SuccessResponse(MapToDto(resource));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course resource for download {Id}", id);
            return ApiResponse<CourseResourceDto>.ErrorResponse($"Kaynak getirilirken hata olustu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<CourseResourceDto>> CreateWithFileAsync(int courseId, CreateCourseResourceDto dto, string? filePath, string? fileName, long? fileSize, string? mimeType)
    {
        try
        {
            // Verify course exists
            var courseExists = await _context.Courses.AnyAsync(c => c.Id == courseId && !c.IsDeleted);
            if (!courseExists)
            {
                return ApiResponse<CourseResourceDto>.ErrorResponse("Ders bulunamadi");
            }

            // Verify curriculum exists if provided
            if (dto.CurriculumId.HasValue)
            {
                var curriculumExists = await _context.Curricula.AnyAsync(c => c.Id == dto.CurriculumId.Value && !c.IsDeleted);
                if (!curriculumExists)
                {
                    return ApiResponse<CourseResourceDto>.ErrorResponse("Mufredat bulunamadi");
                }
            }

            var resource = new CourseResource
            {
                CourseId = courseId,
                CurriculumId = dto.CurriculumId,
                Title = dto.Title,
                ResourceType = dto.ResourceType,
                ResourceUrl = dto.ResourceUrl ?? string.Empty,
                Description = dto.Description,
                FilePath = filePath,
                FileName = fileName,
                FileSize = fileSize,
                MimeType = mimeType,
                IsVisible = dto.IsVisible,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _context.CourseResources.AddAsync(resource);
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            var createdResource = await _context.CourseResources
                .Include(r => r.Course)
                .Include(r => r.Curriculum)
                .FirstOrDefaultAsync(r => r.Id == resource.Id);

            return ApiResponse<CourseResourceDto>.SuccessResponse(MapToDto(createdResource!), "Kaynak basariyla olusturuldu");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating course resource with file for course {CourseId}", courseId);
            return ApiResponse<CourseResourceDto>.ErrorResponse($"Kaynak olusturulurken hata olustu: {ex.Message}");
        }
    }

    public async Task<ApiResponse<CourseResourceDownloadDto>> GetDownloadInfoAsync(int id, string baseUrl)
    {
        try
        {
            var resource = await _context.CourseResources
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted && r.IsVisible);

            if (resource == null)
            {
                return ApiResponse<CourseResourceDownloadDto>.ErrorResponse("Kaynak bulunamadi veya erisime kapali");
            }

            string downloadUrl;
            string fileName;

            // Dosya yolu varsa sunucudan indir
            if (!string.IsNullOrEmpty(resource.FilePath))
            {
                downloadUrl = $"{baseUrl}/uploads/resources/{resource.FilePath}";
                fileName = resource.FileName ?? resource.Title;
            }
            // Harici URL varsa onu kullan
            else if (!string.IsNullOrEmpty(resource.ResourceUrl))
            {
                downloadUrl = resource.ResourceUrl;
                fileName = resource.Title;
            }
            else
            {
                return ApiResponse<CourseResourceDownloadDto>.ErrorResponse("Bu kaynak icin indirilebilir dosya bulunamadi");
            }

            var downloadDto = new CourseResourceDownloadDto
            {
                DownloadUrl = downloadUrl,
                FileName = fileName,
                FileSize = resource.FileSize,
                MimeType = resource.MimeType
            };

            return ApiResponse<CourseResourceDownloadDto>.SuccessResponse(downloadDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting download info for resource {Id}", id);
            return ApiResponse<CourseResourceDownloadDto>.ErrorResponse($"Indirme bilgisi alinirken hata olustu: {ex.Message}");
        }
    }

    private static CourseResourceDto MapToDto(CourseResource resource)
    {
        return new CourseResourceDto
        {
            Id = resource.Id,
            CourseId = resource.CourseId,
            CurriculumId = resource.CurriculumId,
            CurriculumTopicName = resource.Curriculum?.TopicName,
            Title = resource.Title,
            ResourceType = resource.ResourceType,
            ResourceUrl = resource.ResourceUrl,
            Description = resource.Description,
            FilePath = resource.FilePath,
            FileName = resource.FileName,
            FileSize = resource.FileSize,
            MimeType = resource.MimeType,
            IsVisible = resource.IsVisible,
            CreatedAt = resource.CreatedAt
        };
    }
}
