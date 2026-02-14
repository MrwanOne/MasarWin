namespace Masar.Application.DTOs;

public class DiscussionDto
{
    public int DiscussionId { get; set; }
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public int CommitteeId { get; set; }
    public string CommitteeName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Place { get; set; } = string.Empty;
    public decimal SupervisorScore { get; set; }
    public decimal CommitteeScore { get; set; }
    public decimal FinalScore { get; set; }
    public string ReportText { get; set; } = string.Empty;
    public List<DocumentDto> Documents { get; set; } = new();
}
