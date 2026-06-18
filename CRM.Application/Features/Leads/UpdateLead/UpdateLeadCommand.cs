using CRM.Domain.Enums;
using MediatR;

namespace CRM.Application.Features.Leads.UpdateLead;

public record UpdateLeadCommand(
    Guid Id,
    string FirstName,
    string? LastName,
    string CompanyName,
    string Email,
    string? Phone,
    LeadStatus Status,
    Guid? OwnerId
) : IRequest;
