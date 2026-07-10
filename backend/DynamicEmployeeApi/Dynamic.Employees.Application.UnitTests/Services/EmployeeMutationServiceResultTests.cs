using Dynamic.Employees.Application.Services;
using Dynamic.Employees.Domain.Models;
using FluentAssertions;
using FluentAssertions.Execution;

namespace Dynamic.Employees.Application.UnitTests.Services;

public class EmployeeMutationServiceResultTests
{
    [Fact]
    public void Success_WhenEmployeeIsProvided_CreatesValidResult()
    {
        Employee employee = new() { Id = Guid.NewGuid() };

        EmployeeMutationServiceResult result = EmployeeMutationServiceResult.Success(employee);

        using (new AssertionScope())
        {
            result.IsValid.Should().BeTrue();
            result.Employee.Should().BeSameAs(employee);
            result.Errors.Should().BeEmpty();
            result.NotFound.Should().BeFalse();
        }
    }

    [Fact]
    public void Failure_WhenErrorsAreProvided_CreatesInvalidResult()
    {
        string[] errors = ["Invalid value."];

        EmployeeMutationServiceResult result = EmployeeMutationServiceResult.Failure(errors);

        using (new AssertionScope())
        {
            result.IsValid.Should().BeFalse();
            result.Employee.Should().BeNull();
            result.Errors.Should().BeSameAs(errors);
            result.NotFound.Should().BeFalse();
        }
    }

    [Fact]
    public void Missing_WhenCalled_CreatesNotFoundResult()
    {
        EmployeeMutationServiceResult result = EmployeeMutationServiceResult.Missing();

        using (new AssertionScope())
        {
            result.IsValid.Should().BeTrue();
            result.Employee.Should().BeNull();
            result.Errors.Should().BeEmpty();
            result.NotFound.Should().BeTrue();
        }
    }
}
