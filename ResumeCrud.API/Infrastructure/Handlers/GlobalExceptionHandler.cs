using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ResumeCrud.API.Infrastructure.Handlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Tratamento específico para as nossas regras de Domínio
        if (exception is ArgumentException argumentException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            
            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Erro de Validação",
                Detail = argumentException.Message
            }, cancellationToken);

            return true; // Indica que a exceção foi tratada
        }

        // Tratamento genérico para erros inesperados
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Erro Interno do Servidor",
            Detail = "Ocorreu um erro inesperado ao processar o seu pedido."
        }, cancellationToken);

        return true;
    }
}