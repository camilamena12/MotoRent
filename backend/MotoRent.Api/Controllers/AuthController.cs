using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MotoRent.Api.Models;

namespace MotoRent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<Usuario> _userManager;
    private readonly SignInManager<Usuario> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<Usuario> userManager,
        SignInManager<Usuario> signInManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        // Buscar al usuario por correo
        var usuario = await _userManager.FindByEmailAsync(request.Email);

        if (usuario == null)
            return Unauthorized("Correo o contraseña incorrectos.");

        // Verificar contraseña y controlar intentos fallidos
        var resultado = await _signInManager.CheckPasswordSignInAsync(
            usuario, request.Password, lockoutOnFailure: true);

        if (!resultado.Succeeded)
            return Unauthorized("Correo o contraseña incorrectos.");

        // Obtener los roles del usuario
        var roles = await _userManager.GetRolesAsync(usuario);

        // Información que llevará el token
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var rol in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, rol));
        }

        // Generar el token JWT
        var clave = _configuration["Jwt:Key"]
            ?? throw new Exception("Falta la clave JWT.");

        var llave = new SymmetricSecurityKey(
            Convert.FromBase64String(clave)
        );

        var credenciales = new SigningCredentials(
            llave, SecurityAlgorithms.HmacSha256
        );

        var vencimiento = DateTime.UtcNow.AddHours(2);

        var token = new JwtSecurityToken(
            issuer: "MotoRent.Api",
            audience: "MotoRent.Frontend",
            claims: claims,
            expires: vencimiento,
            signingCredentials: credenciales
        );

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            email = usuario.Email,
            roles,
            clienteId = usuario.ClienteId,
            expiraEn = vencimiento
        });
    }
}