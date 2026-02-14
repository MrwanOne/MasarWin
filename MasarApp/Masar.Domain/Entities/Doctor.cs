using System.Collections.Generic;
using Masar.Domain.Enums;
using Masar.Domain.Common;

namespace Masar.Domain.Entities;

public class Doctor : BaseEntity
{
    public int DoctorId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Qualification { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Phone { get; set; } = string.Empty;
    public AcademicRank Rank { get; set; } = AcademicRank.Lecturer;
    public string Specialization { get; set; } = string.Empty;
    public int MaxSupervisionCount { get; set; } = 5;
    public bool IsActive { get; set; } = true;
    public int CollegeId { get; set; }
    public College? College { get; set; }
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public ICollection<Project> SupervisedProjects { get; set; } = new List<Project>();
    public ICollection<Team> SupervisedTeams { get; set; } = new List<Team>();
    public ICollection<CommitteeMember> CommitteeMemberships { get; set; } = new List<CommitteeMember>();
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Department> DepartmentsHeaded { get; set; } = new List<Department>();
}

