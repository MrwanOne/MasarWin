using Masar.Application.Interfaces;
using Masar.Domain.Entities;
using Masar.Domain.Enums;
using Masar.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Infrastructure.Seed;

/// <summary>
/// بذر بيانات اختبار شاملة تغطي 3 كليات، 6 أقسام، 12 دكتور، 36 طالب، 9 فرق، 9 مشاريع، 3 لجان، 3 مناقشات، 5 مستخدمين
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAdminAsync(
        IDbContextFactory<MasarDbContext> contextFactory,
        IPasswordHasher passwordHasher,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        // ══════════════════════════════════════════════════════════════
        // 1. الفصل الدراسي
        // ══════════════════════════════════════════════════════════════
        AcademicTerm? term;
        if (!await context.AcademicTerms.AnyAsync(cancellationToken))
        {
            term = new AcademicTerm
            {
                Year = 2025,
                Semester = 2,
                NameAr = "الفصل الثاني 2025",
                NameEn = "Second Semester 2025",
                StartDate = DateTime.Now.AddMonths(-1),
                EndDate = DateTime.Now.AddMonths(4),
                IsActive = true
            };
            context.AcademicTerms.Add(term);
            await context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            term = await context.AcademicTerms.FirstOrDefaultAsync(t => t.IsActive, cancellationToken);
        }

        // ══════════════════════════════════════════════════════════════
        // 2. معايير التقييم
        // ══════════════════════════════════════════════════════════════
        if (!await context.EvaluationCriteria.AnyAsync(cancellationToken))
        {
            context.EvaluationCriteria.AddRange(
                new EvaluationCriteria { NameAr = "التطبيق والبرمجة", NameEn = "Implementation & Programming", DescriptionAr = "تقييم جودة التطبيق والبرمجة", DescriptionEn = "Evaluate implementation and programming quality", MaxScore = 50, Weight = 0.5m, DisplayOrder = 1, IsActive = true },
                new EvaluationCriteria { NameAr = "التقديم والتقرير", NameEn = "Presentation & Report", DescriptionAr = "تقييم جودة التقديم والتقرير", DescriptionEn = "Evaluate presentation and report quality", MaxScore = 50, Weight = 0.5m, DisplayOrder = 2, IsActive = true }
            );
            await context.SaveChangesAsync(cancellationToken);
        }

        // ══════════════════════════════════════════════════════════════
        // 3. الكليات (3)
        // ══════════════════════════════════════════════════════════════
        if (!await context.Colleges.AnyAsync(cancellationToken))
        {
            context.Colleges.AddRange(
                new College { NameAr = "كلية الهندسة", NameEn = "College of Engineering", Code = "ENG" },
                new College { NameAr = "كلية العلوم", NameEn = "College of Science", Code = "SCI" },
                new College { NameAr = "كلية إدارة الأعمال", NameEn = "College of Business", Code = "BUS" }
            );
            await context.SaveChangesAsync(cancellationToken);
        }

        var colleges = await context.Colleges.OrderBy(c => c.CollegeId).ToListAsync(cancellationToken);
        var eng = colleges[0]; // كلية الهندسة
        var sci = colleges[1]; // كلية العلوم
        var bus = colleges[2]; // كلية إدارة الأعمال

        // ══════════════════════════════════════════════════════════════
        // 4. الأقسام (6 — 2 لكل كلية)
        // ══════════════════════════════════════════════════════════════
        if (!await context.Departments.AnyAsync(cancellationToken))
        {
            context.Departments.AddRange(
                // كلية الهندسة
                new Department { NameAr = "هندسة البرمجيات", NameEn = "Software Engineering", Code = "SWE", CollegeId = eng.CollegeId },
                new Department { NameAr = "هندسة الحاسب", NameEn = "Computer Engineering", Code = "CE", CollegeId = eng.CollegeId },
                // كلية العلوم
                new Department { NameAr = "علوم الحاسب", NameEn = "Computer Science", Code = "CS", CollegeId = sci.CollegeId },
                new Department { NameAr = "الرياضيات", NameEn = "Mathematics", Code = "MATH", CollegeId = sci.CollegeId },
                // كلية إدارة الأعمال
                new Department { NameAr = "نظم المعلومات", NameEn = "Information Systems", Code = "IS", CollegeId = bus.CollegeId },
                new Department { NameAr = "إدارة الأعمال", NameEn = "Business Administration", Code = "BA", CollegeId = bus.CollegeId }
            );
            await context.SaveChangesAsync(cancellationToken);
        }

        var depts = await context.Departments.OrderBy(d => d.DepartmentId).ToListAsync(cancellationToken);
        var swe = depts[0];  // هندسة البرمجيات
        var ce  = depts[1];  // هندسة الحاسب
        var cs  = depts[2];  // علوم الحاسب
        var math = depts[3]; // الرياضيات
        var isys = depts[4]; // نظم المعلومات
        var ba  = depts[5];  // إدارة الأعمال

        // ══════════════════════════════════════════════════════════════
        // 5. الدكاترة (12 — 2 لكل قسم = 4 لكل كلية)
        // ══════════════════════════════════════════════════════════════
        if (!await context.Doctors.AnyAsync(cancellationToken))
        {
            context.Doctors.AddRange(
                // ── كلية الهندسة ──
                new Doctor { FullName = "د. أحمد العتيبي", Email = "ahmed@masar.edu", Phone = "0501111001", Gender = "Male", Qualification = "PhD", Rank = AcademicRank.Professor, Specialization = "هندسة برمجيات", MaxSupervisionCount = 5, IsActive = true, CollegeId = eng.CollegeId, DepartmentId = swe.DepartmentId },
                new Doctor { FullName = "د. فاطمة الزهراني", Email = "fatimah@masar.edu", Phone = "0501111002", Gender = "Female", Qualification = "PhD", Rank = AcademicRank.AssociateProfessor, Specialization = "ذكاء اصطناعي", MaxSupervisionCount = 4, IsActive = true, CollegeId = eng.CollegeId, DepartmentId = swe.DepartmentId },
                new Doctor { FullName = "د. خالد الشمري", Email = "khaled@masar.edu", Phone = "0501111003", Gender = "Male", Qualification = "PhD", Rank = AcademicRank.AssistantProfessor, Specialization = "أنظمة مدمجة", MaxSupervisionCount = 5, IsActive = true, CollegeId = eng.CollegeId, DepartmentId = ce.DepartmentId },
                new Doctor { FullName = "د. نوف القحطاني", Email = "nouf@masar.edu", Phone = "0501111004", Gender = "Female", Qualification = "PhD", Rank = AcademicRank.Lecturer, Specialization = "شبكات حاسب", MaxSupervisionCount = 3, IsActive = true, CollegeId = eng.CollegeId, DepartmentId = ce.DepartmentId },

                // ── كلية العلوم ──
                new Doctor { FullName = "د. نورة السبيعي", Email = "noura@masar.edu", Phone = "0501111005", Gender = "Female", Qualification = "PhD", Rank = AcademicRank.Professor, Specialization = "خوارزميات", MaxSupervisionCount = 5, IsActive = true, CollegeId = sci.CollegeId, DepartmentId = cs.DepartmentId },
                new Doctor { FullName = "د. سعد الحربي", Email = "saad@masar.edu", Phone = "0501111006", Gender = "Male", Qualification = "PhD", Rank = AcademicRank.AssociateProfessor, Specialization = "أمن معلومات", MaxSupervisionCount = 4, IsActive = true, CollegeId = sci.CollegeId, DepartmentId = cs.DepartmentId },
                new Doctor { FullName = "د. هند الدوسري", Email = "hind@masar.edu", Phone = "0501111007", Gender = "Female", Qualification = "PhD", Rank = AcademicRank.AssistantProfessor, Specialization = "تحليل عددي", MaxSupervisionCount = 5, IsActive = true, CollegeId = sci.CollegeId, DepartmentId = math.DepartmentId },
                new Doctor { FullName = "د. عبدالله المطيري", Email = "abdullah@masar.edu", Phone = "0501111008", Gender = "Male", Qualification = "PhD", Rank = AcademicRank.Lecturer, Specialization = "إحصاء تطبيقي", MaxSupervisionCount = 3, IsActive = true, CollegeId = sci.CollegeId, DepartmentId = math.DepartmentId },

                // ── كلية إدارة الأعمال ──
                new Doctor { FullName = "د. يوسف الغامدي", Email = "yousuf@masar.edu", Phone = "0501111009", Gender = "Male", Qualification = "PhD", Rank = AcademicRank.Professor, Specialization = "نظم معلومات إدارية", MaxSupervisionCount = 5, IsActive = true, CollegeId = bus.CollegeId, DepartmentId = isys.DepartmentId },
                new Doctor { FullName = "د. ريم العنزي", Email = "reem@masar.edu", Phone = "0501111010", Gender = "Female", Qualification = "PhD", Rank = AcademicRank.AssociateProfessor, Specialization = "تحليل بيانات", MaxSupervisionCount = 4, IsActive = true, CollegeId = bus.CollegeId, DepartmentId = isys.DepartmentId },
                new Doctor { FullName = "د. عمر الراشدي", Email = "omar@masar.edu", Phone = "0501111011", Gender = "Male", Qualification = "PhD", Rank = AcademicRank.AssistantProfessor, Specialization = "إدارة مشاريع", MaxSupervisionCount = 5, IsActive = true, CollegeId = bus.CollegeId, DepartmentId = ba.DepartmentId },
                new Doctor { FullName = "د. سارة المالكي", Email = "sarah@masar.edu", Phone = "0501111012", Gender = "Female", Qualification = "PhD", Rank = AcademicRank.Lecturer, Specialization = "تسويق رقمي", MaxSupervisionCount = 3, IsActive = true, CollegeId = bus.CollegeId, DepartmentId = ba.DepartmentId }
            );
            await context.SaveChangesAsync(cancellationToken);
        }

        var doctors = await context.Doctors.OrderBy(d => d.DoctorId).ToListAsync(cancellationToken);
        // كلية الهندسة
        var drAhmed   = doctors[0];  // SWE
        var drFatimah = doctors[1];  // SWE
        var drKhaled  = doctors[2];  // CE
        var drNouf    = doctors[3];  // CE
        // كلية العلوم
        var drNoura    = doctors[4]; // CS
        var drSaad     = doctors[5]; // CS
        var drHind     = doctors[6]; // MATH
        var drAbdullah = doctors[7]; // MATH
        // كلية إدارة الأعمال
        var drYousuf = doctors[8];   // IS
        var drReem   = doctors[9];   // IS
        var drOmar   = doctors[10];  // BA
        var drSarah  = doctors[11];  // BA

        // ── تعيين رؤساء أقسام ──
        swe.HeadOfDepartmentId = drAhmed.DoctorId;
        cs.HeadOfDepartmentId  = drNoura.DoctorId;
        isys.HeadOfDepartmentId = drYousuf.DoctorId;
        await context.SaveChangesAsync(cancellationToken);

        // ══════════════════════════════════════════════════════════════
        // 6. الطلاب (36 — 6 لكل قسم)
        // ══════════════════════════════════════════════════════════════
        if (!await context.Students.AnyAsync(cancellationToken))
        {
            var studentData = new (string Name, string Number, string Gender, decimal GPA, int Level, int DeptId, int Year)[]
            {
                // ── هندسة البرمجيات (6) ──
                ("محمد الأحمدي",    "2021001", "Male",   4.50m, 8, swe.DepartmentId, 2021),
                ("عبدالرحمن السالم", "2021002", "Male",   3.80m, 8, swe.DepartmentId, 2021),
                ("لمى الخالدي",     "2021003", "Female", 4.20m, 8, swe.DepartmentId, 2021),
                ("سلطان العمري",    "2022004", "Male",   3.60m, 7, swe.DepartmentId, 2022),
                ("أسماء البلوي",    "2022005", "Female", 4.00m, 7, swe.DepartmentId, 2022),
                ("فهد الحارثي",     "2022006", "Male",   3.90m, 7, swe.DepartmentId, 2022),

                // ── هندسة الحاسب (6) ──
                ("تركي الزهراني",   "2021007", "Male",   4.10m, 8, ce.DepartmentId, 2021),
                ("هيفاء العتيبي",   "2021008", "Female", 4.70m, 8, ce.DepartmentId, 2021),
                ("راكان الشهري",    "2021009", "Male",   3.50m, 8, ce.DepartmentId, 2021),
                ("ريناد القرني",    "2022010", "Female", 4.30m, 7, ce.DepartmentId, 2022),
                ("عبدالعزيز المحيا","2021011", "Male",   3.70m, 8, ce.DepartmentId, 2021),
                ("منى الصاعدي",     "2022012", "Female", 4.00m, 7, ce.DepartmentId, 2022),

                // ── علوم الحاسب (6) ──
                ("ياسر الجهني",     "2021013", "Male",   4.40m, 8, cs.DepartmentId, 2021),
                ("غادة السلمي",     "2021014", "Female", 4.60m, 8, cs.DepartmentId, 2021),
                ("وليد الثقفي",     "2021015", "Male",   3.80m, 8, cs.DepartmentId, 2021),
                ("شهد الحربي",      "2022016", "Female", 4.10m, 7, cs.DepartmentId, 2022),
                ("بدر المغربي",     "2022017", "Male",   3.40m, 7, cs.DepartmentId, 2022),
                ("رنا العمري",      "2022018", "Female", 4.50m, 7, cs.DepartmentId, 2022),

                // ── الرياضيات (6) ──
                ("حسن الفيفي",      "2021019", "Male",   4.20m, 8, math.DepartmentId, 2021),
                ("سمية الشمراني",   "2021020", "Female", 3.90m, 8, math.DepartmentId, 2021),
                ("مشعل الرشيدي",    "2021021", "Male",   3.60m, 8, math.DepartmentId, 2021),
                ("أمل العسيري",     "2022022", "Female", 4.70m, 7, math.DepartmentId, 2022),
                ("طلال الدوسري",    "2022023", "Male",   3.30m, 7, math.DepartmentId, 2022),
                ("دلال المطيري",    "2022024", "Female", 4.00m, 7, math.DepartmentId, 2022),

                // ── نظم المعلومات (6) ──
                ("ماجد الشريف",     "2021025", "Male",   4.30m, 8, isys.DepartmentId, 2021),
                ("نجلاء الكندري",   "2021026", "Female", 4.50m, 8, isys.DepartmentId, 2021),
                ("أنس البقمي",      "2021027", "Male",   3.70m, 8, isys.DepartmentId, 2021),
                ("وجدان الحازمي",   "2022028", "Female", 4.10m, 7, isys.DepartmentId, 2022),
                ("عادل اليامي",     "2022029", "Male",   3.50m, 7, isys.DepartmentId, 2022),
                ("حنان الغامدي",    "2022030", "Female", 4.40m, 7, isys.DepartmentId, 2022),

                // ── إدارة الأعمال (6) ──
                ("صالح الرحيلي",    "2021031", "Male",   4.00m, 8, ba.DepartmentId, 2021),
                ("مها القحطاني",    "2021032", "Female", 4.60m, 8, ba.DepartmentId, 2021),
                ("ناصر الهلالي",    "2021033", "Male",   3.80m, 8, ba.DepartmentId, 2021),
                ("ابتسام السبيعي",  "2022034", "Female", 4.20m, 7, ba.DepartmentId, 2022),
                ("خالد العنزي",     "2022035", "Male",   3.40m, 7, ba.DepartmentId, 2022),
                ("ريما الأحمري",    "2022036", "Female", 4.50m, 7, ba.DepartmentId, 2022),
            };

            foreach (var s in studentData)
            {
                context.Students.Add(new Student
                {
                    FullName = s.Name,
                    StudentNumber = s.Number,
                    Gender = s.Gender,
                    GPA = s.GPA,
                    Level = s.Level,
                    Status = StudentStatus.Active,
                    DepartmentId = s.DeptId,
                    EnrollmentYear = s.Year
                });
            }
            await context.SaveChangesAsync(cancellationToken);
        }

        var students = await context.Students.OrderBy(s => s.StudentId).ToListAsync(cancellationToken);

        // ══════════════════════════════════════════════════════════════
        // 7. الفرق (9)
        // ══════════════════════════════════════════════════════════════
        if (!await context.Teams.AnyAsync(cancellationToken))
        {
            var teamData = new (string Name, int DeptId, int SupervisorId, int[] StudentIndices)[]
            {
                // كلية الهندسة
                ("فريق تخرج 1", swe.DepartmentId, drAhmed.DoctorId,   new[] { 0, 1, 2 }),         // 3 طلاب
                ("فريق تخرج 2", swe.DepartmentId, drFatimah.DoctorId, new[] { 3, 4 }),             // 2 طلاب
                ("فريق تخرج 3", ce.DepartmentId,  drKhaled.DoctorId,  new[] { 6, 7, 8, 9 }),      // 4 طلاب
                // كلية العلوم
                ("فريق تخرج 4", cs.DepartmentId,  drNoura.DoctorId,   new[] { 12, 13, 14 }),      // 3 طلاب
                ("فريق تخرج 5", cs.DepartmentId,  drSaad.DoctorId,    new[] { 15, 16 }),           // 2 طلاب
                ("فريق تخرج 6", math.DepartmentId, drHind.DoctorId,   new[] { 18, 19, 20 }),      // 3 طلاب
                // كلية إدارة الأعمال
                ("فريق تخرج 7", isys.DepartmentId, drYousuf.DoctorId, new[] { 24, 25, 26, 27 }), // 4 طلاب
                ("فريق تخرج 8", isys.DepartmentId, drReem.DoctorId,   new[] { 28, 29 }),          // 2 طلاب
                ("فريق تخرج 9", ba.DepartmentId,  drOmar.DoctorId,    new[] { 30, 31, 32 }),      // 3 طلاب
            };

            foreach (var t in teamData)
            {
                var team = new Team
                {
                    Name = t.Name,
                    DepartmentId = t.DeptId,
                    SupervisorId = t.SupervisorId
                };
                context.Teams.Add(team);
                await context.SaveChangesAsync(cancellationToken);

                // ربط الطلاب بالفريق
                foreach (var idx in t.StudentIndices)
                {
                    if (idx < students.Count)
                        students[idx].TeamId = team.TeamId;
                }
                await context.SaveChangesAsync(cancellationToken);
            }
        }

        var teams = await context.Teams.OrderBy(t => t.TeamId).ToListAsync(cancellationToken);

        // ══════════════════════════════════════════════════════════════
        // 8. اللجان (3 — لجنة لكل كلية) + أعضاء اللجان
        // ══════════════════════════════════════════════════════════════
        if (!await context.Committees.AnyAsync(cancellationToken))
        {
            var committeeData = new (string Name, int DeptId, int ChairId, int MemberId)[]
            {
                ("لجنة مناقشة كلية الهندسة",       swe.DepartmentId,  drNouf.DoctorId,    drFatimah.DoctorId),
                ("لجنة مناقشة كلية العلوم",        cs.DepartmentId,   drAbdullah.DoctorId, drSaad.DoctorId),
                ("لجنة مناقشة كلية إدارة الأعمال", isys.DepartmentId, drSarah.DoctorId,   drReem.DoctorId),
            };

            foreach (var c in committeeData)
            {
                var committee = new Committee
                {
                    Name = c.Name,
                    DepartmentId = c.DeptId,
                    TermId = term?.TermId
                };
                context.Committees.Add(committee);
                await context.SaveChangesAsync(cancellationToken);

                context.CommitteeMembers.AddRange(
                    new CommitteeMember { CommitteeId = committee.CommitteeId, DoctorId = c.ChairId, Role = CommitteeMemberRole.Chair },
                    new CommitteeMember { CommitteeId = committee.CommitteeId, DoctorId = c.MemberId, Role = CommitteeMemberRole.Member }
                );
                await context.SaveChangesAsync(cancellationToken);
            }
        }

        var committees = await context.Committees.OrderBy(c => c.CommitteeId).ToListAsync(cancellationToken);

        // ══════════════════════════════════════════════════════════════
        // 9. المناقشات (3 — مناقشة لكل لجنة)
        // ══════════════════════════════════════════════════════════════
        if (!await context.Discussions.AnyAsync(cancellationToken) && teams.Count >= 9 && committees.Count >= 3)
        {
            context.Discussions.AddRange(
                new Discussion
                {
                    TeamId = teams[0].TeamId,  // فريق تخرج 1 (هندسة)
                    CommitteeId = committees[0].CommitteeId,
                    StartTime = DateTime.Now.AddDays(7).Date.AddHours(9),
                    EndTime = DateTime.Now.AddDays(7).Date.AddHours(11),
                    Place = "قاعة الندوات A"
                },
                new Discussion
                {
                    TeamId = teams[3].TeamId,  // فريق تخرج 4 (علوم)
                    CommitteeId = committees[1].CommitteeId,
                    StartTime = DateTime.Now.AddDays(8).Date.AddHours(10),
                    EndTime = DateTime.Now.AddDays(8).Date.AddHours(12),
                    Place = "قاعة الندوات B"
                },
                new Discussion
                {
                    TeamId = teams[6].TeamId,  // فريق تخرج 7 (إدارة أعمال)
                    CommitteeId = committees[2].CommitteeId,
                    StartTime = DateTime.Now.AddDays(9).Date.AddHours(13),
                    EndTime = DateTime.Now.AddDays(9).Date.AddHours(15),
                    Place = "قاعة المؤتمرات"
                }
            );
            await context.SaveChangesAsync(cancellationToken);
        }

        // ══════════════════════════════════════════════════════════════
        // 10. المشاريع (9 — بحالات مختلفة)
        // ══════════════════════════════════════════════════════════════
        if (!await context.Projects.AnyAsync(cancellationToken) && teams.Count >= 9)
        {
            context.Projects.AddRange(
                new Project
                {
                    Title = "نظام إدارة المستشفيات",
                    Description = "مشروع يهدف لأتمتة العمليات الإدارية في المستشفيات",
                    Beneficiary = "وزارة الصحة",
                    Status = ProjectStatus.Proposed,
                    CompletionRate = 0,
                    ProposedAt = DateTime.Now.AddDays(-5),
                    DepartmentId = swe.DepartmentId,
                    TeamId = teams[0].TeamId,
                    SupervisorId = drAhmed.DoctorId,
                    TermId = term?.TermId
                },
                new Project
                {
                    Title = "تطبيق التعليم الإلكتروني",
                    Description = "منصة تعليمية تفاعلية للطلاب والمعلمين",
                    Beneficiary = "وزارة التعليم",
                    Status = ProjectStatus.Approved,
                    CompletionRate = 15,
                    ProposedAt = DateTime.Now.AddDays(-20),
                    ApprovedAt = DateTime.Now.AddDays(-15),
                    DepartmentId = swe.DepartmentId,
                    TeamId = teams[1].TeamId,
                    SupervisorId = drFatimah.DoctorId,
                    TermId = term?.TermId
                },
                new Project
                {
                    Title = "نظام الحضور الذكي",
                    Description = "نظام لتسجيل حضور الطلاب باستخدام تقنية التعرف على الوجه",
                    Beneficiary = "عمادة شؤون الطلاب",
                    Status = ProjectStatus.InProgress,
                    CompletionRate = 60,
                    ProposedAt = DateTime.Now.AddDays(-60),
                    ApprovedAt = DateTime.Now.AddDays(-55),
                    DepartmentId = ce.DepartmentId,
                    TeamId = teams[2].TeamId,
                    SupervisorId = drKhaled.DoctorId,
                    TermId = term?.TermId
                },
                new Project
                {
                    Title = "منصة التسوق الإلكتروني",
                    Description = "منصة تجارة إلكترونية متكاملة بنظام دفع إلكتروني",
                    Beneficiary = "وزارة التجارة",
                    Status = ProjectStatus.InProgress,
                    CompletionRate = 80,
                    ProposedAt = DateTime.Now.AddDays(-90),
                    ApprovedAt = DateTime.Now.AddDays(-85),
                    DepartmentId = cs.DepartmentId,
                    TeamId = teams[3].TeamId,
                    SupervisorId = drNoura.DoctorId,
                    TermId = term?.TermId
                },
                new Project
                {
                    Title = "نظام إدارة المكتبات",
                    Description = "نظام متكامل لإدارة المكتبات الجامعية وقواعد البيانات",
                    Beneficiary = "عمادة المكتبات",
                    Status = ProjectStatus.Completed,
                    CompletionRate = 100,
                    ProposedAt = DateTime.Now.AddDays(-120),
                    ApprovedAt = DateTime.Now.AddDays(-115),
                    DepartmentId = cs.DepartmentId,
                    TeamId = teams[4].TeamId,
                    SupervisorId = drSaad.DoctorId,
                    TermId = term?.TermId
                },
                new Project
                {
                    Title = "تطبيق الخدمات البنكية",
                    Description = "تطبيق جوال للخدمات المصرفية الإلكترونية",
                    Beneficiary = "البنك الأهلي",
                    Status = ProjectStatus.Rejected,
                    CompletionRate = 0,
                    ProposedAt = DateTime.Now.AddDays(-10),
                    RejectionReason = "المشروع مكرر مع مشروع سابق",
                    DepartmentId = math.DepartmentId,
                    TeamId = teams[5].TeamId,
                    SupervisorId = drHind.DoctorId,
                    TermId = term?.TermId
                },
                new Project
                {
                    Title = "نظام تتبع الشحنات",
                    Description = "منصة لتتبع الشحنات والطرود في الوقت الحقيقي",
                    Beneficiary = "شركة سمسا",
                    Status = ProjectStatus.Proposed,
                    CompletionRate = 0,
                    ProposedAt = DateTime.Now.AddDays(-3),
                    DepartmentId = isys.DepartmentId,
                    TeamId = teams[6].TeamId,
                    SupervisorId = drYousuf.DoctorId,
                    TermId = term?.TermId
                },
                new Project
                {
                    Title = "منصة التوظيف",
                    Description = "منصة ربط الخريجين بفرص العمل في القطاع الخاص",
                    Beneficiary = "صندوق الموارد البشرية",
                    Status = ProjectStatus.Approved,
                    CompletionRate = 30,
                    ProposedAt = DateTime.Now.AddDays(-40),
                    ApprovedAt = DateTime.Now.AddDays(-35),
                    DepartmentId = isys.DepartmentId,
                    TeamId = teams[7].TeamId,
                    SupervisorId = drReem.DoctorId,
                    TermId = term?.TermId
                },
                new Project
                {
                    Title = "نظام إدارة الموارد البشرية",
                    Description = "نظام ERP لإدارة شؤون الموظفين والرواتب",
                    Beneficiary = "شركة أرامكو",
                    Status = ProjectStatus.InProgress,
                    CompletionRate = 45,
                    ProposedAt = DateTime.Now.AddDays(-70),
                    ApprovedAt = DateTime.Now.AddDays(-65),
                    DepartmentId = ba.DepartmentId,
                    TeamId = teams[8].TeamId,
                    SupervisorId = drOmar.DoctorId,
                    TermId = term?.TermId
                }
            );
            await context.SaveChangesAsync(cancellationToken);
        }

        // ══════════════════════════════════════════════════════════════
        // 11. المستخدمون (5: admin + 2 HOD + 2 Supervisor)
        // ══════════════════════════════════════════════════════════════
        var usersToSeed = new (string Username, string Password, UserRole Role, int? DoctorId)[]
        {
            ("admin",       "Admin@123",     UserRole.Admin,            null),
            ("hod.ahmed",   "Hod@123",       UserRole.HeadOfDepartment, drAhmed.DoctorId),
            ("hod.noura",   "Hod@123",       UserRole.HeadOfDepartment, drNoura.DoctorId),
            ("sup.khaled",  "Sup@123",       UserRole.Supervisor,       drKhaled.DoctorId),
            ("sup.yousuf",  "Sup@123",       UserRole.Supervisor,       drYousuf.DoctorId),
        };

        foreach (var u in usersToSeed)
        {
            if (!await context.Users.AnyAsync(x => x.Username == u.Username, cancellationToken))
            {
                var hash = passwordHasher.HashPassword(u.Password, out var salt);
                context.Users.Add(new User
                {
                    Username = u.Username,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    Role = u.Role,
                    IsActive = true,
                    DoctorId = u.DoctorId
                });
            }
        }
        await context.SaveChangesAsync(cancellationToken);
    }
}
