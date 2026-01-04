using EduPortal.Domain.Entities;
using EduPortal.Domain.Entities.Messaging;
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
    public DbSet<StudentParent> StudentParents => Set<StudentParent>();
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
    public DbSet<HomeworkAssignment> HomeworkAssignments => Set<HomeworkAssignment>();
    public DbSet<HomeworkSubmissionFile> HomeworkSubmissionFiles => Set<HomeworkSubmissionFile>();
    public DbSet<HomeworkViewLog> HomeworkViewLogs => Set<HomeworkViewLog>();
    public DbSet<InternalExam> InternalExams => Set<InternalExam>();
    public DbSet<ExamResult> ExamResults => Set<ExamResult>();
    public DbSet<InternationalExam> InternationalExams => Set<InternationalExam>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<ClassPerformance> ClassPerformances => Set<ClassPerformance>();
    public DbSet<AcademicDevelopmentPlan> AcademicDevelopmentPlans => Set<AcademicDevelopmentPlan>();
    public DbSet<AGPMilestone> AGPMilestones => Set<AGPMilestone>();

    // AGP Timeline
    public DbSet<AgpPeriod> AgpPeriods => Set<AgpPeriod>();
    public DbSet<AgpTimelineMilestone> AgpTimelineMilestones => Set<AgpTimelineMilestone>();
    public DbSet<AgpActivity> AgpActivities => Set<AgpActivity>();
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
    public DbSet<StudentPaymentPlan> StudentPaymentPlans => Set<StudentPaymentPlan>();
    public DbSet<PaymentInstallment> PaymentInstallments => Set<PaymentInstallment>();
    public DbSet<Class> Classes => Set<Class>();
    public DbSet<StudentClassAssignment> StudentClassAssignments => Set<StudentClassAssignment>();
    public DbSet<WeeklySchedule> WeeklySchedules => Set<WeeklySchedule>();
    public DbSet<Classroom> Classrooms => Set<Classroom>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<AcademicTerm> AcademicTerms => Set<AcademicTerm>();
    public DbSet<Club> Clubs => Set<Club>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // Study Abroad
    public DbSet<StudyAbroadProgram> StudyAbroadPrograms => Set<StudyAbroadProgram>();
    public DbSet<ApplicationDocument> ApplicationDocuments => Set<ApplicationDocument>();
    public DbSet<VisaProcess> VisaProcesses => Set<VisaProcess>();
    public DbSet<AccommodationArrangement> AccommodationArrangements => Set<AccommodationArrangement>();

    // Assessments
    public DbSet<CareerAssessment> CareerAssessments => Set<CareerAssessment>();
    public DbSet<SchoolRecommendation> SchoolRecommendations => Set<SchoolRecommendation>();
    public DbSet<SportsAssessment> SportsAssessments => Set<SportsAssessment>();

    // Packages
    public DbSet<ServicePackage> ServicePackages => Set<ServicePackage>();
    public DbSet<StudentPackagePurchase> StudentPackagePurchases => Set<StudentPackagePurchase>();

    // Branch
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<StudentBranchTransfer> StudentBranchTransfers => Set<StudentBranchTransfer>();

    // Scheduling
    public DbSet<StudentAvailability> StudentAvailabilities => Set<StudentAvailability>();
    public DbSet<TeacherAvailability> TeacherAvailabilities => Set<TeacherAvailability>();
    public DbSet<LessonSchedule> LessonSchedules => Set<LessonSchedule>();

    // Student Activities
    public DbSet<StudentSummerActivity> StudentSummerActivities => Set<StudentSummerActivity>();
    public DbSet<StudentInternship> StudentInternships => Set<StudentInternship>();
    public DbSet<StudentSocialProject> StudentSocialProjects => Set<StudentSocialProject>();
    public DbSet<SimpleInternship> SimpleInternships => Set<SimpleInternship>();

    // Teacher Extended Form
    public DbSet<TeacherAddress> TeacherAddresses => Set<TeacherAddress>();
    public DbSet<TeacherBranch> TeacherBranches => Set<TeacherBranch>();
    public DbSet<TeacherCertificate> TeacherCertificates => Set<TeacherCertificate>();
    public DbSet<TeacherReference> TeacherReferences => Set<TeacherReference>();
    public DbSet<TeacherWorkType> TeacherWorkTypes => Set<TeacherWorkType>();

    // Student Groups
    public DbSet<StudentGroup> StudentGroups => Set<StudentGroup>();
    public DbSet<StudentGroupMember> StudentGroupMembers => Set<StudentGroupMember>();
    public DbSet<GroupLessonSchedule> GroupLessonSchedules => Set<GroupLessonSchedule>();

    // Admin Calendar
    public DbSet<AdminCalendarEvent> AdminCalendarEvents => Set<AdminCalendarEvent>();

    // Student Certificates
    public DbSet<StudentCertificate> StudentCertificates => Set<StudentCertificate>();

    // Student Extended Info (Foreign Languages, Activities, Readiness Exams)
    public DbSet<StudentForeignLanguage> StudentForeignLanguages => Set<StudentForeignLanguage>();
    public DbSet<StudentActivity> StudentActivities => Set<StudentActivity>();
    public DbSet<StudentReadinessExam> StudentReadinessExams => Set<StudentReadinessExam>();

    // Permission System
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    // Counselor Dashboard Related
    public DbSet<CounselorNote> CounselorNotes => Set<CounselorNote>();
    public DbSet<UniversityRequirement> UniversityRequirements => Set<UniversityRequirement>();
    public DbSet<StudentExamCalendar> StudentExamCalendars => Set<StudentExamCalendar>();
    public DbSet<CharacterixResult> CharacterixResults => Set<CharacterixResult>();
    public DbSet<StudentAward> StudentAwards => Set<StudentAward>();

    // Homework Progress & Curriculum Tracking
    public DbSet<HomeworkAttachment> HomeworkAttachments => Set<HomeworkAttachment>();
    public DbSet<HomeworkDraft> HomeworkDrafts => Set<HomeworkDraft>();
    public DbSet<StudentCurriculumProgress> StudentCurriculumProgresses => Set<StudentCurriculumProgress>();
    public DbSet<Curriculum> Curriculums => Set<Curriculum>();

    // ===============================================
    // MESSAGING MODULE
    // ===============================================
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationParticipant> ConversationParticipants => Set<ConversationParticipant>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<MessageReadReceipt> MessageReadReceipts => Set<MessageReadReceipt>();
    public DbSet<MessageDeliveryReceipt> MessageDeliveryReceipts => Set<MessageDeliveryReceipt>();
    public DbSet<AdminMessageAccessLog> AdminMessageAccessLogs => Set<AdminMessageAccessLog>();
    public DbSet<BroadcastMessage> BroadcastMessages => Set<BroadcastMessage>();
    public DbSet<BroadcastMessageRecipient> BroadcastMessageRecipients => Set<BroadcastMessageRecipient>();
    public DbSet<PushSubscription> PushSubscriptions => Set<PushSubscription>();
    public DbSet<MessageArchive> MessageArchives => Set<MessageArchive>();

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

        // ===============================================
        // AGP TIMELINE RELATIONSHIPS
        // ===============================================

        builder.Entity<AgpPeriod>(entity =>
        {
            entity.HasOne(p => p.Agp)
                .WithMany(a => a.Periods)
                .HasForeignKey(p => p.AgpId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AgpTimelineMilestone>(entity =>
        {
            entity.HasOne(m => m.Period)
                .WithMany(p => p.Milestones)
                .HasForeignKey(m => m.AgpPeriodId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AgpActivity>(entity =>
        {
            entity.HasOne(a => a.Period)
                .WithMany(p => p.Activities)
                .HasForeignKey(a => a.AgpPeriodId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<UniversityApplication>(entity =>
        {
            entity.HasOne(ua => ua.Student)
                .WithMany(s => s.UniversityApplications)
                .HasForeignKey(ua => ua.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // StudentPaymentPlan relationships
        builder.Entity<StudentPaymentPlan>(entity =>
        {
            entity.HasOne(spp => spp.Student)
                .WithMany(s => s.PaymentPlans)
                .HasForeignKey(spp => spp.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(spp => spp.PaymentPlan)
                .WithMany(pp => pp.StudentPaymentPlans)
                .HasForeignKey(spp => spp.PaymentPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(spp => spp.PackagePurchase)
                .WithMany()
                .HasForeignKey(spp => spp.PackagePurchaseId)
                .OnDelete(DeleteBehavior.SetNull);
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

            entity.HasOne(a => a.Teacher)
                .WithMany(t => t.Attendances)
                .HasForeignKey(a => a.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ClassPerformance>(entity =>
        {
            entity.HasOne(cp => cp.Student)
                .WithMany(s => s.ClassPerformances)
                .HasForeignKey(cp => cp.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(cp => cp.Course)
                .WithMany(c => c.ClassPerformances)
                .HasForeignKey(cp => cp.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(cp => cp.Teacher)
                .WithMany(t => t.ClassPerformances)
                .HasForeignKey(cp => cp.TeacherId)
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

        builder.Entity<Homework>(entity =>
        {
            entity.HasOne(h => h.Course)
                .WithMany(c => c.Homeworks)
                .HasForeignKey(h => h.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(h => h.Teacher)
                .WithMany(t => t.Homeworks)
                .HasForeignKey(h => h.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<InternalExam>(entity =>
        {
            entity.HasOne(ie => ie.Course)
                .WithMany(c => c.InternalExams)
                .HasForeignKey(ie => ie.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ie => ie.Teacher)
                .WithMany(t => t.InternalExams)
                .HasForeignKey(ie => ie.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Parent>(entity =>
        {
            entity.HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // StudentParent N:N relationship
        builder.Entity<StudentParent>(entity =>
        {
            entity.HasOne(sp => sp.Student)
                .WithMany(s => s.Parents)
                .HasForeignKey(sp => sp.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sp => sp.Parent)
                .WithMany(p => p.Students)
                .HasForeignKey(sp => sp.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Payment relationships (all Restrict to avoid cascade cycles)
        builder.Entity<Payment>(entity =>
        {
            entity.HasOne(p => p.Student)
                .WithMany()
                .HasForeignKey(p => p.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Installment)
                .WithMany()
                .HasForeignKey(p => p.InstallmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.PaymentPlan)
                .WithMany()
                .HasForeignKey(p => p.PaymentPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Branch)
                .WithMany()
                .HasForeignKey(p => p.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.PackagePurchase)
                .WithMany()
                .HasForeignKey(p => p.PackagePurchaseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // AcademicTerm relationships
        builder.Entity<InternalExam>(entity =>
        {
            entity.HasOne(e => e.AcademicTerm)
                .WithMany(at => at.Exams)
                .HasForeignKey(e => e.AcademicTermId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Class)
                .WithMany(c => c.Exams)
                .HasForeignKey(e => e.ClassId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Homework>(entity =>
        {
            entity.HasOne(h => h.AcademicTerm)
                .WithMany(at => at.Homeworks)
                .HasForeignKey(h => h.AcademicTermId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(h => h.Class)
                .WithMany(c => c.Homeworks)
                .HasForeignKey(h => h.ClassId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<StudentClassAssignment>(entity =>
        {
            entity.HasOne(sca => sca.AcademicTerm)
                .WithMany(at => at.ClassAssignments)
                .HasForeignKey(sca => sca.AcademicTermId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<WeeklySchedule>(entity =>
        {
            entity.HasOne(ws => ws.AcademicTerm)
                .WithMany(at => at.Schedules)
                .HasForeignKey(ws => ws.AcademicTermId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // CalendarEvent class relationship
        builder.Entity<CalendarEvent>(entity =>
        {
            entity.HasOne(ce => ce.Class)
                .WithMany()
                .HasForeignKey(ce => ce.ClassId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Class relationships
        builder.Entity<Class>(entity =>
        {
            entity.HasOne(c => c.ClassTeacher)
                .WithMany()
                .HasForeignKey(c => c.ClassTeacherId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // WeeklySchedule relationships
        builder.Entity<WeeklySchedule>(entity =>
        {
            entity.HasOne(ws => ws.Class)
                .WithMany(c => c.Schedules)
                .HasForeignKey(ws => ws.ClassId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ws => ws.Course)
                .WithMany()
                .HasForeignKey(ws => ws.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ws => ws.Teacher)
                .WithMany()
                .HasForeignKey(ws => ws.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ws => ws.Classroom)
                .WithMany()
                .HasForeignKey(ws => ws.ClassroomId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Notification relationship
        builder.Entity<Notification>(entity =>
        {
            entity.HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // StudentClassAssignment relationship
        builder.Entity<StudentClassAssignment>(entity =>
        {
            entity.HasOne(sca => sca.Student)
                .WithMany()
                .HasForeignKey(sca => sca.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sca => sca.Class)
                .WithMany(c => c.Students)
                .HasForeignKey(sca => sca.ClassId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Club relationship
        builder.Entity<Club>(entity =>
        {
            entity.HasOne(c => c.Advisor)
                .WithMany()
                .HasForeignKey(c => c.AdvisorTeacherId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Announcement relationship
        builder.Entity<Announcement>(entity =>
        {
            entity.HasOne(a => a.Publisher)
                .WithMany()
                .HasForeignKey(a => a.PublishedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ===============================================
        // STUDY ABROAD MODULE RELATIONSHIPS
        // ===============================================

        // StudyAbroadProgram relationships
        builder.Entity<StudyAbroadProgram>(entity =>
        {
            entity.HasOne(sap => sap.Student)
                .WithMany(s => s.StudyAbroadPrograms)
                .HasForeignKey(sap => sap.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sap => sap.Counselor)
                .WithMany()
                .HasForeignKey(sap => sap.CounselorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ApplicationDocument relationships
        builder.Entity<ApplicationDocument>(entity =>
        {
            entity.HasOne(ad => ad.Program)
                .WithMany(sap => sap.Documents)
                .HasForeignKey(ad => ad.ProgramId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // VisaProcess relationships
        builder.Entity<VisaProcess>(entity =>
        {
            entity.HasOne(vp => vp.Program)
                .WithMany(sap => sap.VisaProcesses)
                .HasForeignKey(vp => vp.ProgramId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AccommodationArrangement relationships
        builder.Entity<AccommodationArrangement>(entity =>
        {
            entity.HasOne(aa => aa.Program)
                .WithMany(sap => sap.Accommodations)
                .HasForeignKey(aa => aa.ProgramId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ===============================================
        // ASSESSMENT MODULE RELATIONSHIPS
        // ===============================================

        // CareerAssessment relationships
        builder.Entity<CareerAssessment>(entity =>
        {
            entity.HasOne(ca => ca.Student)
                .WithMany(s => s.CareerAssessments)
                .HasForeignKey(ca => ca.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ca => ca.Counselor)
                .WithMany()
                .HasForeignKey(ca => ca.CounselorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // SchoolRecommendation relationships
        builder.Entity<SchoolRecommendation>(entity =>
        {
            entity.HasOne(sr => sr.Student)
                .WithMany(s => s.SchoolRecommendations)
                .HasForeignKey(sr => sr.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sr => sr.Counselor)
                .WithMany()
                .HasForeignKey(sr => sr.CounselorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // SportsAssessment relationships
        builder.Entity<SportsAssessment>(entity =>
        {
            entity.HasOne(sa => sa.Student)
                .WithMany(s => s.SportsAssessments)
                .HasForeignKey(sa => sa.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sa => sa.Counselor)
                .WithMany()
                .HasForeignKey(sa => sa.CounselorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ===============================================
        // PACKAGE MODULE RELATIONSHIPS
        // ===============================================

        // StudentPackagePurchase relationships
        builder.Entity<StudentPackagePurchase>(entity =>
        {
            entity.HasOne(spp => spp.Student)
                .WithMany(s => s.PackagePurchases)
                .HasForeignKey(spp => spp.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(spp => spp.Package)
                .WithMany(sp => sp.Purchases)
                .HasForeignKey(spp => spp.PackageId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ===============================================
        // BRANCH MODULE RELATIONSHIPS
        // ===============================================

        // Branch relationships
        builder.Entity<Branch>(entity =>
        {
            entity.HasOne(b => b.Manager)
                .WithMany()
                .HasForeignKey(b => b.ManagerId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(b => b.BranchCode)
                .IsUnique();
        });

        // StudentBranchTransfer relationships
        builder.Entity<StudentBranchTransfer>(entity =>
        {
            entity.HasOne(sbt => sbt.Student)
                .WithMany()
                .HasForeignKey(sbt => sbt.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(sbt => sbt.FromBranch)
                .WithMany()
                .HasForeignKey(sbt => sbt.FromBranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(sbt => sbt.ToBranch)
                .WithMany()
                .HasForeignKey(sbt => sbt.ToBranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(sbt => sbt.Approver)
                .WithMany()
                .HasForeignKey(sbt => sbt.ApprovedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Branch relationships with other entities
        builder.Entity<Student>(entity =>
        {
            entity.HasOne(s => s.Branch)
                .WithMany(b => b.Students)
                .HasForeignKey(s => s.BranchId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Teacher>(entity =>
        {
            entity.HasOne(t => t.Branch)
                .WithMany(b => b.Teachers)
                .HasForeignKey(t => t.BranchId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ===============================================
        // TEACHER EXTENDED FORM RELATIONSHIPS
        // ===============================================

        // TeacherAddress relationships (one-to-one)
        builder.Entity<TeacherAddress>(entity =>
        {
            entity.HasOne(ta => ta.Teacher)
                .WithOne(t => t.Address)
                .HasForeignKey<TeacherAddress>(ta => ta.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TeacherBranch relationships (many-to-many through junction table)
        builder.Entity<TeacherBranch>(entity =>
        {
            entity.HasOne(tb => tb.Teacher)
                .WithMany(t => t.TeacherBranches)
                .HasForeignKey(tb => tb.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(tb => tb.Course)
                .WithMany()
                .HasForeignKey(tb => tb.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TeacherCertificate relationships
        builder.Entity<TeacherCertificate>(entity =>
        {
            entity.HasOne(tc => tc.Teacher)
                .WithMany(t => t.TeacherCertificates)
                .HasForeignKey(tc => tc.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TeacherReference relationships
        builder.Entity<TeacherReference>(entity =>
        {
            entity.HasOne(tr => tr.Teacher)
                .WithMany(t => t.TeacherReferences)
                .HasForeignKey(tr => tr.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TeacherWorkType relationships
        builder.Entity<TeacherWorkType>(entity =>
        {
            entity.HasOne(twt => twt.Teacher)
                .WithMany(t => t.TeacherWorkTypes)
                .HasForeignKey(twt => twt.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Class>(entity =>
        {
            entity.HasOne(c => c.BranchLocation)
                .WithMany(b => b.Classes)
                .HasForeignKey(c => c.BranchId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Classroom>(entity =>
        {
            entity.HasOne(c => c.Branch)
                .WithMany(b => b.Classrooms)
                .HasForeignKey(c => c.BranchId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ===============================================
        // SCHEDULING MODULE RELATIONSHIPS
        // ===============================================

        // StudentAvailability relationships
        builder.Entity<StudentAvailability>(entity =>
        {
            entity.HasOne(sa => sa.Student)
                .WithMany()
                .HasForeignKey(sa => sa.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(sa => new { sa.StudentId, sa.DayOfWeek, sa.StartTime });
        });

        // TeacherAvailability relationships
        builder.Entity<TeacherAvailability>(entity =>
        {
            entity.HasOne(ta => ta.Teacher)
                .WithMany()
                .HasForeignKey(ta => ta.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(ta => new { ta.TeacherId, ta.DayOfWeek, ta.StartTime });
        });

        // LessonSchedule relationships
        builder.Entity<LessonSchedule>(entity =>
        {
            entity.HasOne(ls => ls.Student)
                .WithMany()
                .HasForeignKey(ls => ls.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ls => ls.Teacher)
                .WithMany()
                .HasForeignKey(ls => ls.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ls => ls.Course)
                .WithMany()
                .HasForeignKey(ls => ls.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ls => ls.Classroom)
                .WithMany()
                .HasForeignKey(ls => ls.ClassroomId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(ls => new { ls.StudentId, ls.TeacherId, ls.DayOfWeek });
        });

        // ===============================================
        // STUDENT ACTIVITIES MODULE RELATIONSHIPS
        // ===============================================

        // StudentSummerActivity relationships
        builder.Entity<StudentSummerActivity>(entity =>
        {
            entity.HasOne(ssa => ssa.Student)
                .WithMany(s => s.SummerActivities)
                .HasForeignKey(ssa => ssa.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // StudentInternship relationships
        builder.Entity<StudentInternship>(entity =>
        {
            entity.HasOne(si => si.Student)
                .WithMany(s => s.Internships)
                .HasForeignKey(si => si.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // SimpleInternship relationships
        builder.Entity<SimpleInternship>(entity =>
        {
            entity.HasOne(si => si.Student)
                .WithMany(s => s.SimpleInternships)
                .HasForeignKey(si => si.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // StudentSocialProject relationships
        builder.Entity<StudentSocialProject>(entity =>
        {
            entity.HasOne(ssp => ssp.Student)
                .WithMany(s => s.SocialProjects)
                .HasForeignKey(ssp => ssp.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ===============================================
        // HOMEWORK ASSIGNMENT MODULE RELATIONSHIPS
        // ===============================================

        // HomeworkAssignment relationships
        builder.Entity<HomeworkAssignment>(entity =>
        {
            entity.HasOne(ha => ha.Homework)
                .WithMany()
                .HasForeignKey(ha => ha.HomeworkId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ha => ha.Student)
                .WithMany()
                .HasForeignKey(ha => ha.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ha => ha.Teacher)
                .WithMany()
                .HasForeignKey(ha => ha.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(ha => new { ha.HomeworkId, ha.StudentId }).IsUnique();
        });

        // HomeworkSubmissionFile relationships
        builder.Entity<HomeworkSubmissionFile>(entity =>
        {
            entity.HasOne(hsf => hsf.HomeworkAssignment)
                .WithMany(ha => ha.SubmissionFiles)
                .HasForeignKey(hsf => hsf.HomeworkAssignmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // HomeworkViewLog relationships
        builder.Entity<HomeworkViewLog>(entity =>
        {
            entity.HasOne(hvl => hvl.HomeworkAssignment)
                .WithMany(ha => ha.ViewLogs)
                .HasForeignKey(hvl => hvl.HomeworkAssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(hvl => hvl.Student)
                .WithMany()
                .HasForeignKey(hvl => hvl.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ===============================================
        // STUDENT GROUP MODULE RELATIONSHIPS
        // ===============================================

        builder.Entity<StudentGroup>(entity =>
        {
            entity.HasIndex(sg => sg.Name);
        });

        builder.Entity<StudentGroupMember>(entity =>
        {
            entity.HasOne(sgm => sgm.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(sgm => sgm.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sgm => sgm.Student)
                .WithMany()
                .HasForeignKey(sgm => sgm.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Bir öğrenci aynı grupta birden fazla olamaz
            entity.HasIndex(sgm => new { sgm.GroupId, sgm.StudentId }).IsUnique();
        });

        builder.Entity<GroupLessonSchedule>(entity =>
        {
            entity.HasOne(gls => gls.Group)
                .WithMany(g => g.LessonSchedules)
                .HasForeignKey(gls => gls.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(gls => gls.Teacher)
                .WithMany()
                .HasForeignKey(gls => gls.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(gls => gls.Course)
                .WithMany()
                .HasForeignKey(gls => gls.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(gls => gls.Classroom)
                .WithMany()
                .HasForeignKey(gls => gls.ClassroomId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(gls => new { gls.GroupId, gls.TeacherId, gls.DayOfWeek });
        });

        // ===============================================
        // STUDENT CERTIFICATE MODULE RELATIONSHIPS
        // ===============================================

        builder.Entity<StudentCertificate>(entity =>
        {
            entity.HasOne(sc => sc.Student)
                .WithMany()
                .HasForeignKey(sc => sc.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(sc => sc.StudentId);
        });

        // ===============================================
        // STUDENT EXTENDED INFO RELATIONSHIPS
        // ===============================================

        // StudentForeignLanguage relationships
        builder.Entity<StudentForeignLanguage>(entity =>
        {
            entity.HasOne(fl => fl.Student)
                .WithMany(s => s.ForeignLanguages)
                .HasForeignKey(fl => fl.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(fl => fl.StudentId);
        });

        // StudentActivity relationships
        builder.Entity<StudentActivity>(entity =>
        {
            entity.HasOne(a => a.Student)
                .WithMany(s => s.Activities)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(a => a.StudentId);
        });

        // StudentReadinessExam relationships
        builder.Entity<StudentReadinessExam>(entity =>
        {
            entity.HasOne(re => re.Student)
                .WithMany(s => s.ReadinessExams)
                .HasForeignKey(re => re.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(re => re.StudentId);
        });

        // ===============================================
        // PERMISSION SYSTEM RELATIONSHIPS
        // ===============================================

        // Permission entity configuration
        builder.Entity<Permission>(entity =>
        {
            entity.HasIndex(p => p.Code).IsUnique();
            entity.Property(p => p.Code).HasMaxLength(100).IsRequired();
            entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Category).HasMaxLength(100).IsRequired();
            entity.Property(p => p.Icon).HasMaxLength(50);
            entity.Property(p => p.Description).HasMaxLength(500);
        });

        // UserPermission relationships
        builder.Entity<UserPermission>(entity =>
        {
            entity.HasOne(up => up.User)
                .WithMany()
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(up => up.Permission)
                .WithMany(p => p.UserPermissions)
                .HasForeignKey(up => up.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(up => up.GrantedByUser)
                .WithMany()
                .HasForeignKey(up => up.GrantedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(up => new { up.UserId, up.PermissionId }).IsUnique();
            entity.Property(up => up.Notes).HasMaxLength(500);
        });

        // RolePermission relationships
        builder.Entity<RolePermission>(entity =>
        {
            entity.HasOne(rp => rp.Role)
                .WithMany()
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(rp => new { rp.RoleId, rp.PermissionId }).IsUnique();
        });

        // ===============================================
        // COUNSELOR DASHBOARD RELATIONSHIPS
        // ===============================================

        // CounselorNote relationships
        builder.Entity<CounselorNote>(entity =>
        {
            entity.HasOne(cn => cn.Student)
                .WithMany()
                .HasForeignKey(cn => cn.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(cn => cn.Counselor)
                .WithMany()
                .HasForeignKey(cn => cn.CounselorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(cn => cn.CounselingMeeting)
                .WithMany()
                .HasForeignKey(cn => cn.CounselingMeetingId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(cn => cn.StudentId);
            entity.HasIndex(cn => cn.CounselorId);
            entity.HasIndex(cn => cn.NoteDate);
        });

        // UniversityRequirement relationships
        builder.Entity<UniversityRequirement>(entity =>
        {
            entity.HasOne(ur => ur.UniversityApplication)
                .WithMany()
                .HasForeignKey(ur => ur.UniversityApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(ur => ur.UniversityApplicationId);
        });

        // StudentExamCalendar relationships
        builder.Entity<StudentExamCalendar>(entity =>
        {
            entity.HasOne(sec => sec.Student)
                .WithMany()
                .HasForeignKey(sec => sec.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(sec => sec.StudentId);
            entity.HasIndex(sec => sec.ExamDate);
            entity.HasIndex(sec => sec.ExamType);
        });

        // CharacterixResult relationships
        builder.Entity<CharacterixResult>(entity =>
        {
            entity.HasOne(cr => cr.Student)
                .WithMany()
                .HasForeignKey(cr => cr.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(cr => cr.StudentId);
        });

        // StudentAward relationships
        builder.Entity<StudentAward>(entity =>
        {
            entity.HasOne(sa => sa.Student)
                .WithMany()
                .HasForeignKey(sa => sa.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(sa => sa.StudentId);
            entity.HasIndex(sa => sa.Scope);
            entity.HasIndex(sa => sa.Category);
        });

        // ===============================================
        // HOMEWORK ATTACHMENT RELATIONSHIPS
        // ===============================================

        builder.Entity<HomeworkAttachment>(entity =>
        {
            entity.HasOne(ha => ha.HomeworkAssignment)
                .WithMany(h => h.Attachments)
                .HasForeignKey(ha => ha.HomeworkAssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ha => ha.CourseResource)
                .WithMany()
                .HasForeignKey(ha => ha.CourseResourceId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ===============================================
        // STUDENT CURRICULUM PROGRESS RELATIONSHIPS
        // ===============================================

        builder.Entity<StudentCurriculumProgress>(entity =>
        {
            entity.HasOne(scp => scp.Student)
                .WithMany()
                .HasForeignKey(scp => scp.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(scp => scp.Curriculum)
                .WithMany(c => c.StudentProgresses)
                .HasForeignKey(scp => scp.CurriculumId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(scp => scp.ApprovedByTeacher)
                .WithMany()
                .HasForeignKey(scp => scp.ApprovedByTeacherId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(scp => new { scp.StudentId, scp.CurriculumId }).IsUnique();
        });

        // HomeworkAssignment - Curriculum relationship
        builder.Entity<HomeworkAssignment>(entity =>
        {
            entity.HasOne(ha => ha.Curriculum)
                .WithMany()
                .HasForeignKey(ha => ha.CurriculumId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Curriculum relationships
        builder.Entity<Curriculum>(entity =>
        {
            entity.HasOne(c => c.Course)
                .WithMany()
                .HasForeignKey(c => c.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.ExamResource)
                .WithMany()
                .HasForeignKey(c => c.ExamResourceId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ===============================================
        // MESSAGING MODULE RELATIONSHIPS
        // ===============================================

        // Conversation relationships
        builder.Entity<Conversation>(entity =>
        {
            entity.HasOne(c => c.Course)
                .WithMany()
                .HasForeignKey(c => c.CourseId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(c => c.StudentGroup)
                .WithMany()
                .HasForeignKey(c => c.StudentGroupId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(c => c.Type);
            entity.HasIndex(c => c.LastMessageAt);
        });

        // ConversationParticipant relationships
        builder.Entity<ConversationParticipant>(entity =>
        {
            entity.HasOne(cp => cp.Conversation)
                .WithMany(c => c.Participants)
                .HasForeignKey(cp => cp.ConversationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(cp => cp.User)
                .WithMany()
                .HasForeignKey(cp => cp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(cp => cp.LastReadMessage)
                .WithMany()
                .HasForeignKey(cp => cp.LastReadMessageId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasIndex(cp => new { cp.ConversationId, cp.UserId }).IsUnique();
            entity.HasIndex(cp => cp.UserId);
        });

        // ChatMessage relationships
        builder.Entity<ChatMessage>(entity =>
        {
            entity.HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.ReplyToMessage)
                .WithMany()
                .HasForeignKey(m => m.ReplyToMessageId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasIndex(m => m.ConversationId);
            entity.HasIndex(m => m.SentAt);
            entity.HasIndex(m => m.SenderId);
        });

        // MessageReadReceipt relationships
        builder.Entity<MessageReadReceipt>(entity =>
        {
            entity.HasOne(r => r.Message)
                .WithMany(m => m.ReadReceipts)
                .HasForeignKey(r => r.MessageId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(r => new { r.MessageId, r.UserId }).IsUnique();
        });

        // AdminMessageAccessLog relationships
        builder.Entity<AdminMessageAccessLog>(entity =>
        {
            entity.HasOne(l => l.AdminUser)
                .WithMany()
                .HasForeignKey(l => l.AdminUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(l => l.Conversation)
                .WithMany()
                .HasForeignKey(l => l.ConversationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(l => l.Message)
                .WithMany()
                .HasForeignKey(l => l.MessageId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasIndex(l => l.AdminUserId);
            entity.HasIndex(l => l.AccessedAt);
        });

        // BroadcastMessage relationships
        builder.Entity<BroadcastMessage>(entity =>
        {
            entity.HasOne(b => b.Sender)
                .WithMany()
                .HasForeignKey(b => b.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(b => b.SentAt);
            entity.HasIndex(b => b.TargetAudience);
        });

        // BroadcastMessageRecipient relationships
        builder.Entity<BroadcastMessageRecipient>(entity =>
        {
            entity.HasOne(r => r.BroadcastMessage)
                .WithMany(b => b.Recipients)
                .HasForeignKey(r => r.BroadcastMessageId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(r => new { r.BroadcastMessageId, r.UserId }).IsUnique();
            entity.HasIndex(r => r.UserId);
        });

        // PushSubscription relationships
        builder.Entity<PushSubscription>(entity =>
        {
            entity.HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(p => p.UserId);
            entity.HasIndex(p => p.Endpoint).IsUnique();
        });

        // MessageArchive relationships (no FK constraints for archived data)
        builder.Entity<MessageArchive>(entity =>
        {
            entity.HasIndex(a => a.OriginalConversationId);
            entity.HasIndex(a => a.OriginalSentAt);
            entity.HasIndex(a => a.ArchivedAt);
        });

        // Global query filter for soft delete
        builder.Entity<Student>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Parent>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Teacher>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Counselor>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Course>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Homework>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<AcademicDevelopmentPlan>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<HomeworkAssignment>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<HomeworkSubmissionFile>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<HomeworkViewLog>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Conversation>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<ChatMessage>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<ConversationParticipant>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<BroadcastMessage>().HasQueryFilter(e => !e.IsDeleted);

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
