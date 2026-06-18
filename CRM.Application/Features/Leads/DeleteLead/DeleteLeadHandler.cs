using CRM.Application.Common.Exceptions;
using CRM.Application.Interfaces;
using MediatR;

namespace CRM.Application.Features.Leads.DeleteLead;

public class DeleteLeadHandler : IRequestHandler<DeleteLeadCommand>
{
    private readonly ILeadRepository _repo;
    public DeleteLeadHandler(ILeadRepository repo) => _repo = repo;
    public async Task Handle(DeleteLeadCommand request, CancellationToken cancellationToken)
    {
        var lead = await _repo.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Lead {request.Id} not found.");
        await _repo.SoftDeleteAsync(request.Id, cancellationToken);
    }
}
