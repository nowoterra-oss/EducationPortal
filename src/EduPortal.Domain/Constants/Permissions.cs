namespace EduPortal.Domain.Constants;

public static class Permissions
{
    // Öğrenci Yönetimi
    public const string StudentsView = "students.view";
    public const string StudentsCreate = "students.create";
    public const string StudentsEdit = "students.edit";
    public const string StudentsDelete = "students.delete";

    // Öğretmen Yönetimi
    public const string TeachersView = "teachers.view";
    public const string TeachersCreate = "teachers.create";
    public const string TeachersEdit = "teachers.edit";
    public const string TeachersDelete = "teachers.delete";

    // Ders Yönetimi
    public const string CoursesView = "courses.view";
    public const string CoursesCreate = "courses.create";
    public const string CoursesEdit = "courses.edit";
    public const string CoursesDelete = "courses.delete";

    // Ders Planlama
    public const string SchedulingView = "scheduling.view";
    public const string SchedulingCreate = "scheduling.create";
    public const string SchedulingEdit = "scheduling.edit";
    public const string SchedulingDelete = "scheduling.delete";

    // Ödemeler
    public const string PaymentsView = "payments.view";
    public const string PaymentsCreate = "payments.create";
    public const string PaymentsProcess = "payments.process";
    public const string PaymentsRefund = "payments.refund";

    // Maaş Yönetimi
    public const string SalariesView = "salaries.view";
    public const string SalariesCreate = "salaries.create";
    public const string SalariesProcess = "salaries.process";

    // Finans Yönetimi
    public const string FinanceView = "finance.view";
    public const string FinanceCreate = "finance.create";
    public const string FinanceEdit = "finance.edit";
    public const string FinanceDelete = "finance.delete";

    // Öğretmen Maaş Görüntüleme (Öğretmen Paneli)
    public const string TeacherSalaryView = "teacher.salary.view";

    // Raporlar
    public const string ReportsView = "reports.view";
    public const string ReportsExport = "reports.export";

    // Duyurular
    public const string AnnouncementsView = "announcements.view";
    public const string AnnouncementsCreate = "announcements.create";
    public const string AnnouncementsEdit = "announcements.edit";
    public const string AnnouncementsDelete = "announcements.delete";

    // Ödevler
    public const string AssignmentsView = "assignments.view";
    public const string AssignmentsCreate = "assignments.create";
    public const string AssignmentsGrade = "assignments.grade";

    // AGP (Akademik Gelişim Planı)
    public const string AgpView = "agp.view";
    public const string AgpCreate = "agp.create";
    public const string AgpEdit = "agp.edit";

    // Grup Dersleri
    public const string GroupLessonsView = "group-lessons.view";
    public const string GroupLessonsCreate = "group-lessons.create";
    public const string GroupLessonsEdit = "group-lessons.edit";

    // Yoklama
    public const string AttendanceView = "attendance.view";
    public const string AttendanceCreate = "attendance.create";
    public const string AttendanceEdit = "attendance.edit";

    // Sınavlar
    public const string ExamsView = "exams.view";
    public const string ExamsCreate = "exams.create";
    public const string ExamsGrade = "exams.grade";

    // Mesajlar
    public const string MessagesView = "messages.view";
    public const string MessagesSend = "messages.send";
    public const string MessagesBroadcast = "messages.broadcast";
    public const string MessagesAdminRead = "messages.admin-read";
    public const string MessagesAccessLogs = "messages.access-logs";

    // Kullanıcı Yönetimi
    public const string UsersView = "users.view";
    public const string UsersCreate = "users.create";
    public const string UsersEdit = "users.edit";
    public const string UsersDelete = "users.delete";
    public const string UsersManagePermissions = "users.manage-permissions";

    // Ayarlar
    public const string SettingsView = "settings.view";
    public const string SettingsEdit = "settings.edit";

