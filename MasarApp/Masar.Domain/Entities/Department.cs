using Masar.Domain.Common;

namespace Masar.Domain.Entities;

public class Department : BaseEntity
{
    public int DepartmentId { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int CollegeId { get; set; }
    public College? College { get; set; }
    public int? HeadOfDepartmentId { get; set; }
    public Doctor? HeadOfDepartment { get; set; }

    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<Team> Teams { get; set; } = new List<Team>();
    public ICollection<Committee> Committees { get; set; } = new List<Committee>();
}
