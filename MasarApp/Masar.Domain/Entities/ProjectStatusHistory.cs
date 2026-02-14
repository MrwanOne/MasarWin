using Masar.Domain.Enums;

namespace Masar.Domain.Entities;

/// <summary>
/// سجل تتبع تغييرات حالة المشروع
/// Project Status History Entity
/// </summary>
public class ProjectStatusHistory
{
    public int HistoryId { get; set; }
    public int ProjectId { get; set; }
    public ProjectStatus OldStatus { get; set; }
    public ProjectStatus NewStatus { get; set; }
    public int? ChangedByUserId { get; set; }
    public string ChangeReason { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual Project Project { get; set; } = null!;
    public virtual User? ChangedByUser { get; set; }
}
