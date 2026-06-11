using Microsoft.AspNetCore.Identity;

namespace IUE.DesatrasadorMVP.Models;

public class ApplicationUser : IdentityUser
{
    public string NombreCompleto { get; set; } = string.Empty;
}
