using System;

namespace Masar.Application.DTOs;

public class DocumentDto
{
    public int DocumentId { get; set; }
    public int ProjectId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int Version { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Checksum { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedByName { get; set; }
}
