using FluentValidation;

namespace CRM.Application.Features.Leads.UpdateLead;

public class UpdateLeadValidator : AbstractValidator<UpdateLeadCommand>
{
    public UpdateLeadValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.CompanyName).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
