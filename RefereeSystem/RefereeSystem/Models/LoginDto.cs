// RefereeSystem.Client/Models/LoginDto.cs
public class LoginDto
{
    // Ważne: Musi być { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}