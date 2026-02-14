namespace Masar.Application.DTOs;

public class TeamDto
{
    public int TeamId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int CollegeId { get; set; }
    public string CollegeName { get; set; } = string.Empty;
    public int? SupervisorId { get; set; }
    public string SupervisorName { get; set; } = string.Empty;
    public int? CommitteeId { get; set; }
    public string CommitteeName { get; set; } = string.Empty;
    public int StudentCount { get; set; }
    public string ProjectTitle { get; set; } = string.Empty;
    public string StudentNames { get; set; } = string.Empty;
    public string StudentNumbers { get; set; } = string.Empty;
    public int AcademicYear { get; set; }
}
