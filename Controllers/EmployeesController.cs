using Microsoft.AspNetCore.Mvc;
using FionetixAPI.DTOs;
using FionetixAPI.Services;

namespace FionetixAPI.Controllers;

[ApiController]
[Route("api/employees")]
public class EmployeesController : ControllerBase
{
    private readonly EmployeeService _service;

    public EmployeesController(EmployeeService service)
    {
        _service = service;
    }

    private bool IsAdmin => HttpContext.Items["UserRole"]?.ToString() == "Admin";

    private IActionResult Forbidden() =>
        StatusCode(403, new { error = "Admin access required." });

    // GET /api/employees?search=term
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search)
    {
        var employees = await _service.SearchAsync(search);
        return Ok(employees);
    }

    // GET /api/employees/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var employee = await _service.GetByIdAsync(id);
        if (employee == null) return NotFound(new { error = "Employee not found." });
        return Ok(employee);
    }

    // POST /api/employees
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeDto dto)
    {
        if (!IsAdmin) return Forbidden();

        var (result, error) = await _service.CreateAsync(dto);
        if (error != null)
            return Conflict(new { error });

        return CreatedAtAction(nameof(GetById), new { id = result!.Id }, result);
    }

    // PUT /api/employees/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeDto dto)
    {
        if (!IsAdmin) return Forbidden();

        var (result, error) = await _service.UpdateAsync(id, dto);
        if (error == "NOT_FOUND") return NotFound(new { error = "Employee not found." });
        if (error != null) return Conflict(new { error });

        return Ok(result);
    }

    // DELETE /api/employees/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!IsAdmin) return Forbidden();

        var deleted = await _service.DeleteAsync(id);
        if (!deleted) return NotFound(new { error = "Employee not found." });

        return NoContent();
    }

    // PUT /api/employees/{id}/spouse
    [HttpPut("{id:int}/spouse")]
    public async Task<IActionResult> UpsertSpouse(int id, [FromBody] UpsertSpouseDto dto)
    {
        if (!IsAdmin) return Forbidden();

        var (result, error) = await _service.UpsertSpouseAsync(id, dto);
        if (error == "NOT_FOUND") return NotFound(new { error = "Employee not found." });
        if (error != null) return BadRequest(new { error });

        return Ok(result);
    }

    // DELETE /api/employees/{id}/spouse
    [HttpDelete("{id:int}/spouse")]
    public async Task<IActionResult> DeleteSpouse(int id)
    {
        if (!IsAdmin) return Forbidden();

        var (success, error) = await _service.DeleteSpouseAsync(id);
        if (error == "NOT_FOUND") return NotFound(new { error = "Employee not found." });

        return NoContent();
    }

    // POST /api/employees/{id}/children
    [HttpPost("{id:int}/children")]
    public async Task<IActionResult> AddChild(int id, [FromBody] CreateChildDto dto)
    {
        if (!IsAdmin) return Forbidden();

        var (result, error) = await _service.AddChildAsync(id, dto);
        if (error == "NOT_FOUND") return NotFound(new { error = "Employee not found." });
        if (error != null) return BadRequest(new { error });

        return Created($"/api/employees/{id}/children/{result!.Id}", result);
    }

    // PUT /api/employees/{id}/children/{childId}
    [HttpPut("{id:int}/children/{childId:int}")]
    public async Task<IActionResult> UpdateChild(int id, int childId, [FromBody] UpdateChildDto dto)
    {
        if (!IsAdmin) return Forbidden();

        var (result, error) = await _service.UpdateChildAsync(id, childId, dto);
        if (error == "NOT_FOUND") return NotFound(new { error = "Child not found." });
        if (error != null) return BadRequest(new { error });

        return Ok(result);
    }

    // DELETE /api/employees/{id}/children/{childId}
    [HttpDelete("{id:int}/children/{childId:int}")]
    public async Task<IActionResult> DeleteChild(int id, int childId)
    {
        if (!IsAdmin) return Forbidden();

        var (success, error) = await _service.DeleteChildAsync(id, childId);
        if (error == "NOT_FOUND") return NotFound(new { error = "Child not found." });

        return NoContent();
    }
}
