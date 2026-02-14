using System;
using Masar.Domain.Enums;

namespace Masar.Application.DTOs;

public class ProjectDto
{
    public int ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Beneficiary { get; set; } = string.Empty;
    public ProjectStatus Status { get; set; }
    public decimal CompletionRate { get; set; }
    public DateTime ProposedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string RejectionReason { get; set; } = string.Empty;

    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int CollegeId { get; set; }
    public string CollegeName { get; set; } = string.Empty;
    public int? TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public int? SupervisorId { get; set; }
    public string? SupervisorName { get; set; } = string.Empty;
    public string? StatusChangeReason { get; set; }
    public int? TermId { get; set; }
    public string TermName { get; set; } = string.Empty;
    public List<DocumentDto> Documents { get; set; } = new();
}
