using CRM.Domain.Enums;

namespace CRM.Application.Features.Leads;

public record LeadSummaryDto(Guid Id, string FullName, string CompanyName, string Email, LeadStatus Status);
public record LeadDetailDto(Guid Id, string FirstName, string? LastName, string CompanyName, string Email, string? Phone, LeadStatus Status, Guid? OwnerId, DateTime CreatedAt);
