using CRM.Application.Common;
using CRM.Domain.Entities;

namespace CRM.Application.Interfaces;

public interface ILeadRepository
{
    Task<Lead?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<Lead>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Guid> AddAsync(Lead lead, CancellationToken cancellationToken = default);
    Task UpdateAsync(Lead lead, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
