using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SaasACC.Application.Interfaces;
using SaasACC.Model.Entities;
using SaasACC.Model.Servicios.Login;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SaasACC.Application.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<RegisterResponse> RegisterComercioAsync(RegisterComercioRequest request);
    Task<RegisterResponse> RegisterClienteAsync(RegisterClienteRequest request);
    Task<ChangePasswordResponse> ChangePasswordAsync(int usuarioId, ChangePasswordRequest request);
    string GenerateJwtTokenAsync(Usuario usuario, List<ComercioInfo>? comercios = null);
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

            // Si es un cliente, obtener sus comercios
            bool requiereCambioPassword = false;
            List<ComercioInfo>? comercios = null;

            if (usuario.Rol == "Cliente")
            {
                // Obtener todos los clientes (vinculaciones) de este usuario
                var clientes = await _clienteRepository.GetByUsuarioIdAsync(usuario.Id);
                var clientesActivos = clientes.Where(c => c.EstadoId == 2).ToList();

                if (clientesActivos.Count == 0)
                {
                    // No tiene ningún comercio activo
                    var clientesPendientes = clientes.Where(c => c.EstadoId == 1).ToList();
                    if (clientesPendientes.Any())
                    {
                        return new LoginResponse
                        {
                            Success = false,
                            ErrorMessage = "Tu cuenta está pendiente de aprobación por el comercio"
                        };
                    }
                    else
                    {
                        return new LoginResponse
                        {
                            Success = false,
                            ErrorMessage = "Tu cuenta ha sido inactivada. Contacta al comercio para más información"
                        };
                    }
                }

                // Verificar si requiere cambio de contraseña:
                // - Nunca ha iniciado sesión (UltimoAcceso == null)
                // - Fue creado desde administración (OrigenRegistro == 1) en al menos un comercio
                if (usuario.UltimoAcceso == null && clientesActivos.Any(c => c.OrigenRegistro == 1))
                {
                    requiereCambioPassword = true;
                }

                // Crear lista de comercios para el response
                comercios = clientesActivos.Select(c => new ComercioInfo
                {
                    Id = c.ComercioId,
                    Nombre = c.Comercio.Nombre
                }).ToList();
            }

            // Generar token JWT
            var token = GenerateJwtTokenAsync(usuario, comercios);

            // Actualizar último acceso solo si no requiere cambio de contraseña
            // (se actualizará después del cambio de contraseña)
            if (!requiereCambioPassword)
            {
                await _usuarioRepository.UpdateLastAccessAsync(usuario.Id);
            }

            return new LoginResponse
            {
                Success = true,
                Token = token,
                Role = usuario.Rol,
                ComercioId = usuario.ComercioId, // Para Admin/UsuarioComercio
                UserName = usuario.Nombre,
                ErrorMessage = string.Empty,
                RequiereCambioPassword = requiereCambioPassword,
                Comercios = comercios // Para clientes con múltiples comercios
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

    public string GenerateJwtTokenAsync(Usuario usuario, List<ComercioInfo>? comercios = null)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
        var jwtAudience = _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");
        var jwtExpiryHours = int.Parse(_configuration["Jwt:ExpiryHours"] ?? "24");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claimsList = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Name, usuario.Nombre),
            new Claim(ClaimTypes.Role, usuario.Rol),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        // Para Admin/UsuarioComercio: agregar ComercioId
        if (usuario.ComercioId.HasValue)
        {
            claimsList.Add(new Claim("ComercioId", usuario.ComercioId.Value.ToString()));
        }

        // Para Cliente: agregar lista de ComercioIds
        if (comercios != null && comercios.Any())
        {
            claimsList.Add(new Claim("ComercioIds",
                string.Join(",", comercios.Select(c => c.Id))));
        }

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claimsList,
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

            // Validar que se proporcionó la contraseña
            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return new RegisterResponse
                {
                    Success = false,
                    ErrorMessage = "La contraseña es requerida para el registro"
                };
            }

            Usuario usuario;

            // Verificar si el usuario ya existe
            var usuarioExistente = await _usuarioRepository.GetByEmailAsync(request.Email);

            if (usuarioExistente != null)
            {
                // Usuario existe: verificar contraseña
                if (!await _usuarioRepository.ValidatePasswordAsync(request.Email, request.Password))
                {
                    return new RegisterResponse
                    {
                        Success = false,
                        ErrorMessage = "Ya existe un usuario con este email pero la contraseña no coincide"
                    };
                }

                // Verificar que no esté ya vinculado a este comercio
                if (await _clienteRepository.ExisteVinculoAsync(usuarioExistente.Id, request.ComercioId))
                {
                    return new RegisterResponse
                    {
                        Success = false,
                        ErrorMessage = "Ya estás registrado en este comercio"
                    };
                }

                usuario = usuarioExistente;
            }
            else
            {
                // Usuario NO existe: crear nuevo
                usuario = new Usuario
                {
                    Nombre = request.Nombre,
                    Apellido = request.Apellido,
                    Email = request.Email,
                    PasswordHash = HashPassword(request.Password),
                    Rol = "Cliente",
                    ComercioId = null, // NULL para clientes (se relacionan vía Cliente)
                    FechaCreacion = DateTime.UtcNow,
                    Activo = true
                };

                usuario = await _usuarioRepository.CreateAsync(usuario);
            }

            // Crear la vinculación Cliente
            var cliente = new Cliente
            {
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Email = request.Email,
                Telefono = request.Telefono ?? string.Empty,
                Direccion = request.Direccion,
                DNI = request.DNI,
                UsuarioId = usuario.Id,
                ComercioId = request.ComercioId,
                EstadoId = 1, // Pendiente de aprobación
                OrigenRegistro = 2, // Autogestión
                FechaCreacion = DateTime.UtcNow,
                Activo = true
            };

            cliente = await _clienteRepository.CreateAsync(cliente);

            // NO se genera token porque el cliente debe ser aprobado primero
            return new RegisterResponse
            {
                Success = true,
                Message = "Registro exitoso. Tu cuenta está pendiente de aprobación por el comercio.",
                Token = string.Empty,
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

    public async Task<ChangePasswordResponse> ChangePasswordAsync(int usuarioId, ChangePasswordRequest request)
    {
        try
        {
            // Validar que las contraseñas coincidan
            if (request.NewPassword != request.ConfirmPassword)
            {
                return new ChangePasswordResponse
                {
                    Success = false,
                    ErrorMessage = "Las contraseñas no coinciden"
                };
            }

            // Validar longitud de la nueva contraseña
            if (request.NewPassword.Length < 6)
            {
                return new ChangePasswordResponse
                {
                    Success = false,
                    ErrorMessage = "La contraseña debe tener al menos 6 caracteres"
                };
            }

            // Obtener usuario
            var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
            if (usuario == null)
            {
                return new ChangePasswordResponse
                {
                    Success = false,
                    ErrorMessage = "Usuario no encontrado"
                };
            }

            // Validar contraseña actual
            var currentPasswordHash = HashPassword(request.CurrentPassword);
            if (usuario.PasswordHash != currentPasswordHash)
            {
                return new ChangePasswordResponse
                {
                    Success = false,
                    ErrorMessage = "La contraseña actual es incorrecta"
                };
            }

            // Actualizar contraseña
            usuario.PasswordHash = HashPassword(request.NewPassword);
            usuario.FechaModificacion = DateTime.UtcNow;

            // Actualizar último acceso (importante para marcar que ya cambió la contraseña)
            await _usuarioRepository.UpdateLastAccessAsync(usuario.Id);

            await _usuarioRepository.UpdateAsync(usuario);

            return new ChangePasswordResponse
            {
                Success = true,
                Message = "Contraseña actualizada correctamente"
            };
        }
        catch (Exception ex)
        {
            return new ChangePasswordResponse
            {
                Success = false,
                ErrorMessage = $"Error al cambiar contraseña: {ex.Message}"
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
