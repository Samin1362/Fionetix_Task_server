namespace FionetixAPI.DTOs;

public class UpsertSpouseDto
{
    public string Name { get; set; } = string.Empty;
    public string? NID { get; set; }
}

public class SpouseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NID { get; set; }
}
