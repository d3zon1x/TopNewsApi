using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalterApi.Core.DTO_s.User;

namespace WalterApi.Core.Validations
{
    public class CreateUserValidation : AbstractValidator<CreateUserDTO>
    {
        public CreateUserValidation()
        {
            RuleFor(r => r.FirstName).NotEmpty().MinimumLength(2).MaximumLength(128);
            RuleFor(r => r.LastName).NotEmpty().MinimumLength(2).MaximumLength(128);
            RuleFor(r => r.Email).NotEmpty().EmailAddress();
            RuleFor(r => r.Role).NotEmpty();
            RuleFor(r => r.Password).NotEmpty().MinimumLength(6);
            RuleFor(r => r.ConfirmPassword).NotEmpty().MinimumLength(6).Equal(p => p.Password);
        }
    }
}
