using Dsw2025Tpi.Application.Dtos.Users;
using FluentValidation;

namespace Dsw2025Tpi.Application.Validators;

public class CreateAdminRequestValidator : AbstractValidator<CreateAdminRequest>
{
    public CreateAdminRequestValidator()
    {
        RuleFor(x => x.UserName).ValidUserName();
        RuleFor(x => x.Password).ValidPassword(UserRules.MinAdminPasswordLength);
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.DisplayName).ValidDisplayName();
    }
}
