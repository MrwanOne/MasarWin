using Masar.Domain.Common;
using System;

namespace Masar.Domain.Entities;

public class Document : BaseEntity
{
    public int DocumentId { get; set; }
    
    // Foreign Keys to possible parents
    public int? ProjectId { get; set; }
    public Project? Project { get; set; }
    
    public int? DiscussionId { get; set; }
    public Discussion? Discussion { get; set; }

    public int? StudentId { get; set; }
    public Student? Student { get; set; }
    
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int Version { get; set; } = 1;
    public string Status { get; set; } = "Draft"; // Draft, Submitted, Approved, Rejected
    public string? Checksum { get; set; }
    public string? Description { get; set; }
    public string Category { get; set; } = "General"; // TechnicalReport, Poster, Slide, FinalReport, CV, etc.
}
