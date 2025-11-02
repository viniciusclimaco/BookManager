using BookManager.Infrastructure.DTOs;
using FluentValidation;

namespace BookManager.Application.Validators;

/// <summary>
/// Validador para CreateAutorDto
/// </summary>
public class CreateAutorDtoValidator : AbstractValidator<CreateAutorDto>
{
    public CreateAutorDtoValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty()
            .WithMessage("O nome do autor é obrigatório.")
            .MaximumLength(255)
            .WithMessage("O nome do autor não pode exceder 255 caracteres.");
    }
}

/// <summary>
/// Validador para UpdateAutorDto
/// </summary>
public class UpdateAutorDtoValidator : AbstractValidator<UpdateAutorDto>
{
    public UpdateAutorDtoValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty()
            .WithMessage("O nome do autor é obrigatório.")
            .MaximumLength(255)
            .WithMessage("O nome do autor não pode exceder 255 caracteres.");
    }
}
