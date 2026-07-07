using AutoFixture;
using AutoFixture.AutoMoq;
using Dynamic.Employees.Application.Interfaces;
using Dynamic.Employees.Application.Models;
using Dynamic.Employees.Application.Services;
using Dynamic.Employees.Domain.Enums;
using Dynamic.Employees.Domain.Models;
using Dynamic.Json.Search;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;

namespace Dynamic.Employees.Application.UnitTests.Services;

public class EmployeeServiceSearchTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IEmployeeSearchRepository> _employeeSearchRepository;
    private readonly Mock<IEmployeeTypeReader> _employeeTypeReader;
    private readonly Mock<IDynamicSearchQueryParser> _dynamicSearchQueryParser;

    public EmployeeServiceSearchTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _employeeSearchRepository = _fixture.Freeze<Mock<IEmployeeSearchRepository>>();
        _employeeTypeReader = _fixture.Freeze<Mock<IEmployeeTypeReader>>();
        _dynamicSearchQueryParser = _fixture.Freeze<Mock<IDynamicSearchQueryParser>>();
    }

    [Fact]
    public async Task SearchAsync_WhenPageValuesAreOutOfRange_ClampsPagination()
    {
        // Arrange
        EmployeeSearchResult expectedResult = new([], 0, 1, 100);
        EmployeeSearchCriteria? capturedCriteria = null;

        SetupSuccessfulSearch(expectedResult, criteria => capturedCriteria = criteria);

        EmployeeService service = _fixture.Create<EmployeeService>();

        // Act
        EmployeeSearchServiceResult result = await service.SearchAsync(
            employeeTypeId: null,
            pageNumber: 0,
            pageSize: 500,
            parameters: new Dictionary<string, string?>());

        // Assert
        using (new AssertionScope())
        {
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
            result.SearchResult.Should().BeSameAs(expectedResult);

            capturedCriteria.Should().NotBeNull();
            capturedCriteria!.PageNumber.Should().Be(1);
            capturedCriteria.PageSize.Should().Be(100);
        }

        _employeeSearchRepository.Verify(
            repository => repository.SearchAsync(It.IsAny<EmployeeSearchCriteria>()),
            Times.Once);
    }

    [Fact]
    public async Task SearchAsync_WhenCoreFiltersAreProvided_BuildsCoreSearchCriteria()
    {
        // Arrange
        EmployeeSearchResult expectedResult = new([], 0, 1, 100);
        EmployeeSearchCriteria? capturedCriteria = null;

        SetupSuccessfulSearch(expectedResult, criteria => capturedCriteria = criteria);

        EmployeeService service = _fixture.Create<EmployeeService>();

        Dictionary<string, string?> parameters = new()
        {
            ["firstName_contains"] = "  poppy ",
            ["department_exact"] = "Pop Village",
            ["email"] = "  poppy@trolls.example ",
            ["hireDate_startDate"] = "2016-11-04",
            ["hireDate_endDate"] = "2023-11-17",
        };

        // Act
        EmployeeSearchServiceResult result = await service.SearchAsync(
            employeeTypeId: null,
            pageNumber: 1,
            pageSize: 25,
            parameters);

        // Assert
        using (new AssertionScope())
        {
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
            result.SearchResult.Should().BeSameAs(expectedResult);

            capturedCriteria.Should().NotBeNull();

            EmployeeSearchCriteria criteria = capturedCriteria!;
            criteria.EmployeeTypeId.Should().BeNull();
            criteria.Email.Should().Be("poppy@trolls.example");
            criteria.HireDateStart.Should().Be(new DateOnly(2016, 11, 4));
            criteria.HireDateEnd.Should().Be(new DateOnly(2023, 11, 17));
            criteria.DynamicFilters.Should().BeEmpty();

            criteria.TextFilters.Should().BeEquivalentTo(
            [
                new EmployeeTextSearchFilter("firstName", SearchOperator.Contains, "poppy"),
                new EmployeeTextSearchFilter("department", SearchOperator.Exact, "Pop Village"),
            ]);
        }

        _dynamicSearchQueryParser.Verify(
            parser => parser.Parse(
                It.IsAny<IReadOnlyDictionary<string, string?>>(),
                It.IsAny<IEnumerable<DynamicSearchField>>(),
                It.IsAny<DynamicSearchQueryParserOptions?>()),
            Times.Never);

        _employeeSearchRepository.Verify(
            repository => repository.SearchAsync(It.IsAny<EmployeeSearchCriteria>()),
            Times.Once);
    }

    [Fact]
    public async Task SearchAsync_WhenEmployeeTypeHasFields_PassesDynamicSearchFieldsToParser()
    {
        // Arrange
        Guid employeeTypeId = Guid.NewGuid();
        EmployeeSearchResult expectedResult = new([], 0, 1, 100);
        EmployeeSearchCriteria? capturedCriteria = null;
        IEnumerable<DynamicSearchField>? capturedDynamicSearchFields = null;
        IReadOnlyList<DynamicSearchFilter> parsedDynamicFilters =
        [
            new DynamicSearchFilter(
                "favoriteSongName",
                DynamicSearchFieldType.Text,
                SearchOperator.Contains,
                "feeling"),
            new DynamicSearchFilter(
                "numberOfSongs",
                DynamicSearchFieldType.Number,
                SearchOperator.GreaterThanOrEqual,
                "3"),
            new DynamicSearchFilter(
                "movieVersion",
                DynamicSearchFieldType.Select,
                SearchOperator.Exact,
                "band-together-2023"),
        ];

        _employeeTypeReader
            .Setup(reader => reader.GetByIdAsync(employeeTypeId))
            .ReturnsAsync(CreateTrollsTourPerformer(employeeTypeId));

        _dynamicSearchQueryParser
            .Setup(parser => parser.Parse(
                It.IsAny<IReadOnlyDictionary<string, string?>>(),
                It.IsAny<IEnumerable<DynamicSearchField>>(),
                It.IsAny<DynamicSearchQueryParserOptions?>()))
            .Callback<IReadOnlyDictionary<string, string?>, IEnumerable<DynamicSearchField>, DynamicSearchQueryParserOptions?>(
                (_, fields, _) => capturedDynamicSearchFields = fields.ToList())
            .Returns(new DynamicSearchFilterParseResult(parsedDynamicFilters, []));

        SetupSuccessfulSearch(expectedResult, criteria => capturedCriteria = criteria);

        EmployeeService service = _fixture.Create<EmployeeService>();

        Dictionary<string, string?> parameters = new()
        {
            ["favoriteSongName_contains"] = "feeling",
            ["numberOfSongs_gte"] = "3",
            ["movieVersion"] = "band-together-2023",
        };

        // Act
        EmployeeSearchServiceResult result = await service.SearchAsync(
            employeeTypeId,
            pageNumber: 1,
            pageSize: 25,
            parameters);

        // Assert
        using (new AssertionScope())
        {
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
            result.SearchResult.Should().BeSameAs(expectedResult);

            capturedCriteria.Should().NotBeNull();
            capturedCriteria!.EmployeeTypeId.Should().Be(employeeTypeId);
            capturedCriteria.DynamicFilters.Should().BeEquivalentTo(parsedDynamicFilters);

            capturedDynamicSearchFields.Should().BeEquivalentTo(
            [
                new DynamicSearchField("favoriteSongName", DynamicSearchFieldType.Text),
                new DynamicSearchField("numberOfSongs", DynamicSearchFieldType.Number),
                new DynamicSearchField(
                    "movieVersion",
                    DynamicSearchFieldType.Select,
                    ["trolls-2016", "world-tour-2020", "band-together-2023"]),
            ]);
        }

        _dynamicSearchQueryParser.Verify(
            parser => parser.Parse(
                parameters,
                It.IsAny<IEnumerable<DynamicSearchField>>(),
                It.IsAny<DynamicSearchQueryParserOptions?>()),
            Times.Once);

        _employeeSearchRepository.Verify(
            repository => repository.SearchAsync(It.IsAny<EmployeeSearchCriteria>()),
            Times.Once);
    }

    [Fact]
    public async Task SearchAsync_WhenDynamicFiltersAreUsedWithoutEmployeeType_ReturnsValidationError()
    {
        // Arrange
        EmployeeService service = _fixture.Create<EmployeeService>();

        Dictionary<string, string?> parameters = new()
        {
            ["movieVersion"] = "world-tour-2020",
        };

        _dynamicSearchQueryParser
            .Setup(parser => parser.HasDynamicSearchParameters(
                parameters,
                It.IsAny<DynamicSearchQueryParserOptions?>()))
            .Returns(true);

        // Act
        EmployeeSearchServiceResult result = await service.SearchAsync(
            employeeTypeId: null,
            pageNumber: 1,
            pageSize: 25,
            parameters);

        // Assert
        result.IsValid.Should().BeFalse();
        result.SearchResult.Should().BeNull();
        result.Errors.Should().Equal(
            ["Dynamic field filters require a valid employeeTypeId query parameter."]
        );

        _employeeSearchRepository.Verify(
            repository => repository.SearchAsync(It.IsAny<EmployeeSearchCriteria>()),
            Times.Never);

        _dynamicSearchQueryParser.Verify(
            parser => parser.Parse(
                It.IsAny<IReadOnlyDictionary<string, string?>>(),
                It.IsAny<IEnumerable<DynamicSearchField>>(),
                It.IsAny<DynamicSearchQueryParserOptions?>()),
            Times.Never);
    }

    [Fact]
    public async Task SearchAsync_WhenCoreDateFilterIsInvalid_ReturnsValidationError()
    {
        // Arrange
        EmployeeService service = _fixture.Create<EmployeeService>();

        Dictionary<string, string?> parameters = new()
        {
            ["hireDate_startDate"] = "world-tour-release-ish",
        };

        // Act
        EmployeeSearchServiceResult result = await service.SearchAsync(
            employeeTypeId: null,
            pageNumber: 1,
            pageSize: 25,
            parameters);

        // Assert
        result.IsValid.Should().BeFalse();
        result.SearchResult.Should().BeNull();
        result.Errors.Should().Equal(["hireDate_startDate must be a valid date."]);

        _employeeSearchRepository.Verify(
            repository => repository.SearchAsync(It.IsAny<EmployeeSearchCriteria>()),
            Times.Never);
    }

    private void SetupSuccessfulSearch(
        EmployeeSearchResult result,
        Action<EmployeeSearchCriteria> captureCriteria)
    {
        _employeeSearchRepository
            .Setup(repository => repository.SearchAsync(It.IsAny<EmployeeSearchCriteria>()))
            .Callback(captureCriteria)
            .ReturnsAsync(result);
    }

    private static EmployeeType CreateTrollsTourPerformer(Guid employeeTypeId)
    {
        return new EmployeeType
        {
            Id = employeeTypeId,
            Name = "Trolls Tour Performer",
            Fields =
            [
                new EmployeeTypeField { Name = "favoriteSongName", FieldType = FieldType.Text },
                new EmployeeTypeField { Name = "numberOfSongs", FieldType = FieldType.Number },
                new EmployeeTypeField
                {
                    Name = "movieVersion",
                    FieldType = FieldType.Select,
                    Options =
                    [
                        new FieldOption { Label = "Trolls (2016)", Value = "trolls-2016" },
                        new FieldOption { Label = "Trolls World Tour (2020)", Value = "world-tour-2020" },
                        new FieldOption { Label = "Trolls Band Together (2023)", Value = "band-together-2023" },
                    ],
                },
            ],
        };
    }
}
