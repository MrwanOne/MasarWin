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

public static class DbSeeder
{
    public static async Task SeedAdminAsync(IDbContextFactory<MasarDbContext> contextFactory, IPasswordHasher passwordHasher, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        Console.WriteLine(">>> Starting Robust Seeding Process...");

        // 1. Admin User
        if (!await context.Users.AnyAsync(u => u.Username == "admin", cancellationToken))
        {
            Console.WriteLine("Seeding Admin...");
            var hash = passwordHasher.HashPassword("Admin@123", out var salt);
            context.Users.Add(new User
            {
                Username = "admin",
                PasswordHash = hash,
                PasswordSalt = salt,
                Role = UserRole.Admin,
                IsActive = true
            });
            await context.SaveChangesAsync(cancellationToken);
        }

        // 2. Academic Terms
        if (!await context.AcademicTerms.AnyAsync(cancellationToken))
        {
            Console.WriteLine("Seeding Academic Terms...");
            context.AcademicTerms.Add(new AcademicTerm 
            { 
                Year = 2025, 
                Semester = 2, 
                NameAr = "الفصل الثاني 2025", 
                NameEn = "Second Semester 2025", 
                StartDate = DateTime.Now.AddMonths(-1), 
                EndDate = DateTime.Now.AddMonths(4), 
                IsActive = true 
            });
            await context.SaveChangesAsync(cancellationToken);
        }

        // 3. Evaluation Criteria
        if (!await context.EvaluationCriteria.AnyAsync(cancellationToken))
        {
            Console.WriteLine("Seeding Evaluation Criteria...");
            context.EvaluationCriteria.AddRange(
                new EvaluationCriteria { NameAr = "التطبيق والبرمجة", MaxScore = 50, Weight = 0.5m, DisplayOrder = 1, IsActive = true },
                new EvaluationCriteria { NameAr = "التقديم والتقرير", MaxScore = 50, Weight = 0.5m, DisplayOrder = 2, IsActive = true }
            );
            await context.SaveChangesAsync(cancellationToken);
        }

        // 4. Colleges & Departments
        if (!await context.Colleges.AnyAsync(cancellationToken))
        {
            Console.WriteLine("Seeding Colleges & Departments...");
            var engineering = new College { NameAr = "كلية الهندسة", NameEn = "College of Engineering", Code = "ENG" };
            context.Colleges.Add(engineering);
            await context.SaveChangesAsync(cancellationToken);

            context.Departments.Add(new Department { NameAr = "هندسة البرمجيات", NameEn = "Software Engineering", Code = "SWE", CollegeId = engineering.CollegeId });
            await context.SaveChangesAsync(cancellationToken);
        }

        var sweDept = await context.Departments.FirstOrDefaultAsync(d => d.Code == "SWE", cancellationToken) 
                      ?? await context.Departments.FirstOrDefaultAsync(cancellationToken);

        if (sweDept == null) { Console.WriteLine("!!! No Department found, aborting link-heavy seeding."); return; }

        // 5. Doctors
        if (!await context.Doctors.AnyAsync(cancellationToken))
        {
            Console.WriteLine("Seeding Doctors...");
            context.Doctors.Add(new Doctor 
            { 
                FullName = "د. علي محمد", 
                Email = "ali@masar.com", 
                DepartmentId = sweDept.DepartmentId, 
                CollegeId = sweDept.CollegeId, 
                Qualification = "PhD", 
                Rank = AcademicRank.Professor, 
                IsActive = true 
            });
            await context.SaveChangesAsync(cancellationToken);
        }

        var dr = await context.Doctors.FirstOrDefaultAsync(cancellationToken);
        if (dr == null) { Console.WriteLine("!!! No Doctor found, aborting link-heavy seeding."); return; }

        // 6. Students
        if (!await context.Students.AnyAsync(cancellationToken))
        {
            Console.WriteLine("Seeding Students...");
            context.Students.AddRange(
                new Student { FullName = "أحمد خالد", StudentNumber = "2021001", DepartmentId = sweDept.DepartmentId, EnrollmentYear = 2021, Gender = "Male" },
                new Student { FullName = "سعيد محمد", StudentNumber = "2021002", DepartmentId = sweDept.DepartmentId, EnrollmentYear = 2021, Gender = "Male" }
            );
            await context.SaveChangesAsync(cancellationToken);
        }

        // 7. Teams
        if (!await context.Teams.AnyAsync(cancellationToken))
        {
            Console.WriteLine("Seeding Teams...");
            var team = new Team { Name = "فريق تخرج 1", DepartmentId = sweDept.DepartmentId, SupervisorId = dr.DoctorId };
            context.Teams.Add(team);
            await context.SaveChangesAsync(cancellationToken);

            var students = await context.Students.ToListAsync(cancellationToken);
            foreach (var s in students) s.TeamId = team.TeamId;
            await context.SaveChangesAsync(cancellationToken);
        }

        var teamObj = await context.Teams.FirstOrDefaultAsync(cancellationToken);

        // 8. Committees
        if (!await context.Committees.AnyAsync(cancellationToken))
        {
            Console.WriteLine("Seeding Committees...");
            var term = await context.AcademicTerms.FirstOrDefaultAsync(t => t.IsActive, cancellationToken);
            var comm = new Committee { Name = "لجنة المناقشة الرئيسية", DepartmentId = sweDept.DepartmentId, TermId = term?.TermId };
            context.Committees.Add(comm);
            await context.SaveChangesAsync(cancellationToken);

            context.CommitteeMembers.Add(new CommitteeMember { CommitteeId = comm.CommitteeId, DoctorId = dr.DoctorId, Role = CommitteeMemberRole.Chair });
            await context.SaveChangesAsync(cancellationToken);
        }

        var commObj = await context.Committees.FirstOrDefaultAsync(cancellationToken);

        // 9. Discussions
        if (!await context.Discussions.AnyAsync(cancellationToken) && teamObj != null && commObj != null)
        {
            Console.WriteLine("Seeding Discussions...");
            context.Discussions.Add(new Discussion
            {
                TeamId = teamObj.TeamId,
                CommitteeId = commObj.CommitteeId,
                StartTime = DateTime.Now.AddDays(7),
                EndTime = DateTime.Now.AddDays(7).AddHours(2),
                Place = "قاعة الندوات"
            });
            await context.SaveChangesAsync(cancellationToken);
        }

        // 10. Projects (Added for Sprint 1.1 Testing)
        if (!await context.Projects.AnyAsync(cancellationToken) && teamObj != null)
        {
            Console.WriteLine("Seeding Sample Projects...");
            var term = await context.AcademicTerms.FirstOrDefaultAsync(t => t.IsActive, cancellationToken);
            
            context.Projects.AddRange(
                new Project
                {
                    Title = "نظام إدارة المستشفيات الذكي",
                    Description = "مشروع يهدف لأتمتة العمليات الإدارية في المستشفيات باستخدام الذكاء الاصطناعي",
                    Beneficiary = "وزارة الصحة",
                    Status = ProjectStatus.Proposed,
                    CompletionRate = 0,
                    ProposedAt = DateTime.Now.AddDays(-5),
                    DepartmentId = sweDept.DepartmentId,
                    TeamId = teamObj.TeamId,
                    SupervisorId = dr.DoctorId,
                    TermId = term?.TermId
                },
                new Project
                {
                    Title = "تطبيق مسار لإدارة المشاريع",
                    Description = "تطبيق تتبع مشاريع التخرج لطلاب الدراسات العليا",
                    Beneficiary = "جامعة مسار",
                    Status = ProjectStatus.Approved,
                    CompletionRate = 25,
                    ProposedAt = DateTime.Now.AddDays(-10),
                    ApprovedAt = DateTime.Now.AddDays(-8),
                    DepartmentId = sweDept.DepartmentId,
                    TeamId = null,
                    SupervisorId = dr.DoctorId,
                    TermId = term?.TermId
                }
            );
            await context.SaveChangesAsync(cancellationToken);
        }

        Console.WriteLine(">>> Seeding Completed Successfully!");
    }

    private static async Task SeedAcademicTermsAsync(MasarDbContext context, CancellationToken cancellationToken) { /* Deprecated/Integrated above */ }
    private static async Task SeedEvaluationCriteriaAsync(MasarDbContext context, CancellationToken cancellationToken) { /* Deprecated/Integrated above */ }
}
