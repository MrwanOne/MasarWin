using Masar.Application.DTOs;
using Masar.Application.Interfaces;
using Masar.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Services;

public class AcademicTermService : IAcademicTermService
{
    private readonly IAcademicTermRepository _repository;

    public AcademicTermService(IAcademicTermRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<AcademicTermDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var terms = await _repository.GetAllOrderedAsync(cancellationToken);
        return terms.Select(ToDto).ToList();
    }

    public async Task<AcademicTermDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var term = await _repository.GetByIdAsync(id, cancellationToken);
        return term == null ? null : ToDto(term);
    }

    public async Task<AcademicTermDto?> GetActiveTermAsync(CancellationToken cancellationToken = default)
    {
        var term = await _repository.GetActiveTermAsync(cancellationToken);
        return term == null ? null : ToDto(term);
    }

    public async Task<AcademicTermDto> AddAsync(AcademicTermDto dto, CancellationToken cancellationToken = default)
    {
        // Check for duplicate Year+Semester
        if (await _repository.ExistsAsync(dto.Year, dto.Semester, null, cancellationToken))
        {
            throw new InvalidOperationException($"الفصل الدراسي للسنة {dto.Year} الفصل {dto.Semester} موجود مسبقاً. / Academic term for Year {dto.Year} Semester {dto.Semester} already exists.");
        }

        var entity = new AcademicTerm
        {
            Year = dto.Year,
            Semester = dto.Semester,
            NameAr = dto.NameAr,
            NameEn = dto.NameEn,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(entity, cancellationToken);
        return ToDto(entity);
    }

    public async Task<AcademicTermDto> UpdateAsync(AcademicTermDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(dto.TermId, cancellationToken);
        if (entity == null) throw new InvalidOperationException("Academic term not found.");

        entity.Year = dto.Year;
        entity.Semester = dto.Semester;
        entity.NameAr = dto.NameAr;
        entity.NameEn = dto.NameEn;
        entity.StartDate = dto.StartDate;
        entity.EndDate = dto.EndDate;
        entity.IsActive = dto.IsActive;

        await _repository.UpdateAsync(entity, cancellationToken);
        return ToDto(entity);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            await _repository.DeleteAsync(entity, cancellationToken);
        }
    }

    public async Task SetActiveTermAsync(int termId, CancellationToken cancellationToken = default)
    {
        await _repository.SetActiveTermAsync(termId, cancellationToken);
    }

    private static AcademicTermDto ToDto(AcademicTerm entity) => new()
    {
        TermId = entity.TermId,
        Year = entity.Year,
        Semester = entity.Semester,
        NameAr = entity.NameAr,
        NameEn = entity.NameEn,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        IsActive = entity.IsActive
    };
}
