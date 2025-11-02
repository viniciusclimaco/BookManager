namespace BookManager.Domain.Exceptions;

/// <summary>
/// Exceção base para violação de constraints de banco de dados
/// </summary>
public abstract class DatabaseConstraintViolationException : Exception
{
    public string ConstraintName { get; }
    public int SqlErrorNumber { get; }

    protected DatabaseConstraintViolationException(
        string message, 
        string constraintName, 
        int sqlErrorNumber, 
        Exception? innerException = null) 
        : base(message, innerException)
    {
        ConstraintName = constraintName;
        SqlErrorNumber = sqlErrorNumber;
    }
}

/// <summary>
/// Exceção para violação de chave única (UNIQUE constraint)
/// SQL Error Number: 2627
/// </summary>
public class UniqueKeyViolationException : DatabaseConstraintViolationException
{
    public string FieldName { get; }
    public string? DuplicateValue { get; }

    public UniqueKeyViolationException(
        string fieldName, 
        string? duplicateValue = null, 
        Exception? innerException = null)
        : base(
            $"Já existe um registro com {fieldName} '{duplicateValue ?? "esse valor"}'.",
            fieldName,
            2627,
            innerException)
    {
        FieldName = fieldName;
        DuplicateValue = duplicateValue;
    }
}

/// <summary>
/// Exceção para violação de chave estrangeira (FOREIGN KEY constraint)
/// SQL Error Number: 547
/// </summary>
public class ForeignKeyViolationException : DatabaseConstraintViolationException
{
    public string EntityName { get; }
    public int EntityId { get; }
    public string RelatedEntityName { get; }

    public ForeignKeyViolationException(
        string entityName,
        int entityId,
        string relatedEntityName,
        Exception? innerException = null)
        : base(
            $"Não é possível excluir {entityName} (ID: {entityId}) pois existem registros de {relatedEntityName} associados. " +
            $"Para excluir, primeiro remova todos os {relatedEntityName} relacionados.",
            $"FK_{entityName}_{relatedEntityName}",
            547,
            innerException)
    {
        EntityName = entityName;
        EntityId = entityId;
        RelatedEntityName = relatedEntityName;
    }
}

/// <summary>
/// Exceção para recurso duplicado (validação de negócio antes do banco)
/// </summary>
public class DuplicateResourceException : Exception
{
    public string ResourceType { get; }
    public string FieldName { get; }
    public string? Value { get; }

    public DuplicateResourceException(
        string resourceType,
        string fieldName,
        string? value = null)
        : base($"{resourceType} com {fieldName} '{value ?? "esse valor"}' já existe.")
    {
        ResourceType = resourceType;
        FieldName = fieldName;
        Value = value;
    }
}

/// <summary>
/// Exceção para recurso em uso (não pode ser excluído)
/// </summary>
public class ResourceInUseException : Exception
{
    public string ResourceType { get; }
    public int ResourceId { get; }
    public string ResourceName { get; }
    public string DependentEntityType { get; }
    public int DependentCount { get; }

    public ResourceInUseException(
        string resourceType,
        int resourceId,
        string resourceName,
        string dependentEntityType,
        int dependentCount)
        : base(
            $"Não é possível excluir {resourceType} '{resourceName}' (ID: {resourceId}) " +
            $"pois existem {dependentCount} {dependentEntityType}(s) associado(s). " +
            $"Para excluir, primeiro remova ou reatribua os {dependentEntityType}(s).")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
        ResourceName = resourceName;
        DependentEntityType = dependentEntityType;
        DependentCount = dependentCount;
    }
}

/// <summary>
/// Exceção para deadlock (SQL Error Number: 1205)
/// </summary>
public class DatabaseDeadlockException : Exception
{
    public DatabaseDeadlockException(Exception innerException)
        : base("A operação foi cancelada devido a um conflito de bloqueio no banco de dados. Por favor, tente novamente.", innerException)
    {
    }
}

/// <summary>
/// Exceção para timeout de banco de dados (SQL Error Number: -2)
/// </summary>
public class DatabaseTimeoutException : Exception
{
    public DatabaseTimeoutException(Exception innerException)
        : base("A operação excedeu o tempo limite. Por favor, tente novamente.", innerException)
    {
    }
}

/// <summary>
/// Exceção para erro de concorrência otimista
/// </summary>
public class OptimisticConcurrencyException : Exception
{
    public string EntityType { get; }
    public int EntityId { get; }

    public OptimisticConcurrencyException(string entityType, int entityId)
        : base($"O {entityType} (ID: {entityId}) foi modificado por outro usuário. Por favor, recarregue e tente novamente.")
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}
