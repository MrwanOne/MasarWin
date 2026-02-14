using Masar.Domain.Common;

namespace Masar.Domain.Entities;

public class Team : BaseEntity
{
    public int TeamId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }
    public int? SupervisorId { get; set; }
    public Doctor? Supervisor { get; set; }
    public int? CommitteeId { get; set; }
    public Committee? Committee { get; set; }

    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<Discussion> Discussions { get; set; } = new List<Discussion>();
}
