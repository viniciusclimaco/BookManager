namespace BookManager.Domain.Entities;

/// <summary>
/// Entidade de Junção que representa a relação Many-to-Many entre Livro e Autor
/// </summary>
public class LivroAutor
{
    public int IdLivroAutor { get; set; }
    public int IdLivro { get; set; }
    public int IdAutor { get; set; }
    public int Ordem { get; set; }
    public DateTime DataCadastro { get; set; }

    // Relacionamentos
    public virtual Livro? Livro { get; set; }
    public virtual Autor? Autor { get; set; }
}
