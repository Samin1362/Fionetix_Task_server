namespace FionetixAPI.DTOs;

// Request DTOs
public class CreateEmployeeDto
{
    public string Name { get; set; } = string.Empty;
    public string NID { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public UpsertSpouseDto? Spouse { get; set; }
    public List<CreateChildDto>? Children { get; set; }
}

public class UpdateEmployeeDto
{
    public string Name { get; set; } = string.Empty;
    public string NID { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
}

// Response DTOs
public class EmployeeListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NID { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public string Phone { get; set; } = string.Empty;
    public bool HasSpouse { get; set; }
    public int ChildrenCount { get; set; }
}

public class EmployeeDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NID { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public SpouseDto? Spouse { get; set; }
    public List<ChildDto> Children { get; set; } = new();
}
