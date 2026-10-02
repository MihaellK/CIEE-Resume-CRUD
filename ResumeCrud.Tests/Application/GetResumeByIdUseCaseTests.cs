using FluentAssertions;
using NSubstitute;
using ResumeCrud.API.Application.Services;
using ResumeCrud.API.Domain.Entities;
using ResumeCrud.API.Domain.Repositories;

namespace ResumeCrud.Tests.Application;

public class GetResumeByIdUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnResume_WhenResumeExists()
    {
        // Arrange
        var repositoryMock = Substitute.For<IResumeRepository>();
        var targetId = Guid.NewGuid();
        
        var expectedResume = new Resume("Mihaell Alves", "mihaell@teste.com", "11999999999", "Engenharia", "Resumo", null);
        
        // Simulamos que o repositório encontrou o registo
        repositoryMock.GetByIdAsync(targetId).Returns(expectedResume);

        var useCase = new GetResumeByIdUseCase(repositoryMock);

        // Act
        var result = await useCase.ExecuteAsync(targetId);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Mihaell Alves");
        await repositoryMock.Received(1).GetByIdAsync(targetId);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNull_WhenResumeDoesNotExist()
    {
        // Arrange
        var repositoryMock = Substitute.For<IResumeRepository>();
        
        // Simulamos que o repositório NÃO encontrou o registo
        repositoryMock.GetByIdAsync(Arg.Any<Guid>()).Returns((Resume?)null);

        var useCase = new GetResumeByIdUseCase(repositoryMock);

        // Act
        var result = await useCase.ExecuteAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }
}