    // Şube Yönetimi
    public const string BranchesView = "branches.view";
    public const string BranchesCreate = "branches.create";
    public const string BranchesEdit = "branches.edit";
    public const string BranchesDelete = "branches.delete";

    // Danışmanlık (Admin Paneli)
    public const string CounselingView = "counseling.view";
    public const string CounselingCreate = "counseling.create";
    public const string CounselingEdit = "counseling.edit";

    // Öğrenci Paneli Yetkileri
    public const string StudentDashboard = "student.dashboard";
    public const string StudentCourses = "student.courses";
    public const string StudentSchedule = "student.schedule";
    public const string StudentAssignments = "student.assignments";
    public const string StudentExams = "student.exams";
    public const string StudentMessages = "student.messages";
    public const string StudentAnnouncements = "student.announcements";
    public const string StudentProfile = "student.profile";
    public const string StudentPayments = "student.payments";

    // Danışman Öğretmen Yetkileri (Öğretmen paneli için)
    public const string AdvisorStudentView = "advisor.student.view";
    public const string AdvisorAgpView = "advisor.agp.view";

    // Veli Paneli Yetkileri
    public const string ParentDashboard = "parent.dashboard";
    public const string ParentChildren = "parent.children";
    public const string ParentPayments = "parent.payments";
    public const string ParentMessages = "parent.messages";
    public const string ParentReports = "parent.reports";
    public const string ParentMeetings = "parent.meetings";
    public const string ParentStudentView = "parent.student.view";
    public const string ParentHomeworkView = "parent.homework.view";
    public const string ParentAgpView = "parent.agp.view";

    // Paket Yönetimi
    public const string PackagesView = "packages.view";
    public const string PackagesCreate = "packages.create";
    public const string PackagesEdit = "packages.edit";
    public const string PackagesDelete = "packages.delete";

