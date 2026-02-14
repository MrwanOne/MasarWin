using Masar.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Interfaces;

public interface IDiscussionRepository : IRepository<Discussion>
{
    Task<List<Discussion>> GetWithDetailsAsync(CancellationToken cancellationToken = default);
    Task<List<Discussion>> GetConflictingAsync(DateTime start, DateTime end, string place, int? committeeId, int? teamId, CancellationToken cancellationToken = default);
}
