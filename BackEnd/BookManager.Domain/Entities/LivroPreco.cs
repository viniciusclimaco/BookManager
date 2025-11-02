namespace BookManager.Domain.Entities;

/// <summary>
/// Entidade que representa o Preço de um Livro para uma Forma de Pagamento específica
/// </summary>
public class LivroPreco
{
    public int IdLivroPreco { get; set; }
    public int IdLivro { get; set; }
    public int IdFormaPagamento { get; set; }
    public decimal Valor { get; set; }
    public DateTime DataCadastro { get; set; }
    public DateTime DataAtualizacao { get; set; }

    // Relacionamentos
    public virtual Livro? Livro { get; set; }
    public virtual FormaPagamento? FormaPagamento { get; set; }
}
