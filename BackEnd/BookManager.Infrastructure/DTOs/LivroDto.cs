namespace BookManager.Infrastructure.DTOs;

/// <summary>
/// DTO para transferência de dados do Livro (leitura)
/// </summary>
public class LivroDto
{
    public int IdLivro { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Editora { get; set; }
    public int? AnoPublicacao { get; set; }
    public string? ISBN { get; set; }
    public int IdAssunto { get; set; }
    public string? AssuntoDescricao { get; set; }
    public bool Ativo { get; set; }
    public List<LivroAutorDto> Autores { get; set; } = new();
    public List<LivroPrecoDto> Precos { get; set; } = new();
}

/// <summary>
/// DTO para criação de Livro
/// </summary>
public class CreateLivroDto
{
    public string Titulo { get; set; } = string.Empty;
    public string? Editora { get; set; }
    public int? AnoPublicacao { get; set; }
    public string? ISBN { get; set; }
    public int IdAssunto { get; set; }
    public List<int> IdAutores { get; set; } = new();
    public List<CreateLivroPrecoDto> Precos { get; set; } = new();
}

/// <summary>
/// DTO para atualização de Livro
/// </summary>
public class UpdateLivroDto
{
    public string Titulo { get; set; } = string.Empty;
    public string? Editora { get; set; }
    public int? AnoPublicacao { get; set; }
    public string? ISBN { get; set; }
    public int IdAssunto { get; set; }
    public bool Ativo { get; set; }
    public List<int> IdAutores { get; set; } = new();
    public List<UpdateLivroPrecoDto> Precos { get; set; } = new();
}

/// <summary>
/// DTO para relacionamento Livro-Autor
/// </summary>
public class LivroAutorDto
{
    public int IdAutor { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int Ordem { get; set; }
}
