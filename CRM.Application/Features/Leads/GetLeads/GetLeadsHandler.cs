using CRM.Application.Common;
using CRM.Application.Features.Leads;
using CRM.Application.Interfaces;
using MediatR;

namespace CRM.Application.Features.Leads.GetLeads;

public class GetLeadsHandler : IRequestHandler<GetLeadsQuery, PagedResult<LeadSummaryDto>>
{
    private readonly ILeadRepository _repo;
    public GetLeadsHandler(ILeadRepository repo) => _repo = repo;
    public async Task<PagedResult<LeadSummaryDto>> Handle(GetLeadsQuery request, CancellationToken cancellationToken)
    {
        var result = await _repo.GetPagedAsync(request.Search, request.Page, request.PageSize, cancellationToken);
        var dtos = result.Items.Select(l => new LeadSummaryDto(
            l.Id,
            $"{l.FirstName} {l.LastName}".Trim(),
            l.CompanyName,
            l.Email,
            l.Status
        )).ToList();
        return new PagedResult<LeadSummaryDto>(dtos, result.TotalCount, result.Page, result.PageSize);
    }
}
