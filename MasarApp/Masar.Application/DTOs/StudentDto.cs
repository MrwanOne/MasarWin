using Masar.Domain.Enums;

namespace Masar.Application.DTOs;

public class StudentDto
{
    public int StudentId { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int CollegeId { get; set; }
    public string CollegeName { get; set; } = string.Empty;
    public int? TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public int EnrollmentYear { get; set; }
    public string Gender { get; set; } = string.Empty;
    public decimal GPA { get; set; }
    public int Level { get; set; }
    public StudentStatus Status { get; set; }
}
