using CRM.Domain.Enums;
using MediatR;

namespace CRM.Application.Features.Leads.CreateLead;

public record CreateLeadCommand(
    string FirstName,
    string? LastName,
    string CompanyName,
    string Email,
    string? Phone,
    LeadStatus Status,
    Guid? OwnerId
) : IRequest<Guid>;
