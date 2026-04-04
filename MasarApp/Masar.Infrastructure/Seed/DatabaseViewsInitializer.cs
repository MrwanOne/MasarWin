using Masar.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Infrastructure.Seed;

/// <summary>
/// يُنشئ الـ Views في Oracle مرة واحدة عند بدء التطبيق.
/// يستخدم CREATE OR REPLACE VIEW حتى لا يفشل عند التكرار.
/// </summary>
public static class DatabaseViewsInitializer
{
    public static async Task InitializeAsync(
        IDbContextFactory<MasarDbContext> contextFactory,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        Console.WriteLine("DatabaseViewsInitializer: Creating/updating Oracle Views...");

        await CreateView(context, "VW_PROJECT_FULL_DETAIL", @"
            SELECT p.project_id,
                   p.title,
                   p.description,
                   p.beneficiary,
                   p.status,
                   p.completion_rate,
                   p.proposed_at,
                   p.approved_at,
                   p.rejection_reason,
                   p.department_id,
                   d.name_ar  AS department_name_ar,
                   d.name_en  AS department_name_en,
                   c.college_id,
                   c.name_ar  AS college_name_ar,
                   c.name_en  AS college_name_en,
                   t.team_id,
                   t.name     AS team_name,
                   doc.doctor_id  AS supervisor_id,
                   doc.full_name  AS supervisor_name,
                   at2.term_id,
                   at2.name_ar AS term_name_ar,
                   at2.name_en AS term_name_en
            FROM project p
            LEFT JOIN department   d   ON p.department_id = d.department_id AND d.is_deleted = 0
            LEFT JOIN college      c   ON d.college_id    = c.college_id    AND c.is_deleted = 0
            LEFT JOIN team         t   ON p.team_id       = t.team_id       AND t.is_deleted = 0
            LEFT JOIN doctor       doc ON p.supervisor_id = doc.doctor_id   AND doc.is_deleted = 0
            LEFT JOIN academic_term at2 ON p.term_id      = at2.term_id     AND at2.is_deleted = 0
            WHERE p.is_deleted = 0", cancellationToken);

        await CreateView(context, "VW_STUDENT_FULL_DETAIL", @"
            SELECT s.student_id,
                   s.student_number,
                   s.full_name,
                   s.email,
                   s.phone,
                   s.gender,
                   s.gpa,
                   s.level,
                   s.status,
                   s.enrollment_year,
                   s.department_id,
                   d.name_ar  AS department_name_ar,
                   d.name_en  AS department_name_en,
                   c.college_id,
                   c.name_ar  AS college_name_ar,
                   s.team_id,
                   t.name     AS team_name,
                   p.project_id,
                   p.title    AS project_title,
                   p.status   AS project_status,
                   p.completion_rate AS project_completion_rate
            FROM student s
            LEFT JOIN department d ON s.department_id = d.department_id AND d.is_deleted = 0
            LEFT JOIN college    c ON d.college_id    = c.college_id    AND c.is_deleted = 0
            LEFT JOIN team       t ON s.team_id       = t.team_id       AND t.is_deleted = 0
            LEFT JOIN project    p ON p.team_id       = t.team_id       AND p.is_deleted = 0
            WHERE s.is_deleted = 0", cancellationToken);

        await CreateView(context, "VW_DISCUSSION_RESULTS", @"
            SELECT d.discussion_id,
                   d.start_time,
                   d.end_time,
                   d.place,
                   d.supervisor_score,
                   d.committee_score,
                   d.final_score,
                   d.report_text,
                   d.created_at,
                   t.team_id,
                   t.name      AS team_name,
                   com.committee_id,
                   com.name    AS committee_name,
                   dept.department_id,
                   dept.name_ar AS department_name_ar,
                   col.name_ar  AS college_name_ar,
                   (SELECT COUNT(*) FROM student_evaluation se
                    WHERE se.discussion_id = d.discussion_id) AS evaluated_students_count
            FROM discussion d
            LEFT JOIN team       t    ON d.team_id      = t.team_id      AND t.is_deleted  = 0
            LEFT JOIN committee  com  ON d.committee_id = com.committee_id AND com.is_deleted = 0
            LEFT JOIN department dept ON t.department_id = dept.department_id AND dept.is_deleted = 0
            LEFT JOIN college    col  ON dept.college_id = col.college_id  AND col.is_deleted = 0
            WHERE d.is_deleted = 0", cancellationToken);

        await CreateView(context, "VW_COMMITTEE_COMPOSITION", @"
            SELECT com.committee_id,
                   com.name        AS committee_name,
                   com.created_at,
                   d.department_id,
                   d.name_ar       AS department_name_ar,
                   at2.term_id,
                   at2.name_ar     AS term_name_ar,
                   doc.doctor_id,
                   doc.full_name   AS member_name,
                   doc.qualification,
                   doc.rank,
                   cm.role         AS member_role
            FROM committee_member cm
            JOIN committee    com ON cm.committee_id = com.committee_id   AND com.is_deleted = 0
            JOIN doctor       doc ON cm.doctor_id    = doc.doctor_id      AND doc.is_deleted = 0
            LEFT JOIN department d   ON com.department_id = d.department_id AND d.is_deleted   = 0
            LEFT JOIN academic_term at2 ON com.term_id    = at2.term_id   AND at2.is_deleted  = 0", cancellationToken);

        await CreateView(context, "VW_DEPARTMENT_STATS", @"
            SELECT d.department_id,
                   d.name_ar  AS department_name_ar,
                   d.name_en  AS department_name_en,
                   c.college_id,
                   c.name_ar  AS college_name_ar,
                   (SELECT COUNT(*) FROM student  s WHERE s.department_id = d.department_id AND s.is_deleted = 0) AS total_students,
                   (SELECT COUNT(*) FROM team     t WHERE t.department_id = d.department_id AND t.is_deleted = 0) AS total_teams,
                   (SELECT COUNT(*) FROM project  p WHERE p.department_id = d.department_id AND p.is_deleted = 0) AS total_projects,
                   (SELECT COUNT(*) FROM project  p WHERE p.department_id = d.department_id AND p.status = 0 AND p.is_deleted = 0) AS proposed_projects,
                   (SELECT COUNT(*) FROM project  p WHERE p.department_id = d.department_id AND p.status = 1 AND p.is_deleted = 0) AS approved_projects,
                   (SELECT COUNT(*) FROM project  p WHERE p.department_id = d.department_id AND p.status = 2 AND p.is_deleted = 0) AS inprogress_projects,
                   (SELECT COUNT(*) FROM project  p WHERE p.department_id = d.department_id AND p.status = 3 AND p.is_deleted = 0) AS completed_projects,
                   (SELECT COUNT(*) FROM project  p WHERE p.department_id = d.department_id AND p.status = 4 AND p.is_deleted = 0) AS rejected_projects,
                   (SELECT COUNT(*) FROM doctor   doc WHERE doc.department_id = d.department_id AND doc.is_deleted = 0) AS total_doctors
            FROM department d
            LEFT JOIN college c ON d.college_id = c.college_id AND c.is_deleted = 0
            WHERE d.is_deleted = 0", cancellationToken);

        await CreateView(context, "VW_DASHBOARD_STATS", @"
            SELECT
                (SELECT COUNT(*) FROM project  p WHERE p.is_deleted = 0)                              AS total_projects,
                (SELECT COUNT(*) FROM project  p WHERE p.status = 0 AND p.is_deleted = 0)             AS proposed_projects,
                (SELECT COUNT(*) FROM project  p WHERE p.status = 1 AND p.is_deleted = 0)             AS approved_projects,
                (SELECT COUNT(*) FROM project  p WHERE p.status = 2 AND p.is_deleted = 0)             AS inprogress_projects,
                (SELECT COUNT(*) FROM project  p WHERE p.status = 3 AND p.is_deleted = 0)             AS completed_projects,
                (SELECT COUNT(*) FROM project  p WHERE p.status = 4 AND p.is_deleted = 0)             AS rejected_projects,
                (SELECT COUNT(*) FROM student  s WHERE s.is_deleted = 0)                              AS total_students,
                (SELECT COUNT(*) FROM team     t WHERE t.is_deleted = 0)                              AS total_teams,
                (SELECT COUNT(*) FROM committee c WHERE c.is_deleted = 0)                             AS total_committees,
                (SELECT COUNT(*) FROM doctor   d WHERE d.is_deleted = 0)                              AS total_doctors
            FROM DUAL", cancellationToken);

        Console.WriteLine("DatabaseViewsInitializer: All 6 Views created/updated successfully.");
    }

    private static async Task CreateView(
        MasarDbContext context,
        string viewName,
        string selectSql,
        CancellationToken cancellationToken)
    {
        try
        {
            var sql = $"CREATE OR REPLACE VIEW {viewName} AS {selectSql}";
            await context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
            Console.WriteLine($"DatabaseViewsInitializer: View '{viewName}' created/updated.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"DatabaseViewsInitializer: Failed to create View '{viewName}'. Exception: {ex.Message}");
            throw;
        }
    }
}
