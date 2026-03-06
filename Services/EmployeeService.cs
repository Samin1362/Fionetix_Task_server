using Microsoft.EntityFrameworkCore;
using FionetixAPI.Data;
using FionetixAPI.DTOs;
using FionetixAPI.Models;

namespace FionetixAPI.Services;

public class EmployeeService
{
    private readonly AppDbContext _db;

    public EmployeeService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<EmployeeListItemDto>> SearchAsync(string? search)
    {
        var query = _db.Employees
            .Include(e => e.Spouse)
            .Include(e => e.Children)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(e =>
                EF.Functions.ILike(e.Name, $"%{term}%") ||
                EF.Functions.ILike(e.NID, $"%{term}%") ||
                EF.Functions.ILike(e.Department, $"%{term}%"));
        }

        return await query
            .OrderBy(e => e.Name)
            .Select(e => new EmployeeListItemDto
            {
                Id = e.Id,
                Name = e.Name,
                NID = e.NID,
                Department = e.Department,
                BasicSalary = e.BasicSalary,
                Phone = e.Phone,
                HasSpouse = e.Spouse != null,
                ChildrenCount = e.Children.Count
            })
            .ToListAsync();
    }

    public async Task<EmployeeDetailDto?> GetByIdAsync(int id)
    {
        var employee = await _db.Employees
            .Include(e => e.Spouse)
            .Include(e => e.Children)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null) return null;

        return MapToDetail(employee);
    }

    public async Task<(EmployeeDetailDto? result, string? error)> CreateAsync(CreateEmployeeDto dto)
    {
        // Check duplicate NID
        if (await _db.Employees.AnyAsync(e => e.NID == dto.NID))
            return (null, "An employee with this NID already exists.");

        var employee = new Employee
        {
            Name = dto.Name,
            NID = dto.NID,
            Phone = dto.Phone,
            Department = dto.Department,
            BasicSalary = dto.BasicSalary
        };

        if (dto.Spouse != null)
        {
            employee.Spouse = new Spouse
            {
                Name = dto.Spouse.Name,
                NID = dto.Spouse.NID
            };
        }

        if (dto.Children is { Count: > 0 })
        {
            foreach (var childDto in dto.Children)
            {
                employee.Children.Add(new Child
                {
                    Name = childDto.Name,
                    DateOfBirth = childDto.DateOfBirth
                });
            }
        }

        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();

        // Reload with nav properties
        var created = await GetByIdAsync(employee.Id);
        return (created, null);
    }

    public async Task<(EmployeeDetailDto? result, string? error)> UpdateAsync(int id, UpdateEmployeeDto dto)
    {
        var employee = await _db.Employees.FindAsync(id);
        if (employee == null) return (null, "NOT_FOUND");

        // Check duplicate NID (exclude current employee)
        if (await _db.Employees.AnyAsync(e => e.NID == dto.NID && e.Id != id))
            return (null, "An employee with this NID already exists.");

        employee.Name = dto.Name;
        employee.NID = dto.NID;
        employee.Phone = dto.Phone;
        employee.Department = dto.Department;
        employee.BasicSalary = dto.BasicSalary;

        await _db.SaveChangesAsync();
        return (await GetByIdAsync(id), null);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var employee = await _db.Employees.FindAsync(id);
        if (employee == null) return false;

        _db.Employees.Remove(employee);
        await _db.SaveChangesAsync();
        return true;
    }

    // --- Spouse operations ---

    public async Task<(SpouseDto? result, string? error)> UpsertSpouseAsync(int employeeId, UpsertSpouseDto dto)
    {
        var employee = await _db.Employees.Include(e => e.Spouse).FirstOrDefaultAsync(e => e.Id == employeeId);
        if (employee == null) return (null, "NOT_FOUND");

        if (employee.Spouse != null)
        {
            employee.Spouse.Name = dto.Name;
            employee.Spouse.NID = dto.NID;
        }
        else
        {
            employee.Spouse = new Spouse
            {
                Name = dto.Name,
                NID = dto.NID
            };
        }

        await _db.SaveChangesAsync();

        return (new SpouseDto
        {
            Id = employee.Spouse.Id,
            Name = employee.Spouse.Name,
            NID = employee.Spouse.NID
        }, null);
    }

    public async Task<(bool success, string? error)> DeleteSpouseAsync(int employeeId)
    {
        var spouse = await _db.Spouses.FirstOrDefaultAsync(s => s.EmployeeId == employeeId);
        if (spouse == null)
        {
            var exists = await _db.Employees.AnyAsync(e => e.Id == employeeId);
            return exists ? (true, null) : (false, "NOT_FOUND");
        }

        _db.Spouses.Remove(spouse);
        await _db.SaveChangesAsync();
        return (true, null);
    }

    // --- Children operations ---

    public async Task<(ChildDto? result, string? error)> AddChildAsync(int employeeId, CreateChildDto dto)
    {
        if (!await _db.Employees.AnyAsync(e => e.Id == employeeId))
            return (null, "NOT_FOUND");

        var child = new Child
        {
            EmployeeId = employeeId,
            Name = dto.Name,
            DateOfBirth = dto.DateOfBirth
        };

        _db.Children.Add(child);
        await _db.SaveChangesAsync();

        return (new ChildDto { Id = child.Id, Name = child.Name, DateOfBirth = child.DateOfBirth }, null);
    }

    public async Task<(ChildDto? result, string? error)> UpdateChildAsync(int employeeId, int childId, UpdateChildDto dto)
    {
        var child = await _db.Children.FirstOrDefaultAsync(c => c.Id == childId && c.EmployeeId == employeeId);
        if (child == null) return (null, "NOT_FOUND");

        child.Name = dto.Name;
        child.DateOfBirth = dto.DateOfBirth;

        await _db.SaveChangesAsync();
        return (new ChildDto { Id = child.Id, Name = child.Name, DateOfBirth = child.DateOfBirth }, null);
    }

    public async Task<(bool success, string? error)> DeleteChildAsync(int employeeId, int childId)
    {
        var child = await _db.Children.FirstOrDefaultAsync(c => c.Id == childId && c.EmployeeId == employeeId);
        if (child == null) return (false, "NOT_FOUND");

        _db.Children.Remove(child);
        await _db.SaveChangesAsync();
        return (true, null);
    }

    // --- Helpers ---

    private static EmployeeDetailDto MapToDetail(Employee e)
    {
        return new EmployeeDetailDto
        {
            Id = e.Id,
            Name = e.Name,
            NID = e.NID,
            Phone = e.Phone,
            Department = e.Department,
            BasicSalary = e.BasicSalary,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt,
            Spouse = e.Spouse != null ? new SpouseDto
            {
                Id = e.Spouse.Id,
                Name = e.Spouse.Name,
                NID = e.Spouse.NID
            } : null,
            Children = e.Children.Select(c => new ChildDto
            {
                Id = c.Id,
                Name = c.Name,
                DateOfBirth = c.DateOfBirth
            }).ToList()
        };
    }
}
