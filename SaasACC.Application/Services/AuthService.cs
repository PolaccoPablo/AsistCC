using Microsoft.IdentityModel.Tokens;
using SaasACC.Application.Interfaces;
using SaasACC.Model.Entities;
using SaasACC.Model.Servicios.Login;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace SaasACC.Application.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<RegisterResponse> RegisterComercioAsync(RegisterComercioRequest request);
    Task<RegisterResponse> RegisterClienteAsync(RegisterClienteRequest request);
    string GenerateJwtTokenAsync(Usuario usuario);
}

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IComercioRepository _comercioRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUsuarioRepository usuarioRepository,
        IComercioRepository comercioRepository,
        IClienteRepository clienteRepository,
        IConfiguration configuration)
    {
        _usuarioRepository = usuarioRepository;
        _comercioRepository = comercioRepository;
        _clienteRepository = clienteRepository;
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

    public async Task<RegisterResponse> RegisterComercioAsync(RegisterComercioRequest request)
    {
        try
        {
            // Validar que no exista el email del comercio
            if (await _comercioRepository.EmailExistsAsync(request.EmailComercio))
            {
                return new RegisterResponse
                {
                    Success = false,
                    ErrorMessage = "Ya existe un comercio registrado con este email"
                };
            }

            // Validar que no exista el email del administrador
            var adminExists = await _usuarioRepository.GetByEmailAsync(request.EmailAdmin);
            if (adminExists != null)
            {
                return new RegisterResponse
                {
                    Success = false,
                    ErrorMessage = "Ya existe un usuario registrado con este email"
                };
            }

            // Crear el comercio
            var comercio = new Comercio
            {
                Nombre = request.NombreComercio,
                Email = request.EmailComercio,
                Telefono = request.TelefonoComercio,
                Direccion = request.DireccionComercio,
                NotificacionesEmail = true,
                NotificacionesWhatsApp = false,
                FechaCreacion = DateTime.UtcNow,
                Activo = true
            };

            comercio = await _comercioRepository.CreateAsync(comercio);

            // Crear el usuario administrador
            var usuario = new Usuario
            {
                Nombre = request.NombreAdmin,
                Email = request.EmailAdmin,
                PasswordHash = HashPassword(request.Password),
                Rol = "Admin",
                ComercioId = comercio.Id
            };

            usuario = await _usuarioRepository.CreateAsync(usuario);

            // Generar token JWT
            var token = GenerateJwtTokenAsync(usuario);

            return new RegisterResponse
            {
                Success = true,
                Message = "Comercio registrado exitosamente",
                Token = token,
                ComercioId = comercio.Id
            };
        }
        catch (Exception ex)
        {
            return new RegisterResponse
            {
                Success = false,
                ErrorMessage = $"Error al registrar comercio: {ex.Message}"
            };
        }
    }

    public async Task<RegisterResponse> RegisterClienteAsync(RegisterClienteRequest request)
    {
        try
        {
            // Validar que el comercio existe
            if (!await _comercioRepository.ExistsAsync(request.ComercioId))
            {
                return new RegisterResponse
                {
                    Success = false,
                    ErrorMessage = "El comercio seleccionado no existe"
                };
            }

            // Validar que no exista el email del cliente en este comercio
            if (await _clienteRepository.EmailExistsAsync(request.Email, request.ComercioId))
            {
                return new RegisterResponse
                {
                    Success = false,
                    ErrorMessage = "Ya existe un cliente con este email en el comercio seleccionado"
                };
            }

            // Crear el cliente
            var cliente = new Cliente
            {
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Email = request.Email,
                Telefono = request.Telefono,
                Direccion = request.Direccion,
                DNI = request.DNI,
                ComercioId = request.ComercioId,
                FechaCreacion = DateTime.UtcNow,
                Activo = true
            };

            cliente = await _clienteRepository.CreateAsync(cliente);

            // Si se proporcionó password, crear credenciales de acceso
            // TODO: Implementar sistema de credenciales para clientes si se requiere
            string token = string.Empty;

            return new RegisterResponse
            {
                Success = true,
                Message = "Cliente registrado exitosamente",
                Token = token,
                ComercioId = request.ComercioId,
                ClienteId = cliente.Id
            };
        }
        catch (Exception ex)
        {
            return new RegisterResponse
            {
                Success = false,
                ErrorMessage = $"Error al registrar cliente: {ex.Message}"
            };
        }
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
