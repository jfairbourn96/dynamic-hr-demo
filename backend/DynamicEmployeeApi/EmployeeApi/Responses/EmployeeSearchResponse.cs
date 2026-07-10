namespace EmployeeApi.Responses;

/// <summary>
/// Represents one page of employees in the external API contract.
/// </summary>
public class EmployeeSearchResponse
{
    public IReadOnlyCollection<EmployeeResponse> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
