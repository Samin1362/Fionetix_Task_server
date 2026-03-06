namespace FionetixAPI.Models;

public class AppUser
{
    public int Id { get; set; }
    public string FirebaseUid { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "Viewer"; // "Admin" | "Viewer"
    public DateTime CreatedAt { get; set; }
}
