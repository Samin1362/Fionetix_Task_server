namespace FionetixAPI.Models;

public class Child
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }

    // Navigation property
    public Employee Employee { get; set; } = null!;
}
