using Masar.Domain.Enums;
using Masar.Domain.Common;

namespace Masar.Domain.Entities;

public class CommitteeMember : BaseEntity
{
    public int CommitteeId { get; set; }
    public Committee? Committee { get; set; }
    public int DoctorId { get; set; }
    public Doctor? Doctor { get; set; }
    public CommitteeMemberRole Role { get; set; } = CommitteeMemberRole.Member;
}
