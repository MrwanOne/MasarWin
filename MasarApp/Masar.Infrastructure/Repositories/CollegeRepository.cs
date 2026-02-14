using Masar.Application.Interfaces;
using Masar.Domain.Entities;
using Masar.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Masar.Infrastructure.Repositories;

public class CollegeRepository : EfRepository<College>, ICollegeRepository
{
    public CollegeRepository(IDbContextFactory<MasarDbContext> contextFactory) : base(contextFactory)
    {
    }
}
