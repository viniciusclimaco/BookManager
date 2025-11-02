using FluentValidation;
using BookManager.Infrastructure.DTOs;

namespace BookManager.Application.Validators;

/// <summary>
/// Validador para CreateAssuntoDto
/// </summary>
public class CreateAssuntoDtoValidator : AbstractValidator<CreateAssuntoDto>
{
    public CreateAssuntoDtoValidator()
    {
        RuleFor(x => x.Descricao)
            .NotEmpty()
            .WithMessage("A descrição do assunto é obrigatória.")
            .MaximumLength(255)
            .WithMessage("A descrição do assunto não pode exceder 255 caracteres.");
    }
}

/// <summary>
/// Validador para UpdateAssuntoDto
/// </summary>
public class UpdateAssuntoDtoValidator : AbstractValidator<UpdateAssuntoDto>
{
    public UpdateAssuntoDtoValidator()
    {
        RuleFor(x => x.Descricao)
            .NotEmpty()
            .WithMessage("A descrição do assunto é obrigatória.")
            .MaximumLength(255)
            .WithMessage("A descrição do assunto não pode exceder 255 caracteres.");
    }
}
