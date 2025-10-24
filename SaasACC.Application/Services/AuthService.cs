using Microsoft.IdentityModel.Tokens;
using SaasACC.Application.Interfaces;
using SaasACC.Model.Entities;
using SaasACC.Model.Servicios.Login;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace SaasACC.Application.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    string GenerateJwtTokenAsync(Usuario usuario);
}

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUsuarioRepository usuarioRepository, IConfiguration configuration)
    {
        _usuarioRepository = usuarioRepository;
        _configuration = configuration;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            // Buscar usuario por email
            var usuario = await _usuarioRepository.GetByEmailAsync(request.Email);
            
            if (usuario == null)
            {
                return new LoginResponse
                {
                    Success = false,
                    UserNotFound = true,
                    ErrorMessage = "Usuario no encontrado. ¿Desea crear una nueva cuenta?"
                };
            }

            // Validar contraseña
            var isValidPassword = await _usuarioRepository.ValidatePasswordAsync(request.Email, request.Password);
            
            if (!isValidPassword)
            {
                return new LoginResponse
                {
                    Success = false,
                    ErrorMessage = "Credenciales inválidas"
                };
            }

            // Generar token JWT
            var token = GenerateJwtTokenAsync(usuario);

            // Actualizar último acceso
            await _usuarioRepository.UpdateLastAccessAsync(usuario.Id);

            return new LoginResponse
            {
                Success = true,
                Token = token,
                Role = usuario.Rol,
                ComercioId = usuario.ComercioId,
                UserName = usuario.Nombre,
                ErrorMessage = string.Empty
            };
        }
        catch (Exception ex)
        {
            return new LoginResponse
            {
                Success = false,
                ErrorMessage = $"Error interno: {ex.Message}"
            };
        }
    }

    public string GenerateJwtTokenAsync(Usuario usuario)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
        var jwtAudience = _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");
        var jwtExpiryHours = int.Parse(_configuration["Jwt:ExpiryHours"] ?? "24");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Name, usuario.Nombre),
            new Claim(ClaimTypes.Role, usuario.Rol),
            new Claim("ComercioId", usuario.ComercioId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, 
                new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), 
                ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(jwtExpiryHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
