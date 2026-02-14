using Masar.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Interfaces;

public interface ICommitteeRepository : IRepository<Committee>
{
    Task<List<Committee>> GetWithMembersAsync(CancellationToken cancellationToken = default);
    Task<Committee?> GetWithMembersAsync(int committeeId, CancellationToken cancellationToken = default);
    Task<bool> DoctorHasCommitteeInTermAsync(int doctorId, int termId, int? excludeCommitteeId, CancellationToken cancellationToken = default);
    Task AddMemberAsync(CommitteeMember member, CancellationToken cancellationToken = default);
    Task UpdateMemberAsync(CommitteeMember member, CancellationToken cancellationToken = default);
    Task RemoveMemberAsync(int committeeId, int doctorId, CancellationToken cancellationToken = default);
    Task<CommitteeMember?> GetMemberAsync(int committeeId, int doctorId, CancellationToken cancellationToken = default);
}
