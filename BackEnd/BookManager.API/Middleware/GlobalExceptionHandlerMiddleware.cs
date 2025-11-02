using BookManager.Domain.Exceptions;
using FluentValidation;
using System.Net;
using System.Text.Json;

namespace BookManager.API.Middleware;

/// <summary>
/// Middleware global para tratamento centralizado de exceções
/// Converte exceções de domínio em respostas HTTP apropriadas
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Determina o status code e mensagem baseado no tipo de exceção
        var (statusCode, errorResponse) = exception switch
        {
            // Exceções de validação (400 Bad Request)
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Dados inválidos",
                    Errors = validationEx.Errors.Select(e => e.ErrorMessage).ToList(),
                    TraceId = context.TraceIdentifier
                }),

            // Exceções de duplicação (409 Conflict)
            DuplicateResourceException duplicateEx => (
                HttpStatusCode.Conflict,
                new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.Conflict,
                    Message = duplicateEx.Message,
                    Details = new Dictionary<string, object>
                    {
                        ["resourceType"] = duplicateEx.ResourceType,
                        ["fieldName"] = duplicateEx.FieldName,
                        ["value"] = duplicateEx.Value ?? "não informado"
                    },
                    TraceId = context.TraceIdentifier
                }),

            // Exceções de recurso em uso (409 Conflict)
            ResourceInUseException inUseEx => (
                HttpStatusCode.Conflict,
                new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.Conflict,
                    Message = inUseEx.Message,
                    Details = new Dictionary<string, object>
                    {
                        ["resourceType"] = inUseEx.ResourceType,
                        ["resourceId"] = inUseEx.ResourceId,
                        ["resourceName"] = inUseEx.ResourceName,
                        ["dependentEntityType"] = inUseEx.DependentEntityType,
                        ["dependentCount"] = inUseEx.DependentCount
                    },
                    TraceId = context.TraceIdentifier
                }),

            // Exceções de unique key violation (409 Conflict)
            UniqueKeyViolationException uniqueEx => (
                HttpStatusCode.Conflict,
                new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.Conflict,
                    Message = uniqueEx.Message,
                    Details = new Dictionary<string, object>
                    {
                        ["fieldName"] = uniqueEx.FieldName,
                        ["duplicateValue"] = uniqueEx.DuplicateValue ?? "não informado"
                    },
                    TraceId = context.TraceIdentifier
                }),

            // Exceções de foreign key violation (409 Conflict)
            ForeignKeyViolationException fkEx => (
                HttpStatusCode.Conflict,
                new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.Conflict,
                    Message = fkEx.Message,
                    Details = new Dictionary<string, object>
                    {
                        ["entityName"] = fkEx.EntityName,
                        ["entityId"] = fkEx.EntityId,
                        ["relatedEntityName"] = fkEx.RelatedEntityName
                    },
                    TraceId = context.TraceIdentifier
                }),

            // Exceções de recurso não encontrado (404 Not Found)
            KeyNotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = notFoundEx.Message,
                    TraceId = context.TraceIdentifier
                }),

            // Exceções de operação inválida (400 Bad Request)
            InvalidOperationException invalidOpEx => (
                HttpStatusCode.BadRequest,
                new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = invalidOpEx.Message,
                    TraceId = context.TraceIdentifier
                }),

            // Exceções de argumento (400 Bad Request)
            ArgumentException argEx => (
                HttpStatusCode.BadRequest,
                new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = argEx.Message,
                    TraceId = context.TraceIdentifier
                }),

            // Deadlock (503 Service Unavailable - pode tentar novamente)
            DatabaseDeadlockException _ => (
                HttpStatusCode.ServiceUnavailable,
                new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.ServiceUnavailable,
                    Message = "A operação foi cancelada devido a um conflito. Por favor, tente novamente.",
                    TraceId = context.TraceIdentifier,
                    RetryAfter = 5 // segundos
                }),

            // Timeout (504 Gateway Timeout)
            DatabaseTimeoutException _ => (
                HttpStatusCode.GatewayTimeout,
                new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.GatewayTimeout,
                    Message = "A operação excedeu o tempo limite. Por favor, tente novamente.",
                    TraceId = context.TraceIdentifier
                }),

            // Exceções não tratadas (500 Internal Server Error)
            _ => (
                HttpStatusCode.InternalServerError,
                new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "Ocorreu um erro interno no servidor. Por favor, contate o suporte.",
                    TraceId = context.TraceIdentifier
                })
        };

        // Log apropriado baseado na severidade
        LogException(exception, statusCode, context);

        // Configura a resposta HTTP
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        if (errorResponse.RetryAfter.HasValue)
        {
            context.Response.Headers["Retry-After"] = errorResponse.RetryAfter.Value.ToString();
        }

        // Serializa e retorna a resposta
        var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        await context.Response.WriteAsync(json);
    }

    private void LogException(Exception exception, HttpStatusCode statusCode, HttpContext context)
    {
        var logLevel = statusCode switch
        {
            HttpStatusCode.InternalServerError => LogLevel.Error,
            HttpStatusCode.ServiceUnavailable => LogLevel.Warning,
            HttpStatusCode.GatewayTimeout => LogLevel.Warning,
            _ => LogLevel.Information
        };

        _logger.Log(
            logLevel,
            exception,
            "Exception handled: {ExceptionType} - Status: {StatusCode} - Path: {Path} - TraceId: {TraceId}",
            exception.GetType().Name,
            (int)statusCode,
            context.Request.Path,
            context.TraceIdentifier);
    }
}

/// <summary>
/// Modelo de resposta de erro padronizado
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Código de status HTTP
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Mensagem de erro principal
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Lista de erros detalhados (usado em validação)
    /// </summary>
    public List<string>? Errors { get; set; }

    /// <summary>
    /// Detalhes adicionais sobre o erro
    /// </summary>
    public Dictionary<string, object>? Details { get; set; }

    /// <summary>
    /// ID de rastreamento para correlação de logs
    /// </summary>
    public string TraceId { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp do erro
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Tempo sugerido para retry (em segundos)
    /// </summary>
    public int? RetryAfter { get; set; }
}

/// <summary>
/// Extension method para registrar o middleware
/// </summary>
public static class GlobalExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
