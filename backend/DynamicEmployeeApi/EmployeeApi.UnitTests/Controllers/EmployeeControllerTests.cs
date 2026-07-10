using Dynamic.Employees.Application.Commands;
using Dynamic.Employees.Application.Models;
using Dynamic.Employees.Application.Services;
using Dynamic.Employees.Domain.Models;
using EmployeeApi.Controllers;
using EmployeeApi.Requests;
using EmployeeApi.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EmployeeApi.UnitTests.Controllers;

public class EmployeeControllerTests
{
    private readonly Mock<IEmployeeService> _service = new();

    [Fact]
    public async Task Search_WhenFiltersAreValid_ReturnsMappedPageAndPassesQueryParameters()
    {
        Employee employee = CreateEmployee();
        EmployeeSearchResult searchResult = new(
            [ToSearchItem(employee)], 1, 1, 20);
        IReadOnlyDictionary<string, string?>? capturedParameters = null;
        _service
            .Setup(service => service.SearchAsync(
                employee.EmployeeTypeId, 1, 20,
                It.IsAny<IReadOnlyDictionary<string, string?>>(),
                It.IsAny<CancellationToken>()))
            .Callback<Guid?, int, int, IReadOnlyDictionary<string, string?>, CancellationToken>(
                (_, _, _, parameters, _) => capturedParameters = parameters)
            .ReturnsAsync(EmployeeSearchServiceResult.Success(searchResult));
        EmployeeController controller = CreateController("?firstName_exact=Poppy");

        IActionResult action = await controller.Search(employee.EmployeeTypeId);

        OkObjectResult ok = action.Should().BeOfType<OkObjectResult>().Subject;
        EmployeeSearchResponse response = ok.Value.Should().BeOfType<EmployeeSearchResponse>().Subject;
        response.Items.Should().ContainSingle(item => item.Id == employee.Id);
        capturedParameters!["firstName_exact"].Should().Be("Poppy");
    }

