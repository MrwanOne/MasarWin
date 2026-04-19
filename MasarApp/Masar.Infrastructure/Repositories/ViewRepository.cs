using Masar.Application.DTOs;
using Masar.Application.Interfaces;
using Masar.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Infrastructure.Repositories;

/// <summary>
/// استعلام الـ Views في Oracle.
/// يُعيد تسمية أعمدة Oracle (snake_case) لتطابق خصائص الـ DTO (PascalCase).
/// </summary>
public class ViewRepository : IViewRepository
{
    private readonly IDbContextFactory<MasarDbContext> _contextFactory;

    public ViewRepository(IDbContextFactory<MasarDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    // ═══════════════════════════════════════════════════════════════
    // VW_PROJECT_FULL_DETAIL
    // ═══════════════════════════════════════════════════════════════
    public async Task<List<ProjectFullDetailDto>> GetProjectFullDetailsAsync(CancellationToken ct = default)
    {
        const string sql = @"
            SELECT
                project_id           AS ""ProjectId"",
                title                AS ""Title"",
                description          AS ""Description"",
                beneficiary          AS ""Beneficiary"",
                status               AS ""Status"",
                completion_rate      AS ""CompletionRate"",
                proposed_at          AS ""ProposedAt"",
                approved_at          AS ""ApprovedAt"",
                rejection_reason     AS ""RejectionReason"",
                department_id        AS ""DepartmentId"",
                department_name_ar   AS ""DepartmentNameAr"",
                department_name_en   AS ""DepartmentNameEn"",
                college_id           AS ""CollegeId"",
                college_name_ar      AS ""CollegeNameAr"",
                college_name_en      AS ""CollegeNameEn"",
                team_id              AS ""TeamId"",
                team_name            AS ""TeamName"",
                supervisor_id        AS ""SupervisorId"",
                supervisor_name      AS ""SupervisorName"",
                term_id              AS ""TermId"",
                term_name_ar         AS ""TermNameAr"",
                term_name_en         AS ""TermNameEn""
            FROM VW_PROJECT_FULL_DETAIL";

        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Database
            .SqlQueryRaw<ProjectFullDetailDto>(sql)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    // ═══════════════════════════════════════════════════════════════
    // VW_STUDENT_FULL_DETAIL
    // ═══════════════════════════════════════════════════════════════
    public async Task<List<StudentFullDetailDto>> GetStudentFullDetailsAsync(CancellationToken ct = default)
    {
        const string sql = @"
            SELECT
                student_id               AS ""StudentId"",
                student_number           AS ""StudentNumber"",
                full_name                AS ""FullName"",
                email                    AS ""Email"",
                phone                    AS ""Phone"",
                gender                   AS ""Gender"",
                gpa                      AS ""Gpa"",
                level                    AS ""Level"",
                status                   AS ""Status"",
                enrollment_year          AS ""EnrollmentYear"",
                department_id            AS ""DepartmentId"",
                department_name_ar       AS ""DepartmentNameAr"",
                department_name_en       AS ""DepartmentNameEn"",
                college_id               AS ""CollegeId"",
                college_name_ar          AS ""CollegeNameAr"",
                team_id                  AS ""TeamId"",
                team_name                AS ""TeamName"",
                project_id               AS ""ProjectId"",
                project_title            AS ""ProjectTitle"",
                project_status           AS ""ProjectStatus"",
                project_completion_rate  AS ""ProjectCompletionRate""
            FROM VW_STUDENT_FULL_DETAIL";

        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Database
            .SqlQueryRaw<StudentFullDetailDto>(sql)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    // ═══════════════════════════════════════════════════════════════
    // VW_DISCUSSION_RESULTS
    // ═══════════════════════════════════════════════════════════════
    public async Task<List<DiscussionResultDto>> GetDiscussionResultsAsync(CancellationToken ct = default)
    {
        const string sql = @"
            SELECT
                discussion_id              AS ""DiscussionId"",
                start_time                 AS ""StartTime"",
                end_time                   AS ""EndTime"",
                place                      AS ""Place"",
                supervisor_score           AS ""SupervisorScore"",
                committee_score            AS ""CommitteeScore"",
                final_score                AS ""FinalScore"",
                report_text                AS ""ReportText"",
                created_at                 AS ""CreatedAt"",
                team_id                    AS ""TeamId"",
                team_name                  AS ""TeamName"",
                committee_id               AS ""CommitteeId"",
                committee_name             AS ""CommitteeName"",
                department_id              AS ""DepartmentId"",
                department_name_ar         AS ""DepartmentNameAr"",
                college_name_ar            AS ""CollegeNameAr"",
                evaluated_students_count   AS ""EvaluatedStudentsCount""
            FROM VW_DISCUSSION_RESULTS";

        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Database
            .SqlQueryRaw<DiscussionResultDto>(sql)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    // ═══════════════════════════════════════════════════════════════
    // VW_COMMITTEE_COMPOSITION
    // ═══════════════════════════════════════════════════════════════
    public async Task<List<CommitteeCompositionDto>> GetCommitteeCompositionAsync(CancellationToken ct = default)
    {
        const string sql = @"
            SELECT
                committee_id       AS ""CommitteeId"",
                committee_name     AS ""CommitteeName"",
                created_at         AS ""CreatedAt"",
                department_id      AS ""DepartmentId"",
                department_name_ar AS ""DepartmentNameAr"",
                term_id            AS ""TermId"",
                term_name_ar       AS ""TermNameAr"",
                doctor_id          AS ""DoctorId"",
                member_name        AS ""MemberName"",
                qualification      AS ""Qualification"",
                rank               AS ""Rank"",
                member_role        AS ""MemberRole""
            FROM VW_COMMITTEE_COMPOSITION";

        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Database
            .SqlQueryRaw<CommitteeCompositionDto>(sql)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    // ═══════════════════════════════════════════════════════════════
    // VW_DEPARTMENT_STATS
    // ═══════════════════════════════════════════════════════════════
    public async Task<List<DepartmentStatsDto>> GetDepartmentStatsAsync(CancellationToken ct = default)
    {
        const string sql = @"
            SELECT
                department_id      AS ""DepartmentId"",
                department_name_ar AS ""DepartmentNameAr"",
                department_name_en AS ""DepartmentNameEn"",
                college_id         AS ""CollegeId"",
                college_name_ar    AS ""CollegeNameAr"",
                total_students     AS ""TotalStudents"",
                total_teams        AS ""TotalTeams"",
                total_projects     AS ""TotalProjects"",
                proposed_projects  AS ""ProposedProjects"",
                approved_projects  AS ""ApprovedProjects"",
                inprogress_projects AS ""InProgressProjects"",
                completed_projects AS ""CompletedProjects"",
                rejected_projects  AS ""RejectedProjects"",
                total_doctors      AS ""TotalDoctors""
            FROM VW_DEPARTMENT_STATS";

        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Database
            .SqlQueryRaw<DepartmentStatsDto>(sql)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    // ═══════════════════════════════════════════════════════════════
    // VW_DASHBOARD_STATS
    // ═══════════════════════════════════════════════════════════════
    public async Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken ct = default)
    {
        const string sql = @"
            SELECT
                total_projects      AS ""TotalProjects"",
                proposed_projects   AS ""ProposedProjects"",
                approved_projects   AS ""ApprovedProjects"",
                inprogress_projects AS ""InProgressProjects"",
                completed_projects  AS ""CompletedProjects"",
                rejected_projects   AS ""RejectedProjects"",
                total_students      AS ""TotalStudents"",
                total_teams         AS ""TotalTeams"",
                total_committees    AS ""TotalCommittees"",
                total_doctors       AS ""TotalDoctors""
            FROM VW_DASHBOARD_STATS";

        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var result = await context.Database
            .SqlQueryRaw<DashboardStatsDto>(sql)
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);

        return result ?? new DashboardStatsDto();
    }
}
