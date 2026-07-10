using Dynamic.Employees.Application.Models;
using Dynamic.Employees.Application.Services;
using FluentAssertions;
using FluentAssertions.Execution;

namespace Dynamic.Employees.Application.UnitTests.Services;

public class EmployeeSearchServiceResultTests
{
    [Fact]
    public void Success_WhenSearchResultIsProvided_CreatesValidResult()
    {
        EmployeeSearchResult searchResult = new([], 0, 1, 20);

        EmployeeSearchServiceResult result = EmployeeSearchServiceResult.Success(searchResult);

        using (new AssertionScope())
        {
            result.IsValid.Should().BeTrue();
            result.SearchResult.Should().BeSameAs(searchResult);
            result.Errors.Should().BeEmpty();
        }
    }

    [Fact]
    public void Failure_WhenErrorsAreProvided_CreatesInvalidResult()
    {
        string[] errors = ["Invalid filter."];

        EmployeeSearchServiceResult result = EmployeeSearchServiceResult.Failure(errors);

        using (new AssertionScope())
        {
            result.IsValid.Should().BeFalse();
            result.SearchResult.Should().BeNull();
            result.Errors.Should().BeSameAs(errors);
        }
    }
}
