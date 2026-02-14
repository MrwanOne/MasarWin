using System.Collections.Generic;

namespace Masar.Application.DTOs;

public class ReportResultDto
{
    public string Title { get; set; } = string.Empty;
    public List<ProjectDto> Projects { get; set; } = new();
}
