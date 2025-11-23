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

        // Veritabanında veri varsa seed etme
        if (await context.Branches.AnyAsync())
        {
            Console.WriteLine("Database already seeded. Skipping seed data.");
            return;
        }

        await SeedBranchesAsync(context);
        await SeedClassroomsAsync(context);
        await SeedAcademicTermsAsync(context);
        await SeedCoursesAsync(context);
        await SeedUsersAndEntitiesAsync(userManager, context);
        await SeedClassesAsync(context);
        await SeedSchedulingDataAsync(context);
        await SeedHomeworksAsync(context);
        await SeedExamsAsync(context);
        await SeedPaymentPlansAsync(context);
        await SeedServicePackagesAsync(context);
        await SeedEmailTemplatesAsync(context);

        Console.WriteLine("✅ Database seeded successfully with comprehensive data!");
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = {
            "Admin",
            "Ogrenci",      // Student
            "Ogretmen",     // Teacher
            "Danışman",     // Counselor
            "Muhasebe",     // Accounting
            "Veli",         // Parent
            "Kayitci",      // Registrar
            "Coach"         // Coach
        };

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
                await userManager.AddToRoleAsync(user, "Ogretmen");
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
                await userManager.AddToRoleAsync(user, "Danışman");
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

        // 3. COACH'LAR (15 coach)
        var coachUsers = new List<ApplicationUser>();
        for (int i = 1; i <= 15; i++)
        {
            var isMale = i % 2 == 0;
            var firstName = isMale ? maleNames[random.Next(maleNames.Length)] : femaleNames[random.Next(femaleNames.Length)];
            var lastName = lastNames[random.Next(lastNames.Length)];
            var email = $"coach.{firstName.ToLower()}.{lastName.ToLower()}{i}@eduportal.com";

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

            var result = await userManager.CreateAsync(user, "Coach@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Coach");
                coachUsers.Add(user);
            }
        }

        var coaches = new List<Coach>();
        var specializations = new[] { "Kariyer Koçluğu", "Akademik Koçluk", "Spor Koçluğu", "Yaşam Koçluğu", "Okul Seçimi Danışmanlığı" };
        foreach (var user in coachUsers)
        {
            coaches.Add(new Coach
            {
                UserId = user.Id,
                Specialization = specializations[random.Next(specializations.Length)],
                ExperienceYears = random.Next(2, 15),
                IsAvailable = true,
                BranchId = branches[random.Next(branches.Count)].Id
            });
        }
        await context.Coaches.AddRangeAsync(coaches);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ {coaches.Count} coaches created");

        // 4. VELİLER VE ÖĞRENCİLER (200 öğrenci, 300+ veli)
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

            await userManager.AddToRoleAsync(studentUser, "Ogrenci");

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
                await userManager.AddToRoleAsync(motherUser, "Veli");
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
                await userManager.AddToRoleAsync(fatherUser, "Veli");
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
}
