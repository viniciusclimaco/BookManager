namespace BookManager.Infrastructure.DTOs.Report;

/// <summary>
/// DTO para Relatório 1: Livros por Assunto
/// Utilizado pela view vw_RelatorioLivrosPorAssunto
/// </summary>
public class LivroPorAssuntoDto
{
    public int IdAssunto { get; set; }
    public string Assunto { get; set; } = string.Empty;
    public int IdLivro { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Editora { get; set; }
    public string? ISBN { get; set; }
    public int? AnoPublicacao { get; set; }
    public string? Autores { get; set; }
    public bool Ativo { get; set; }
}

/// <summary>
/// DTO para Relatório 2: Autores por Livro
/// Utilizado pela view vw_RelatorioAutoresPorLivro
/// </summary>
public class AutorPorLivroDto
{
    public int IdAutor { get; set; }
    public string Autor { get; set; } = string.Empty;
    public int? IdLivro { get; set; }
    public string? Titulo { get; set; }
    public string? Editora { get; set; }
    public string? Assunto { get; set; }
    public int? AnoPublicacao { get; set; }
    public string? ISBN { get; set; }
    public int? OrdemAutor { get; set; }
}

/// <summary>
/// DTO para Relatório 3: Livros com Preço
/// Utilizado pela view vw_RelatorioLivrosComPreco
/// </summary>
public class LivroComPrecoDto
{
    public int IdLivro { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Editora { get; set; }
    public string? ISBN { get; set; }
    public int? AnoPublicacao { get; set; }
    public string? Assunto { get; set; }
    public string? Autores { get; set; }
    public int IdFormaPagamento { get; set; }
    public string FormaPagamento { get; set; } = string.Empty;
    public decimal Preco { get; set; }
}
