using Dynamic.Employees.Application.Services;
using Dynamic.Employees.Domain.Models;
using EmployeeApi.Mappers;
using EmployeeApi.Requests;
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

    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search(
        [FromQuery] Guid? employeeTypeId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        EmployeeSearchServiceResult result = await _service.SearchAsync(
            employeeTypeId,
            pageNumber,
            pageSize,
            ToParameterDictionary(Request.Query));

        if (!result.IsValid)
        {
            return BadRequest(new { result.Errors });
        }

        return Ok(result.SearchResult);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest request)
    {
        Employee employee = await _service.CreateAsync(request.ToCreateCommand());

        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee.Id);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Employee), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        Employee? employee = await _service.GetByIdAsync(id);

        if (employee is null)
        {
            return NotFound();
        }

        return Ok(employee);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Employee), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmployeeRequest request)
    {
        Employee? employee = await _service.UpdateAsync(id, request.ToUpdateCommand());

        if (employee is null)
        {
            return NotFound();
        }

        return Ok(employee);
    }

    [HttpPatch("{id:guid}/field")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateField(Guid id, [FromBody] UpdateEmployeeFieldRequest request)
    {
        if (!await _service.UpdateFieldAsync(id, request.ToUpdateFieldCommand()))
        {
            return NotFound();
        }

        return NoContent();
    }

    private static IReadOnlyDictionary<string, string?> ToParameterDictionary(IQueryCollection query)
    {
        return query.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.FirstOrDefault(),
            StringComparer.OrdinalIgnoreCase);
    }

}
