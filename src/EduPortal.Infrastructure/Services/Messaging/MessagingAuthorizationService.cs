using EduPortal.Application.Interfaces.Messaging;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduPortal.Infrastructure.Services.Messaging;

/// <summary>
/// Mesajlasma yetkilendirme servisi - kim kime mesaj atabilir kurallari
///
/// KURALLAR:
/// - Admin: Herkese mesaj atabilir (bireysel ve toplu)
/// - Ogretmen: Kendi bireysel ders ogrencilerine, kendi grup dersi ogrencilerine
/// - Ogrenci: Kendi ogretmenine, danisman ogretmenine, grup dersi arkadaslarina
/// - Veli: Cocugunun danisman ogretmenine
/// - Danisman: Danismanligi yaptigi ogrencilere, bu ogrencilerin velilerine, ders verdigi ogrencilere
/// </summary>
public class MessagingAuthorizationService : IMessagingAuthorizationService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<MessagingAuthorizationService> _logger;

    public MessagingAuthorizationService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<MessagingAuthorizationService> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<(bool canMessage, string? reason)> CanMessageUserAsync(string senderUserId, string recipientUserId)
    {
        if (senderUserId == recipientUserId)
        {
            return (false, "Kendinize mesaj atamazsınız.");
        }

        var sender = await _userManager.FindByIdAsync(senderUserId);
        var recipient = await _userManager.FindByIdAsync(recipientUserId);

        if (sender == null || recipient == null)
        {
            return (false, "Kullanıcı bulunamadı.");
        }

        var senderRoles = await _userManager.GetRolesAsync(sender);
        var recipientRoles = await _userManager.GetRolesAsync(recipient);

        // Admin her zaman herkese mesaj atabilir
        if (senderRoles.Contains("Admin"))
        {
            return (true, null);
        }

        // Ogretmen kontrolu
        if (senderRoles.Contains("Ogretmen"))
        {
            return await CanTeacherMessageAsync(senderUserId, recipientUserId, recipientRoles);
        }

        // Ogrenci kontrolu
        if (senderRoles.Contains("Ogrenci"))
        {
            return await CanStudentMessageAsync(senderUserId, recipientUserId, recipientRoles);
        }

        // Veli kontrolu
        if (senderRoles.Contains("Veli"))
        {
            return await CanParentMessageAsync(senderUserId, recipientUserId, recipientRoles);
        }

        // Danisman kontrolu
        if (senderRoles.Contains("Danışman"))
        {
            return await CanCounselorMessageAsync(senderUserId, recipientUserId, recipientRoles);
        }

        return (false, "Bu kişiye mesaj atma yetkiniz bulunmamaktadır.");
    }

    public async Task<(bool canMessage, string? reason)> CanMessageInConversationAsync(string userId, int conversationId)
    {
        // Kullanici bu konusmada katilimci mi?
        var participant = await _context.ConversationParticipants
            .AnyAsync(cp => cp.ConversationId == conversationId &&
                           cp.UserId == userId &&
                           cp.LeftAt == null);

        if (!participant)
        {
            return (false, "Bu konuşmada yer almıyorsunuz.");
        }

        return (true, null);
    }

    public async Task<(bool canMessage, string? reason)> CanMessageGroupAsync(string userId, int groupId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return (false, "Kullanıcı bulunamadı.");
        }

        var userRoles = await _userManager.GetRolesAsync(user);

        // Admin her gruba mesaj atabilir
        if (userRoles.Contains("Admin"))
        {
            return (true, null);
        }

        // Ogretmen kendi grubu mu? (GroupLessonSchedule uzerinden)
        if (userRoles.Contains("Ogretmen"))
        {
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
            if (teacher != null)
            {
                var isTeacherOfGroup = await _context.GroupLessonSchedules
                    .AnyAsync(gls => gls.GroupId == groupId && gls.TeacherId == teacher.Id);

                if (isTeacherOfGroup)
                {
                    return (true, null);
                }
            }
        }

        // Ogrenci bu grupta mi?
        if (userRoles.Contains("Ogrenci"))
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student != null)
            {
                var isGroupMember = await _context.StudentGroupMembers
                    .AnyAsync(gm => gm.GroupId == groupId && gm.StudentId == student.Id);

                if (isGroupMember)
                {
                    return (true, null);
                }
            }
        }

        return (false, "Bu gruba mesaj atma yetkiniz bulunmamaktadır.");
    }

    public async Task<(bool canBroadcast, string? reason)> CanSendBroadcastAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return (false, "Kullanıcı bulunamadı.");
        }

        var userRoles = await _userManager.GetRolesAsync(user);

        // Sadece Admin toplu mesaj gonderebilir
        if (userRoles.Contains("Admin"))
        {
            return (true, null);
        }

        return (false, "Toplu mesaj gönderme yetkiniz bulunmamaktadır.");
    }

    public async Task<List<string>> GetAllowedRecipientsAsync(string userId)
    {
        _logger.LogInformation("[GetAllowedRecipientsAsync] userId: {UserId}", userId);
        var allowedUserIds = new HashSet<string>();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("[GetAllowedRecipientsAsync] User not found: {UserId}", userId);
            return allowedUserIds.ToList();
        }

        var userRoles = await _userManager.GetRolesAsync(user);
        _logger.LogInformation("[GetAllowedRecipientsAsync] User {UserId} roles: {Roles}", userId, string.Join(", ", userRoles));

        // Admin herkese mesaj atabilir
        if (userRoles.Contains("Admin"))
        {
            var allUsers = await _userManager.Users.Select(u => u.Id).ToListAsync();
            _logger.LogInformation("[GetAllowedRecipientsAsync] Admin user, returning all {Count} users", allUsers.Count - 1);
            return allUsers.Where(id => id != userId).ToList();
        }

        // Ogretmen: Ogrencileri
        if (userRoles.Contains("Ogretmen"))
        {
            _logger.LogInformation("[GetAllowedRecipientsAsync] User is Ogretmen, getting students...");
            var teacherStudents = await GetTeacherStudentsAsync(userId);
            _logger.LogInformation("[GetAllowedRecipientsAsync] Teacher has {Count} students", teacherStudents.Count);
            foreach (var studentUserId in teacherStudents)
            {
                allowedUserIds.Add(studentUserId);
            }

            // Danismanlik yaptigi ogrenciler ve velileri
            var counselorStudents = await GetCounselorStudentsAndParentsAsync(userId);
            _logger.LogInformation("[GetAllowedRecipientsAsync] Counselor students/parents: {Count}", counselorStudents.Count);
            foreach (var id in counselorStudents)
            {
                allowedUserIds.Add(id);
            }
        }

        // Ogrenci: Ogretmenleri, danisman, grup arkadaslari
        if (userRoles.Contains("Ogrenci"))
        {
            var studentRecipients = await GetStudentRecipientsAsync(userId);
            foreach (var id in studentRecipients)
            {
                allowedUserIds.Add(id);
            }
        }

        // Veli: Cocuklarinin ogretmenleri
        // Not: "Veli" rolu olmasa bile, Parents tablosunda kaydi varsa kontrol yapilir
        var isParentInTable = await _context.Parents.AnyAsync(p => p.UserId == userId);
        if (userRoles.Contains("Veli") || isParentInTable)
        {
            _logger.LogInformation("[GetAllowedRecipientsAsync] User is Veli (role: {HasRole}, table: {InTable}), getting parent recipients...",
                userRoles.Contains("Veli"), isParentInTable);
            var parentRecipients = await GetParentRecipientsAsync(userId);
            _logger.LogInformation("[GetAllowedRecipientsAsync] Veli has {Count} recipients", parentRecipients.Count);
            foreach (var id in parentRecipients)
            {
                allowedUserIds.Add(id);
            }
        }

        // Danisman: Ogrenciler, veliler, ders verdigi ogrenciler
        if (userRoles.Contains("Danışman"))
        {
            var counselorRecipients = await GetCounselorRecipientsAsync(userId);
            foreach (var id in counselorRecipients)
            {
                allowedUserIds.Add(id);
            }
        }

        allowedUserIds.Remove(userId); // Kendisini cikar
        return allowedUserIds.ToList();
    }

    public async Task<List<int>> GetAllowedGroupsAsync(string userId)
    {
        var allowedGroups = new List<int>();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return allowedGroups;
        }

        var userRoles = await _userManager.GetRolesAsync(user);

        // Admin tum gruplara erisebilir
        if (userRoles.Contains("Admin"))
        {
            return await _context.StudentGroups.Select(g => g.Id).ToListAsync();
        }

        // Ogretmen: Kendi gruplari (GroupLessonSchedule uzerinden)
        if (userRoles.Contains("Ogretmen"))
        {
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
            if (teacher != null)
            {
                var teacherGroups = await _context.GroupLessonSchedules
                    .Where(gls => gls.TeacherId == teacher.Id)
                    .Select(gls => gls.GroupId)
                    .Distinct()
                    .ToListAsync();

                allowedGroups.AddRange(teacherGroups);
            }
        }

        // Ogrenci: Uye oldugu gruplar
        if (userRoles.Contains("Ogrenci"))
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student != null)
            {
                var studentGroups = await _context.StudentGroupMembers
                    .Where(gm => gm.StudentId == student.Id)
                    .Select(gm => gm.GroupId)
                    .ToListAsync();

                allowedGroups.AddRange(studentGroups);
            }
        }

        return allowedGroups.Distinct().ToList();
    }

    #region Private Helper Methods

    private async Task<(bool canMessage, string? reason)> CanTeacherMessageAsync(
        string teacherUserId, string recipientUserId, IList<string> recipientRoles)
    {
        var today = DateTime.Today;
        var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == teacherUserId);
        if (teacher == null)
        {
            return (false, "Öğretmen bilgisi bulunamadı.");
        }

        // Ogrenciye mesaj
        if (recipientRoles.Contains("Ogrenci"))
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == recipientUserId);
            if (student == null)
            {
                return (false, "Öğrenci bulunamadı.");
            }

            // Bireysel ders var mi? (aktif ders)
            var hasIndividualLesson = await _context.LessonSchedules
                .AnyAsync(ls => ls.TeacherId == teacher.Id &&
                               ls.StudentId == student.Id &&
                               !ls.IsDeleted &&
                               (ls.EffectiveTo == null || ls.EffectiveTo >= today));

            if (hasIndividualLesson)
            {
                return (true, null);
            }

            // Grup dersi var mi? (GroupLessonSchedule uzerinden - aktif dersler)
            var teacherGroupIds = await _context.GroupLessonSchedules
                .Where(gls => gls.TeacherId == teacher.Id &&
                             !gls.IsDeleted &&
                             (gls.EffectiveTo == null || gls.EffectiveTo >= today))
                .Select(gls => gls.GroupId)
                .Distinct()
                .ToListAsync();

            var isInTeacherGroup = await _context.StudentGroupMembers
                .AnyAsync(gm => teacherGroupIds.Contains(gm.GroupId) &&
                               gm.StudentId == student.Id &&
                               gm.IsActive &&
                               !gm.IsDeleted);

            if (isInTeacherGroup)
            {
                return (true, null);
            }

            return (false, "Bu öğrenci sizin aktif ders verdiğiniz öğrenciler arasında değil.");
        }

        // Veliye mesaj (danismanlik iliskisi varsa)
        if (recipientRoles.Contains("Veli"))
        {
            // Danisman olarak bagli ogrencilerin velileri
            var counselor = await _context.Counselors.FirstOrDefaultAsync(c => c.TeacherId == teacher.Id);
            if (counselor != null)
            {
                var counselorStudentIds = await _context.StudentCounselorAssignments
                    .Where(sca => sca.CounselorId == counselor.Id)
                    .Select(sca => sca.StudentId)
                    .ToListAsync();

                var parent = await _context.Parents.FirstOrDefaultAsync(p => p.UserId == recipientUserId);
                if (parent != null)
                {
                    var isParentOfCounselorStudent = await _context.StudentParents
                        .AnyAsync(sp => counselorStudentIds.Contains(sp.StudentId) && sp.ParentId == parent.Id);

                    if (isParentOfCounselorStudent)
                    {
                        return (true, null);
                    }
                }
            }

            return (false, "Bu veliye mesaj atma yetkiniz yok.");
        }

        // Admin'e her zaman mesaj atabilir
        if (recipientRoles.Contains("Admin"))
        {
            return (true, null);
        }

        return (false, "Bu kişiye mesaj atma yetkiniz bulunmamaktadır.");
    }

    private async Task<(bool canMessage, string? reason)> CanStudentMessageAsync(
        string studentUserId, string recipientUserId, IList<string> recipientRoles)
    {
        var today = DateTime.Today;
        var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == studentUserId);
        if (student == null)
        {
            return (false, "Öğrenci bilgisi bulunamadı.");
        }

        // Ogretmene mesaj
        if (recipientRoles.Contains("Ogretmen"))
        {
            var recipientTeacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == recipientUserId);
            if (recipientTeacher == null)
            {
                return (false, "Öğretmen bulunamadı.");
            }

            // Bireysel ders var mi? (aktif ders)
            var hasIndividualLesson = await _context.LessonSchedules
                .AnyAsync(ls => ls.TeacherId == recipientTeacher.Id &&
                               ls.StudentId == student.Id &&
                               !ls.IsDeleted &&
                               (ls.EffectiveTo == null || ls.EffectiveTo >= today));

            if (hasIndividualLesson)
            {
                return (true, null);
            }

            // Grup dersi var mi? (GroupLessonSchedule uzerinden - aktif dersler)
            var teacherGroupIds = await _context.GroupLessonSchedules
                .Where(gls => gls.TeacherId == recipientTeacher.Id &&
                             !gls.IsDeleted &&
                             (gls.EffectiveTo == null || gls.EffectiveTo >= today))
                .Select(gls => gls.GroupId)
                .Distinct()
                .ToListAsync();

            var isInTeacherGroup = await _context.StudentGroupMembers
                .AnyAsync(gm => teacherGroupIds.Contains(gm.GroupId) &&
                               gm.StudentId == student.Id &&
                               gm.IsActive &&
                               !gm.IsDeleted);

            if (isInTeacherGroup)
            {
                return (true, null);
            }

            // Danisman mi?
            var counselor = await _context.Counselors.FirstOrDefaultAsync(c => c.TeacherId == recipientTeacher.Id);
            if (counselor != null)
            {
                var isCounselor = await _context.StudentCounselorAssignments
                    .AnyAsync(sca => sca.CounselorId == counselor.Id &&
                                    sca.StudentId == student.Id &&
                                    !sca.IsDeleted);

                if (isCounselor)
                {
                    return (true, null);
                }
            }

            return (false, "Bu öğretmene mesaj atma yetkiniz yok.");
        }

        // Baska bir ogrenciye mesaj (grup dersi arkadaslari)
        if (recipientRoles.Contains("Ogrenci"))
        {
            var recipientStudent = await _context.Students.FirstOrDefaultAsync(s => s.UserId == recipientUserId);
            if (recipientStudent == null)
            {
                return (false, "Öğrenci bulunamadı.");
            }

            // Ayni grupta mi? (aktif uyelikler)
            var studentGroupIds = await _context.StudentGroupMembers
                .Where(gm => gm.StudentId == student.Id &&
                            gm.IsActive &&
                            !gm.IsDeleted)
                .Select(gm => gm.GroupId)
                .ToListAsync();

            var isInSameGroup = await _context.StudentGroupMembers
                .AnyAsync(gm => studentGroupIds.Contains(gm.GroupId) &&
                               gm.StudentId == recipientStudent.Id &&
                               gm.IsActive &&
                               !gm.IsDeleted);

            if (isInSameGroup)
            {
                return (true, null);
            }

            return (false, "Bu öğrenciye mesaj atma yetkiniz yok. Sadece grup dersi arkadaşlarınıza mesaj atabilirsiniz.");
        }

        // Admin'e her zaman mesaj atabilir
        if (recipientRoles.Contains("Admin"))
        {
            return (true, null);
        }

        return (false, "Bu kişiye mesaj atma yetkiniz bulunmamaktadır.");
    }

    private async Task<(bool canMessage, string? reason)> CanParentMessageAsync(
        string parentUserId, string recipientUserId, IList<string> recipientRoles)
    {
        var today = DateTime.Today;
        var parent = await _context.Parents.FirstOrDefaultAsync(p => p.UserId == parentUserId);
        if (parent == null)
        {
            return (false, "Veli bilgisi bulunamadı.");
        }

        // Cocuklarini bul
        var childrenIds = await _context.StudentParents
            .Where(sp => sp.ParentId == parent.Id)
            .Select(sp => sp.StudentId)
            .ToListAsync();

        // Ogretmene mesaj
        if (recipientRoles.Contains("Ogretmen") || recipientRoles.Contains("Danışman"))
        {
            var recipientTeacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == recipientUserId);
            if (recipientTeacher == null)
            {
                return (false, "Öğretmen bulunamadı.");
            }

            // 1. Danisman ogretmen mi?
            var counselor = await _context.Counselors.FirstOrDefaultAsync(c => c.TeacherId == recipientTeacher.Id);
            if (counselor != null)
            {
                var isCounselorOfChild = await _context.StudentCounselorAssignments
                    .AnyAsync(sca => childrenIds.Contains(sca.StudentId) &&
                                    sca.CounselorId == counselor.Id &&
                                    !sca.IsDeleted);

                if (isCounselorOfChild)
                {
                    return (true, null);
                }
            }

            // 2. Bireysel ders ogretmeni mi?
            var hasIndividualLesson = await _context.LessonSchedules
                .AnyAsync(ls => childrenIds.Contains(ls.StudentId) &&
                               ls.TeacherId == recipientTeacher.Id &&
                               !ls.IsDeleted &&
                               (ls.EffectiveTo == null || ls.EffectiveTo >= today));

            if (hasIndividualLesson)
            {
                return (true, null);
            }

            // 3. Grup dersi ogretmeni mi?
            // Cocuklarin gruplarini bul
            var childGroupIds = await _context.StudentGroupMembers
                .Where(gm => childrenIds.Contains(gm.StudentId) &&
                            gm.IsActive &&
                            !gm.IsDeleted)
                .Select(gm => gm.GroupId)
                .Distinct()
                .ToListAsync();

            var isGroupTeacher = await _context.GroupLessonSchedules
                .AnyAsync(gls => childGroupIds.Contains(gls.GroupId) &&
                                gls.TeacherId == recipientTeacher.Id &&
                                !gls.IsDeleted &&
                                (gls.EffectiveTo == null || gls.EffectiveTo >= today));

            if (isGroupTeacher)
            {
                return (true, null);
            }

            return (false, "Bu öğretmene mesaj atma yetkiniz yok. Sadece çocuğunuzun danışman veya ders öğretmenlerine mesaj atabilirsiniz.");
        }

        // Admin'e her zaman mesaj atabilir
        if (recipientRoles.Contains("Admin"))
        {
            return (true, null);
        }

        return (false, "Bu kişiye mesaj atma yetkiniz bulunmamaktadır.");
    }

    private async Task<(bool canMessage, string? reason)> CanCounselorMessageAsync(
        string counselorUserId, string recipientUserId, IList<string> recipientRoles)
    {
        // Danisman bir ogretmen oldugu icin Teacher tablosundan bulalim
        var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == counselorUserId);
        if (teacher == null)
        {
            return (false, "Danışman bilgisi bulunamadı.");
        }

        var counselor = await _context.Counselors.FirstOrDefaultAsync(c => c.TeacherId == teacher.Id);
        if (counselor == null)
        {
            return (false, "Danışmanlık kaydı bulunamadı.");
        }

        // Danismanlik yaptigi ogrenciler
        var counselorStudentIds = await _context.StudentCounselorAssignments
            .Where(sca => sca.CounselorId == counselor.Id)
            .Select(sca => sca.StudentId)
            .ToListAsync();

        // Ogrenciye mesaj
        if (recipientRoles.Contains("Ogrenci"))
        {
            var recipientStudent = await _context.Students.FirstOrDefaultAsync(s => s.UserId == recipientUserId);
            if (recipientStudent == null)
            {
                return (false, "Öğrenci bulunamadı.");
            }

            // Danismanlik yaptigi ogrenci mi?
            if (counselorStudentIds.Contains(recipientStudent.Id))
            {
                return (true, null);
            }

            // Ders verdigi ogrenci mi?
            var hasLesson = await _context.LessonSchedules
                .AnyAsync(ls => ls.TeacherId == teacher.Id && ls.StudentId == recipientStudent.Id);

            if (hasLesson)
            {
                return (true, null);
            }

            return (false, "Bu öğrenciye mesaj atma yetkiniz yok.");
        }

        // Veliye mesaj
        if (recipientRoles.Contains("Veli"))
        {
            var recipientParent = await _context.Parents.FirstOrDefaultAsync(p => p.UserId == recipientUserId);
            if (recipientParent == null)
            {
                return (false, "Veli bulunamadı.");
            }

            // Danismanlik yaptigi ogrencilerin velisi mi?
            var isParentOfCounselorStudent = await _context.StudentParents
                .AnyAsync(sp => counselorStudentIds.Contains(sp.StudentId) && sp.ParentId == recipientParent.Id);

            if (isParentOfCounselorStudent)
            {
                return (true, null);
            }

            return (false, "Bu veliye mesaj atma yetkiniz yok.");
        }

        // Admin'e her zaman mesaj atabilir
        if (recipientRoles.Contains("Admin"))
        {
            return (true, null);
        }

        return (false, "Bu kişiye mesaj atma yetkiniz bulunmamaktadır.");
    }

    private async Task<List<string>> GetTeacherStudentsAsync(string teacherUserId)
    {
        _logger.LogInformation("[GetTeacherStudentsAsync] teacherUserId: {TeacherUserId}", teacherUserId);
        var userIds = new List<string>();
        var today = DateTime.Today;
        _logger.LogInformation("[GetTeacherStudentsAsync] Today: {Today}", today);

        var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == teacherUserId);
        if (teacher == null)
        {
            _logger.LogWarning("[GetTeacherStudentsAsync] Teacher not found for UserId: {TeacherUserId}", teacherUserId);
            return userIds;
        }
        _logger.LogInformation("[GetTeacherStudentsAsync] Found Teacher Id: {TeacherId}", teacher.Id);

        // Bireysel ders ogrencileri - sadece aktif dersler
        // Aktif ders: IsDeleted = false VE (EffectiveTo null VEYA EffectiveTo >= bugun)
        var individualStudentUserIds = await _context.LessonSchedules
            .Where(ls => ls.TeacherId == teacher.Id &&
                        ls.Student != null &&
                        !ls.IsDeleted &&
                        (ls.EffectiveTo == null || ls.EffectiveTo >= today))
            .Select(ls => ls.Student!.UserId)
            .Distinct()
            .ToListAsync();

        _logger.LogInformation("[GetTeacherStudentsAsync] Individual lesson students: {Count}", individualStudentUserIds.Count);
        userIds.AddRange(individualStudentUserIds);

        // Grup dersi ogrencileri (GroupLessonSchedule uzerinden) - sadece aktif dersler
        var teacherGroupIds = await _context.GroupLessonSchedules
            .Where(gls => gls.TeacherId == teacher.Id &&
                         !gls.IsDeleted &&
                         (gls.EffectiveTo == null || gls.EffectiveTo >= today))
            .Select(gls => gls.GroupId)
            .Distinct()
            .ToListAsync();

        _logger.LogInformation("[GetTeacherStudentsAsync] Teacher group IDs: {Count}", teacherGroupIds.Count);

        var groupStudentUserIds = await _context.StudentGroupMembers
            .Where(gm => teacherGroupIds.Contains(gm.GroupId) &&
                        gm.IsActive &&
                        !gm.IsDeleted)
            .Include(gm => gm.Student)
            .Select(gm => gm.Student.UserId)
            .Distinct()
            .ToListAsync();

        _logger.LogInformation("[GetTeacherStudentsAsync] Group lesson students: {Count}", groupStudentUserIds.Count);
        userIds.AddRange(groupStudentUserIds);

        _logger.LogInformation("[GetTeacherStudentsAsync] Total students: {Count}", userIds.Distinct().Count());
        return userIds.Distinct().ToList();
    }

    private async Task<List<string>> GetCounselorStudentsAndParentsAsync(string teacherUserId)
    {
        var userIds = new List<string>();

        var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == teacherUserId);
        if (teacher == null) return userIds;

        var counselor = await _context.Counselors.FirstOrDefaultAsync(c => c.TeacherId == teacher.Id);
        if (counselor == null) return userIds;

        // Danismanlik yaptigi ogrenciler
        var counselorStudentIds = await _context.StudentCounselorAssignments
            .Where(sca => sca.CounselorId == counselor.Id)
            .Select(sca => sca.StudentId)
            .ToListAsync();

        var studentUserIds = await _context.Students
            .Where(s => counselorStudentIds.Contains(s.Id))
            .Select(s => s.UserId)
            .ToListAsync();

        userIds.AddRange(studentUserIds);

        // Bu ogrencilerin velileri
        var parentUserIds = await _context.StudentParents
            .Where(sp => counselorStudentIds.Contains(sp.StudentId))
            .Include(sp => sp.Parent)
            .Select(sp => sp.Parent.UserId)
            .Distinct()
            .ToListAsync();

        userIds.AddRange(parentUserIds);

        return userIds.Distinct().ToList();
    }

    private async Task<List<string>> GetStudentRecipientsAsync(string studentUserId)
    {
        var userIds = new List<string>();
        var today = DateTime.Today;

        var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == studentUserId);
        if (student == null) return userIds;

        // Ogretmenler (bireysel ders) - sadece aktif dersler
        var teacherUserIds = await _context.LessonSchedules
            .Where(ls => ls.StudentId == student.Id &&
                        ls.Teacher != null &&
                        !ls.IsDeleted &&
                        (ls.EffectiveTo == null || ls.EffectiveTo >= today))
            .Select(ls => ls.Teacher!.UserId)
            .Distinct()
            .ToListAsync();

        userIds.AddRange(teacherUserIds);

        // Ogrencinin aktif grup uyelikleri
        var studentGroupIds = await _context.StudentGroupMembers
            .Where(gm => gm.StudentId == student.Id &&
                        gm.IsActive &&
                        !gm.IsDeleted)
            .Select(gm => gm.GroupId)
            .ToListAsync();

        // Grup ogretmenleri (GroupLessonSchedule uzerinden) - sadece aktif dersler
        var groupTeacherUserIds = await _context.GroupLessonSchedules
            .Where(gls => studentGroupIds.Contains(gls.GroupId) &&
                         !gls.IsDeleted &&
                         (gls.EffectiveTo == null || gls.EffectiveTo >= today))
            .Include(gls => gls.Teacher)
            .Where(gls => gls.Teacher != null)
            .Select(gls => gls.Teacher!.UserId)
            .Distinct()
            .ToListAsync();

        userIds.AddRange(groupTeacherUserIds);

        // Danisman ogretmen (danismanlik iliskisi aktif oldugu surece)
        var counselorTeacherUserIds = await _context.StudentCounselorAssignments
            .Where(sca => sca.StudentId == student.Id && !sca.IsDeleted)
            .Include(sca => sca.Counselor)
                .ThenInclude(c => c!.Teacher)
            .Where(sca => sca.Counselor != null && sca.Counselor.Teacher != null)
            .Select(sca => sca.Counselor!.Teacher!.UserId)
            .ToListAsync();

        userIds.AddRange(counselorTeacherUserIds);

        // Grup arkadaslari (sadece aktif uyelikler)
        var groupMateUserIds = await _context.StudentGroupMembers
            .Where(gm => studentGroupIds.Contains(gm.GroupId) &&
                        gm.StudentId != student.Id &&
                        gm.IsActive &&
                        !gm.IsDeleted)
            .Include(gm => gm.Student)
            .Select(gm => gm.Student.UserId)
            .Distinct()
            .ToListAsync();

        userIds.AddRange(groupMateUserIds);

        return userIds.Distinct().ToList();
    }

    private async Task<List<string>> GetParentRecipientsAsync(string parentUserId)
    {
        _logger.LogInformation("[GetParentRecipientsAsync] Starting for parentUserId: {ParentUserId}", parentUserId);
        var userIds = new List<string>();

        var parent = await _context.Parents.FirstOrDefaultAsync(p => p.UserId == parentUserId);
        if (parent == null)
        {
            _logger.LogWarning("[GetParentRecipientsAsync] Parent not found for UserId: {ParentUserId}", parentUserId);
            return userIds;
        }
        _logger.LogInformation("[GetParentRecipientsAsync] Found parent with Id: {ParentId}", parent.Id);

        // Cocuklarini bul
        var childrenIds = await _context.StudentParents
            .Where(sp => sp.ParentId == parent.Id)
            .Select(sp => sp.StudentId)
            .ToListAsync();

        _logger.LogInformation("[GetParentRecipientsAsync] Parent {ParentId} has {Count} children", parent.Id, childrenIds.Count);

        // 1. Cocuklarinin danisman ogretmenleri
        var counselorTeacherUserIds = await _context.StudentCounselorAssignments
            .Where(sca => childrenIds.Contains(sca.StudentId) && !sca.IsDeleted)
            .Include(sca => sca.Counselor)
                .ThenInclude(c => c!.Teacher)
            .Where(sca => sca.Counselor != null && sca.Counselor.Teacher != null)
            .Select(sca => sca.Counselor!.Teacher!.UserId)
            .Distinct()
            .ToListAsync();

        _logger.LogInformation("[GetParentRecipientsAsync] Found {Count} counselor teachers", counselorTeacherUserIds.Count);
        userIds.AddRange(counselorTeacherUserIds);

        // 2. Cocuklarinin bireysel ders ogretmenleri (LessonSchedules)
        // Not: EffectiveTo filtresi kaldirildi - veli, cocuguyla gecmiste veya simdiki zamanda ders yapan ogretmenlere ulasabilmeli
        var individualLessonTeacherUserIds = await _context.LessonSchedules
            .Where(ls => childrenIds.Contains(ls.StudentId) &&
                        ls.Teacher != null &&
                        !ls.IsDeleted)
            .Select(ls => ls.Teacher!.UserId)
            .Distinct()
            .ToListAsync();

        _logger.LogInformation("[GetParentRecipientsAsync] Found {Count} individual lesson teachers", individualLessonTeacherUserIds.Count);
        userIds.AddRange(individualLessonTeacherUserIds);

        // 3. Cocuklarinin grup dersi ogretmenleri (GroupLessonSchedules)
        // Oncelikle cocuklarin uye oldugu gruplari bul
        var childGroupIds = await _context.StudentGroupMembers
            .Where(gm => childrenIds.Contains(gm.StudentId) &&
                        gm.IsActive &&
                        !gm.IsDeleted)
            .Select(gm => gm.GroupId)
            .Distinct()
            .ToListAsync();

        _logger.LogInformation("[GetParentRecipientsAsync] Children are in {Count} groups", childGroupIds.Count);

        // Not: EffectiveTo filtresi kaldirildi - veli, cocuguyla gecmiste veya simdiki zamanda ders yapan ogretmenlere ulasabilmeli
        var groupLessonTeacherUserIds = await _context.GroupLessonSchedules
            .Where(gls => childGroupIds.Contains(gls.GroupId) &&
                         !gls.IsDeleted)
            .Include(gls => gls.Teacher)
            .Where(gls => gls.Teacher != null)
            .Select(gls => gls.Teacher!.UserId)
            .Distinct()
            .ToListAsync();

        _logger.LogInformation("[GetParentRecipientsAsync] Found {Count} group lesson teachers", groupLessonTeacherUserIds.Count);
        userIds.AddRange(groupLessonTeacherUserIds);

        var result = userIds.Distinct().ToList();
        _logger.LogInformation("[GetParentRecipientsAsync] Total recipients for parent: {Count}", result.Count);

        return result;
    }

    private async Task<List<string>> GetCounselorRecipientsAsync(string counselorUserId)
    {
        // Danisman bir ogretmen, GetTeacherStudentsAsync + GetCounselorStudentsAndParentsAsync
        var userIds = new List<string>();

        userIds.AddRange(await GetTeacherStudentsAsync(counselorUserId));
        userIds.AddRange(await GetCounselorStudentsAndParentsAsync(counselorUserId));

        return userIds.Distinct().ToList();
    }

    #endregion
}
