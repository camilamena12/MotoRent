using Microsoft.AspNetCore.Identity;

namespace MotoRent.Api.Models;

public class Usuario : IdentityUser
{
    public int? ClienteId { get; set; }

    public Cliente? Cliente { get; set; }
}