using FluentAssertions;
using NSubstitute;
using ResumeCrud.API.Application.DTOs;
using ResumeCrud.API.Application.Services;
using ResumeCrud.API.Domain.Entities;
using ResumeCrud.API.Domain.Repositories;

namespace ResumeCrud.Tests.Application;

public class CreateResumeUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldSaveAndReturnResume_WhenDataIsValid()
    {
        // Arrange
        var repositoryMock = Substitute.For<IResumeRepository>();
        var useCase = new CreateResumeUseCase(repositoryMock);

        var requestDto = new CreateResumeRequestDto
        {
            Name = "Mihaell Alves",
            Email = "mihaell@teste.com",
            Phone = "11999999999",
            AreaOfInterest = "Engenharia de Software",
            ProfessionalSummary = "Especialista em React e .NET"
        };

        // Act
        var result = await useCase.ExecuteAsync(requestDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be(requestDto.Name);
        result.Email.Should().Be(requestDto.Email);
        result.AreaOfInterest.Should().Be(requestDto.AreaOfInterest);
        
        // Verifica se o repositório foi chamado exatamente 1 vez com a entidade correta
        await repositoryMock.Received(1).AddAsync(Arg.Is<Resume>(r => 
            r.Name == requestDto.Name && 
            r.Email == requestDto.Email &&
            r.AreaOfInterest == requestDto.AreaOfInterest
        ));
    }
}