using System.Globalization;
using Dynamic.Employees.Application.Commands;
using Dynamic.Employees.Application.Interfaces;
using Dynamic.Employees.Application.Models;
using Dynamic.Employees.Domain.Enums;
using Dynamic.Employees.Domain.Models;
using Dynamic.Json.Search;

namespace Dynamic.Employees.Application.Services;

/// <summary>
/// Implements business logic operations for employees.
/// </summary>
public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeSearchRepository _employeeSearchRepository;
    private readonly IEmployeeReader _employeeReader;
    private readonly IEmployeeWriter _employeeWriter;
    private readonly IEmployeeTypeReader _employeeTypeReader;
    private readonly IDynamicSearchQueryParser _dynamicSearchQueryParser;

    private static readonly DynamicSearchQueryParserOptions DynamicSearchParserOptions = CreateDynamicSearchParserOptions();

    public EmployeeService(
        IEmployeeSearchRepository employeeSearchRepository,
        IEmployeeReader employeeReader,
        IEmployeeWriter employeeWriter,
        IEmployeeTypeReader employeeTypeReader,
        IDynamicSearchQueryParser dynamicSearchQueryParser)
    {
        _employeeSearchRepository = employeeSearchRepository;
        _employeeReader = employeeReader;
        _employeeWriter = employeeWriter;
        _employeeTypeReader = employeeTypeReader;
        _dynamicSearchQueryParser = dynamicSearchQueryParser;
    }

    /// <inheritdoc />
    public async Task<EmployeeSearchServiceResult> SearchAsync(
        Guid? employeeTypeId,
        int pageNumber,
        int pageSize,
        IReadOnlyDictionary<string, string?> parameters)
    {
        pageNumber = Math.Max(pageNumber, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        List<string> errors = [];
        EmployeeType? employeeType = null;

        if (employeeTypeId.HasValue)
        {
            employeeType = await _employeeTypeReader.GetByIdAsync(employeeTypeId.Value);

            if (employeeType is null)
            {
                errors.Add("Employee type was not found.");
            }
        }

        IReadOnlyCollection<EmployeeTextSearchFilter> textFilters = GetCoreTextFilters(parameters);
        string? email = GetParameterValue(parameters, "email");
        DateOnly? hireDateStart = GetCoreDateFilter(parameters, "hireDate_startDate", errors);
        DateOnly? hireDateEnd = GetCoreDateFilter(parameters, "hireDate_endDate", errors);
        IReadOnlyCollection<DynamicSearchFilter> dynamicFilters = GetDynamicSearchFilters(parameters, employeeType, errors);

        if (errors.Count > 0)
        {
            return EmployeeSearchServiceResult.Failure(errors);
        }

        EmployeeSearchCriteria criteria = new(
            employeeTypeId,
            textFilters,
            email,
            hireDateStart,
            hireDateEnd,
            dynamicFilters,
            pageNumber,
            pageSize);

        EmployeeSearchResult result = await _employeeSearchRepository.SearchAsync(criteria);

        return EmployeeSearchServiceResult.Success(result);
    }

    /// <inheritdoc />
    public async Task<Employee> CreateAsync(CreateEmployeeCommand command)
    {
        Employee employee = new()
        {
            Id = Guid.NewGuid(),
            FirstName = command.FirstName,
            LastName = command.LastName,
            Email = command.Email,
            HireDate = command.HireDate,
            EndDate = command.EndDate ?? DateOnly.MinValue,
            Department = command.Department,
            EmployeeTypeId = command.EmployeeTypeId,
            FieldValues = command.FieldValues,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow,
        };

        await _employeeWriter.AddAsync(employee);

        return employee;
    }

    /// <inheritdoc />
    public async Task<Employee?> GetByIdAsync(Guid id)
    {
        return await _employeeReader.GetByIdAsync(id);
    }

    /// <inheritdoc />
    public async Task<bool> UpdateFieldAsync(Guid id, UpdateEmployeeFieldCommand command)
    {
        return await _employeeWriter.UpdateFieldAsync(id, command.FieldName, command.Value);
    }

    private static IReadOnlyCollection<EmployeeTextSearchFilter> GetCoreTextFilters(
        IReadOnlyDictionary<string, string?> parameters)
    {
        List<EmployeeTextSearchFilter> filters = [];

        foreach (string fieldName in new[] { "firstName", "lastName", "department" })
        {
            foreach (SearchOperator searchOperator in new[] { SearchOperator.Contains, SearchOperator.StartsWith, SearchOperator.Exact })
            {
                string key = BuildTextQueryKey(fieldName, searchOperator);
                string? value = GetParameterValue(parameters, key);

                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                filters.Add(new EmployeeTextSearchFilter(fieldName, searchOperator, value));
            }
        }

        return filters;
    }

    private static DateOnly? GetCoreDateFilter(
        IReadOnlyDictionary<string, string?> parameters,
        string key,
        List<string> errors)
    {
        string? value = GetParameterValue(parameters, key);

        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date))
        {
            return date;
        }

        errors.Add($"{key} must be a valid date.");
        return null;
    }

    private IReadOnlyCollection<DynamicSearchFilter> GetDynamicSearchFilters(
        IReadOnlyDictionary<string, string?> parameters,
        EmployeeType? employeeType,
        List<string> errors)
    {
        if (employeeType is null)
        {
            if (_dynamicSearchQueryParser.HasDynamicSearchParameters(parameters, DynamicSearchParserOptions))
            {
                errors.Add("Dynamic field filters require a valid employeeTypeId query parameter.");
            }

            return [];
        }

        DynamicSearchFilterParseResult result = _dynamicSearchQueryParser.Parse(
            parameters,
            employeeType.Fields.Select(ToDynamicSearchField),
            DynamicSearchParserOptions);

        errors.AddRange(result.Errors.Select(error => FormatDynamicSearchParseError(error, employeeType.Name)));

        return result.Filters;
    }

    private static string? GetParameterValue(IReadOnlyDictionary<string, string?> parameters, string key)
    {
        return parameters.TryGetValue(key, out string? value)
            ? value?.Trim()
            : null;
    }

    private static string BuildTextQueryKey(string fieldName, SearchOperator searchOperator)
    {
        string suffix = searchOperator switch
        {
            SearchOperator.Contains => "contains",
            SearchOperator.StartsWith => "startsWith",
            SearchOperator.Exact => "exact",
            _ => throw new ArgumentOutOfRangeException(nameof(searchOperator), searchOperator, null),
        };

        return $"{fieldName}_{suffix}";
    }

    private static DynamicSearchField ToDynamicSearchField(EmployeeTypeField field)
    {
        DynamicSearchFieldType fieldType = field.FieldType switch
        {
            FieldType.Text => DynamicSearchFieldType.Text,
            FieldType.Address => DynamicSearchFieldType.Text,
            FieldType.Number => DynamicSearchFieldType.Number,
            FieldType.Date => DynamicSearchFieldType.Date,
            FieldType.Boolean => DynamicSearchFieldType.Boolean,
            FieldType.Select => DynamicSearchFieldType.Select,
            _ => throw new ArgumentOutOfRangeException(nameof(field), field.FieldType, null),
        };

        return new DynamicSearchField(field.Name, fieldType, field.Options.Select(option => option.Value).ToArray());
    }

    private static DynamicSearchQueryParserOptions CreateDynamicSearchParserOptions()
    {
        DynamicSearchQueryParserOptions options = new();

        options.IgnoredKeys.UnionWith(
        [
            "employeeTypeId",
            "pageNumber",
            "pageSize",
            "email",
            "hireDate_startDate",
            "hireDate_endDate",
        ]);

        options.IgnoredKeyPrefixes.UnionWith(["firstName_", "lastName_", "department_"]);

        return options;
    }

    private static string FormatDynamicSearchParseError(
        DynamicSearchParseError error,
        string employeeTypeName)
    {
        return error.Code switch
        {
            DynamicSearchParseErrorCode.UnsupportedSearchParameter =>
                $"Unsupported search parameter '{error.QueryKey}'.",
            DynamicSearchParseErrorCode.InvalidFieldName =>
                $"Dynamic field '{error.FieldName}' is not a valid field name.",
            DynamicSearchParseErrorCode.UnknownField =>
                $"Dynamic field '{error.FieldName}' does not exist on employee type '{employeeTypeName}'.",
            DynamicSearchParseErrorCode.InvalidOperatorForFieldType =>
                $"Search operator '{error.Operator}' is not valid for dynamic field '{error.FieldName}'.",
            DynamicSearchParseErrorCode.InvalidNumberValue =>
                $"Dynamic field '{error.FieldName}' must be a valid number.",
            DynamicSearchParseErrorCode.InvalidDateValue =>
                $"Dynamic field '{error.FieldName}' must be a valid date.",
            DynamicSearchParseErrorCode.InvalidBooleanValue =>
                $"Dynamic field '{error.FieldName}' must be true or false.",
            DynamicSearchParseErrorCode.InvalidSelectOptionValue =>
                $"Dynamic field '{error.FieldName}' has an invalid option value.",
            _ => $"Unsupported search parameter '{error.QueryKey}'.",
        };
    }
}
