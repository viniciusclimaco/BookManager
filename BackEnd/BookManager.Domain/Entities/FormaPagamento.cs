namespace BookManager.Domain.Entities;

/// <summary>
/// Entidade que representa uma Forma de Pagamento
/// </summary>
public class FormaPagamento
{
    public int IdFormaPagamento { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool Ativo { get; set; }

    // Relacionamento
    public virtual ICollection<LivroPreco> LivrosPreco { get; set; } = new List<LivroPreco>();
}
