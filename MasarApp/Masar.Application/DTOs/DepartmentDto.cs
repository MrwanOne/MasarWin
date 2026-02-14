namespace Masar.Application.DTOs;

public class DepartmentDto
{
    public int DepartmentId { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int CollegeId { get; set; }
    public string CollegeName { get; set; } = string.Empty;
    public int? HeadOfDepartmentId { get; set; }
    public string HeadOfDepartmentName { get; set; } = string.Empty;
}
