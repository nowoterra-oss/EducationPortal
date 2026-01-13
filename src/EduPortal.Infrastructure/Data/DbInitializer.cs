using EduPortal.Domain.Constants;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EduPortal.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Otomatik migration uygula
        await context.Database.MigrateAsync();

        // Seed data'ları sırayla çağır
        await SeedRolesAsync(roleManager);
        await SeedAdminUserAsync(userManager);
        await SeedPermissionsAsync(context, roleManager);

        // Veritabanında veri varsa seed etme (ama test kullanıcılarını her zaman kontrol et)
        var hasExistingData = await context.Branches.AnyAsync();

        // Test kullanıcılarını ve finans verilerini her zaman kontrol et
        if (hasExistingData)
        {
            await SeedTestUsersAsync(userManager, context);
            await SeedFinanceTestDataAsync(context);
            Console.WriteLine("Database already seeded. Checked test users and finance data.");
            return;
        }

        await SeedBranchesAsync(context);
        await SeedClassroomsAsync(context);
        await SeedAcademicTermsAsync(context);
        await SeedCoursesAsync(context);
        await SeedUsersAndEntitiesAsync(userManager, context);
        await SeedTestUsersAsync(userManager, context);
        await SeedClassesAsync(context);
        await SeedSchedulingDataAsync(context);
        await SeedHomeworksAsync(context);
        await SeedExamsAsync(context);
        await SeedPaymentPlansAsync(context);
        await SeedServicePackagesAsync(context);
        await SeedEmailTemplatesAsync(context);
        await SeedFinanceTestDataAsync(context);

        Console.WriteLine("✅ Database seeded successfully with comprehensive data!");
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        // Sadece Admin rolü oluşturulur - diğer roller permission sistemi ile yönetilir
        string[] roles = { "Admin" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                Console.WriteLine($"✓ Role created: {role}");
            }
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
    {
        var adminEmail = "admin@eduportal.com";

        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User",
                PhoneNumber = "05551234567",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                Console.WriteLine("✓ Admin user created");
            }
        }
    }

    private static async Task SeedTestUsersAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        var branch = await context.Branches.FirstOrDefaultAsync();
        Student? testStudentEntity = null;
        Teacher? testTeacherEntity = null;

        // Test Student User
        var studentEmail = "student@eduportal.com";
        var existingStudent = await userManager.FindByEmailAsync(studentEmail);
        // Eski email ile de kontrol et
        if (existingStudent == null)
        {
            existingStudent = await userManager.FindByEmailAsync("student@test.com");
        }
        if (existingStudent == null)
        {
            var studentUser = new ApplicationUser
            {
                UserName = studentEmail,
                Email = studentEmail,
                FirstName = "Test",
                LastName = "Öğrenci",
                PhoneNumber = "05351234567",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-6)
            };

            var result = await userManager.CreateAsync(studentUser, "Student@123");
            if (result.Succeeded)
            {
                // Permission tabanlı yetki ataması (rol yerine)
                await AssignDefaultPermissionsAsync(context, studentUser.Id, "Student");

                if (branch != null)
                {
                    testStudentEntity = new Student
                    {
                        UserId = studentUser.Id,
                        StudentNo = $"STD{DateTime.Now.Year}TEST",
                        IdentityType = IdentityType.TCKimlik,
                        IdentityNumber = "12345678901",
                        Nationality = "TR",
                        SchoolName = "Kadıköy Anadolu Lisesi",
                        CurrentGrade = 11,
                        Gender = Gender.Erkek,
                        DateOfBirth = new DateTime(2008, 3, 15),
                        Address = "Caferağa Mah. Moda Cad. No:45 Kadıköy/İstanbul",
                        EnrollmentDate = DateTime.UtcNow.AddMonths(-6),
                        BranchId = branch.Id,
                        LGSPercentile = 92.5m,
                        IsBilsem = true,
                        BilsemField = "Matematik",
                        LanguageLevel = "B2",
                        TargetMajor = "Bilgisayar Mühendisliği",
                        TargetCountry = "ABD",
                        ReferenceSource = "Tanıdık tavsiyesi",
                        InterviewResult = InterviewResult.KesınKayit,
                        InterviewsJson = "[{\"number\":1,\"notes\":\"Öğrenci motivasyonu yüksek, akademik hedefleri net.\",\"evaluation\":\"Olumlu\",\"date\":\"2024-06-15T14:00:00Z\"},{\"number\":2,\"notes\":\"SAT hazırlık planı yapıldı, TOEFL hedefi belirlendi.\",\"evaluation\":\"Olumlu\",\"date\":\"2024-07-20T10:30:00Z\"}]"
                    };
                    await context.Students.AddAsync(testStudentEntity);
                    await context.SaveChangesAsync();

                    // Öğrenci hobilerini ekle
                    var hobbies = new List<StudentHobby>
                    {
                        new StudentHobby { StudentId = testStudentEntity.Id, Category = "Spor", Name = "Satranç", Achievements = "Okul takımı kaptanı" },
                        new StudentHobby { StudentId = testStudentEntity.Id, Category = "Diger", Name = "Kodlama", Achievements = "Python ve JavaScript ile projeler" },
                        new StudentHobby { StudentId = testStudentEntity.Id, Category = "Spor", Name = "Basketbol", Achievements = "Hafta sonları okul takımıyla antrenman" }
                    };
                    await context.StudentHobbies.AddRangeAsync(hobbies);
                    await context.SaveChangesAsync();
                }
                Console.WriteLine("✓ Test student user created with enriched data: student@eduportal.com / Student@123");
            }
        }
        else
        {
            // Kullanıcı var, entity'yi kontrol et ve güncelle
            testStudentEntity = await context.Students.FirstOrDefaultAsync(s => s.UserId == existingStudent.Id);
            if (testStudentEntity == null && branch != null)
            {
                testStudentEntity = new Student
                {
                    UserId = existingStudent.Id,
                    StudentNo = $"STD{DateTime.Now.Year}TEST",
                    IdentityType = IdentityType.TCKimlik,
                    IdentityNumber = "12345678901",
                    Nationality = "TR",
                    SchoolName = "Kadıköy Anadolu Lisesi",
                    CurrentGrade = 11,
                    Gender = Gender.Erkek,
                    DateOfBirth = new DateTime(2008, 3, 15),
                    Address = "Caferağa Mah. Moda Cad. No:45 Kadıköy/İstanbul",
                    EnrollmentDate = DateTime.UtcNow.AddMonths(-6),
                    BranchId = branch.Id,
                    LGSPercentile = 92.5m,
                    IsBilsem = true,
                    BilsemField = "Matematik",
                    LanguageLevel = "B2",
                    TargetMajor = "Bilgisayar Mühendisliği",
                    TargetCountry = "ABD",
                    ReferenceSource = "Tanıdık tavsiyesi",
                    InterviewResult = InterviewResult.KesınKayit,
                    InterviewsJson = "[{\"number\":1,\"notes\":\"Öğrenci motivasyonu yüksek, akademik hedefleri net.\",\"evaluation\":\"Olumlu\",\"date\":\"2024-06-15T14:00:00Z\"},{\"number\":2,\"notes\":\"SAT hazırlık planı yapıldı, TOEFL hedefi belirlendi.\",\"evaluation\":\"Olumlu\",\"date\":\"2024-07-20T10:30:00Z\"}]"
                };
                await context.Students.AddAsync(testStudentEntity);
                await context.SaveChangesAsync();
                Console.WriteLine("✓ Student entity created for existing test user with enriched data");
            }
            else if (testStudentEntity != null)
            {
                // Mevcut entity'yi güncelle (eksik alanları doldur)
                var needsUpdate = false;
                if (string.IsNullOrEmpty(testStudentEntity.IdentityNumber))
                {
                    testStudentEntity.IdentityNumber = "12345678901";
                    testStudentEntity.IdentityType = IdentityType.TCKimlik;
                    testStudentEntity.Nationality = "TR";
                    needsUpdate = true;
                }
                if (string.IsNullOrEmpty(testStudentEntity.Address))
                {
                    testStudentEntity.Address = "Caferağa Mah. Moda Cad. No:45 Kadıköy/İstanbul";
                    needsUpdate = true;
                }
                if (testStudentEntity.SchoolName == "Test Lisesi")
                {
                    testStudentEntity.SchoolName = "Kadıköy Anadolu Lisesi";
                    testStudentEntity.CurrentGrade = 11;
                    needsUpdate = true;
                }
                if (!testStudentEntity.LGSPercentile.HasValue || testStudentEntity.LGSPercentile < 90)
                {
                    testStudentEntity.LGSPercentile = 92.5m;
                    needsUpdate = true;
                }
                if (!testStudentEntity.IsBilsem)
                {
                    testStudentEntity.IsBilsem = true;
                    testStudentEntity.BilsemField = "Matematik";
                    needsUpdate = true;
                }
                if (string.IsNullOrEmpty(testStudentEntity.LanguageLevel))
                {
                    testStudentEntity.LanguageLevel = "B2";
                    needsUpdate = true;
                }
                if (string.IsNullOrEmpty(testStudentEntity.TargetMajor))
                {
                    testStudentEntity.TargetMajor = "Bilgisayar Mühendisliği";
                    testStudentEntity.TargetCountry = "ABD";
                    needsUpdate = true;
                }
                if (string.IsNullOrEmpty(testStudentEntity.ReferenceSource))
                {
                    testStudentEntity.ReferenceSource = "Tanıdık tavsiyesi";
                    needsUpdate = true;
                }
                if (!testStudentEntity.InterviewResult.HasValue)
                {
                    testStudentEntity.InterviewResult = InterviewResult.KesınKayit;
                    testStudentEntity.InterviewsJson = "[{\"number\":1,\"notes\":\"Öğrenci motivasyonu yüksek, akademik hedefleri net.\",\"evaluation\":\"Olumlu\",\"date\":\"2024-06-15T14:00:00Z\"},{\"number\":2,\"notes\":\"SAT hazırlık planı yapıldı, TOEFL hedefi belirlendi.\",\"evaluation\":\"Olumlu\",\"date\":\"2024-07-20T10:30:00Z\"}]";
                    needsUpdate = true;
                }

                if (needsUpdate)
                {
                    context.Students.Update(testStudentEntity);
                    await context.SaveChangesAsync();
                    Console.WriteLine("✓ Test student entity updated with enriched data");
                }

                // User bilgilerini güncelle
                // İsmi "Test Öğrenci" olarak güncelle (arama için)
                if (existingStudent.FirstName != "Test" || existingStudent.LastName != "Öğrenci" || !existingStudent.IsActive)
                {
                    existingStudent.FirstName = "Test";
                    existingStudent.LastName = "Öğrenci";
                    existingStudent.PhoneNumber = "05351234567";
                    existingStudent.IsActive = true; // Öğrencinin aramada görünmesi için aktif olmalı
                    await userManager.UpdateAsync(existingStudent);
                    Console.WriteLine("✓ Test student user info updated");
                }

                // Hobiler ekle
                var existingHobbies = await context.StudentHobbies.Where(h => h.StudentId == testStudentEntity.Id).CountAsync();
                if (existingHobbies == 0)
                {
                    var hobbies = new List<StudentHobby>
                    {
                        new StudentHobby { StudentId = testStudentEntity.Id, Category = "Spor", Name = "Satranç", Achievements = "Okul takımı kaptanı" },
                        new StudentHobby { StudentId = testStudentEntity.Id, Category = "Diger", Name = "Kodlama", Achievements = "Python ve JavaScript ile projeler" },
                        new StudentHobby { StudentId = testStudentEntity.Id, Category = "Spor", Name = "Basketbol", Achievements = "Hafta sonları okul takımıyla antrenman" }
                    };
                    await context.StudentHobbies.AddRangeAsync(hobbies);
                    await context.SaveChangesAsync();
                    Console.WriteLine("✓ Test student hobbies added");
                }
            }
        }

        // Test Teacher User
        var teacherEmail = "teacher@eduportal.com";
        var existingTeacher = await userManager.FindByEmailAsync(teacherEmail);
        // Eski email ile de kontrol et
        if (existingTeacher == null)
        {
            existingTeacher = await userManager.FindByEmailAsync("teacher@test.com");
        }
        if (existingTeacher == null)
        {
            var teacherUser = new ApplicationUser
            {
                UserName = teacherEmail,
                Email = teacherEmail,
                FirstName = "Test",
                LastName = "Öğretmen",
                PhoneNumber = "05421234567",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddYears(-2)
            };

            var result = await userManager.CreateAsync(teacherUser, "Teacher@123");
            if (result.Succeeded)
            {
                // Permission tabanlı yetki ataması (rol yerine)
                await AssignDefaultPermissionsAsync(context, teacherUser.Id, "Teacher");

                if (branch != null)
                {
                    testTeacherEntity = new Teacher
                    {
                        UserId = teacherUser.Id,
                        Specialization = "İngilizce",
                        Experience = 8,
                        IsActive = true,
                        BranchId = branch.Id,
                        IdentityType = IdentityType.TCKimlik,
                        IdentityNumber = "98765432101",
                        Nationality = "TR",
                        Department = "Yabancı Diller",
                        Biography = "8 yıllık İngilizce öğretmenliği deneyimi. Cambridge CELTA sertifikalı. SAT, TOEFL ve IELTS hazırlık konusunda uzman. Yüzlerce öğrencinin yurtdışı eğitim hayallerini gerçekleştirmelerine yardımcı oldu.",
                        Education = "Boğaziçi Üniversitesi - İngiliz Dili ve Edebiyatı (Lisans), Yıldız Teknik Üniversitesi - İngilizce Öğretmenliği (Yüksek Lisans)",
                        Certifications = "Cambridge CELTA, TOEFL iBT Instructor, IELTS Academic Trainer",
                        OfficeLocation = "Kadıköy Şubesi - 2. Kat, Oda 205",
                        OfficeHours = "Pazartesi-Cuma 09:00-18:00",
                        HireDate = DateTime.UtcNow.AddYears(-2),
                        ExperienceScore = 85
                    };
                    await context.Teachers.AddAsync(testTeacherEntity);
                    await context.SaveChangesAsync();

                    // Öğretmen sertifikalarını ekle
                    var certificates = new List<TeacherCertificate>
                    {
                        new TeacherCertificate { TeacherId = testTeacherEntity.Id, Name = "Cambridge CELTA", Institution = "Cambridge Assessment English", IssueDate = new DateTime(2018, 6, 15) },
                        new TeacherCertificate { TeacherId = testTeacherEntity.Id, Name = "TOEFL iBT Instructor", Institution = "ETS", IssueDate = new DateTime(2019, 3, 20) },
                        new TeacherCertificate { TeacherId = testTeacherEntity.Id, Name = "IELTS Academic Trainer", Institution = "British Council", IssueDate = new DateTime(2020, 9, 10) }
                    };
                    await context.TeacherCertificates.AddRangeAsync(certificates);

                    // Öğretmen referanslarını ekle
                    var references = new List<TeacherReference>
                    {
                        new TeacherReference { TeacherId = testTeacherEntity.Id, FullName = "Prof. Dr. Mehmet Öztürk", Title = "Bölüm Başkanı", Organization = "Boğaziçi Üniversitesi", PhoneNumber = "05321234567", Email = "mehmet.ozturk@boun.edu.tr" },
                        new TeacherReference { TeacherId = testTeacherEntity.Id, FullName = "Sarah Williams", Title = "Academic Director", Organization = "British Council Istanbul", PhoneNumber = "05331234567", Email = "sarah.williams@britishcouncil.org" }
                    };
                    await context.TeacherReferences.AddRangeAsync(references);
                    await context.SaveChangesAsync();
                }
                Console.WriteLine("✓ Test teacher user created with enriched data: teacher@eduportal.com / Teacher@123");
            }
        }
        else
        {
            // Kullanıcı var, entity'yi kontrol et ve güncelle
            testTeacherEntity = await context.Teachers.FirstOrDefaultAsync(t => t.UserId == existingTeacher.Id);
            if (testTeacherEntity == null && branch != null)
            {
                testTeacherEntity = new Teacher
                {
                    UserId = existingTeacher.Id,
                    Specialization = "İngilizce",
                    Experience = 8,
                    IsActive = true,
                    BranchId = branch.Id,
                    IdentityType = IdentityType.TCKimlik,
                    IdentityNumber = "98765432101",
                    Nationality = "TR",
                    Department = "Yabancı Diller",
                    Biography = "8 yıllık İngilizce öğretmenliği deneyimi. Cambridge CELTA sertifikalı. SAT, TOEFL ve IELTS hazırlık konusunda uzman.",
                    Education = "Boğaziçi Üniversitesi - İngiliz Dili ve Edebiyatı (Lisans), Yıldız Teknik Üniversitesi - İngilizce Öğretmenliği (Yüksek Lisans)",
                    Certifications = "Cambridge CELTA, TOEFL iBT Instructor, IELTS Academic Trainer",
                    OfficeLocation = "Kadıköy Şubesi - 2. Kat, Oda 205",
                    OfficeHours = "Pazartesi-Cuma 09:00-18:00",
                    HireDate = DateTime.UtcNow.AddYears(-2),
                    ExperienceScore = 85
                };
                await context.Teachers.AddAsync(testTeacherEntity);
                await context.SaveChangesAsync();
                Console.WriteLine("✓ Teacher entity created for existing test user with enriched data");
            }
            else if (testTeacherEntity != null)
            {
                // Mevcut entity'yi güncelle
                var needsUpdate = false;
                if (testTeacherEntity.Specialization != "İngilizce")
                {
                    testTeacherEntity.Specialization = "İngilizce";
                    needsUpdate = true;
                }
                if (testTeacherEntity.Experience < 8)
                {
                    testTeacherEntity.Experience = 8;
                    needsUpdate = true;
                }
                if (string.IsNullOrEmpty(testTeacherEntity.IdentityNumber))
                {
                    testTeacherEntity.IdentityType = IdentityType.TCKimlik;
                    testTeacherEntity.IdentityNumber = "98765432101";
                    testTeacherEntity.Nationality = "TR";
                    needsUpdate = true;
                }
                if (string.IsNullOrEmpty(testTeacherEntity.Department))
                {
                    testTeacherEntity.Department = "Yabancı Diller";
                    needsUpdate = true;
                }
                if (string.IsNullOrEmpty(testTeacherEntity.Biography))
                {
                    testTeacherEntity.Biography = "8 yıllık İngilizce öğretmenliği deneyimi. Cambridge CELTA sertifikalı. SAT, TOEFL ve IELTS hazırlık konusunda uzman. Yüzlerce öğrencinin yurtdışı eğitim hayallerini gerçekleştirmelerine yardımcı oldu.";
                    needsUpdate = true;
                }
                if (string.IsNullOrEmpty(testTeacherEntity.Education))
                {
                    testTeacherEntity.Education = "Boğaziçi Üniversitesi - İngiliz Dili ve Edebiyatı (Lisans), Yıldız Teknik Üniversitesi - İngilizce Öğretmenliği (Yüksek Lisans)";
                    needsUpdate = true;
                }
                if (string.IsNullOrEmpty(testTeacherEntity.Certifications))
                {
                    testTeacherEntity.Certifications = "Cambridge CELTA, TOEFL iBT Instructor, IELTS Academic Trainer";
                    needsUpdate = true;
                }
                if (string.IsNullOrEmpty(testTeacherEntity.OfficeLocation))
                {
                    testTeacherEntity.OfficeLocation = "Kadıköy Şubesi - 2. Kat, Oda 205";
                    testTeacherEntity.OfficeHours = "Pazartesi-Cuma 09:00-18:00";
                    needsUpdate = true;
                }
                if (!testTeacherEntity.HireDate.HasValue)
                {
                    testTeacherEntity.HireDate = DateTime.UtcNow.AddYears(-2);
                    needsUpdate = true;
                }
                if (!testTeacherEntity.ExperienceScore.HasValue)
                {
                    testTeacherEntity.ExperienceScore = 85;
                    needsUpdate = true;
                }

                if (needsUpdate)
                {
                    context.Teachers.Update(testTeacherEntity);
                    await context.SaveChangesAsync();
                    Console.WriteLine("✓ Test teacher entity updated with enriched data");
                }

                // İsmi "Test Öğretmen" olarak güncelle (arama için)
                if (existingTeacher.FirstName != "Test" || existingTeacher.LastName != "Öğretmen")
                {
                    existingTeacher.FirstName = "Test";
                    existingTeacher.LastName = "Öğretmen";
                    existingTeacher.PhoneNumber = "05421234567";
                    await userManager.UpdateAsync(existingTeacher);
                    Console.WriteLine("✓ Test teacher user info updated");
                }

                // Sertifikalar ekle
                var existingCerts = await context.TeacherCertificates.Where(c => c.TeacherId == testTeacherEntity.Id).CountAsync();
                if (existingCerts == 0)
                {
                    var certificates = new List<TeacherCertificate>
                    {
                        new TeacherCertificate { TeacherId = testTeacherEntity.Id, Name = "Cambridge CELTA", Institution = "Cambridge Assessment English", IssueDate = new DateTime(2018, 6, 15) },
                        new TeacherCertificate { TeacherId = testTeacherEntity.Id, Name = "TOEFL iBT Instructor", Institution = "ETS", IssueDate = new DateTime(2019, 3, 20) },
                        new TeacherCertificate { TeacherId = testTeacherEntity.Id, Name = "IELTS Academic Trainer", Institution = "British Council", IssueDate = new DateTime(2020, 9, 10) }
                    };
                    await context.TeacherCertificates.AddRangeAsync(certificates);
                    await context.SaveChangesAsync();
                    Console.WriteLine("✓ Test teacher certificates added");
                }

                // Referanslar ekle
                var existingRefs = await context.TeacherReferences.Where(r => r.TeacherId == testTeacherEntity.Id).CountAsync();
                if (existingRefs == 0)
                {
                    var references = new List<TeacherReference>
                    {
                        new TeacherReference { TeacherId = testTeacherEntity.Id, FullName = "Prof. Dr. Mehmet Öztürk", Title = "Bölüm Başkanı", Organization = "Boğaziçi Üniversitesi", PhoneNumber = "05321234567", Email = "mehmet.ozturk@boun.edu.tr" },
                        new TeacherReference { TeacherId = testTeacherEntity.Id, FullName = "Sarah Williams", Title = "Academic Director", Organization = "British Council Istanbul", PhoneNumber = "05331234567", Email = "sarah.williams@britishcouncil.org" }
                    };
                    await context.TeacherReferences.AddRangeAsync(references);
                    await context.SaveChangesAsync();
                    Console.WriteLine("✓ Test teacher references added");
                }
            }
        }

        // Test Parent User
        var parentEmail = "parent@eduportal.com";
        var existingParent = await userManager.FindByEmailAsync(parentEmail);
        // Eski email ile de kontrol et
        if (existingParent == null)
        {
            existingParent = await userManager.FindByEmailAsync("parent@test.com");
        }
        if (existingParent == null)
        {
            var parentUser = new ApplicationUser
            {
                UserName = parentEmail,
                Email = parentEmail,
                FirstName = "Test",
                LastName = "Veli",
                PhoneNumber = "05321234567",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-6)
            };

            var result = await userManager.CreateAsync(parentUser, "Parent@123");
            if (result.Succeeded)
            {
                // Permission tabanlı yetki ataması (rol yerine)
                await AssignDefaultPermissionsAsync(context, parentUser.Id, "Parent");

                var parent = new Parent
                {
                    UserId = parentUser.Id,
                    Occupation = "Yazılım Mühendisi",
                    WorkPhone = "02161234567"
                };
                await context.Parents.AddAsync(parent);
                await context.SaveChangesAsync();

                // Test öğrenci ile ilişkilendir
                if (testStudentEntity != null)
                {
                    var studentParent = new StudentParent
                    {
                        StudentId = testStudentEntity.Id,
                        ParentId = parent.Id,
                        Relationship = "Baba",
                        IsPrimaryContact = true
                    };
                    await context.StudentParents.AddAsync(studentParent);
                    await context.SaveChangesAsync();
                }
                Console.WriteLine("✓ Test parent user created: parent@eduportal.com / Parent@123");
            }
        }
        else
        {
            // Kullanıcı var ama Parent entity yok mu kontrol et
            var existingParentEntity = await context.Parents.FirstOrDefaultAsync(p => p.UserId == existingParent.Id);
            if (existingParentEntity == null)
            {
                var parent = new Parent
                {
                    UserId = existingParent.Id,
                    Occupation = "Yazılım Mühendisi",
                    WorkPhone = "02161234567"
                };
                await context.Parents.AddAsync(parent);
                await context.SaveChangesAsync();

                // Test öğrenci ile ilişkilendir
                var testStudent = await context.Students.FirstOrDefaultAsync(s => s.StudentNo == $"STD{DateTime.Now.Year}TEST");
                if (testStudent != null)
                {
                    var existingRelation = await context.StudentParents.FirstOrDefaultAsync(sp => sp.StudentId == testStudent.Id && sp.ParentId == parent.Id);
                    if (existingRelation == null)
                    {
                        var studentParent = new StudentParent
                        {
                            StudentId = testStudent.Id,
                            ParentId = parent.Id,
                            Relationship = "Baba",
                            IsPrimaryContact = true
                        };
                        await context.StudentParents.AddAsync(studentParent);
                        await context.SaveChangesAsync();
                    }
                }
                Console.WriteLine("✓ Parent entity created for existing test user");
            }

            // İsmi "Test Veli" olarak güncelle (arama için)
            if (existingParent.FirstName != "Test" || existingParent.LastName != "Veli")
            {
                existingParent.FirstName = "Test";
                existingParent.LastName = "Veli";
                existingParent.PhoneNumber = "05321234567";
                await userManager.UpdateAsync(existingParent);
                Console.WriteLine("✓ Test parent user info updated");
            }
        }

        // Test öğrenci ve öğretmen arasında ders programı oluştur
        if (testStudentEntity != null && testTeacherEntity != null)
        {
            await CreateTestLessonSchedulesAsync(context, testStudentEntity.Id, testTeacherEntity.Id);
        }
        else
        {
            // Entity'ler zaten varsa ID'lerini bul
            var studentId = testStudentEntity?.Id ?? (await context.Students.FirstOrDefaultAsync(s => s.StudentNo == $"STD{DateTime.Now.Year}TEST"))?.Id;
            var teacherId = testTeacherEntity?.Id ?? (await context.Teachers.FirstOrDefaultAsync(t => t.Specialization == "İngilizce" && t.User.Email == "teacher@eduportal.com"))?.Id;

            if (studentId.HasValue && teacherId.HasValue)
            {
                await CreateTestLessonSchedulesAsync(context, studentId.Value, teacherId.Value);
            }
        }
    }

    private static async Task CreateTestLessonSchedulesAsync(ApplicationDbContext context, int studentId, int teacherId)
    {
        // Mevcut ders programını kontrol et
        var existingSchedule = await context.LessonSchedules
            .FirstOrDefaultAsync(ls => ls.StudentId == studentId && ls.TeacherId == teacherId);

        if (existingSchedule != null)
        {
            Console.WriteLine("✓ Lesson schedules already exist for test student and teacher");
            return;
        }

        // İngilizce dersini bul (TOEFL Hazırlık)
        var englishCourse = await context.Courses.FirstOrDefaultAsync(c => c.CourseCode == "ENG-TOEFL");
        if (englishCourse == null)
        {
            englishCourse = await context.Courses.FirstOrDefaultAsync(c => c.Subject == "İngilizce");
        }

        if (englishCourse == null)
        {
            Console.WriteLine("⚠ English course not found, skipping lesson schedule creation");
            return;
        }

        // Sınıf bul
        var classroom = await context.Classrooms.FirstOrDefaultAsync(c => c.IsAvailable);

        // Haftalık ders programı oluştur (Pazartesi, Çarşamba, Cuma)
        var schedules = new List<LessonSchedule>
        {
            new LessonSchedule
            {
                StudentId = studentId,
                TeacherId = teacherId,
                CourseId = englishCourse.Id,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeSpan(17, 0, 0),
                EndTime = new TimeSpan(18, 30, 0),
                EffectiveFrom = DateTime.UtcNow.AddDays(-30),
                Status = LessonStatus.Scheduled,
                IsRecurring = true,
                ClassroomId = classroom?.Id,
                Notes = "TOEFL Reading & Listening"
            },
            new LessonSchedule
            {
                StudentId = studentId,
                TeacherId = teacherId,
                CourseId = englishCourse.Id,
                DayOfWeek = DayOfWeek.Wednesday,
                StartTime = new TimeSpan(17, 0, 0),
                EndTime = new TimeSpan(18, 30, 0),
                EffectiveFrom = DateTime.UtcNow.AddDays(-30),
                Status = LessonStatus.Scheduled,
                IsRecurring = true,
                ClassroomId = classroom?.Id,
                Notes = "TOEFL Speaking & Writing"
            },
            new LessonSchedule
            {
                StudentId = studentId,
                TeacherId = teacherId,
                CourseId = englishCourse.Id,
                DayOfWeek = DayOfWeek.Friday,
                StartTime = new TimeSpan(15, 0, 0),
                EndTime = new TimeSpan(16, 30, 0),
                EffectiveFrom = DateTime.UtcNow.AddDays(-30),
                Status = LessonStatus.Scheduled,
                IsRecurring = true,
                ClassroomId = classroom?.Id,
                Notes = "TOEFL Practice Test"
            },
            new LessonSchedule
            {
                StudentId = studentId,
                TeacherId = teacherId,
                CourseId = englishCourse.Id,
                DayOfWeek = DayOfWeek.Saturday,
                StartTime = new TimeSpan(10, 0, 0),
                EndTime = new TimeSpan(12, 0, 0),
                EffectiveFrom = DateTime.UtcNow.AddDays(-30),
                Status = LessonStatus.Scheduled,
                IsRecurring = true,
                ClassroomId = classroom?.Id,
                Notes = "TOEFL Mock Exam & Review"
            }
        };

        await context.LessonSchedules.AddRangeAsync(schedules);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ {schedules.Count} lesson schedules created between test student and teacher");

        // StudentTeacherAssignment oluştur
        var existingAssignment = await context.StudentTeacherAssignments
            .FirstOrDefaultAsync(sta => sta.StudentId == studentId && sta.TeacherId == teacherId && sta.CourseId == englishCourse.Id);

        if (existingAssignment == null)
        {
            var assignment = new StudentTeacherAssignment
            {
                StudentId = studentId,
                TeacherId = teacherId,
                CourseId = englishCourse.Id,
                StartDate = DateTime.UtcNow.AddDays(-30),
                IsActive = true,
                Notes = "TOEFL Hazırlık Programı - 3 aylık yoğun kurs"
            };
            await context.StudentTeacherAssignments.AddAsync(assignment);
            await context.SaveChangesAsync();
            Console.WriteLine("✓ StudentTeacherAssignment created for test student and teacher");
        }
    }

    private static async Task SeedBranchesAsync(ApplicationDbContext context)
    {
        var branches = new List<Branch>
        {
            new Branch
            {
                BranchName = "Kadıköy Merkez Şubesi",
                BranchCode = "KDK-001",
                Type = BranchType.Main,
                Address = "Caferağa Mah. Moda Cad. No:45 Kadıköy",
                City = "İstanbul",
                District = "Kadıköy",
                Phone = "02161234567",
                Email = "kadikoy@eduportal.com",
                Capacity = 500,
                OpeningDate = new DateTime(2020, 9, 1),
                IsActive = true
            },
            new Branch
            {
                BranchName = "Beşiktaş Şubesi",
                BranchCode = "BSK-002",
                Type = BranchType.Branch,
                Address = "Levazım Mah. Zorlu Center No:12 Beşiktaş",
                City = "İstanbul",
                District = "Beşiktaş",
                Phone = "02122345678",
                Email = "besiktas@eduportal.com",
                Capacity = 300,
                OpeningDate = new DateTime(2021, 1, 15),
                IsActive = true
            },
            new Branch
            {
                BranchName = "Ankara Çankaya Şubesi",
                BranchCode = "ANK-003",
                Type = BranchType.Branch,
                Address = "Kızılay Mah. Atatürk Bulvarı No:88 Çankaya",
                City = "Ankara",
                District = "Çankaya",
                Phone = "03123456789",
                Email = "ankara@eduportal.com",
                Capacity = 400,
                OpeningDate = new DateTime(2021, 6, 1),
                IsActive = true
            },
            new Branch
            {
                BranchName = "İzmir Alsancak Şubesi",
                BranchCode = "IZM-004",
                Type = BranchType.Branch,
                Address = "Alsancak Mah. Kıbrıs Şehitleri Cad. No:55",
                City = "İzmir",
                District = "Konak",
                Phone = "02324567890",
                Email = "izmir@eduportal.com",
                Capacity = 250,
                OpeningDate = new DateTime(2022, 2, 1),
                IsActive = true
            }
        };

        await context.Branches.AddRangeAsync(branches);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ {branches.Count} branches created");
    }

    private static async Task SeedClassroomsAsync(ApplicationDbContext context)
    {
        var branches = await context.Branches.ToListAsync();
        var classrooms = new List<Classroom>();

        foreach (var branch in branches)
        {
            // Her şube için 15 sınıf oluştur
            for (int i = 1; i <= 15; i++)
            {
                classrooms.Add(new Classroom
                {
                    RoomNumber = $"{branch.BranchCode.Split('-')[0]}-{i:D2}",
                    BuildingName = "Ana Bina",
                    Capacity = i % 3 == 0 ? 30 : (i % 2 == 0 ? 20 : 15),
                    Floor = $"{(i - 1) / 5 + 1}. Kat",
                    HasProjector = i % 2 == 0,
                    HasSmartBoard = i % 3 == 0,
                    IsAvailable = true,
                    BranchId = branch.Id
                });
            }
        }

        await context.Classrooms.AddRangeAsync(classrooms);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ {classrooms.Count} classrooms created");
    }

    private static async Task SeedAcademicTermsAsync(ApplicationDbContext context)
    {
        var currentYear = DateTime.Now.Year;
        var terms = new List<AcademicTerm>();

        for (int year = currentYear - 1; year <= currentYear + 1; year++)
        {
            terms.Add(new AcademicTerm
            {
                TermName = $"{year}-{year + 1} Güz Dönemi",
                StartDate = new DateTime(year, 9, 15),
                EndDate = new DateTime(year + 1, 1, 31),
                IsActive = year == currentYear
            });

            terms.Add(new AcademicTerm
            {
                TermName = $"{year}-{year + 1} Bahar Dönemi",
                StartDate = new DateTime(year + 1, 2, 1),
                EndDate = new DateTime(year + 1, 6, 30),
                IsActive = year == currentYear
            });
        }

        await context.AcademicTerms.AddRangeAsync(terms);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ {terms.Count} academic terms created");
    }

    private static async Task SeedCoursesAsync(ApplicationDbContext context)
    {
        var courses = new List<Course>
        {
            // Matematik Dersleri
            new Course { CourseName = "Temel Matematik", CourseCode = "MAT-101", Subject = "Matematik", Level = "Başlangıç", Credits = 4, Description = "Temel matematik kavramları", IsActive = true },
            new Course { CourseName = "İleri Matematik", CourseCode = "MAT-201", Subject = "Matematik", Level = "Orta", Credits = 5, Description = "İleri düzey matematik", IsActive = true },
            new Course { CourseName = "Analitik Geometri", CourseCode = "MAT-301", Subject = "Matematik", Level = "İleri", Credits = 4, Description = "Analitik geometri ve uygulamaları", IsActive = true },
            new Course { CourseName = "Kalkülüs I", CourseCode = "MAT-401", Subject = "Matematik", Level = "İleri", Credits = 6, Description = "Diferansiyel kalkülüs", IsActive = true },
            new Course { CourseName = "Kalkülüs II", CourseCode = "MAT-402", Subject = "Matematik", Level = "İleri", Credits = 6, Description = "İntegral kalkülüs", IsActive = true },

            // Fizik Dersleri
            new Course { CourseName = "Temel Fizik", CourseCode = "FIZ-101", Subject = "Fizik", Level = "Başlangıç", Credits = 4, Description = "Temel fizik kavramları", IsActive = true },
            new Course { CourseName = "Mekanik", CourseCode = "FIZ-201", Subject = "Fizik", Level = "Orta", Credits = 5, Description = "Klasik mekanik", IsActive = true },
            new Course { CourseName = "Elektrik ve Manyetizma", CourseCode = "FIZ-301", Subject = "Fizik", Level = "İleri", Credits = 5, Description = "Elektromanyetik teori", IsActive = true },
            new Course { CourseName = "Modern Fizik", CourseCode = "FIZ-401", Subject = "Fizik", Level = "İleri", Credits = 6, Description = "Kuantum fiziği ve görelilik", IsActive = true },

            // Kimya Dersleri
            new Course { CourseName = "Genel Kimya", CourseCode = "KIM-101", Subject = "Kimya", Level = "Başlangıç", Credits = 4, Description = "Temel kimya kavramları", IsActive = true },
            new Course { CourseName = "Organik Kimya", CourseCode = "KIM-201", Subject = "Kimya", Level = "Orta", Credits = 5, Description = "Organik bileşikler", IsActive = true },
            new Course { CourseName = "Anorganik Kimya", CourseCode = "KIM-301", Subject = "Kimya", Level = "İleri", Credits = 5, Description = "Anorganik bileşikler", IsActive = true },

            // Biyoloji Dersleri
            new Course { CourseName = "Genel Biyoloji", CourseCode = "BIO-101", Subject = "Biyoloji", Level = "Başlangıç", Credits = 4, Description = "Canlılar dünyası", IsActive = true },
            new Course { CourseName = "Moleküler Biyoloji", CourseCode = "BIO-201", Subject = "Biyoloji", Level = "Orta", Credits = 5, Description = "DNA ve genetik", IsActive = true },
            new Course { CourseName = "Ekoloji", CourseCode = "BIO-301", Subject = "Biyoloji", Level = "İleri", Credits = 4, Description = "Ekosistemler ve çevre", IsActive = true },

            // İngilizce Dersleri
            new Course { CourseName = "Başlangıç İngilizce", CourseCode = "ENG-101", Subject = "İngilizce", Level = "A1-A2", Credits = 3, Description = "Temel İngilizce", IsActive = true },
            new Course { CourseName = "Orta Düzey İngilizce", CourseCode = "ENG-201", Subject = "İngilizce", Level = "B1-B2", Credits = 4, Description = "Orta seviye İngilizce", IsActive = true },
            new Course { CourseName = "İleri İngilizce", CourseCode = "ENG-301", Subject = "İngilizce", Level = "C1-C2", Credits = 5, Description = "İleri seviye İngilizce", IsActive = true },
            new Course { CourseName = "SAT İngilizce Hazırlık", CourseCode = "ENG-SAT", Subject = "İngilizce", Level = "İleri", Credits = 6, Description = "SAT sınavı İngilizce", IsActive = true },
            new Course { CourseName = "TOEFL Hazırlık", CourseCode = "ENG-TOEFL", Subject = "İngilizce", Level = "İleri", Credits = 6, Description = "TOEFL sınavı hazırlık", IsActive = true },

            // Edebiyat Dersleri
            new Course { CourseName = "Türk Dili ve Edebiyatı", CourseCode = "TDE-101", Subject = "Edebiyat", Level = "Başlangıç", Credits = 3, Description = "Türk edebiyatı", IsActive = true },
            new Course { CourseName = "Dünya Edebiyatı", CourseCode = "TDE-201", Subject = "Edebiyat", Level = "Orta", Credits = 3, Description = "Dünya klasikleri", IsActive = true },

            // Sosyal Bilimler
            new Course { CourseName = "Tarih", CourseCode = "TAR-101", Subject = "Tarih", Level = "Başlangıç", Credits = 3, Description = "Dünya ve Türk tarihi", IsActive = true },
            new Course { CourseName = "Coğrafya", CourseCode = "COG-101", Subject = "Coğrafya", Level = "Başlangıç", Credits = 3, Description = "Fiziki ve beşeri coğrafya", IsActive = true },
            new Course { CourseName = "Felsefe", CourseCode = "FEL-101", Subject = "Felsefe", Level = "Orta", Credits = 2, Description = "Temel felsefe", IsActive = true },

            // Bilgisayar Bilimleri
            new Course { CourseName = "Programlamaya Giriş", CourseCode = "CS-101", Subject = "Bilgisayar", Level = "Başlangıç", Credits = 4, Description = "Python ile programlama", IsActive = true },
            new Course { CourseName = "Web Geliştirme", CourseCode = "CS-201", Subject = "Bilgisayar", Level = "Orta", Credits = 5, Description = "HTML, CSS, JavaScript", IsActive = true },
            new Course { CourseName = "Veri Yapıları", CourseCode = "CS-301", Subject = "Bilgisayar", Level = "İleri", Credits = 6, Description = "Algoritmalar ve veri yapıları", IsActive = true }
        };

        await context.Courses.AddRangeAsync(courses);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ {courses.Count} courses created");
    }

    private static async Task SeedUsersAndEntitiesAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        var branches = await context.Branches.ToListAsync();
        var random = new Random();

        // Türkçe İsimler
        var maleNames = new[] { "Ahmet", "Mehmet", "Mustafa", "Ali", "Hasan", "Hüseyin", "İbrahim", "Yusuf", "Ömer", "Halil", "Emre", "Burak", "Can", "Cem", "Deniz", "Efe", "Ege", "Kaan", "Kerem", "Mert", "Onur", "Ozan", "Serkan", "Tolga", "Utku", "Volkan", "Yiğit", "Barış", "Berkay", "Oğuz" };
        var femaleNames = new[] { "Ayşe", "Fatma", "Zeynep", "Emine", "Hatice", "Elif", "Meryem", "Zehra", "Selin", "Defne", "Nil", "Ece", "Esra", "İrem", "Merve", "Nazlı", "Pınar", "Seda", "Yasemin", "Burcu", "Cansu", "Duygu", "Ebru", "Gamze", "Gizem", "Gül", "Melis", "Naz", "Pelin", "Tuba" };
        var lastNames = new[] { "Yılmaz", "Kaya", "Demir", "Şahin", "Çelik", "Yıldız", "Yıldırım", "Öztürk", "Aydın", "Özdemir", "Arslan", "Doğan", "Kılıç", "Aslan", "Çetin", "Kara", "Koç", "Kurt", "Özkan", "Şimşek", "Erdoğan", "Güneş", "Korkmaz", "Taş", "Polat", "Yavuz", "Acar", "Bulut", "Çakır", "Türk" };

        // 1. ÖĞRETMENLER (50 öğretmen)
        var teacherUsers = new List<ApplicationUser>();
        for (int i = 1; i <= 50; i++)
        {
            var isMale = i % 2 == 0;
            var firstName = isMale ? maleNames[random.Next(maleNames.Length)] : femaleNames[random.Next(femaleNames.Length)];
            var lastName = lastNames[random.Next(lastNames.Length)];
            var email = $"{firstName.ToLower()}.{lastName.ToLower()}{i}@eduportal.com";

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = $"555{random.Next(1000000, 9999999)}",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(100, 500))
            };

            var result = await userManager.CreateAsync(user, "Teacher@123");
            if (result.Succeeded)
            {
                // Permission tabanlı yetki ataması (rol yerine)
                await AssignDefaultPermissionsAsync(context, user.Id, "Teacher");
                teacherUsers.Add(user);
            }
        }

        // Teacher entity'lerini oluştur
        var teachers = new List<Teacher>();
        var subjects = new[] { "Matematik", "Fizik", "Kimya", "Biyoloji", "İngilizce", "Edebiyat", "Tarih", "Coğrafya", "Bilgisayar" };
        foreach (var user in teacherUsers)
        {
            teachers.Add(new Teacher
            {
                UserId = user.Id,
                Specialization = subjects[random.Next(subjects.Length)],
                Experience = random.Next(1, 20),
                IsActive = true,
                BranchId = branches[random.Next(branches.Count)].Id
            });
        }
        await context.Teachers.AddRangeAsync(teachers);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ {teachers.Count} teachers created");

        // 2. DANIŞMANLAR (20 danışman)
        var counselorUsers = new List<ApplicationUser>();
        for (int i = 1; i <= 20; i++)
        {
            var isMale = i % 2 == 0;
            var firstName = isMale ? maleNames[random.Next(maleNames.Length)] : femaleNames[random.Next(femaleNames.Length)];
            var lastName = lastNames[random.Next(lastNames.Length)];
            var email = $"counselor.{firstName.ToLower()}.{lastName.ToLower()}{i}@eduportal.com";

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = $"555{random.Next(1000000, 9999999)}",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(100, 500))
            };

            var result = await userManager.CreateAsync(user, "Counselor@123");
            if (result.Succeeded)
            {
                // Permission tabanlı yetki ataması (rol yerine)
                await AssignDefaultPermissionsAsync(context, user.Id, "Counselor");
                counselorUsers.Add(user);
            }
        }

        var counselors = new List<Counselor>();
        foreach (var user in counselorUsers)
        {
            counselors.Add(new Counselor
            {
                UserId = user.Id,
                Specialization = new[] { "Akademik Danışmanlık", "Kariyer Danışmanlığı", "Psikolojik Danışmanlık", "Üniversite Yerleştirme" }[random.Next(4)],
                IsActive = true
            });
        }
        await context.Counselors.AddRangeAsync(counselors);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ {counselors.Count} counselors created");

        // 3. VELİLER VE ÖĞRENCİLER (200 öğrenci, 300+ veli)
        var students = new List<Student>();
        var parents = new List<Parent>();
        var studentParents = new List<StudentParent>();
        var parentUserIds = new List<string>();

        for (int i = 1; i <= 200; i++)
        {
            var isMale = i % 2 == 0;
            var studentFirstName = isMale ? maleNames[random.Next(maleNames.Length)] : femaleNames[random.Next(femaleNames.Length)];
            var studentLastName = lastNames[random.Next(lastNames.Length)];
            var studentEmail = $"student.{studentFirstName.ToLower()}.{studentLastName.ToLower()}{i}@eduportal.com";

            // Öğrenci User
            var studentUser = new ApplicationUser
            {
                UserName = studentEmail,
                Email = studentEmail,
                FirstName = studentFirstName,
                LastName = studentLastName,
                PhoneNumber = $"555{random.Next(1000000, 9999999)}",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(50, 300))
            };

            var studentResult = await userManager.CreateAsync(studentUser, "Student@123");
            if (!studentResult.Succeeded) continue;

            // Permission tabanlı yetki ataması (rol yerine)
            await AssignDefaultPermissionsAsync(context, studentUser.Id, "Student");

            // Student Entity
            var birthYear = DateTime.Now.Year - random.Next(14, 18);
            var student = new Student
            {
                UserId = studentUser.Id,
                StudentNo = $"STD{DateTime.Now.Year}{i:D4}",
                SchoolName = new[] { "Kadıköy Anadolu Lisesi", "Beşiktaş Fen Lisesi", "Ankara Fen Lisesi", "İzmir Anadolu Lisesi", "Robert Kolej", "Galatasaray Lisesi" }[random.Next(6)],
                CurrentGrade = random.Next(9, 13),
                Gender = isMale ? Gender.Erkek : Gender.Kiz,
                DateOfBirth = new DateTime(birthYear, random.Next(1, 13), random.Next(1, 28)),
                EnrollmentDate = DateTime.UtcNow.AddDays(-random.Next(30, 200)),
                BranchId = branches[random.Next(branches.Count)].Id,
                LGSPercentile = random.Next(70, 100),
                IsBilsem = random.Next(100) < 15
            };
            students.Add(student);

            // Anne
            var motherFirstName = femaleNames[random.Next(femaleNames.Length)];
            var motherEmail = $"parent.{motherFirstName.ToLower()}.{studentLastName.ToLower()}.anne{i}@gmail.com";
            var motherUser = new ApplicationUser
            {
                UserName = motherEmail,
                Email = motherEmail,
                FirstName = motherFirstName,
                LastName = studentLastName,
                PhoneNumber = $"555{random.Next(1000000, 9999999)}",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(50, 300))
            };

            var motherResult = await userManager.CreateAsync(motherUser, "Parent@123");
            if (motherResult.Succeeded)
            {
                // Permission tabanlı yetki ataması (rol yerine)
                await AssignDefaultPermissionsAsync(context, motherUser.Id, "Parent");
                parentUserIds.Add(motherUser.Id);

                var mother = new Parent
                {
                    UserId = motherUser.Id,
                    Occupation = new[] { "Öğretmen", "Hemşire", "Mühendis", "Doktor", "Avukat", "İşletmeci", "Ev Hanımı", "Memur" }[random.Next(8)],
                    WorkPhone = $"555{random.Next(1000000, 9999999)}"
                };
                parents.Add(mother);
            }

            // Baba
            var fatherFirstName = maleNames[random.Next(maleNames.Length)];
            var fatherEmail = $"parent.{fatherFirstName.ToLower()}.{studentLastName.ToLower()}.baba{i}@gmail.com";
            var fatherUser = new ApplicationUser
            {
                UserName = fatherEmail,
                Email = fatherEmail,
                FirstName = fatherFirstName,
                LastName = studentLastName,
                PhoneNumber = $"555{random.Next(1000000, 9999999)}",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(50, 300))
            };

            var fatherResult = await userManager.CreateAsync(fatherUser, "Parent@123");
            if (fatherResult.Succeeded)
            {
                // Permission tabanlı yetki ataması (rol yerine)
                await AssignDefaultPermissionsAsync(context, fatherUser.Id, "Parent");
                parentUserIds.Add(fatherUser.Id);

                var father = new Parent
                {
                    UserId = fatherUser.Id,
                    Occupation = new[] { "Mühendis", "Doktor", "Avukat", "İşletmeci", "Memur", "Esnaf", "Öğretmen" }[random.Next(7)],
                    WorkPhone = $"555{random.Next(1000000, 9999999)}"
                };
                parents.Add(father);
            }
        }

        await context.Students.AddRangeAsync(students);
        await context.Parents.AddRangeAsync(parents);
        await context.SaveChangesAsync();

        // StudentParent ilişkilerini kur
        foreach (var student in students)
        {
            var relatedParents = parents.Where(p => p.User.LastName == student.User.LastName).Take(2).ToList();
            foreach (var parent in relatedParents)
            {
                studentParents.Add(new StudentParent
                {
                    StudentId = student.Id,
                    ParentId = parent.Id,
                    Relationship = parent.User.FirstName.EndsWith('a') ? "Anne" : "Baba",
                    IsPrimaryContact = parent.User.FirstName.EndsWith('a')
                });
            }
        }

        await context.StudentParents.AddRangeAsync(studentParents);
        await context.SaveChangesAsync();

        Console.WriteLine($"✓ {students.Count} students created");
        Console.WriteLine($"✓ {parents.Count} parents created");
        Console.WriteLine($"✓ {studentParents.Count} student-parent relationships created");
    }

    private static async Task SeedClassesAsync(ApplicationDbContext context)
    {
        var branches = await context.Branches.Include(b => b.Teachers).ToListAsync();
        var currentTerm = await context.AcademicTerms.FirstOrDefaultAsync(t => t.IsActive);
        var random = new Random();
        var classes = new List<Class>();

        foreach (var branch in branches)
        {
            var branchTeachers = branch.Teachers.ToList();
            if (!branchTeachers.Any()) continue;

            // Her şube için 8-12 sınıf
            for (int grade = 9; grade <= 12; grade++)
            {
                for (int section = 1; section <= 3; section++)
                {
                    var className = $"{grade}/{(char)('A' + section - 1)}";
                    var classTeacher = branchTeachers[random.Next(branchTeachers.Count)];

                    classes.Add(new Class
                    {
                        ClassName = className,
                        Grade = grade,
                        Branch = ((char)('A' + section - 1)).ToString(),
                        ClassTeacherId = classTeacher.Id,
                        Capacity = 30,
                        AcademicYear = $"{DateTime.Now.Year}-{DateTime.Now.Year + 1}",
                        BranchId = branch.Id,
                        IsActive = true
                    });
                }
            }
        }

        await context.Classes.AddRangeAsync(classes);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ {classes.Count} classes created");

        // Öğrencileri sınıflara ata
        var students = await context.Students.Include(s => s.Branch).ToListAsync();
        var studentClassAssignments = new List<StudentClassAssignment>();

        foreach (var student in students)
        {
            var matchingClasses = classes
                .Where(c => c.Grade == student.CurrentGrade && c.BranchId == student.BranchId)
                .ToList();

            if (matchingClasses.Any() && currentTerm != null)
            {
                var assignedClass = matchingClasses[random.Next(matchingClasses.Count)];
                studentClassAssignments.Add(new StudentClassAssignment
                {
                    StudentId = student.Id,
                    ClassId = assignedClass.Id,
                    AcademicTermId = currentTerm.Id,
                    AssignmentDate = DateTime.UtcNow.AddDays(-random.Next(30, 90)),
                    IsActive = true
                });
            }
        }

        await context.StudentClassAssignments.AddRangeAsync(studentClassAssignments);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ {studentClassAssignments.Count} student-class assignments created");
    }

    private static async Task SeedSchedulingDataAsync(ApplicationDbContext context)
    {
        var students = await context.Students.Include(s => s.User).ToListAsync();
        var teachers = await context.Teachers.Include(t => t.User).ToListAsync();
        var courses = await context.Courses.Where(c => c.IsActive).ToListAsync();
        var random = new Random();

        var studentAvailabilities = new List<StudentAvailability>();
        var teacherAvailabilities = new List<TeacherAvailability>();
        var lessonSchedules = new List<LessonSchedule>();

        // Öğrenci müsaitlikleri
        foreach (var student in students.Take(150)) // İlk 150 öğrenci
        {
            // Okul saatleri (Pazartesi-Cuma 08:00-16:00)
            for (int day = 1; day <= 5; day++)
            {
                studentAvailabilities.Add(new StudentAvailability
                {
                    StudentId = student.Id,
                    DayOfWeek = (DayOfWeek)day,
                    StartTime = new TimeSpan(8, 0, 0),
                    EndTime = new TimeSpan(16, 0, 0),
                    Type = AvailabilityType.School,
                    Notes = "Okul saatleri",
                    IsRecurring = true
                });

                // Akşam müsait saatler (17:00-21:00 arası random slotlar)
                int eveningSlots = random.Next(2, 5);
                for (int i = 0; i < eveningSlots; i++)
                {
                    var startHour = random.Next(17, 20);
                    studentAvailabilities.Add(new StudentAvailability
                    {
                        StudentId = student.Id,
                        DayOfWeek = (DayOfWeek)day,
                        StartTime = new TimeSpan(startHour, 0, 0),
                        EndTime = new TimeSpan(startHour + 1, 30, 0),
                        Type = AvailabilityType.Available,
                        Notes = "Etüt saati",
                        IsRecurring = true
                    });
                }
            }

            // Cumartesi müsaitlikleri
            int saturdaySlots = random.Next(3, 6);
            for (int i = 0; i < saturdaySlots; i++)
            {
                var startHour = random.Next(9, 18);
                studentAvailabilities.Add(new StudentAvailability
                {
                    StudentId = student.Id,
                    DayOfWeek = DayOfWeek.Saturday,
                    StartTime = new TimeSpan(startHour, 0, 0),
                    EndTime = new TimeSpan(startHour + 2, 0, 0),
                    Type = AvailabilityType.Available,
                    Notes = "Hafta sonu etüdü",
                    IsRecurring = true
                });
            }
        }

        // Öğretmen müsaitlikleri
        foreach (var teacher in teachers)
        {
            // Hafta içi gün boyu müsait (08:00-22:00)
            for (int day = 1; day <= 5; day++)
            {
                teacherAvailabilities.Add(new TeacherAvailability
                {
                    TeacherId = teacher.Id,
                    DayOfWeek = (DayOfWeek)day,
                    StartTime = new TimeSpan(8, 0, 0),
                    EndTime = new TimeSpan(22, 0, 0),
                    Type = AvailabilityType.Available,
                    Notes = "Ders saatleri",
                    IsRecurring = true
                });
            }

            // Cumartesi
            teacherAvailabilities.Add(new TeacherAvailability
            {
                TeacherId = teacher.Id,
                DayOfWeek = DayOfWeek.Saturday,
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(18, 0, 0),
                Type = AvailabilityType.Available,
                Notes = "Cumartesi ders saatleri",
                IsRecurring = true
            });

            // Pazar (yarım gün)
            if (random.Next(100) < 30) // %30 öğretmen pazar da çalışıyor
            {
                teacherAvailabilities.Add(new TeacherAvailability
                {
                    TeacherId = teacher.Id,
                    DayOfWeek = DayOfWeek.Sunday,
                    StartTime = new TimeSpan(10, 0, 0),
                    EndTime = new TimeSpan(14, 0, 0),
                    Type = AvailabilityType.Available,
                    Notes = "Pazar ders saatleri",
                    IsRecurring = true
                });
            }
        }

        await context.StudentAvailabilities.AddRangeAsync(studentAvailabilities);
        await context.TeacherAvailabilities.AddRangeAsync(teacherAvailabilities);
        await context.SaveChangesAsync();

        Console.WriteLine($"✓ {studentAvailabilities.Count} student availabilities created");
        Console.WriteLine($"✓ {teacherAvailabilities.Count} teacher availabilities created");

        // Ders programları oluştur (Her öğrenci için 3-5 ders)
        foreach (var student in students.Take(100)) // İlk 100 öğrenciye ders ata
        {
            var numberOfLessons = random.Next(3, 6);
            var assignedTeachers = new HashSet<int>();

            for (int i = 0; i < numberOfLessons; i++)
            {
                var teacher = teachers[random.Next(teachers.Count)];
                while (assignedTeachers.Contains(teacher.Id))
                {
                    teacher = teachers[random.Next(teachers.Count)];
                }
                assignedTeachers.Add(teacher.Id);

                var course = courses[random.Next(courses.Count)];
                var dayOfWeek = (DayOfWeek)random.Next(1, 7); // Pazartesi-Cumartesi
                var startHour = random.Next(9, 20);

                lessonSchedules.Add(new LessonSchedule
                {
                    StudentId = student.Id,
                    TeacherId = teacher.Id,
                    CourseId = course.Id,
                    DayOfWeek = dayOfWeek,
                    StartTime = new TimeSpan(startHour, 0, 0),
                    EndTime = new TimeSpan(startHour + 1, 30, 0),
                    EffectiveFrom = DateTime.UtcNow.AddDays(-random.Next(10, 60)),
                    Status = LessonStatus.Scheduled,
                    IsRecurring = true,
                    Notes = $"{course.CourseName} dersi"
                });
            }
        }

        await context.LessonSchedules.AddRangeAsync(lessonSchedules);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ {lessonSchedules.Count} lesson schedules created");
    }

    private static async Task SeedHomeworksAsync(ApplicationDbContext context)
    {
        var courses = await context.Courses.Include(c => c.Teachers).ToListAsync();
        var classes = await context.Classes.ToListAsync();
        var random = new Random();
        var homeworks = new List<Homework>();

        foreach (var course in courses.Where(c => c.Teachers.Any()).Take(20))
        {
            var teacher = course.Teachers.First();
            var relatedClass = classes[random.Next(classes.Count)];

            for (int i = 1; i <= 5; i++)
            {
                homeworks.Add(new Homework
                {
                    Title = $"{course.CourseName} - Ödev {i}",
                    Description = $"{course.CourseName} konusu üzerine araştırma ve uygulama ödevi",
                    CourseId = course.Id,
                    TeacherId = teacher.Id,
                    ClassId = relatedClass.Id,
                    AssignedDate = DateTime.UtcNow.AddDays(-random.Next(1, 10)),
                    DueDate = DateTime.UtcNow.AddDays(random.Next(7, 30)),
                    MaxScore = 100
                });
            }
        }

        await context.Homeworks.AddRangeAsync(homeworks);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ {homeworks.Count} homeworks created");
    }

    private static async Task SeedExamsAsync(ApplicationDbContext context)
    {
        var courses = await context.Courses.Include(c => c.Teachers).ToListAsync();
        var classes = await context.Classes.ToListAsync();
        var currentTerm = await context.AcademicTerms.FirstOrDefaultAsync(t => t.IsActive);
        var random = new Random();
        var exams = new List<InternalExam>();

        foreach (var course in courses.Where(c => c.Teachers.Any()).Take(15))
        {
            var teacher = course.Teachers.First();
            var relatedClass = classes[random.Next(classes.Count)];

            exams.Add(new InternalExam
            {
                Title = $"{course.CourseName} Vize Sınavı",
                ExamType = "Deneme",
                CourseId = course.Id,
                TeacherId = teacher.Id,
                ClassId = relatedClass.Id,
                AcademicTermId = currentTerm?.Id,
                ExamDate = DateTime.UtcNow.AddDays(random.Next(-30, 30)),
                Duration = random.Next(60, 120),
                MaxScore = 100
            });

            exams.Add(new InternalExam
            {
                Title = $"{course.CourseName} Final Sınavı",
                ExamType = "Donem",
                CourseId = course.Id,
                TeacherId = teacher.Id,
                ClassId = relatedClass.Id,
                AcademicTermId = currentTerm?.Id,
                ExamDate = DateTime.UtcNow.AddDays(random.Next(30, 90)),
                Duration = random.Next(90, 150),
                MaxScore = 100
            });
        }

        await context.InternalExams.AddRangeAsync(exams);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ {exams.Count} exams created");
    }

    private static async Task SeedPaymentPlansAsync(ApplicationDbContext context)
    {
        var paymentPlans = new List<PaymentPlan>
        {
            new PaymentPlan
            {
                PlanName = "Standart 10 Taksit Planı",
                Description = "10 aylık taksit seçeneği",
                InstallmentCount = 10,
                DaysBetweenInstallments = 30,
                DownPaymentDiscount = 5,
                IsActive = true
            },
            new PaymentPlan
            {
                PlanName = "Premium 12 Taksit Planı",
                Description = "12 aylık uzun vadeli taksit",
                InstallmentCount = 12,
                DaysBetweenInstallments = 30,
                DownPaymentDiscount = 10,
                IsActive = true
            },
            new PaymentPlan
            {
                PlanName = "Peşin Ödeme Planı",
                Description = "Tek seferde peşin ödeme - %15 indirim",
                InstallmentCount = 1,
                DaysBetweenInstallments = 0,
                DownPaymentDiscount = 15,
                IsActive = true
            }
        };

        await context.PaymentPlans.AddRangeAsync(paymentPlans);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ {paymentPlans.Count} payment plans created");
    }

    private static async Task SeedServicePackagesAsync(ApplicationDbContext context)
    {
        var packages = new List<ServicePackage>
        {
            new ServicePackage
            {
                PackageName = "SAT Hazırlık Paketi",
                Description = "SAT sınavına kapsamlı hazırlık programı",
                Type = PackageType.TuitionPremium,
                Price = 12000,
                SessionCount = 48,
                ValidityMonths = 6,
                Includes = "12 haftalık yoğun program, Mock testler, Bireysel danışmanlık",
                IsActive = true
            },
            new ServicePackage
            {
                PackageName = "TOEFL Hazırlık Paketi",
                Description = "TOEFL sınavı hazırlık programı",
                Type = PackageType.TuitionStandard,
                Price = 10000,
                SessionCount = 32,
                ValidityMonths = 4,
                Includes = "Speaking, Writing, Reading, Listening modülleri",
                IsActive = true
            },
            new ServicePackage
            {
                PackageName = "Üniversite Başvuru Paketi",
                Description = "ABD/UK üniversite başvuru danışmanlığı",
                Type = PackageType.StudyAbroadPremium,
                Price = 15000,
                SessionCount = 24,
                ValidityMonths = 12,
                Includes = "Okul seçimi, Essay yazımı, Başvuru takibi",
                IsActive = true
            },
            new ServicePackage
            {
                PackageName = "Kariyer Koçluğu Paketi",
                Description = "Bireysel kariyer planlama ve koçluk",
                Type = PackageType.CareerCoaching,
                Price = 8000,
                SessionCount = 16,
                ValidityMonths = 6,
                Includes = "Meslek testleri, Bireysel görüşmeler, Hedef belirleme",
                IsActive = true
            }
        };

        await context.ServicePackages.AddRangeAsync(packages);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ {packages.Count} service packages created");
    }

    private static async Task SeedEmailTemplatesAsync(ApplicationDbContext context)
    {
        if (await context.EmailTemplates.AnyAsync())
            return;

        var templates = new List<EmailTemplate>
        {
            new EmailTemplate
            {
                TemplateName = "Veli Ödeme Bilgilendirme",
                TemplateType = EmailTemplateType.VeliOdemeBilgilendirme,
                Subject = "Ödeme Bildirimi - {OgrenciAdi}",
                Body = @"
                <h2>Sayın {VeliAdi},</h2>
                <p><strong>{OgrenciAdi}</strong> ({OgrenciNo}) için ödeme bildirimi:</p>
                <ul>
                    <li><strong>Tutar:</strong> {OdemeTutari}</li>
                    <li><strong>Son Ödeme Tarihi:</strong> {OdemeTarihi}</li>
                    <li><strong>Durum:</strong> {OdemeDurumu}</li>
                    <li><strong>Açıklama:</strong> {Aciklama}</li>
                </ul>
                <p>Saygılarımızla,<br/>EduPortal</p>
            ",
                VariablesJson = "{\"VeliAdi\":\"\",\"OgrenciAdi\":\"\",\"OgrenciNo\":\"\",\"OdemeTutari\":\"\",\"OdemeTarihi\":\"\",\"OdemeDurumu\":\"\",\"Aciklama\":\"\"}",
                IsActive = true
            },
            new EmailTemplate
            {
                TemplateName = "Sınav Sonucu Bildirimi",
                TemplateType = EmailTemplateType.SinavSonucBildirimi,
                Subject = "Sınav Sonucunuz - {DersAdi}",
                Body = @"
                <h2>Merhaba {OgrenciAdi},</h2>
                <p><strong>{DersAdi}</strong> dersi <strong>{SinavAdi}</strong> sınavı sonucunuz:</p>
                <ul>
                    <li><strong>Aldığınız Puan:</strong> {Puan}</li>
                    <li><strong>Toplam Puan:</strong> {ToplamPuan}</li>
                    <li><strong>Başarı Yüzdesi:</strong> %{Yuzde}</li>
                    <li><strong>Sınav Tarihi:</strong> {SinavTarihi}</li>
                </ul>
                <p>Başarılar dileriz!<br/>EduPortal</p>
            ",
                VariablesJson = "{\"OgrenciAdi\":\"\",\"DersAdi\":\"\",\"SinavAdi\":\"\",\"Puan\":\"\",\"ToplamPuan\":\"\",\"Yuzde\":\"\",\"SinavTarihi\":\"\"}",
                IsActive = true
            },
            new EmailTemplate
            {
                TemplateName = "Devamsızlık Uyarısı",
                TemplateType = EmailTemplateType.DevamsizlikUyari,
                Subject = "Devamsızlık Uyarısı - {OgrenciAdi}",
                Body = @"
                <h2>Sayın Veli/Öğrenci,</h2>
                <p><strong>{OgrenciAdi}</strong> ({OgrenciNo}) için devamsızlık uyarısı:</p>
                <ul>
                    <li><strong>Toplam Devamsızlık:</strong> {DevamsizlikSayisi}</li>
                    <li><strong>Toplam Ders:</strong> {ToplamDers}</li>
                    <li><strong>Devamsızlık Oranı:</strong> %{DevamsizlikOrani}</li>
                </ul>
                <p>Lütfen devamsızlık durumunu takip ediniz.</p>
                <p>Saygılarımızla,<br/>EduPortal</p>
            ",
                VariablesJson = "{\"OgrenciAdi\":\"\",\"OgrenciNo\":\"\",\"DevamsizlikSayisi\":\"\",\"ToplamDers\":\"\",\"DevamsizlikOrani\":\"\"}",
                IsActive = true
            },
            new EmailTemplate
            {
                TemplateName = "Taksit Hatırlatma",
                TemplateType = EmailTemplateType.TaksitHatirlatma,
                Subject = "Ödeme Hatırlatma - Taksit {TaksitNo}",
                Body = @"
                <h2>Sayın Veli,</h2>
                <p><strong>{OgrenciAdi}</strong> ({OgrenciNo}) için ödeme hatırlatması:</p>
                <ul>
                    <li><strong>Taksit No:</strong> {TaksitNo}</li>
                    <li><strong>Tutar:</strong> {TaksitTutari}</li>
                    <li><strong>Son Ödeme Tarihi:</strong> {SonOdemeTarihi}</li>
                    <li><strong>Kalan Gün:</strong> {KalanGun} gün</li>
                </ul>
                <p>Zamanında ödeme yapmanızı rica ederiz.</p>
                <p>Saygılarımızla,<br/>EduPortal</p>
            ",
                VariablesJson = "{\"OgrenciAdi\":\"\",\"OgrenciNo\":\"\",\"TaksitNo\":\"\",\"TaksitTutari\":\"\",\"SonOdemeTarihi\":\"\",\"KalanGun\":\"\",\"Durum\":\"\"}",
                IsActive = true
            }
        };

        await context.EmailTemplates.AddRangeAsync(templates);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ {templates.Count} email templates created");
    }

    private static async Task SeedPermissionsAsync(ApplicationDbContext context, RoleManager<IdentityRole> roleManager)
    {
        // Check if permissions already exist
        if (await context.Permissions.AnyAsync())
        {
            Console.WriteLine("✓ Permissions already seeded");
            return;
        }

        // Create all permissions from constants
        var permissions = Permissions.All.Select(kvp => new Permission
        {
            Code = kvp.Key,
            Name = kvp.Value.Name,
            Category = kvp.Value.Category,
            Icon = kvp.Value.Icon,
            DisplayOrder = kvp.Value.Order,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        await context.Permissions.AddRangeAsync(permissions);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ {permissions.Count} permissions created");

        // Assign all permissions to Admin role
        var adminRole = await roleManager.FindByNameAsync("Admin");
        if (adminRole != null)
        {
            var rolePermissions = permissions.Select(p => new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = p.Id,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await context.RolePermissions.AddRangeAsync(rolePermissions);
            await context.SaveChangesAsync();
            Console.WriteLine($"✓ All permissions assigned to Admin role");
        }

        // NOT: Diğer roller (Kayitci, Ogretmen, Danışman, Coach, Muhasebe) kaldırıldı.
        // Yetkiler artık permission tabanlı sistem ile kullanıcı bazında yönetilmektedir.
    }

    private static async Task AssignDefaultPermissionsAsync(ApplicationDbContext context, string userId, string userType)
    {
        // Kullanıcı tipine göre varsayılan yetki kodlarını al
        var defaultPermissionCodes = Permissions.GetDefaultPermissionsForUserType(userType);
        if (!defaultPermissionCodes.Any())
        {
            return;
        }

        // Kodlara göre permission ID'lerini bul
        var permissions = await context.Permissions
            .Where(p => defaultPermissionCodes.Contains(p.Code) && !p.IsDeleted && p.IsActive)
            .ToListAsync();

        if (!permissions.Any())
        {
            return;
        }

        // Yetkileri ata
        foreach (var permission in permissions)
        {
            var userPermission = new UserPermission
            {
                UserId = userId,
                PermissionId = permission.Id,
                IsGranted = true,
                GrantedAt = DateTime.UtcNow,
                Notes = $"Varsayılan {userType} yetkileri - DbInitializer"
            };
            await context.UserPermissions.AddAsync(userPermission);
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedFinanceTestDataAsync(ApplicationDbContext context)
    {
        // Tekrarlayan giderler ekle
        if (!await context.RecurringExpenses.AnyAsync())
        {
            var branch = await context.Branches.FirstOrDefaultAsync();
            var branchId = branch?.Id;

            var recurringExpenses = new List<RecurringExpense>
            {
                new RecurringExpense
                {
                    Title = "Ofis Kirası",
                    Description = "Aylık ofis kira ödemesi",
                    Category = FinanceCategory.Kira,
                    Amount = 25000,
                    RecurrenceType = RecurrenceType.Aylik,
                    RecurrenceDay = 1,
                    StartDate = new DateTime(2025, 1, 1),
                    NextDueDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1),
                    IsActive = true,
                    BranchId = branchId
                },
                new RecurringExpense
                {
                    Title = "İnternet Faturası",
                    Description = "Aylık internet ve telefon faturası",
                    Category = FinanceCategory.Fatura,
                    Amount = 1500,
                    RecurrenceType = RecurrenceType.Aylik,
                    RecurrenceDay = 15,
                    StartDate = new DateTime(2025, 1, 1),
                    NextDueDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 15).AddMonths(DateTime.Now.Day > 15 ? 1 : 0),
                    IsActive = true,
                    BranchId = branchId
                },
                new RecurringExpense
                {
                    Title = "Temizlik Hizmeti",
                    Description = "Haftalık temizlik hizmeti ödemesi",
                    Category = FinanceCategory.Bakim,
                    Amount = 2000,
                    RecurrenceType = RecurrenceType.Haftalik,
                    RecurrenceDay = 1,
                    StartDate = new DateTime(2025, 1, 1),
                    NextDueDate = DateTime.Now.Date.AddDays((7 - (int)DateTime.Now.DayOfWeek + 1) % 7 + 1),
                    IsActive = true,
                    BranchId = branchId
                },
                new RecurringExpense
                {
                    Title = "Elektrik Faturası",
                    Description = "Aylık elektrik faturası",
                    Category = FinanceCategory.Fatura,
                    Amount = 3500,
                    RecurrenceType = RecurrenceType.Aylik,
                    RecurrenceDay = 20,
                    StartDate = new DateTime(2025, 1, 1),
                    NextDueDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 20).AddMonths(DateTime.Now.Day > 20 ? 1 : 0),
                    IsActive = true,
                    BranchId = branchId
                },
                new RecurringExpense
                {
                    Title = "Su Faturası",
                    Description = "Aylık su faturası",
                    Category = FinanceCategory.Fatura,
                    Amount = 800,
                    RecurrenceType = RecurrenceType.Aylik,
                    RecurrenceDay = 25,
                    StartDate = new DateTime(2025, 1, 1),
                    NextDueDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 25).AddMonths(DateTime.Now.Day > 25 ? 1 : 0),
                    IsActive = true,
                    BranchId = branchId
                }
            };

            await context.RecurringExpenses.AddRangeAsync(recurringExpenses);
            await context.SaveChangesAsync();
            Console.WriteLine("✓ Recurring expenses seeded");
        }

        // Finans kayıtları ekle (hem gelir hem gider)
        if (!await context.FinanceRecords.Where(f => f.Type == FinanceType.Gider).AnyAsync())
        {
            var branch = await context.Branches.FirstOrDefaultAsync();
            var branchId = branch?.Id;
            var today = DateTime.Today;

            var financeRecords = new List<FinanceRecord>
            {
                // Gider kayıtları
                new FinanceRecord
                {
                    Type = FinanceType.Gider,
                    Category = FinanceCategory.Kira,
                    Title = $"{today.AddMonths(1):MMMM yyyy} Kirası",
                    Description = "Gelecek ay ofis kirası",
                    Amount = 25000,
                    TransactionDate = new DateTime(today.Year, today.Month, 1).AddMonths(1),
                    PaymentMethod = PaymentMethod.Havale,
                    BranchId = branchId
                },
                new FinanceRecord
                {
                    Type = FinanceType.Gider,
                    Category = FinanceCategory.Fatura,
                    Title = $"{today:MMMM yyyy} Elektrik Faturası",
                    Description = "Bu ay elektrik faturası",
                    Amount = 3500,
                    TransactionDate = today.AddDays(-5),
                    PaymentMethod = PaymentMethod.Havale,
                    BranchId = branchId
                },
                new FinanceRecord
                {
                    Type = FinanceType.Gider,
                    Category = FinanceCategory.Malzeme,
                    Title = "Kırtasiye Malzemeleri",
                    Description = "Sınıflar için kırtasiye malzemesi alımı",
                    Amount = 2500,
                    TransactionDate = today.AddDays(-10),
                    PaymentMethod = PaymentMethod.Kredi,
                    BranchId = branchId
                },
                new FinanceRecord
                {
                    Type = FinanceType.Gider,
                    Category = FinanceCategory.Bakim,
                    Title = "Klima Bakım",
                    Description = "Yıllık klima bakım ve temizliği",
                    Amount = 1800,
                    TransactionDate = today.AddDays(-15),
                    PaymentMethod = PaymentMethod.Nakit,
                    BranchId = branchId
                },
                new FinanceRecord
                {
                    Type = FinanceType.Gider,
                    Category = FinanceCategory.DigerGider,
                    Title = "Güvenlik Sistemi",
                    Description = "Güvenlik kamerası yenileme",
                    Amount = 8500,
                    TransactionDate = today.AddDays(-20),
                    PaymentMethod = PaymentMethod.Havale,
                    BranchId = branchId
                }
            };

            await context.FinanceRecords.AddRangeAsync(financeRecords);
            await context.SaveChangesAsync();
            Console.WriteLine("✓ Finance records (expenses) seeded");
        }

        // Gecikmiş öğretmen maaşları ekle
        var teachers = await context.Teachers.Take(3).ToListAsync();
        if (teachers.Any())
        {
            var today = DateTime.Today;
            var lastMonth = today.AddMonths(-1);
            var twoMonthsAgo = today.AddMonths(-2);

            foreach (var teacher in teachers)
            {
                // Geçmiş aylarda bekleyen (gecikmiş) maaş var mı kontrol et
                var hasOverdueSalary = await context.TeacherSalaries
                    .AnyAsync(s => s.TeacherId == teacher.Id &&
                                   s.Status == SalaryStatus.Bekliyor &&
                                   s.DueDate < today &&
                                   !s.IsDeleted);

                if (!hasOverdueSalary)
                {
                    // 2 ay önceki gecikmiş maaş
                    var overdueSalary1 = new TeacherSalary
                    {
                        TeacherId = teacher.Id,
                        BaseSalary = teacher.MonthlySalary ?? 15000,
                        Bonus = 0,
                        Deduction = 0,
                        Year = twoMonthsAgo.Year,
                        Month = twoMonthsAgo.Month,
                        DueDate = new DateTime(twoMonthsAgo.Year, twoMonthsAgo.Month, DateTime.DaysInMonth(twoMonthsAgo.Year, twoMonthsAgo.Month)),
                        Status = SalaryStatus.Bekliyor,
                        Description = "Gecikmiş maaş - test verisi"
                    };

                    // 1 ay önceki gecikmiş maaş
                    var overdueSalary2 = new TeacherSalary
                    {
                        TeacherId = teacher.Id,
                        BaseSalary = teacher.MonthlySalary ?? 15000,
                        Bonus = 500,
                        Deduction = 0,
                        Year = lastMonth.Year,
                        Month = lastMonth.Month,
                        DueDate = new DateTime(lastMonth.Year, lastMonth.Month, DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month)),
                        Status = SalaryStatus.Bekliyor,
                        Description = "Gecikmiş maaş - test verisi"
                    };

                    await context.TeacherSalaries.AddAsync(overdueSalary1);
                    await context.TeacherSalaries.AddAsync(overdueSalary2);
                }
            }

            await context.SaveChangesAsync();
            Console.WriteLine("✓ Overdue teacher salaries seeded");
        }
    }
}
