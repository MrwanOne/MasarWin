namespace Masar.Application.DTOs;

public class DoctorDto
{
    public int DoctorId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Qualification { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string CollegeName { get; set; } = string.Empty;
    public int CollegeId { get; set; }
    public string Rank { get; set; } = string.Empty;
    public bool IsHeadOfDepartment { get; set; }
    public string HeadOfDepartmentName { get; set; } = string.Empty;
}
