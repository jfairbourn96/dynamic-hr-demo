using System.Globalization;
using Dynamic.Employees.Application.Interfaces;
using Dynamic.Employees.Application.Models;
using Dynamic.Employees.Domain.Models;
using Dynamic.Json.EfCore.Querying;
using Dynamic.Json.Search;
using Microsoft.EntityFrameworkCore;

namespace Dynamic.Employees.Data.Repositories;

/// <inheritdoc/>
public class EfEmployeeRepository(BaseEmployeeDbContext context) :
    IEmployeeSearchRepository,
    IEmployeeReader,
    IEmployeeWriter
{
    /// <inheritdoc/>
    public async Task<EmployeeSearchResult> SearchAsync(EmployeeSearchCriteria criteria)
    {
        IQueryable<Employee> query = context.Employee
            .AsNoTracking()
            .Include(e => e.EmployeeType)
                .ThenInclude(et => et!.Fields);

        if (criteria.EmployeeTypeId.HasValue)
        {
            query = query.Where(e => e.EmployeeTypeId == criteria.EmployeeTypeId.Value);
        }

        query = ApplyCoreFilters(query, criteria);
        query = ApplyDynamicFilters(query, criteria.DynamicFilters);

        int totalCount = await query.CountAsync();
        List<Employee> page = await query
            .Skip((criteria.PageNumber - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync();

        return new EmployeeSearchResult(
            page.Select(ToSearchItem).ToList(),
            totalCount,
            criteria.PageNumber,
            criteria.PageSize);
    }

    /// <inheritdoc/>
    public async Task<Employee?> GetByIdAsync(Guid id)
    {
        return await context.Employee
            .Include(e => e.EmployeeType)
                .ThenInclude(et => et!.Fields)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    /// <inheritdoc/>
    public async Task AddAsync(Employee employee)
    {
        await context.Employee.AddAsync(employee);
        await context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(Employee employee)
    {
        context.Employee.Update(employee);
        await context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateFieldAsync(Guid id, string fieldName, System.Text.Json.Nodes.JsonNode? value)
    {
        Employee? employee = await context.Employee.FindAsync(id);

        if (employee is null)
        {
            return false;
        }

        employee.FieldValues[fieldName] = value;
        employee.UpdatedDate = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return true;
    }

    private static IQueryable<Employee> ApplyCoreFilters(
        IQueryable<Employee> query,
        EmployeeSearchCriteria criteria)
    {
        foreach (EmployeeTextSearchFilter filter in criteria.TextFilters)
        {
            query = ApplyCoreTextFilter(query, filter);
        }

        if (!string.IsNullOrWhiteSpace(criteria.Email))
        {
            string pattern = BuildLikePattern(criteria.Email, SearchOperator.Contains);
            query = query.Where(e => EF.Functions.Like(e.Email, pattern, @"\"));
        }

        if (criteria.HireDateStart.HasValue)
        {
            query = query.Where(e => e.HireDate >= criteria.HireDateStart.Value);
        }

        if (criteria.HireDateEnd.HasValue)
        {
            query = query.Where(e => e.HireDate <= criteria.HireDateEnd.Value);
        }

        return query;
    }

    private static IQueryable<Employee> ApplyCoreTextFilter(
        IQueryable<Employee> query,
        EmployeeTextSearchFilter filter)
    {
        string propertyName = ToPropertyName(filter.FieldName);

        if (filter.Operator == SearchOperator.Exact)
        {
            return query.Where(e => EF.Property<string?>(e, propertyName) == filter.Value);
        }

        string pattern = BuildLikePattern(filter.Value, filter.Operator);
        return query.Where(e => EF.Functions.Like(EF.Property<string>(e, propertyName), pattern, @"\"));
    }

    private static IQueryable<Employee> ApplyDynamicFilters(
        IQueryable<Employee> query,
        IEnumerable<DynamicSearchFilter> filters)
    {
        foreach (DynamicSearchFilter filter in filters)
        {
            string path = ToJsonPath(filter.FieldName);

            query = filter.FieldType switch
            {
                DynamicSearchFieldType.Text => ApplyDynamicTextFilter(query, path, filter),
                DynamicSearchFieldType.Number => ApplyDynamicNumberFilter(query, path, filter),
                DynamicSearchFieldType.Date => ApplyDynamicDateFilter(query, path, filter),
                DynamicSearchFieldType.Boolean => ApplyDynamicBooleanFilter(query, path, filter),
                DynamicSearchFieldType.Select => ApplyDynamicSelectFilter(query, path, filter),
                _ => query,
            };
        }

        return query;
    }

    private static IQueryable<Employee> ApplyDynamicTextFilter(
        IQueryable<Employee> query,
        string path,
        DynamicSearchFilter filter)
    {
        if (filter.Operator == SearchOperator.Exact)
        {
            return query.Where(e => DynamicJsonFunctions.Value(e.FieldValues, path) == filter.Value);
        }

        string pattern = BuildLikePattern(filter.Value, filter.Operator);
        return query.Where(e => EF.Functions.Like(DynamicJsonFunctions.Value(e.FieldValues, path)!, pattern, @"\"));
    }

    private static IQueryable<Employee> ApplyDynamicNumberFilter(
        IQueryable<Employee> query,
        string path,
        DynamicSearchFilter filter)
    {
        decimal number = decimal.Parse(filter.Value, NumberStyles.Number, CultureInfo.InvariantCulture);

        return filter.Operator switch
        {
            SearchOperator.LessThan => query.Where(e => DynamicJsonFunctions.ValueDecimal(e.FieldValues, path) < number),
            SearchOperator.LessThanOrEqual => query.Where(e => DynamicJsonFunctions.ValueDecimal(e.FieldValues, path) <= number),
            SearchOperator.GreaterThan => query.Where(e => DynamicJsonFunctions.ValueDecimal(e.FieldValues, path) > number),
            SearchOperator.GreaterThanOrEqual => query.Where(e => DynamicJsonFunctions.ValueDecimal(e.FieldValues, path) >= number),
            _ => query.Where(e => DynamicJsonFunctions.ValueDecimal(e.FieldValues, path) == number),
        };
    }

    private static IQueryable<Employee> ApplyDynamicDateFilter(
        IQueryable<Employee> query,
        string path,
        DynamicSearchFilter filter)
    {
        DateOnly date = DateOnly.Parse(filter.Value, CultureInfo.InvariantCulture);

        return filter.Operator switch
        {
            SearchOperator.StartDate => query.Where(e => DynamicJsonFunctions.ValueDate(e.FieldValues, path) >= date),
            SearchOperator.EndDate => query.Where(e => DynamicJsonFunctions.ValueDate(e.FieldValues, path) <= date),
            _ => query,
        };
    }

    private static IQueryable<Employee> ApplyDynamicBooleanFilter(
        IQueryable<Employee> query,
        string path,
        DynamicSearchFilter filter)
    {
        bool boolValue = bool.Parse(filter.Value);
        string jsonValue = boolValue ? "true" : "false";

        return query.Where(e => DynamicJsonFunctions.Value(e.FieldValues, path) == jsonValue);
    }

    private static IQueryable<Employee> ApplyDynamicSelectFilter(
        IQueryable<Employee> query,
        string path,
        DynamicSearchFilter filter)
    {
        return query.Where(e => DynamicJsonFunctions.Value(e.FieldValues, path) == filter.Value);
    }

    private static string BuildLikePattern(string value, SearchOperator searchOperator)
    {
        string escapedValue = EscapeLikePattern(value.Trim());

        return searchOperator switch
        {
            SearchOperator.StartsWith => $"{escapedValue}%",
            _ => $"%{escapedValue}%",
        };
    }

    private static string EscapeLikePattern(string value)
    {
        return value
            .Replace(@"\", @"\\", StringComparison.Ordinal)
            .Replace("%", @"\%", StringComparison.Ordinal)
            .Replace("_", @"\_", StringComparison.Ordinal)
            .Replace("[", @"\[", StringComparison.Ordinal);
    }

    private static string ToJsonPath(string fieldName) => "$." + fieldName;

    private static EmployeeSearchItem ToSearchItem(Employee employee)
    {
        return new EmployeeSearchItem(
            employee.Id,
            employee.FirstName,
            employee.LastName,
            employee.Email,
            employee.HireDate,
            employee.EndDate,
            employee.Department,
            employee.EmployeeTypeId,
            employee.EmployeeType,
            employee.CreatedDate,
            employee.UpdatedDate,
            employee.FieldValues);
    }

    private static string ToPropertyName(string queryFieldName)
    {
        return queryFieldName switch
        {
            "firstName" => nameof(Employee.FirstName),
            "lastName" => nameof(Employee.LastName),
            "department" => nameof(Employee.Department),
            _ => throw new ArgumentOutOfRangeException(nameof(queryFieldName), queryFieldName, null),
        };
    }
}
