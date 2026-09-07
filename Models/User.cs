namespace gamebox.Models;

public abstract class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public virtual void Login() { }
    public virtual void Logout() { }
}