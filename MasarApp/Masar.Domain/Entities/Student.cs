using System.Collections.Generic;
using Masar.Domain.Enums;
using Masar.Domain.Common;

namespace Masar.Domain.Entities;

public class Student : BaseEntity
{
    public int StudentId { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty; // Male, Female
    public string? Email { get; set; }
    public string Phone { get; set; } = string.Empty;
    public decimal? GPA { get; set; } // 0.00 - 5.00
    public int Level { get; set; } = 1; // 1-8
    public StudentStatus Status { get; set; } = StudentStatus.Active;
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }
    public int? TeamId { get; set; }
    public Team? Team { get; set; }
    public int EnrollmentYear { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<StudentEvaluation> Evaluations { get; set; } = new List<StudentEvaluation>();
}

