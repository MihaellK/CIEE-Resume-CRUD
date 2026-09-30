using FluentAssertions;
using Microsoft.AspNetCore.Http;
using ResumeCrud.API.Infrastructure.Handlers;

namespace ResumeCrud.Tests.API;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_ShouldReturnBadRequest_WhenExceptionIsArgumentException()
    {
        // Arrange
        var handler = new GlobalExceptionHandler();
        
        // Simulamos o contexto de uma requisição HTTP real
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream(); 
        
        // Simulamos a exceção lançada pelo nosso Domínio
        var exception = new ArgumentException("O conteúdo do currículo não pode estar vazio.");

        // Act
        var isHandled = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        isHandled.Should().BeTrue(); // Confirma que o handler processou o erro
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }
}