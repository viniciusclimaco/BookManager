namespace BookManager.Domain.Entities;

/// <summary>
/// Entidade que representa um Assunto/Categoria de Livro
/// </summary>
public class Assunto
{
    public int IdAssunto { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataCadastro { get; set; }
    public bool Ativo { get; set; }

    // Relacionamento
    public virtual ICollection<Livro> Livros { get; set; } = new List<Livro>();
}
