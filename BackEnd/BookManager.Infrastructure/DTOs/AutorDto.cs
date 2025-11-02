namespace BookManager.Infrastructure.DTOs;

/// <summary>
/// DTO para transferência de dados do Autor
/// </summary>
public class AutorDto
{
    public int IdAutor { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Ativo { get; set; }
}

/// <summary>
/// DTO para criação de Autor
/// </summary>
public class CreateAutorDto
{
    public string Nome { get; set; } = string.Empty;
}

/// <summary>
/// DTO para atualização de Autor
/// </summary>
public class UpdateAutorDto
{
    public string Nome { get; set; } = string.Empty;
    public bool Ativo { get; set; }
}
