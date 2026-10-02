using FluentAssertions;
using NSubstitute;
using ResumeCrud.API.Application.Services;
using ResumeCrud.API.Domain.Entities;
using ResumeCrud.API.Domain.Repositories;

namespace ResumeCrud.Tests.Application;

public class GetResumesUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnListOfResumes_WhenResumesExist()
    {
        // Arrange
        var repositoryMock = Substitute.For<IResumeRepository>();
        
        // Simulamos o retorno do repositório
        var mockResumes = new List<Resume>
        {
            new Resume("João Silva", "joao@email.com", "999999999", new byte[] { 1, 2 }),
            new Resume("Maria Santos", "maria@email.com", "888888888", new byte[] { 3, 4 })
        };
        
        repositoryMock.GetAllAsync().Returns(mockResumes);

        var useCase = new GetResumesUseCase(repositoryMock);

        // Act
        var result = await useCase.ExecuteAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        
        // Verificamos se o mapeamento para DTO foi feito corretamente
        result.First().Name.Should().Be("João Silva");
        result.Last().Name.Should().Be("Maria Santos");
        
        // Garantimos que o repositório foi chamado exatamente 1 vez
        await repositoryMock.Received(1).GetAllAsync();
    }
}