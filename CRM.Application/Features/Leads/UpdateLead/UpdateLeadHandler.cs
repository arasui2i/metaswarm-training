using CRM.Application.Common.Exceptions;
using CRM.Application.Interfaces;
using MediatR;

namespace CRM.Application.Features.Leads.UpdateLead;

public class UpdateLeadHandler : IRequestHandler<UpdateLeadCommand>
{
    private readonly ILeadRepository _repo;
    public UpdateLeadHandler(ILeadRepository repo) => _repo = repo;
    public async Task Handle(UpdateLeadCommand request, CancellationToken cancellationToken)
    {
        var lead = await _repo.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Lead {request.Id} not found.");
        lead.FirstName = request.FirstName;
        lead.LastName = request.LastName;
        lead.CompanyName = request.CompanyName;
        lead.Email = request.Email;
        lead.Phone = request.Phone;
        lead.Status = request.Status;
        lead.OwnerId = request.OwnerId;
        await _repo.UpdateAsync(lead, cancellationToken);
    }
}
