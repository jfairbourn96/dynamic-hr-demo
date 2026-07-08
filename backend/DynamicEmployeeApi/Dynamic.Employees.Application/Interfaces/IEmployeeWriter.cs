using Dynamic.Employees.Domain.Models;
using System.Text.Json.Nodes;

namespace Dynamic.Employees.Application.Interfaces;

/// <summary>
/// Provides write operations for employees.
/// </summary>
public interface IEmployeeWriter
{
    /// <summary>
    /// Adds a new employee and persists the change.
    /// </summary>
    Task AddAsync(Employee employee);

    /// <summary>
    /// Updates one dynamic field value for an employee and persists the change.
    /// </summary>
    Task<bool> UpdateFieldAsync(Guid id, string fieldName, JsonNode? value);
}
