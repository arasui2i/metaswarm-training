using CRM.Application.Interfaces;
using CRM.Domain.Entities;
using MediatR;

namespace CRM.Application.Features.Leads.CreateLead;

public class CreateLeadHandler : IRequestHandler<CreateLeadCommand, Guid>
{
    private readonly ILeadRepository _repo;
    public CreateLeadHandler(ILeadRepository repo) => _repo = repo;
    public async Task<Guid> Handle(CreateLeadCommand request, CancellationToken cancellationToken)
    {
        var lead = new Lead
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            CompanyName = request.CompanyName,
            Email = request.Email,
            Phone = request.Phone,
            Status = request.Status,
            OwnerId = request.OwnerId,
            CreatedAt = DateTime.UtcNow
        };
        return await _repo.AddAsync(lead, cancellationToken);
    }
}
