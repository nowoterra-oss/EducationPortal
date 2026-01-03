using EduPortal.Application.DTOs.Parent;
using EduPortal.Application.Interfaces;
using EduPortal.Application.Services.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using EduPortal.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class ParentService : IParentService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPermissionService _permissionService;

    public ParentService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IPermissionService permissionService)
    {
        _context = context;
        _userManager = userManager;
        _permissionService = permissionService;
    }

    public async Task<IEnumerable<ParentDto>> GetAllParentsAsync()
    {
        var parents = await _context.Parents
            .Include(p => p.User)
            .Include(p => p.Students)
                .ThenInclude(sp => sp.Student)
                    .ThenInclude(s => s.User)
            .Where(p => !p.IsDeleted)
            .ToListAsync();

        return parents.Select(MapToDto);
    }

    public async Task<(IEnumerable<ParentSummaryDto> Items, int TotalCount)> GetParentsPagedAsync(int pageNumber, int pageSize)
    {
        var query = _context.Parents
            .Include(p => p.User)
            .Include(p => p.Students)
            .Where(p => !p.IsDeleted);

        var totalCount = await query.CountAsync();

        var parents = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = parents.Select(p => new ParentSummaryDto
        {
            Id = p.Id,
            FullName = $"{p.User.FirstName} {p.User.LastName}",
            Email = p.User.Email,
            PhoneNumber = p.User.PhoneNumber,
            Occupation = p.Occupation,
            StudentCount = p.Students.Count(sp => !sp.IsDeleted)
        });

        return (items, totalCount);
    }

    public async Task<ParentDto?> GetParentByIdAsync(int id)
    {
        var parent = await _context.Parents
            .Include(p => p.User)
            .Include(p => p.Students)
                .ThenInclude(sp => sp.Student)
                    .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        return parent != null ? MapToDto(parent) : null;
    }

    public async Task<ParentDto?> GetParentByUserIdAsync(string userId)
    {
        var parent = await _context.Parents
            .Include(p => p.User)
            .Include(p => p.Students)
                .ThenInclude(sp => sp.Student)
                    .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);

        return parent != null ? MapToDto(parent) : null;
    }

    public async Task<ParentDto> CreateParentAsync(CreateParentDto dto)
    {
        var parent = new Parent
        {
            UserId = dto.UserId,
            Occupation = dto.Occupation,
            WorkPhone = dto.WorkPhone
        };

        _context.Parents.Add(parent);
        await _context.SaveChangesAsync();

        // Eğer öğrenci ilişkileri belirtildiyse ekle
        if (dto.StudentRelationships != null && dto.StudentRelationships.Any())
        {
            foreach (var relationship in dto.StudentRelationships)
            {
                var studentParent = new StudentParent
                {
                    ParentId = parent.Id,
                    StudentId = relationship.StudentId,
                    Relationship = relationship.Relationship,
                    IsPrimaryContact = relationship.IsPrimaryContact,
                    IsEmergencyContact = relationship.IsEmergencyContact
                };
                _context.StudentParents.Add(studentParent);
            }
            await _context.SaveChangesAsync();
        }

        return (await GetParentByIdAsync(parent.Id))!;
    }

    public async Task<ParentDto> UpdateParentAsync(int id, UpdateParentDto dto)
    {
        var parent = await _context.Parents
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (parent == null)
            throw new Exception("Parent not found");

        // Parent tablosundaki alanları güncelle
        if (dto.Occupation != null)
            parent.Occupation = dto.Occupation;
        if (dto.WorkPhone != null)
            parent.WorkPhone = dto.WorkPhone;
        if (dto.IdentityType.HasValue)
            parent.IdentityType = (IdentityType)dto.IdentityType.Value;
        if (dto.IdentityNumber != null)
            parent.IdentityNumber = dto.IdentityNumber;
        if (dto.Nationality != null)
            parent.Nationality = dto.Nationality;
        if (dto.DateOfBirth.HasValue)
            parent.DateOfBirth = dto.DateOfBirth.Value;
        if (dto.Gender.HasValue)
            parent.Gender = (Gender)dto.Gender.Value;
        if (dto.City != null)
            parent.City = dto.City;
        if (dto.District != null)
            parent.District = dto.District;
        if (dto.Address != null)
            parent.Address = dto.Address;

        parent.UpdatedAt = DateTime.UtcNow;

        // User tablosundaki alanları güncelle
        if (parent.User != null)
        {
            if (!string.IsNullOrEmpty(dto.FirstName))
                parent.User.FirstName = dto.FirstName;
            if (!string.IsNullOrEmpty(dto.LastName))
                parent.User.LastName = dto.LastName;
            if (!string.IsNullOrEmpty(dto.Email))
            {
                parent.User.Email = dto.Email;
                parent.User.NormalizedEmail = dto.Email.ToUpperInvariant();
            }
            if (!string.IsNullOrEmpty(dto.PhoneNumber))
                parent.User.PhoneNumber = dto.PhoneNumber;
        }

        await _context.SaveChangesAsync();

        return (await GetParentByIdAsync(id))!;
    }

    public async Task<bool> DeleteParentAsync(int id)
    {
        var parent = await _context.Parents.FindAsync(id);
        if (parent == null || parent.IsDeleted)
            return false;

        parent.IsDeleted = true;
        parent.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<ParentDto>> GetParentsByStudentIdAsync(int studentId)
    {
        var parentIds = await _context.StudentParents
            .Where(sp => sp.StudentId == studentId && !sp.IsDeleted)
            .Select(sp => sp.ParentId)
            .ToListAsync();

        var parents = await _context.Parents
            .Include(p => p.User)
            .Include(p => p.Students)
                .ThenInclude(sp => sp.Student)
                    .ThenInclude(s => s.User)
            .Where(p => parentIds.Contains(p.Id) && !p.IsDeleted)
            .ToListAsync();

        return parents.Select(MapToDto);
    }

    public async Task<bool> AddStudentRelationshipAsync(int parentId, StudentRelationshipDto relationship)
    {
        var parent = await _context.Parents.FindAsync(parentId);
        if (parent == null || parent.IsDeleted)
            return false;

        var existingRelationship = await _context.StudentParents
            .FirstOrDefaultAsync(sp => sp.ParentId == parentId && sp.StudentId == relationship.StudentId && !sp.IsDeleted);

        if (existingRelationship != null)
            return false; // İlişki zaten var

        var studentParent = new StudentParent
        {
            ParentId = parentId,
            StudentId = relationship.StudentId,
            Relationship = relationship.Relationship,
            IsPrimaryContact = relationship.IsPrimaryContact,
            IsEmergencyContact = relationship.IsEmergencyContact
        };

        _context.StudentParents.Add(studentParent);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveStudentRelationshipAsync(int parentId, int studentId)
    {
        var relationship = await _context.StudentParents
            .FirstOrDefaultAsync(sp => sp.ParentId == parentId && sp.StudentId == studentId && !sp.IsDeleted);

        if (relationship == null)
            return false;

        relationship.IsDeleted = true;
        relationship.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<(IEnumerable<ParentWithStudentDto> Items, int TotalCount)> GetParentsWithStudentPagedAsync(int pageNumber, int pageSize)
    {
        var query = _context.Parents
            .Include(p => p.User)
            .Include(p => p.Students)
                .ThenInclude(sp => sp.Student)
                    .ThenInclude(s => s.User)
            .Where(p => !p.IsDeleted);

        var totalCount = await query.CountAsync();

        var parents = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = parents.Select(MapToWithStudentDto);

        return (items, totalCount);
    }

    public async Task<ParentWithStudentDto?> GetParentWithStudentByIdAsync(int id)
    {
        var parent = await _context.Parents
            .Include(p => p.User)
            .Include(p => p.Students)
                .ThenInclude(sp => sp.Student)
                    .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        return parent != null ? MapToWithStudentDto(parent) : null;
    }

    public async Task<(IEnumerable<ParentWithStudentDto> Items, int TotalCount)> SearchAsync(string term, int pageNumber, int pageSize)
    {
        var searchTerm = term?.ToLower() ?? "";

        var query = _context.Parents
            .Include(p => p.User)
            .Include(p => p.Students)
                .ThenInclude(sp => sp.Student)
                    .ThenInclude(s => s.User)
            .Where(p => !p.IsDeleted &&
                (p.User.FirstName.ToLower().Contains(searchTerm) ||
                 p.User.LastName.ToLower().Contains(searchTerm) ||
                 (p.User.Email != null && p.User.Email.ToLower().Contains(searchTerm)) ||
                 (p.User.FirstName + " " + p.User.LastName).ToLower().Contains(searchTerm)));

        var totalCount = await query.CountAsync();

        var parents = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = parents.Select(MapToWithStudentDto);

        return (items, totalCount);
    }

    public async Task<ParentDto> CreateParentForStudentAsync(int studentId, CreateParentForStudentDto dto)
    {
        // Öğrenci var mı kontrol et
        var student = await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == studentId && !s.IsDeleted);

        if (student == null)
            throw new Exception("Öğrenci bulunamadı");

        // Email benzersizlik kontrolü
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            throw new Exception("Bu email adresi ile kayıtlı bir kullanıcı zaten mevcut");

        // ApplicationUser oluştur
        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhoneNumber = dto.PhoneNumber,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Varsayılan şifre - kimlik numarası yoksa email'in @ öncesi
        var defaultPassword = !string.IsNullOrEmpty(dto.IdentityNumber)
            ? dto.IdentityNumber
            : dto.Email.Split('@')[0] + "123!";

        var result = await _userManager.CreateAsync(user, defaultPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"Kullanıcı oluşturulamadı: {errors}");
        }

        // Veli yetkilerini ata
        await _permissionService.AssignDefaultPermissionsToUserAsync(user.Id, "Parent");

        // String'den enum'a dönüştürme
        var identityType = Domain.Enums.IdentityType.TCKimlik;
        if (!string.IsNullOrEmpty(dto.IdentityType))
        {
            Enum.TryParse<Domain.Enums.IdentityType>(dto.IdentityType, true, out identityType);
        }

        Domain.Enums.Gender? gender = null;
        if (!string.IsNullOrEmpty(dto.Gender))
        {
            if (Enum.TryParse<Domain.Enums.Gender>(dto.Gender, true, out var parsedGender))
            {
                gender = parsedGender;
            }
        }

        // Parent entity oluştur
        var parent = new Parent
        {
            UserId = user.Id,
            Occupation = dto.Occupation,
            WorkPhone = dto.PhoneNumber,
            IdentityType = identityType,
            IdentityNumber = dto.IdentityNumber,
            Nationality = dto.Nationality,
            DateOfBirth = dto.DateOfBirth,
            Gender = gender,
            City = dto.City,
            District = dto.District,
            Address = dto.Address
        };

        _context.Parents.Add(parent);
        await _context.SaveChangesAsync();

        // Öğrenci-veli ilişkisini oluştur
        var studentParent = new StudentParent
        {
            ParentId = parent.Id,
            StudentId = studentId,
            Relationship = dto.ParentType, // Anne, Baba, Vasi
            IsPrimaryContact = dto.IsPrimaryContact,
            IsEmergencyContact = dto.IsEmergencyContact
        };

        _context.StudentParents.Add(studentParent);
        await _context.SaveChangesAsync();

        return (await GetParentByIdAsync(parent.Id))!;
    }

    private ParentDto MapToDto(Parent parent)
    {
        return new ParentDto
        {
            Id = parent.Id,
            UserId = parent.UserId,
            UserName = parent.User.UserName ?? "",
            FullName = $"{parent.User.FirstName} {parent.User.LastName}",
            FirstName = parent.User.FirstName,
            LastName = parent.User.LastName,
            Email = parent.User.Email,
            PhoneNumber = parent.User.PhoneNumber,
            Occupation = parent.Occupation,
            WorkPhone = parent.WorkPhone,
            // Kimlik Bilgileri
            IdentityType = parent.IdentityType.ToString(),
            IdentityNumber = parent.IdentityNumber,
            Nationality = parent.Nationality,
            // Kişisel Bilgiler
            Gender = parent.Gender?.ToString(),
            DateOfBirth = parent.DateOfBirth,
            // Adres Bilgileri
            City = parent.City,
            District = parent.District,
            Address = parent.Address,
            Students = parent.Students
                .Where(sp => !sp.IsDeleted)
                .Select(sp => new ParentStudentInfo
                {
                    StudentId = sp.StudentId,
                    StudentName = sp.Student != null && sp.Student.User != null
                        ? $"{sp.Student.User.FirstName} {sp.Student.User.LastName}"
                        : "",
                    StudentNo = sp.Student?.StudentNo,
                    Relationship = sp.Relationship,
                    IsPrimaryContact = sp.IsPrimaryContact,
                    IsEmergencyContact = sp.IsEmergencyContact
                }).ToList(),
            CreatedDate = parent.CreatedAt
        };
    }

    private ParentWithStudentDto MapToWithStudentDto(Parent parent)
    {
        var firstStudent = parent.Students.FirstOrDefault(sp => !sp.IsDeleted);

        return new ParentWithStudentDto
        {
            Id = parent.Id,
            UserId = parent.UserId,
            UserName = parent.User.UserName ?? "",
            FullName = $"{parent.User.FirstName} {parent.User.LastName}",
            FirstName = parent.User.FirstName,
            LastName = parent.User.LastName,
            Email = parent.User.Email,
            PhoneNumber = parent.User.PhoneNumber,
            Occupation = parent.Occupation,
            WorkPhone = parent.WorkPhone,
            // Kimlik Bilgileri
            IdentityType = parent.IdentityType.ToString(),
            IdentityNumber = parent.IdentityNumber,
            Nationality = parent.Nationality,
            // Kişisel Bilgiler
            Gender = parent.Gender?.ToString(),
            DateOfBirth = parent.DateOfBirth,
            // Adres Bilgileri
            City = parent.City,
            District = parent.District,
            Address = parent.Address,
            Students = parent.Students
                .Where(sp => !sp.IsDeleted)
                .Select(sp => new ParentStudentInfo
                {
                    StudentId = sp.StudentId,
                    StudentName = sp.Student != null && sp.Student.User != null
                        ? $"{sp.Student.User.FirstName} {sp.Student.User.LastName}"
                        : "",
                    StudentNo = sp.Student?.StudentNo,
                    Relationship = sp.Relationship,
                    IsPrimaryContact = sp.IsPrimaryContact,
                    IsEmergencyContact = sp.IsEmergencyContact
                }).ToList(),
            CreatedDate = parent.CreatedAt,
            // İlk öğrenci bilgileri
            StudentId = firstStudent?.StudentId,
            StudentFirstName = firstStudent?.Student?.User?.FirstName,
            StudentLastName = firstStudent?.Student?.User?.LastName,
            StudentNo = firstStudent?.Student?.StudentNo,
            Relationship = firstStudent?.Relationship
        };
    }
}
