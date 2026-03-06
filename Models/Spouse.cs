namespace FionetixAPI.Models;

public class Spouse
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NID { get; set; }

    // Navigation property
    public Employee Employee { get; set; } = null!;
}
