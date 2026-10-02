using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using ResumeCrud.API.Application.Services;
using ResumeCrud.API.Controllers;
using ResumeCrud.API.Domain.Entities;
using ResumeCrud.API.Application.DTOs;

namespace ResumeCrud.Tests.API;

public class ResumesControllerTests
{
[Fact]
    public async Task Upload_ShouldReturnCreatedResult_WhenDataIsValid()
    {
        // Arrange
        var uploadUseCaseMock = Substitute.For<IUploadResumeUseCase>();
        var getUseCaseMock = Substitute.For<IGetResumesUseCase>();
        
        var fakeBytes = new byte[] { 1, 2, 3 };
        var expectedResume = new Resume("Mihaell Alves", "email@teste.com", "11999999999", fakeBytes);
        uploadUseCaseMock.ExecuteAsync("Mihaell Alves", Arg.Any<byte[]>()).Returns(expectedResume);

        var mockFile = Substitute.For<IFormFile>();
        mockFile.Length.Returns(1024);
        mockFile.FileName.Returns("curriculo.pdf");
        mockFile.ContentType.Returns("application/pdf");
        
        var ms = new MemoryStream(fakeBytes);
        mockFile.OpenReadStream().Returns(ms);

        var controller = new ResumesController(uploadUseCaseMock, getUseCaseMock);

        // Act
        // Chamada limpa passando os dois parâmetros explicitamente
        var result = await controller.Upload("Mihaell Alves", mockFile);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        
        createdResult.Value.Should().BeEquivalentTo(new 
        {
            expectedResume.Id,
            expectedResume.Name,
            expectedResume.Email,
            expectedResume.Phone
        });
    }

    [Fact]
    public async Task Get_ShouldReturnOkWithListOfResumes()
    {
        // Arrange
        var uploadUseCaseMock = Substitute.For<IUploadResumeUseCase>();
        var getUseCaseMock = Substitute.For<IGetResumesUseCase>();

        var mockResumes = new List<ResumeDto>
        {
            new ResumeDto(Guid.NewGuid(), "João Silva", "joao@email.com", "999999999"),
            new ResumeDto(Guid.NewGuid(), "Maria Santos", "maria@email.com", "888888888")
        };

        getUseCaseMock.ExecuteAsync().Returns(mockResumes);

        var controller = new ResumesController(uploadUseCaseMock, getUseCaseMock);

        // Act
        var result = await controller.Get();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedResumes = okResult.Value.Should().BeAssignableTo<IEnumerable<ResumeDto>>().Subject;

        returnedResumes.Should().HaveCount(2);
        await getUseCaseMock.Received(1).ExecuteAsync();
    }
}