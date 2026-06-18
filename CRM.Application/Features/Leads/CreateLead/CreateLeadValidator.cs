using FluentValidation;

namespace CRM.Application.Features.Leads.CreateLead;

public class CreateLeadValidator : AbstractValidator<CreateLeadCommand>
{
    public CreateLeadValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.CompanyName).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
