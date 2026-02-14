using Masar.Domain.Common;
using Masar.Domain.Enums;

namespace Masar.Domain.Entities;

public class Project : BaseEntity
{
    public int ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Beneficiary { get; set; } = string.Empty;
    public ProjectStatus Status { get; set; } = ProjectStatus.Proposed;
    public decimal CompletionRate { get; set; }
    public string DocumentationPath { get; set; } = string.Empty;
    public DateTime ProposedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }
    public string RejectionReason { get; set; } = string.Empty;

    public int DepartmentId { get; set; }
    public Department? Department { get; set; }
    public int? TeamId { get; set; }
    public Team? Team { get; set; }
    public int? SupervisorId { get; set; }
    public Doctor? Supervisor { get; set; }
    public int? TermId { get; set; }
    public AcademicTerm? Term { get; set; }

    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
