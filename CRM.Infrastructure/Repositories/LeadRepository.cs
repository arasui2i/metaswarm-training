using System.Diagnostics.CodeAnalysis;
using CRM.Application.Common;
using CRM.Application.Interfaces;
using CRM.Domain.Entities;
using CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Repositories;

[ExcludeFromCodeCoverage]
public class LeadRepository : ILeadRepository
{
    private readonly AppDbContext _context;

    public LeadRepository(AppDbContext context) => _context = context;

    public Task<Lead?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Leads
            .FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted, cancellationToken);

    public async Task<PagedResult<Lead>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Leads.Where(l => !l.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var lower = search.ToLower();
            query = query.Where(l =>
                l.FirstName.ToLower().Contains(lower) ||
                (l.LastName != null && l.LastName.ToLower().Contains(lower)) ||
                l.CompanyName.ToLower().Contains(lower) ||
                l.Email.ToLower().Contains(lower));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Lead>(items, total, page, pageSize);
    }

    public async Task<Guid> AddAsync(Lead lead, CancellationToken cancellationToken = default)
    {
        _context.Leads.Add(lead);
        await _context.SaveChangesAsync(cancellationToken);
        return lead.Id;
    }

    public async Task UpdateAsync(Lead lead, CancellationToken cancellationToken = default)
    {
        _context.Leads.Update(lead);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var lead = await _context.Leads.FindAsync([id], cancellationToken);
        if (lead is not null)
        {
            lead.IsDeleted = true;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
