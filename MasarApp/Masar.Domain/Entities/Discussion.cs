using System.Collections.Generic;
using Masar.Domain.Common;

namespace Masar.Domain.Entities;

public class Discussion : BaseEntity
{
    public int DiscussionId { get; set; }
    public int TeamId { get; set; }
    public Team? Team { get; set; }
    public int CommitteeId { get; set; }
    public Committee? Committee { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Place { get; set; } = string.Empty;
    public decimal SupervisorScore { get; set; }
    public decimal CommitteeScore { get; set; }
    public decimal FinalScore { get; set; }
    public string ReportText { get; set; } = string.Empty;

    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<StudentEvaluation> StudentEvaluations { get; set; } = new List<StudentEvaluation>();
}
