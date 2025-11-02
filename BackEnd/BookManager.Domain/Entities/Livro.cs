namespace BookManager.Domain.Entities;

/// <summary>
/// Entidade que representa um Livro
/// </summary>
public class Livro
{
    public int IdLivro { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Editora { get; set; }
    public int? AnoPublicacao { get; set; }
    public string? ISBN { get; set; }
    public int IdAssunto { get; set; }
    public DateTime DataCadastro { get; set; }
    public bool Ativo { get; set; }

    // Relacionamentos
    public virtual Assunto? Assunto { get; set; }
    public virtual ICollection<LivroAutor> Autores { get; set; } = new List<LivroAutor>();
    public virtual ICollection<LivroPreco> Precos { get; set; } = new List<LivroPreco>();
}