    // Tüm yetkileri listeleyen dictionary
    public static Dictionary<string, PermissionInfo> All => new()
    {
        // Öğrenci Yönetimi
        { StudentsView, new PermissionInfo("Öğrenci Görüntüle", "Öğrenci Yönetimi", "👨‍🎓", 1) },
        { StudentsCreate, new PermissionInfo("Öğrenci Kayıt", "Öğrenci Yönetimi", "👨‍🎓", 2) },
        { StudentsEdit, new PermissionInfo("Öğrenci Düzenle", "Öğrenci Yönetimi", "👨‍🎓", 3) },
        { StudentsDelete, new PermissionInfo("Öğrenci Sil", "Öğrenci Yönetimi", "👨‍🎓", 4) },

        // Öğretmen Yönetimi
        { TeachersView, new PermissionInfo("Öğretmen Görüntüle", "Öğretmen Yönetimi", "👨‍🏫", 5) },
        { TeachersCreate, new PermissionInfo("Öğretmen Kayıt", "Öğretmen Yönetimi", "👨‍🏫", 6) },
        { TeachersEdit, new PermissionInfo("Öğretmen Düzenle", "Öğretmen Yönetimi", "👨‍🏫", 7) },
        { TeachersDelete, new PermissionInfo("Öğretmen Sil", "Öğretmen Yönetimi", "👨‍🏫", 8) },

        // Ders Yönetimi
        { CoursesView, new PermissionInfo("Ders Görüntüle", "Ders Yönetimi", "📚", 9) },
        { CoursesCreate, new PermissionInfo("Ders Oluştur", "Ders Yönetimi", "📚", 10) },
        { CoursesEdit, new PermissionInfo("Ders Düzenle", "Ders Yönetimi", "📚", 11) },
        { CoursesDelete, new PermissionInfo("Ders Sil", "Ders Yönetimi", "📚", 12) },

        // Ders Planlama
        { SchedulingView, new PermissionInfo("Program Görüntüle", "Ders Planlama", "📅", 13) },
        { SchedulingCreate, new PermissionInfo("Program Oluştur", "Ders Planlama", "📅", 14) },
        { SchedulingEdit, new PermissionInfo("Program Düzenle", "Ders Planlama", "📅", 15) },
        { SchedulingDelete, new PermissionInfo("Program Sil", "Ders Planlama", "📅", 16) },

        // Ödemeler
        { PaymentsView, new PermissionInfo("Ödeme Görüntüle", "Ödemeler", "💰", 17) },
        { PaymentsCreate, new PermissionInfo("Ödeme Oluştur", "Ödemeler", "💰", 18) },
        { PaymentsProcess, new PermissionInfo("Ödeme İşle", "Ödemeler", "💰", 19) },
        { PaymentsRefund, new PermissionInfo("Ödeme İade", "Ödemeler", "💰", 20) },

        // Maaş Yönetimi
        { SalariesView, new PermissionInfo("Maaş Görüntüle", "Maaş Yönetimi", "💵", 63) },
        { SalariesCreate, new PermissionInfo("Maaş Oluştur", "Maaş Yönetimi", "💵", 64) },
        { SalariesProcess, new PermissionInfo("Maaş Öde", "Maaş Yönetimi", "💵", 65) },

        // Finans Yönetimi
        { FinanceView, new PermissionInfo("Finans Görüntüle", "Finans Yönetimi", "📈", 66) },
        { FinanceCreate, new PermissionInfo("Finans Kaydı Oluştur", "Finans Yönetimi", "📈", 67) },
        { FinanceEdit, new PermissionInfo("Finans Kaydı Düzenle", "Finans Yönetimi", "📈", 68) },
        { FinanceDelete, new PermissionInfo("Finans Kaydı Sil", "Finans Yönetimi", "📈", 69) },

        // Öğretmen Maaş Görüntüleme
        { TeacherSalaryView, new PermissionInfo("Maaşlarım", "Öğretmen Paneli", "👨‍🏫", 211) },

        // Raporlar
        { ReportsView, new PermissionInfo("Rapor Görüntüle", "Raporlar", "📊", 21) },
        { ReportsExport, new PermissionInfo("Rapor Dışa Aktar", "Raporlar", "📊", 22) },

        // Duyurular
        { AnnouncementsView, new PermissionInfo("Duyuru Görüntüle", "Duyurular", "📢", 23) },
        { AnnouncementsCreate, new PermissionInfo("Duyuru Oluştur", "Duyurular", "📢", 24) },
        { AnnouncementsEdit, new PermissionInfo("Duyuru Düzenle", "Duyurular", "📢", 25) },
        { AnnouncementsDelete, new PermissionInfo("Duyuru Sil", "Duyurular", "📢", 26) },

        // Ödevler
        { AssignmentsView, new PermissionInfo("Ödev Görüntüle", "Ödevler", "📝", 27) },
        { AssignmentsCreate, new PermissionInfo("Ödev Oluştur", "Ödevler", "📝", 28) },
        { AssignmentsGrade, new PermissionInfo("Ödev Notlandır", "Ödevler", "📝", 29) },

        // AGP
        { AgpView, new PermissionInfo("AGP Görüntüle", "Akademik Gelişim Planı", "🎯", 30) },
        { AgpCreate, new PermissionInfo("AGP Oluştur", "Akademik Gelişim Planı", "🎯", 31) },
        { AgpEdit, new PermissionInfo("AGP Düzenle", "Akademik Gelişim Planı", "🎯", 32) },

        // Grup Dersleri
        { GroupLessonsView, new PermissionInfo("Grup Dersi Görüntüle", "Grup Dersleri", "👥", 33) },
        { GroupLessonsCreate, new PermissionInfo("Grup Dersi Oluştur", "Grup Dersleri", "👥", 34) },
        { GroupLessonsEdit, new PermissionInfo("Grup Dersi Düzenle", "Grup Dersleri", "👥", 35) },

        // Yoklama
        { AttendanceView, new PermissionInfo("Yoklama Görüntüle", "Yoklama", "✅", 36) },
        { AttendanceCreate, new PermissionInfo("Yoklama Al", "Yoklama", "✅", 37) },
        { AttendanceEdit, new PermissionInfo("Yoklama Düzenle", "Yoklama", "✅", 38) },

        // Sınavlar
        { ExamsView, new PermissionInfo("Sınav Görüntüle", "Sınavlar", "📋", 39) },
        { ExamsCreate, new PermissionInfo("Sınav Oluştur", "Sınavlar", "📋", 40) },
        { ExamsGrade, new PermissionInfo("Sınav Notlandır", "Sınavlar", "📋", 41) },

        // Mesajlar
        { MessagesView, new PermissionInfo("Mesaj Görüntüle", "Mesajlar", "💬", 42) },
        { MessagesSend, new PermissionInfo("Mesaj Gönder", "Mesajlar", "💬", 43) },
        { MessagesBroadcast, new PermissionInfo("Toplu Mesaj Gönder", "Mesajlar", "💬", 44) },
        { MessagesAdminRead, new PermissionInfo("Mesaj Oku (Admin)", "Mesajlar", "💬", 45) },
        { MessagesAccessLogs, new PermissionInfo("Erişim Logları", "Mesajlar", "💬", 46) },

        // Kullanıcı Yönetimi
        { UsersView, new PermissionInfo("Kullanıcı Görüntüle", "Kullanıcı Yönetimi", "👤", 45) },
        { UsersCreate, new PermissionInfo("Kullanıcı Oluştur", "Kullanıcı Yönetimi", "👤", 46) },
        { UsersEdit, new PermissionInfo("Kullanıcı Düzenle", "Kullanıcı Yönetimi", "👤", 47) },
        { UsersDelete, new PermissionInfo("Kullanıcı Sil", "Kullanıcı Yönetimi", "👤", 48) },
        { UsersManagePermissions, new PermissionInfo("Yetki Yönet", "Kullanıcı Yönetimi", "🔐", 49) },

        // Ayarlar
        { SettingsView, new PermissionInfo("Ayar Görüntüle", "Ayarlar", "⚙️", 50) },
        { SettingsEdit, new PermissionInfo("Ayar Düzenle", "Ayarlar", "⚙️", 51) },

        // Şube Yönetimi
        { BranchesView, new PermissionInfo("Şube Görüntüle", "Şube Yönetimi", "🏢", 52) },
        { BranchesCreate, new PermissionInfo("Şube Oluştur", "Şube Yönetimi", "🏢", 53) },
        { BranchesEdit, new PermissionInfo("Şube Düzenle", "Şube Yönetimi", "🏢", 54) },
        { BranchesDelete, new PermissionInfo("Şube Sil", "Şube Yönetimi", "🏢", 55) },

        // Danışmanlık (Admin Paneli)
        { CounselingView, new PermissionInfo("Danışmanlık Görüntüle", "Danışmanlık", "🧑‍💼", 56) },
        { CounselingCreate, new PermissionInfo("Danışmanlık Oluştur", "Danışmanlık", "🧑‍💼", 57) },
        { CounselingEdit, new PermissionInfo("Danışmanlık Düzenle", "Danışmanlık", "🧑‍💼", 58) },

        // Paket Yönetimi
        { PackagesView, new PermissionInfo("Paket Görüntüle", "Paket Yönetimi", "📦", 59) },
        { PackagesCreate, new PermissionInfo("Paket Oluştur", "Paket Yönetimi", "📦", 60) },
        { PackagesEdit, new PermissionInfo("Paket Düzenle", "Paket Yönetimi", "📦", 61) },
        { PackagesDelete, new PermissionInfo("Paket Sil", "Paket Yönetimi", "📦", 62) },

        // Öğrenci Paneli
        { StudentDashboard, new PermissionInfo("Ana Sayfa", "Öğrenci Paneli", "🎓", 100) },
        { StudentCourses, new PermissionInfo("Derslerim", "Öğrenci Paneli", "🎓", 101) },
        { StudentSchedule, new PermissionInfo("Ders Programım", "Öğrenci Paneli", "🎓", 102) },
        { StudentAssignments, new PermissionInfo("Ödevlerim", "Öğrenci Paneli", "🎓", 103) },
        { StudentExams, new PermissionInfo("Sınavlarım", "Öğrenci Paneli", "🎓", 104) },
        { StudentMessages, new PermissionInfo("Mesajlarım", "Öğrenci Paneli", "🎓", 105) },
        { StudentAnnouncements, new PermissionInfo("Duyurular", "Öğrenci Paneli", "🎓", 106) },
        { StudentProfile, new PermissionInfo("Profilim", "Öğrenci Paneli", "🎓", 107) },
        { StudentPayments, new PermissionInfo("Ödemelerim", "Öğrenci Paneli", "🎓", 108) },

        // Danışman Öğretmen Yetkileri
        { AdvisorStudentView, new PermissionInfo("Danışman Öğrenci Görüntüle", "Danışman Öğretmen", "👨‍🏫", 209) },
        { AdvisorAgpView, new PermissionInfo("Danışman AGP Görüntüle", "Danışman Öğretmen", "👨‍🏫", 210) },

        // Veli Paneli
        { ParentDashboard, new PermissionInfo("Ana Sayfa", "Veli Paneli", "👪", 300) },
        { ParentChildren, new PermissionInfo("Çocuklarım", "Veli Paneli", "👪", 301) },
        { ParentPayments, new PermissionInfo("Ödemeler", "Veli Paneli", "👪", 302) },
        { ParentMessages, new PermissionInfo("Mesajlar", "Veli Paneli", "👪", 303) },
        { ParentReports, new PermissionInfo("Raporlar", "Veli Paneli", "👪", 304) },
        { ParentMeetings, new PermissionInfo("Görüşmeler", "Veli Paneli", "👪", 305) },
        { ParentStudentView, new PermissionInfo("Öğrenci Görüntüle", "Veli Paneli", "👪", 306) },
        { ParentHomeworkView, new PermissionInfo("Ödev Görüntüle", "Veli Paneli", "👪", 307) },
        { ParentAgpView, new PermissionInfo("AGP Görüntüle", "Veli Paneli", "👪", 308) },
    };

