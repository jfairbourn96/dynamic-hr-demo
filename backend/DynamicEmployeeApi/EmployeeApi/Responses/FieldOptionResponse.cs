namespace EmployeeApi.Responses;

/// <summary>
/// A selectable option on a field of type <c>Select</c>.
/// </summary>
public class FieldOptionResponse
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
