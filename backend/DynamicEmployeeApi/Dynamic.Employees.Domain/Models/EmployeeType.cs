namespace Dynamic.Employees.Domain.Models;

/// <summary>
/// Defines an employee classification and its runtime field schema.
/// </summary>
public class EmployeeType
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<EmployeeTypeField> Fields { get; set; } = [];
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}
