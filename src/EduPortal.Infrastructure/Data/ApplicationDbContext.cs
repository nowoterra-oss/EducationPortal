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

    // Coaching
    public DbSet<Coach> Coaches => Set<Coach>();
    public DbSet<StudentCoachAssignment> StudentCoachAssignments => Set<StudentCoachAssignment>();
    public DbSet<CoachingSession> CoachingSessions => Set<CoachingSession>();

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

            entity.HasOne(p => p.CoachingSession)
                .WithMany()
                .HasForeignKey(p => p.CoachingSessionId)
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
        // COACHING MODULE RELATIONSHIPS
        // ===============================================

        // Coach relationships
        builder.Entity<Coach>(entity =>
        {
            entity.HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.TeacherProfile)
                .WithOne(t => t.CoachProfile)
                .HasForeignKey<Coach>(c => c.TeacherId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // StudentCoachAssignment relationships
        builder.Entity<StudentCoachAssignment>(entity =>
        {
            entity.HasOne(sca => sca.Student)
                .WithMany(s => s.CoachAssignments)
                .HasForeignKey(sca => sca.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sca => sca.Coach)
                .WithMany(c => c.Students)
                .HasForeignKey(sca => sca.CoachId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // CoachingSession relationships
        builder.Entity<CoachingSession>(entity =>
        {
            entity.HasOne(cs => cs.Student)
                .WithMany(s => s.CoachingSessions)
                .HasForeignKey(cs => cs.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(cs => cs.Coach)
                .WithMany(c => c.Sessions)
                .HasForeignKey(cs => cs.CoachId)
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

            entity.HasOne(sap => sap.Coach)
                .WithMany()
                .HasForeignKey(sap => sap.CoachId)
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

            entity.HasOne(ca => ca.Coach)
                .WithMany()
                .HasForeignKey(ca => ca.CoachId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // SchoolRecommendation relationships
        builder.Entity<SchoolRecommendation>(entity =>
        {
            entity.HasOne(sr => sr.Student)
                .WithMany(s => s.SchoolRecommendations)
                .HasForeignKey(sr => sr.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sr => sr.Coach)
                .WithMany()
                .HasForeignKey(sr => sr.CoachId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // SportsAssessment relationships
        builder.Entity<SportsAssessment>(entity =>
        {
            entity.HasOne(sa => sa.Student)
                .WithMany(s => s.SportsAssessments)
                .HasForeignKey(sa => sa.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sa => sa.Coach)
                .WithMany()
                .HasForeignKey(sa => sa.CoachId)
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

        builder.Entity<Coach>(entity =>
        {
            entity.HasOne(c => c.Branch)
                .WithMany(b => b.Coaches)
                .HasForeignKey(c => c.BranchId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<CoachingSession>(entity =>
        {
            entity.HasOne(cs => cs.Branch)
                .WithMany(b => b.CoachingSessions)
                .HasForeignKey(cs => cs.BranchId)
                .OnDelete(DeleteBehavior.SetNull);
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
            entity.HasOne(hsf => hsf.Submission)
                .WithMany()
                .HasForeignKey(hsf => hsf.SubmissionId)
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
