namespace FionetixAPI.DTOs;

public class CreateChildDto
{
    public string Name { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
}

public class UpdateChildDto
{
    public string Name { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
}

public class ChildDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
}
