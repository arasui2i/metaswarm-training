using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Leads;
using CRM.Application.Interfaces;
using MediatR;

namespace CRM.Application.Features.Leads.GetLeadById;

public class GetLeadByIdHandler : IRequestHandler<GetLeadByIdQuery, LeadDetailDto>
{
    private readonly ILeadRepository _repo;
    public GetLeadByIdHandler(ILeadRepository repo) => _repo = repo;
    public async Task<LeadDetailDto> Handle(GetLeadByIdQuery request, CancellationToken cancellationToken)
    {
        var lead = await _repo.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Lead {request.Id} not found.");
        return new LeadDetailDto(lead.Id, lead.FirstName, lead.LastName, lead.CompanyName, lead.Email, lead.Phone, lead.Status, lead.OwnerId, lead.CreatedAt);
    }
}