    [Fact]
    public async Task Search_WhenFiltersAreInvalid_ReturnsBadRequest()
    {
        _service
            .Setup(service => service.SearchAsync(
                null, 1, 20, It.IsAny<IReadOnlyDictionary<string, string?>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmployeeSearchServiceResult.Failure(["Invalid filter."]));
        EmployeeController controller = CreateController();

        IActionResult action = await controller.Search(null);

        action.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_WhenValuesAreValid_ReturnsCreatedEmployeeIdAndMapsRequest()
    {
        Guid id = Guid.NewGuid();
        CreateEmployeeCommand? captured = null;
        _service
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateEmployeeCommand>(), It.IsAny<CancellationToken>()))
            .Callback<CreateEmployeeCommand, CancellationToken>((command, _) => captured = command)
            .ReturnsAsync(EmployeeMutationServiceResult.Success(new Employee { Id = id }));
        EmployeeController controller = CreateController();
        CreateEmployeeRequest request = CreateRequest();

        IActionResult action = await controller.Create(request, default);

        CreatedAtActionResult created = action.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(EmployeeController.GetById));
        created.Value.Should().Be(id);
        captured!.Email.Should().Be(request.Email);
        captured.EmployeeTypeId.Should().Be(request.EmployeeTypeId);
    }

    [Fact]
    public async Task Create_WhenValuesAreInvalid_ReturnsBadRequest()
    {
        _service
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateEmployeeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmployeeMutationServiceResult.Failure(["Invalid employee."]));
        EmployeeController controller = CreateController();

        IActionResult action = await controller.Create(CreateRequest(), default);

        action.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetById_WhenEmployeeExists_ReturnsMappedResponse()
    {
        Employee employee = CreateEmployee();
        _service.Setup(service => service.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        EmployeeController controller = CreateController();

        IActionResult action = await controller.GetById(employee.Id, default);

        EmployeeResponse response = action.Should().BeOfType<OkObjectResult>().Subject.Value
            .Should().BeOfType<EmployeeResponse>().Subject;
        response.Id.Should().Be(employee.Id);
        response.CreatedAt.Should().Be(employee.CreatedDate);
    }

    [Fact]
    public async Task GetById_WhenEmployeeDoesNotExist_ReturnsNotFound()
    {
        Guid id = Guid.NewGuid();
        _service.Setup(service => service.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);
        EmployeeController controller = CreateController();

        IActionResult action = await controller.GetById(id, default);

        action.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Update_WhenEmployeeDoesNotExist_ReturnsNotFound()
    {
        _service
            .Setup(service => service.UpdateAsync(
                It.IsAny<Guid>(), It.IsAny<UpdateEmployeeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmployeeMutationServiceResult.Missing());
        EmployeeController controller = CreateController();

        IActionResult action = await controller.Update(
            Guid.NewGuid(), CreateUpdateRequest(), default);

        action.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Update_WhenValuesAreInvalid_ReturnsBadRequest()
    {
        _service
            .Setup(service => service.UpdateAsync(
                It.IsAny<Guid>(), It.IsAny<UpdateEmployeeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmployeeMutationServiceResult.Failure(["Invalid employee."]));
        EmployeeController controller = CreateController();

        IActionResult action = await controller.Update(
            Guid.NewGuid(), CreateUpdateRequest(), default);

        action.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_WhenValuesAreValid_ReturnsMappedEmployee()
    {
        Employee employee = CreateEmployee();
        UpdateEmployeeCommand? captured = null;
        _service
            .Setup(service => service.UpdateAsync(
                employee.Id, It.IsAny<UpdateEmployeeCommand>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, UpdateEmployeeCommand, CancellationToken>((_, command, _) => captured = command)
            .ReturnsAsync(EmployeeMutationServiceResult.Success(employee));
        EmployeeController controller = CreateController();
        UpdateEmployeeRequest request = CreateUpdateRequest();

        IActionResult action = await controller.Update(employee.Id, request, default);

        EmployeeResponse response = action.Should().BeOfType<OkObjectResult>().Subject.Value
            .Should().BeOfType<EmployeeResponse>().Subject;
        response.Id.Should().Be(employee.Id);
        captured!.FirstName.Should().Be(request.FirstName);
    }

    private EmployeeController CreateController(string queryString = "")
    {
        DefaultHttpContext context = new();
        context.Request.QueryString = new QueryString(queryString);
        return new EmployeeController(_service.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = context },
        };
    }

    private static CreateEmployeeRequest CreateRequest() => new()
    {
        FirstName = "Poppy",
        LastName = "Troll",
        Email = "poppy@trolls.example",
        HireDate = new DateOnly(2016, 11, 4),
        Department = "Pop Village",
        EmployeeTypeId = Guid.NewGuid(),
    };

    private static UpdateEmployeeRequest CreateUpdateRequest()
    {
        CreateEmployeeRequest create = CreateRequest();
        return new UpdateEmployeeRequest
        {
            FirstName = create.FirstName,
            LastName = create.LastName,
            Email = create.Email,
            HireDate = create.HireDate,
            Department = create.Department,
            EmployeeTypeId = create.EmployeeTypeId,
        };
    }

    private static Employee CreateEmployee() => new()
    {
        Id = Guid.NewGuid(),
        FirstName = "Poppy",
        LastName = "Troll",
        Email = "poppy@trolls.example",
        HireDate = new DateOnly(2016, 11, 4),
        Department = "Pop Village",
        EmployeeTypeId = Guid.NewGuid(),
        CreatedDate = DateTime.UtcNow.AddDays(-1),
        UpdatedDate = DateTime.UtcNow,
    };

    private static EmployeeSearchItem ToSearchItem(Employee employee) => new(
        employee.Id, employee.FirstName, employee.LastName, employee.Email,
        employee.HireDate, employee.EndDate, employee.Department, employee.EmployeeTypeId,
        employee.EmployeeType, employee.CreatedDate, employee.UpdatedDate, employee.FieldValues);
}
