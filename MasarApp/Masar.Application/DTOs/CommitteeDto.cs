namespace Masar.Application.DTOs;

public class CommitteeDto
{
    public int CommitteeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int CollegeId { get; set; }
    public string CollegeName { get; set; } = string.Empty;
    public int? TermId { get; set; }
    public string TermName { get; set; } = string.Empty;
    public int MemberCount { get; set; }
}
