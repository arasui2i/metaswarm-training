using MediatR;

namespace CRM.Application.Features.Leads.DeleteLead;

public record DeleteLeadCommand(Guid Id) : IRequest;
