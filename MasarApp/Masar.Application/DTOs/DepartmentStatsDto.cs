namespace Masar.Application.DTOs;

/// <summary>
/// إحصائيات تحليلية للقسم من VW_DEPARTMENT_STATS
/// </summary>
public class DepartmentStatsDto
{
    public int    DepartmentId     { get; set; }
    public string DepartmentNameAr { get; set; } = string.Empty;
    public string DepartmentNameEn { get; set; } = string.Empty;
    public int    CollegeId        { get; set; }
    public string CollegeNameAr    { get; set; } = string.Empty;

    // إحصائيات
    public int TotalStudents      { get; set; }
    public int TotalTeams         { get; set; }
    public int TotalProjects      { get; set; }
    public int ProposedProjects   { get; set; }
    public int ApprovedProjects   { get; set; }
    public int InProgressProjects { get; set; }
    public int CompletedProjects  { get; set; }
    public int RejectedProjects   { get; set; }
    public int TotalDoctors       { get; set; }
}
