namespace BookManager.Infrastructure.DTOs;

/// <summary>
/// DTO para transferência de dados do Assunto
/// </summary>
public class AssuntoDto
{
    public int IdAssunto { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public bool Ativo { get; set; }
}

/// <summary>
/// DTO para criação de Assunto
/// </summary>
public class CreateAssuntoDto
{
    public string Descricao { get; set; } = string.Empty;
}

/// <summary>
/// DTO para atualização de Assunto
/// </summary>
public class UpdateAssuntoDto
{
    public string Descricao { get; set; } = string.Empty;
    public bool Ativo { get; set; }
}
