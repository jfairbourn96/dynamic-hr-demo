using System.Text.Json.Nodes;
using Dynamic.Employees.Domain.Models;

namespace Dynamic.Employees.Application.Models;

/// <summary>Represents an employee returned by a search.</summary>
public record EmployeeSearchItem(Guid Id, string FirstName, string LastName, string Email,
    DateOnly HireDate, DateOnly? EndDate, string? Department, Guid EmployeeTypeId,
    EmployeeType? EmployeeType, DateTime CreatedDate, DateTime UpdatedDate, JsonObject FieldValues);
