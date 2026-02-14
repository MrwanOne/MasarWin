using System.Collections.Generic;
using Masar.Domain.Common;

namespace Masar.Domain.Entities;

public class Committee : BaseEntity
{
    public int CommitteeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public int? TermId { get; set; }
    public AcademicTerm? Term { get; set; }

    public ICollection<CommitteeMember> Members { get; set; } = new List<CommitteeMember>();
    public ICollection<Discussion> Discussions { get; set; } = new List<Discussion>();
    public ICollection<Team> Teams { get; set; } = new List<Team>();
}