    // Kullanıcı tipine göre varsayılan yetkiler
    public static List<string> GetDefaultPermissionsForUserType(string userType)
    {
        return userType.ToLower() switch
        {
            "student" or "ogrenci" => new List<string>
            {
                StudentDashboard, StudentCourses, StudentSchedule, StudentAssignments,
                StudentExams, StudentMessages, StudentAnnouncements, StudentProfile, StudentPayments
            },
            "teacher" or "ogretmen" => new List<string>
            {
                // Temel öğretmen yetkileri - Ödev yönetimi
                AssignmentsCreate, AssignmentsGrade,
                // Sınav yönetimi
                ExamsCreate, ExamsGrade,
                // Yoklama yönetimi
                AttendanceCreate, AttendanceEdit,
                // Mesajlaşma
                MessagesView, MessagesSend,
                // Danışman erişimi (sadece danışmanı olduğu öğrencilere erişebilir)
                AdvisorStudentView, AdvisorAgpView,
                // Maaş görüntüleme
                TeacherSalaryView
                // NOT: Aşağıdaki yetkiler varsayılan olarak VERİLMEZ, gerekirse admin tarafından manuel verilmeli:
                // StudentsView, CoursesView, SchedulingView, GroupLessonsView,
                // AssignmentsView, ExamsView, AttendanceView, AnnouncementsView, AgpView
            },
            "parent" or "veli" => new List<string>
            {
                ParentDashboard, ParentChildren, ParentPayments,
                ParentMessages, ParentReports, ParentMeetings,
                ParentStudentView, ParentHomeworkView, ParentAgpView
            },
            _ => new List<string>()
        };
    }
}

public record PermissionInfo(string Name, string Category, string Icon, int Order);
