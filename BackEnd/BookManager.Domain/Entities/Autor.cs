namespace BookManager.Domain.Entities;

/// <summary>
/// Entidade que representa um Autor de Livros
/// </summary>
public class Autor
{
    public int IdAutor { get; set; }
    public string Nome { get; set; } = string.Empty;
    public DateTime DataCadastro { get; set; }
    public bool Ativo { get; set; }

    // Relacionamento
    public virtual ICollection<LivroAutor> LivrosAutor { get; set; } = new List<LivroAutor>();
}
