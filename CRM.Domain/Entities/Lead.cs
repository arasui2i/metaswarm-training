using CRM.Domain.Enums;

namespace CRM.Domain.Entities;

public class Lead
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public LeadStatus Status { get; set; } = LeadStatus.New;
    public Guid? OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
}
