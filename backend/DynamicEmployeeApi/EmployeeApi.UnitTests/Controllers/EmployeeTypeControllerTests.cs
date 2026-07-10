using Dynamic.Employees.Application.Commands;
using Dynamic.Employees.Application.Services;
using Dynamic.Employees.Domain.Models;
using EmployeeApi.Controllers;
using EmployeeApi.Requests;
using EmployeeApi.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EmployeeApi.UnitTests.Controllers;

public class EmployeeTypeControllerTests
{
    private readonly Mock<IEmployeeTypeService> _service = new();

    [Fact]
    public async Task GetAll_WhenTypesExist_ReturnsMappedResponses()
    {
        EmployeeType type = CreateType();
        _service.Setup(service => service.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([type]);
        EmployeeTypeController controller = new(_service.Object);

        IActionResult action = await controller.GetAll(default);

        List<EmployeeTypeResponse> response = action.Should().BeOfType<OkObjectResult>().Subject.Value
            .Should().BeOfType<List<EmployeeTypeResponse>>().Subject;
        response.Should().ContainSingle(item => item.Id == type.Id.ToString());
    }

    [Fact]
    public async Task GetById_WhenTypeExists_ReturnsMappedResponse()
    {
        EmployeeType type = CreateType();
        _service.Setup(service => service.GetByIdAsync(type.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(type);
        EmployeeTypeController controller = new(_service.Object);

        IActionResult action = await controller.GetById(type.Id, default);

        EmployeeTypeResponse response = action.Should().BeOfType<OkObjectResult>().Subject.Value
            .Should().BeOfType<EmployeeTypeResponse>().Subject;
        response.Name.Should().Be(type.Name);
    }

    [Fact]
    public async Task GetById_WhenTypeDoesNotExist_ReturnsNotFound()
    {
        Guid id = Guid.NewGuid();
        _service.Setup(service => service.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmployeeType?)null);
        EmployeeTypeController controller = new(_service.Object);

        IActionResult action = await controller.GetById(id, default);

        action.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_WhenRequestIsProvided_MapsCommandAndReturnsCreatedResponse()
    {
        EmployeeType type = CreateType();
        CreateEmployeeTypeCommand? captured = null;
        _service
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateEmployeeTypeCommand>(), It.IsAny<CancellationToken>()))
            .Callback<CreateEmployeeTypeCommand, CancellationToken>((command, _) => captured = command)
            .ReturnsAsync(type);
        EmployeeTypeController controller = new(_service.Object);
        CreateEmployeeTypeRequest request = new() { Name = type.Name, Description = "Description" };

        IActionResult action = await controller.Create(request, default);

        CreatedAtActionResult created = action.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(EmployeeTypeController.GetById));
        created.Value.Should().BeOfType<EmployeeTypeResponse>();
        captured!.Name.Should().Be(request.Name);
    }

    [Fact]
    public async Task Update_WhenTypeExists_MapsCommandAndReturnsResponse()
    {
        EmployeeType type = CreateType();
        UpdateEmployeeTypeCommand? captured = null;
        _service
            .Setup(service => service.UpdateAsync(
                type.Id, It.IsAny<UpdateEmployeeTypeCommand>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, UpdateEmployeeTypeCommand, CancellationToken>((_, command, _) => captured = command)
            .ReturnsAsync(type);
        EmployeeTypeController controller = new(_service.Object);
        UpdateEmployeeTypeRequest request = new() { Name = "Updated Type" };

        IActionResult action = await controller.Update(type.Id, request, default);

        action.Should().BeOfType<OkObjectResult>();
        captured!.Name.Should().Be("Updated Type");
    }

    [Fact]
    public async Task Update_WhenTypeDoesNotExist_ReturnsNotFound()
    {
        _service
            .Setup(service => service.UpdateAsync(
                It.IsAny<Guid>(), It.IsAny<UpdateEmployeeTypeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmployeeType?)null);
        EmployeeTypeController controller = new(_service.Object);

        IActionResult action = await controller.Update(
            Guid.NewGuid(), new UpdateEmployeeTypeRequest { Name = "Missing" }, default);

        action.Should().BeOfType<NotFoundResult>();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Delete_WhenCalled_ReturnsStatusMatchingServiceResult(bool deleted)
    {
        Guid id = Guid.NewGuid();
        _service.Setup(service => service.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(deleted);
        EmployeeTypeController controller = new(_service.Object);

        IActionResult action = await controller.Delete(id, default);

        if (deleted)
        {
            action.Should().BeOfType<NoContentResult>();
        }
        else
        {
            action.Should().BeOfType<NotFoundResult>();
        }
    }

    private static EmployeeType CreateType() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Trolls Tour Performer",
        CreatedDate = DateTime.UtcNow.AddDays(-1),
        UpdatedDate = DateTime.UtcNow,
    };
}
