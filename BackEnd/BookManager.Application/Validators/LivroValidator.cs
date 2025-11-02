using BookManager.Infrastructure.DTOs;
using FluentValidation;

namespace BookManager.Application.Validators;

/// <summary>
/// Validador para CreateLivroDto
/// </summary>
public class CreateLivroDtoValidator : AbstractValidator<CreateLivroDto>
{
    public CreateLivroDtoValidator()
    {
        RuleFor(x => x.Titulo)
            .NotEmpty()
            .WithMessage("O título do livro é obrigatório.")
            .MaximumLength(255)
            .WithMessage("O título do livro não pode exceder 255 caracteres.");

        RuleFor(x => x.IdAssunto)
            .GreaterThan(0)
            .WithMessage("O assunto do livro é obrigatório.");

        RuleFor(x => x.IdAutores)
            .NotEmpty()
            .WithMessage("O livro deve ter pelo menos um autor.")
            .Must(autores => autores.All(a => a > 0))
            .WithMessage("Todos os IDs de autores devem ser válidos.");

        RuleFor(x => x.AnoPublicacao)
            .GreaterThan(1000)
            .LessThanOrEqualTo(DateTime.Now.Year)
            .When(x => x.AnoPublicacao.HasValue)
            .WithMessage("O ano de publicação deve estar entre 1000 e o ano atual.");

        RuleFor(x => x.ISBN)
            .Matches(@"^\d{3}-\d{10}(\d|X)$|^\d{13}$")
            .When(x => !string.IsNullOrEmpty(x.ISBN))
            .WithMessage("O ISBN deve estar em um formato válido.");

        RuleFor(x => x.Precos)
            .NotEmpty()
            .WithMessage("O livro deve ter pelo menos um preço.")
            .Must(precos => precos.All(p => p.IdFormaPagamento > 0 && p.Valor > 0))
            .WithMessage("Todos os preços devem ter forma de pagamento e valor válidos.");
    }
}

/// <summary>
/// Validador para UpdateLivroDto
/// </summary>
public class UpdateLivroDtoValidator : AbstractValidator<UpdateLivroDto>
{
    public UpdateLivroDtoValidator()
    {
        RuleFor(x => x.Titulo)
            .NotEmpty()
            .WithMessage("O título do livro é obrigatório.")
            .MaximumLength(255)
            .WithMessage("O título do livro não pode exceder 255 caracteres.");

        RuleFor(x => x.IdAssunto)
            .GreaterThan(0)
            .WithMessage("O assunto do livro é obrigatório.");

        RuleFor(x => x.IdAutores)
            .NotEmpty()
            .WithMessage("O livro deve ter pelo menos um autor.")
            .Must(autores => autores.All(a => a > 0))
            .WithMessage("Todos os IDs de autores devem ser válidos.");

        RuleFor(x => x.AnoPublicacao)
            .GreaterThan(1000)
            .LessThanOrEqualTo(DateTime.Now.Year)
            .When(x => x.AnoPublicacao.HasValue)
            .WithMessage("O ano de publicação deve estar entre 1000 e o ano atual.");

        RuleFor(x => x.ISBN)
            .Matches(@"^\d{3}-\d{10}(\d|X)$|^\d{13}$")
            .When(x => !string.IsNullOrEmpty(x.ISBN))
            .WithMessage("O ISBN deve estar em um formato válido.");
    }
}

/// <summary>
/// Validador para CreateLivroPrecoDto
/// </summary>
public class CreateLivroPrecoDtoValidator : AbstractValidator<CreateLivroPrecoDto>
{
    public CreateLivroPrecoDtoValidator()
    {
        RuleFor(x => x.IdFormaPagamento)
            .GreaterThan(0)
            .WithMessage("A forma de pagamento é obrigatória.");

        RuleFor(x => x.Valor)
            .GreaterThan(0)
            .WithMessage("O valor do livro deve ser maior que zero.")
            // Ensure at most 2 decimal places
            .Must(v => Decimal.Round(v, 2) == v)
            .WithMessage("O valor deve ter no máximo 2 casas decimais.");
    }
}
