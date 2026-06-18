using CRM.Application.Common;
using CRM.Application.Features.Leads;
using MediatR;

namespace CRM.Application.Features.Leads.GetLeads;

public record GetLeadsQuery(string? Search, int Page = 1, int PageSize = 10) : IRequest<PagedResult<LeadSummaryDto>>;
