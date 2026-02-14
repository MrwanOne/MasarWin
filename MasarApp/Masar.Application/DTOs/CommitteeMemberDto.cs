using Masar.Domain.Enums;

namespace Masar.Application.DTOs;

public class CommitteeMemberDto
{
    public int CommitteeId { get; set; }
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public CommitteeMemberRole Role { get; set; }
}
