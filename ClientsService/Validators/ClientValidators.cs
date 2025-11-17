using Clients;
using ClientsService.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ClientsService.Validators;

public class CreateClientValidator : AbstractValidator<CreateClientRequest>
{
    public CreateClientValidator(ClientsDbContext context)
    {
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.Email)
            .NotEmpty().EmailAddress()
            .Must(email => email.EndsWith("@censudex.cl"))
            .WithMessage("Email must be a @censudex.cl domain.")
            .MustAsync(async (email, token) =>
                !await context.Clients.IgnoreQueryFilters().AnyAsync(c => c.Email == email, token))
            .WithMessage("This email address is already registered.");

        RuleFor(x => x.Username)
            .NotEmpty()
            .MustAsync(async (username, token) =>
                !await context.Clients.IgnoreQueryFilters().AnyAsync(c => c.Username == username, token))
            .WithMessage("This username is already taken.");

        RuleFor(x => x.BirthDate)
            .Must(BeOver18)
            .WithMessage("Client must be at least 18 years old.");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+56\d{9}$")
            .WithMessage("Phone number must be in Chilean format (+56XXXXXXXXX).");
    }

    private bool BeOver18(string birthDateStr)
    {
        if (DateOnly.TryParse(birthDateStr, out var date))
        {
            return date <= DateOnly.FromDateTime(DateTime.Today.AddYears(-18));
        }
        return false;
    }
}

public class UpdateClientValidator : AbstractValidator<UpdateClientRequest>
{
    public UpdateClientValidator()
    {
        // Add validation for password only if it's provided
        When(x => !string.IsNullOrEmpty(x.Password), () =>
        {
            RuleFor(x => x.Password)
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one number.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
        });

        When(x => !string.IsNullOrEmpty(x.BirthDate), () =>
        {
            RuleFor(x => x.BirthDate)
                .Must(BeOver18).WithMessage("Client must be at least 18 years old.");
        });

        // Solo validar Teléfono si NO es vacío
        When(x => !string.IsNullOrEmpty(x.PhoneNumber), () =>
        {
            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+56\d{9}$").WithMessage("Phone number must be in Chilean format (+56XXXXXXXXX).");
        });
    }
    private bool BeOver18(string birthDateStr)
    {
        if (DateOnly.TryParse(birthDateStr, out var date))
        {
            return date <= DateOnly.FromDateTime(DateTime.Today.AddYears(-18));
        }
        return false;
    }
}