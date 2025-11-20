using EduPortal.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Parent> Parents => Set<Parent>();
    public DbSet<StudentSibling> StudentSiblings => Set<StudentSibling>();
    public DbSet<StudentHobby> StudentHobbies => Set<StudentHobby>();
    public DbSet<StudentClubMembership> StudentClubMemberships => Set<StudentClubMembership>();
    public DbSet<CompetitionAndAward> CompetitionsAndAwards => Set<CompetitionAndAward>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Counselor> Counselors => Set<Counselor>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Curriculum> Curricula => Set<Curriculum>();
    public DbSet<CourseResource> CourseResources => Set<CourseResource>();
    public DbSet<StudentTeacherAssignment> StudentTeacherAssignments => Set<StudentTeacherAssignment>();
    public DbSet<Homework> Homeworks => Set<Homework>();
    public DbSet<StudentHomeworkSubmission> StudentHomeworkSubmissions => Set<StudentHomeworkSubmission>();
    public DbSet<InternalExam> InternalExams => Set<InternalExam>();
    public DbSet<ExamResult> ExamResults => Set<ExamResult>();
    public DbSet<InternationalExam> InternationalExams => Set<InternationalExam>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<ClassPerformance> ClassPerformances => Set<ClassPerformance>();
    public DbSet<AcademicDevelopmentPlan> AcademicDevelopmentPlans => Set<AcademicDevelopmentPlan>();
    public DbSet<AGPMilestone> AGPMilestones => Set<AGPMilestone>();
    public DbSet<StudentCounselorAssignment> StudentCounselorAssignments => Set<StudentCounselorAssignment>();
    public DbSet<CounselingMeeting> CounselingMeetings => Set<CounselingMeeting>();
    public DbSet<StudentDocument> StudentDocuments => Set<StudentDocument>();
    public DbSet<UniversityApplication> UniversityApplications => Set<UniversityApplication>();
    public DbSet<UniversitySpecificExam> UniversitySpecificExams => Set<UniversitySpecificExam>();
    public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<MessageGroup> MessageGroups => Set<MessageGroup>();
    public DbSet<MessageGroupMember> MessageGroupMembers => Set<MessageGroupMember>();
    public DbSet<GroupMessage> GroupMessages => Set<GroupMessage>();
    public DbSet<PaymentPlan> PaymentPlans => Set<PaymentPlan>();
    public DbSet<PaymentInstallment> PaymentInstallments => Set<PaymentInstallment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all entity configurations from assembly
        // builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Configure ApplicationUser relationships
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.HasOne(u => u.Student)
                .WithOne(s => s.User)
                .HasForeignKey<Student>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(u => u.Teacher)
                .WithOne(t => t.User)
                .HasForeignKey<Teacher>(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(u => u.Counselor)
                .WithOne(c => c.User)
                .HasForeignKey<Counselor>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.SentMessages)
                .WithOne(m => m.Sender)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(u => u.ReceivedMessages)
                .WithOne(m => m.Recipient)
                .HasForeignKey(m => m.RecipientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Student unique index
        builder.Entity<Student>(entity =>
        {
            entity.HasIndex(s => s.StudentNo)
                .IsUnique();
        });

        // Configure Message self-referencing relationship
        builder.Entity<Message>(entity =>
        {
            entity.HasOne(m => m.ParentMessage)
                .WithMany(m => m.Replies)
                .HasForeignKey(m => m.ParentMessageId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Fix multiple cascade paths for Student-related entities
        builder.Entity<CounselingMeeting>(entity =>
        {
            entity.HasOne(cm => cm.Student)
                .WithMany(s => s.CounselingMeetings)
                .HasForeignKey(cm => cm.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(cm => cm.Counselor)
                .WithMany(c => c.CounselingMeetings)
                .HasForeignKey(cm => cm.CounselorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<StudentCounselorAssignment>(entity =>
        {
            entity.HasOne(sca => sca.Student)
                .WithMany(s => s.CounselorAssignments)
                .HasForeignKey(sca => sca.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(sca => sca.Counselor)
                .WithMany(c => c.Students)
                .HasForeignKey(sca => sca.CounselorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<StudentTeacherAssignment>(entity =>
        {
            entity.HasOne(sta => sta.Student)
                .WithMany(s => s.TeacherAssignments)
                .HasForeignKey(sta => sta.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(sta => sta.Teacher)
                .WithMany(t => t.StudentAssignments)
                .HasForeignKey(sta => sta.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<AcademicDevelopmentPlan>(entity =>
        {
            entity.HasOne(adp => adp.Student)
                .WithMany(s => s.AcademicDevelopmentPlans)
                .HasForeignKey(adp => adp.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<UniversityApplication>(entity =>
        {
            entity.HasOne(ua => ua.Student)
                .WithMany(s => s.UniversityApplications)
                .HasForeignKey(ua => ua.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<PaymentPlan>(entity =>
        {
            entity.HasOne(pp => pp.Student)
                .WithMany(s => s.PaymentPlans)
                .HasForeignKey(pp => pp.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Attendance>(entity =>
        {
            entity.HasOne(a => a.Student)
                .WithMany(s => s.Attendances)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Course)
                .WithMany(c => c.Attendances)
                .HasForeignKey(a => a.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ExamResult>(entity =>
        {
            entity.HasOne(er => er.Student)
                .WithMany(s => s.ExamResults)
                .HasForeignKey(er => er.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<InternationalExam>(entity =>
        {
            entity.HasOne(ie => ie.Student)
                .WithMany(s => s.InternationalExams)
                .HasForeignKey(ie => ie.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<CompetitionAndAward>(entity =>
        {
            entity.HasOne(ca => ca.Student)
                .WithMany(s => s.Competitions)
                .HasForeignKey(ca => ca.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<StudentDocument>(entity =>
        {
            entity.HasOne(sd => sd.Student)
                .WithMany(s => s.Documents)
                .HasForeignKey(sd => sd.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<StudentHomeworkSubmission>(entity =>
        {
            entity.HasOne(shs => shs.Student)
                .WithMany(s => s.Homeworks)
                .HasForeignKey(shs => shs.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(shs => shs.Homework)
                .WithMany(h => h.Submissions)
                .HasForeignKey(shs => shs.HomeworkId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Global query filter for soft delete
        builder.Entity<Student>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Parent>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Teacher>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Counselor>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Course>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Homework>().HasQueryFilter(e => !e.IsDeleted);

        // Configure decimal precision
        foreach (var property in builder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            if (property.GetColumnType() == null)
            {
                property.SetColumnType("decimal(18,2)");
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity is Domain.Common.BaseEntity baseEntity)
                {
                    baseEntity.CreatedAt = DateTime.UtcNow;
                }
            }

            if (entry.State == EntityState.Modified)
            {
                if (entry.Entity is Domain.Common.BaseEntity baseEntity)
                {
                    baseEntity.UpdatedAt = DateTime.UtcNow;
                }
            }

            if (entry.State == EntityState.Deleted)
            {
                if (entry.Entity is Domain.Common.BaseEntity baseEntity)
                {
                    entry.State = EntityState.Modified;
                    baseEntity.IsDeleted = true;
                    baseEntity.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
