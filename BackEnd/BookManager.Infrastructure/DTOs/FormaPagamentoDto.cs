namespace BookManager.Infrastructure.DTOs;

/// <summary>
/// DTO para transferência de dados da Forma de Pagamento
/// </summary>
public class FormaPagamentoDto
{
    public int IdFormaPagamento { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool Ativo { get; set; }
}

/// <summary>
/// DTO para criação de Forma de Pagamento
/// </summary>
public class CreateFormaPagamentoDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
}

/// <summary>
/// DTO para atualização de Forma de Pagamento
/// </summary>
public class UpdateFormaPagamentoDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool Ativo { get; set; }
}
