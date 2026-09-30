using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using ResumeCrud.API.Application.Services;
using ResumeCrud.API.Controllers;
using ResumeCrud.API.Domain.Entities;

namespace ResumeCrud.Tests.API;

public class ResumesControllerTests
{
    [Fact]
    public async Task Upload_ShouldReturnCreatedResult_WhenDataIsValid()
    {
        // Arrange
        var mockUseCase = Substitute.For<IUploadResumeUseCase>();
        
        // Simula o retorno esperado do UseCase
        var fakeBytes = new byte[] { 1, 2, 3 };
        var expectedResume = new Resume("Mihaell Alves", "email@teste.com", "11999999999", fakeBytes);
        mockUseCase.ExecuteAsync("Mihaell Alves", Arg.Any<byte[]>()).Returns(expectedResume);

        // Simula um arquivo IFormFile vindo do upload HTTP
        var mockFile = Substitute.For<IFormFile>();
        mockFile.Length.Returns(1024);
        mockFile.FileName.Returns("curriculo.pdf");
        mockFile.ContentType.Returns("application/pdf");

        // O Controller injetará a interface
        var controller = new ResumesController(mockUseCase);

        // Act
        // Simulamos o recebimento via [FromForm]
        var result = await controller.Upload("Mihaell Alves", mockFile);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        
        // Verifica se a API não expôs os bytes do PDF no payload de retorno
        createdResult.Value.Should().BeEquivalentTo(new 
        {
            expectedResume.Id,
            expectedResume.Name,
            expectedResume.Email,
            expectedResume.Phone
        });
    }
}