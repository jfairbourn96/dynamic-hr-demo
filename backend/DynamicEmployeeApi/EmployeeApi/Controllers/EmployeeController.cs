using Dynamic.Employees.Application.Services;
using Dynamic.Employees.Domain.Models;
using EmployeeApi.Mappers;
using EmployeeApi.Requests;
using EmployeeApi.Responses;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeApi.Controllers;

/// <summary>
/// Manages employee records and dynamic employee search.
/// </summary>
[ApiController]
[Route("api/employees")]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _service;

    public EmployeeController(IEmployeeService service)
    {
        _service = service;
    }

    /// <summary>Searches employees using core and runtime-defined field filters.</summary>
    /// <returns>A validated page of matching employees, or 400 for invalid filters.</returns>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search(
        [FromQuery] Guid? employeeTypeId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        EmployeeSearchServiceResult result = await _service.SearchAsync(
            employeeTypeId,
            pageNumber,
            pageSize,
            ToParameterDictionary(Request.Query),
            cancellationToken);

        return result.IsValid
            ? Ok(result.SearchResult!.ToResponse())
            : BadRequest(new { result.Errors });
    }

    /// <summary>Creates an employee after validating its dynamic values against its employee type.</summary>
    /// <returns>The new employee identifier, or 400 for invalid employee values.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        EmployeeMutationServiceResult result = await _service.CreateAsync(
            request.ToCreateCommand(),
            cancellationToken);

        if (!result.IsValid)
        {
            return BadRequest(new { result.Errors });
        }

        Guid id = result.Employee!.Id;
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>Returns an employee by identifier.</summary>
    /// <returns>The employee API representation, or 404 when it does not exist.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        Employee? employee = await _service.GetByIdAsync(id, cancellationToken);

        return employee is null
            ? NotFound()
            : Ok(employee.ToResponse());
    }

    /// <summary>Replaces an employee's editable core and dynamic values.</summary>
    /// <returns>The updated API representation, 400 for invalid values, or 404 when not found.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        EmployeeMutationServiceResult result = await _service.UpdateAsync(
            id,
            request.ToUpdateCommand(),
            cancellationToken);

        if (result.NotFound)
        {
            return NotFound();
        }

        return result.IsValid
            ? Ok(result.Employee!.ToResponse())
            : BadRequest(new { result.Errors });
    }

    private static IReadOnlyDictionary<string, string?> ToParameterDictionary(IQueryCollection query)
    {
        return query.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.FirstOrDefault(),
            StringComparer.OrdinalIgnoreCase);
    }
}
