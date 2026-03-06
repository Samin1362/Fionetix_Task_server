using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FionetixAPI.Data;
using FionetixAPI.Services;

namespace FionetixAPI.Controllers;

[ApiController]
[Route("api/export")]
public class ExportController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly PdfService _pdfService;

    public ExportController(AppDbContext db, PdfService pdfService)
    {
        _db = db;
        _pdfService = pdfService;
    }

    // GET /api/export/pdf-list?search=term
    [HttpGet("pdf-list")]
    public async Task<IActionResult> ExportTablePdf([FromQuery] string? search)
    {
        var query = _db.Employees.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(e =>
                EF.Functions.ILike(e.Name, $"%{term}%") ||
                EF.Functions.ILike(e.NID, $"%{term}%") ||
                EF.Functions.ILike(e.Department, $"%{term}%"));
        }

        var employees = await query.OrderBy(e => e.Name).ToListAsync();
        var pdfBytes = _pdfService.GenerateTablePdf(employees, search);

        return File(pdfBytes, "application/pdf", "employees.pdf");
    }

    // GET /api/export/cv/{id}
    [HttpGet("cv/{id:int}")]
    public async Task<IActionResult> ExportCvPdf(int id)
    {
        var employee = await _db.Employees
            .Include(e => e.Spouse)
            .Include(e => e.Children)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null)
            return NotFound(new { error = "Employee not found." });

        var pdfBytes = _pdfService.GenerateCvPdf(employee);

        return File(pdfBytes, "application/pdf", $"employee_cv_{id}.pdf");
    }
}
