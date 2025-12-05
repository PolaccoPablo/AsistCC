# Plan de Refactorización: Cliente como Relación Muchos-a-Muchos

## 📋 Objetivo
Permitir que un Usuario pueda ser Cliente de múltiples Comercios, usando la entidad `Cliente` como tabla intermedia.

---

## 🔄 Cambios en el Modelo de Datos

### ANTES (Modelo Actual)
```
Usuario 1:N Cliente
  - Un Usuario puede tener muchos Clientes (pero solo uno está vinculado)
  - Restricción: Email único en Usuario (global)
  - Restricción: ComercioId+Email único en Cliente
  - Problema: No se puede crear otro Usuario con el mismo email
```

### DESPUÉS (Modelo Propuesto)
```
Usuario 1:N Cliente N:1 Comercio
  - Un Usuario puede tener múltiples Clientes
  - Cada Cliente representa la vinculación Usuario-Comercio
  - Restricción: Email único en Usuario (global) ✅
  - Restricción: UsuarioId+ComercioId único en Cliente ✅
  - Ventaja: Mismo usuario puede estar en múltiples comercios
```

---

## 📊 Cambios en Entidades

### 1. Usuario (sin cambios estructurales)
```csharp
public class Usuario : BaseEntity
{
    public string Nombre { get; set; }
    public string? Apellido { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string Rol { get; set; } = "Usuario";

    // QUITAR: public int ComercioId { get; set; }
    // RAZON: Usuario ya no "pertenece" a un comercio específico

    public DateTime? UltimoAcceso { get; set; }

    // Navegación
    // QUITAR: public virtual Comercio Comercio { get; set; }
    public virtual ICollection<Cliente> Clientes { get; set; } // NUEVO
    public virtual ICollection<Cliente> ClientesAprobados { get; set; }
}
```

**⚠️ IMPACTO**: Los usuarios Admin y UsuarioComercio TAMBIÉN tendrán esta relación.
**SOLUCIÓN**: Mantener ComercioId SOLO para usuarios tipo Admin/UsuarioComercio

### 2. Cliente (cambios críticos)
```csharp
public class Cliente : BaseEntity
{
    // QUITAR campos duplicados de Usuario
    // QUITAR: public string Nombre { get; set; }
    // QUITAR: public string Apellido { get; set; }
    // QUITAR: public string Email { get; set; }
    // QUITAR: public string Telefono { get; set; }
    // QUITAR: public string? Direccion { get; set; }
    // QUITAR: public string? DNI { get; set; }

    // MANTENER: Relaciones
    public int UsuarioId { get; set; } // REQUIRED (no nullable)
    public int ComercioId { get; set; }

    // MANTENER: Estado y aprobación
    public int EstadoId { get; set; }
    public int OrigenRegistro { get; set; }
    public DateTime? FechaAprobacion { get; set; }
    public int? AprobadoPorUsuarioId { get; set; }

    // NUEVO: Datos específicos de esta vinculación (opcional)
    public string? Alias { get; set; } // Ej: "Juan el del taller"
    public string? NotasComercio { get; set; } // Notas privadas del comercio

    // Navegación
    public virtual Usuario Usuario { get; set; } = null!;
    public virtual Comercio Comercio { get; set; } = null!;
    public virtual CuentaCorriente? CuentaCorriente { get; set; }
    public virtual EstadoCliente Estado { get; set; } = null!;
    public virtual Usuario? AprobadoPor { get; set; }
}
```

**⚠️ PROBLEMA**: Cliente ya no tiene Nombre, Apellido, Email propios
**SOLUCIÓN**: Usar propiedades calculadas que obtienen del Usuario

```csharp
// Propiedades calculadas (NO mapeadas a BD)
[NotMapped]
public string Nombre => Usuario?.Nombre ?? string.Empty;

[NotMapped]
public string Apellido => Usuario?.Apellido ?? string.Empty;

[NotMapped]
public string Email => Usuario?.Email ?? string.Empty;

[NotMapped]
public string NombreCompleto => $"{Nombre} {Apellido}".Trim();
```

---

## 🗄️ Cambios en ApplicationDbContext

### ConfigurarUsuario
```csharp
private static void ConfigurarUsuario(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Usuario>(entity =>
    {
        entity.ToTable("Usuarios");
        entity.HasKey(u => u.Id);

        entity.Property(u => u.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(150);

        entity.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        entity.Property(u => u.Rol)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Usuario");

        // QUITAR relación con Comercio para clientes
        // MANTENER ComercioId solo para Admin/UsuarioComercio
        entity.Property(u => u.ComercioId).IsRequired(false); // NULLABLE

        entity.HasOne(u => u.Comercio)
            .WithMany(c => c.Usuarios)
            .HasForeignKey(u => u.ComercioId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false); // NULLABLE

        // Email único GLOBAL
        entity.HasIndex(u => u.Email).IsUnique();
    });
}
```

### ConfigurarCliente
```csharp
private static void ConfigurarCliente(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Cliente>(entity =>
    {
        entity.ToTable("Clientes");
        entity.HasKey(c => c.Id);

        // QUITAR columnas de datos personales
        // MANTENER solo relaciones y estado

        entity.Property(c => c.OrigenRegistro)
            .IsRequired()
            .HasDefaultValue(1);

        entity.Property(c => c.Alias)
            .HasMaxLength(100);

        entity.Property(c => c.NotasComercio)
            .HasMaxLength(500);

        // Relación Usuario → Cliente (1:N)
        entity.HasOne(c => c.Usuario)
            .WithMany(u => u.Clientes)
            .HasForeignKey(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(true); // REQUIRED

        // Relación Comercio → Cliente (1:N)
        entity.HasOne(c => c.Comercio)
            .WithMany(co => co.Clientes)
            .HasForeignKey(c => c.ComercioId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación 1:1 con CuentaCorriente
        entity.HasOne(c => c.CuentaCorriente)
            .WithOne(cc => cc.Cliente)
            .HasForeignKey<CuentaCorriente>(cc => cc.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación con EstadoCliente
        entity.HasOne(c => c.Estado)
            .WithMany(e => e.Clientes)
            .HasForeignKey(c => c.EstadoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Aprobación
        entity.HasOne(c => c.AprobadoPor)
            .WithMany(u => u.ClientesAprobados)
            .HasForeignKey(c => c.AprobadoPorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        // ÍNDICE ÚNICO: Un usuario solo puede ser cliente una vez por comercio
        entity.HasIndex(c => new { c.UsuarioId, c.ComercioId }).IsUnique();

        // QUITAR: entity.HasIndex(c => new { c.ComercioId, c.Email }).IsUnique();
        // Ya no tiene sentido porque Cliente no tiene Email propio

        entity.HasIndex(c => c.EstadoId);
    });
}
```

---

## 🔧 Migración de Datos

### Script de Migración
```sql
-- 1. Hacer nullable el ComercioId en Usuario (para clientes)
ALTER TABLE Usuarios
ALTER COLUMN ComercioId INT NULL;

-- 2. Hacer UsuarioId required en Cliente
UPDATE Clientes SET UsuarioId = 0 WHERE UsuarioId IS NULL; -- Temporal
ALTER TABLE Clientes
ALTER COLUMN UsuarioId INT NOT NULL;

-- 3. Quitar índice único de Email+Comercio en Cliente
DROP INDEX IX_Clientes_ComercioId_Email ON Clientes;

-- 4. Crear índice único de Usuario+Comercio en Cliente
CREATE UNIQUE INDEX IX_Clientes_UsuarioId_ComercioId
ON Clientes (UsuarioId, ComercioId);

-- 5. Agregar nuevas columnas
ALTER TABLE Clientes ADD Alias NVARCHAR(100) NULL;
ALTER TABLE Clientes ADD NotasComercio NVARCHAR(500) NULL;

-- 6. Quitar columnas de datos personales de Cliente (CUIDADO!)
-- ANTES de ejecutar esto, asegúrate de que todos los datos estén en Usuario
-- ALTER TABLE Clientes DROP COLUMN Nombre;
-- ALTER TABLE Clientes DROP COLUMN Apellido;
-- ALTER TABLE Clientes DROP COLUMN Email;
-- ALTER TABLE Clientes DROP COLUMN Telefono;
-- ALTER TABLE Clientes DROP COLUMN Direccion;
-- ALTER TABLE Clientes DROP COLUMN DNI;
```

**⚠️ ADVERTENCIA**: No ejecutar DROP COLUMN hasta confirmar que la migración funciona correctamente.

---

## 📝 Cambios en Servicios

### ClienteService - CreateClienteAsync

**ANTES:**
```csharp
public async Task<ClienteDto> CreateClienteAsync(CreateClienteRequest request)
{
    // Validar email de cliente
    if (await _clienteRepository.EmailExistsAsync(request.Email, request.ComercioId))
        throw new InvalidOperationException("Email ya existe");

    // Validar email de usuario
    var usuarioExistente = await _usuarioRepository.GetByEmailAsync(request.Email);
    if (usuarioExistente != null)
        throw new InvalidOperationException("Usuario ya existe");

    // Crear usuario
    var usuario = new Usuario { ... };
    usuario = await _usuarioRepository.CreateAsync(usuario);

    // Crear cliente
    var cliente = new Cliente { UsuarioId = usuario.Id, ... };
    ...
}
```

**DESPUÉS:**
```csharp
public async Task<ClienteDto> CreateClienteAsync(CreateClienteRequest request)
{
    Usuario usuario;

    // Verificar si el usuario ya existe
    var usuarioExistente = await _usuarioRepository.GetByEmailAsync(request.Email);

    if (usuarioExistente != null)
    {
        // Usuario existe: verificar que no esté ya vinculado a este comercio
        var yaEsCliente = await _clienteRepository.ExisteVinculoAsync(
            usuarioExistente.Id, request.ComercioId);

        if (yaEsCliente)
            throw new InvalidOperationException(
                "Este usuario ya es cliente de este comercio");

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
            PasswordHash = HashPassword(request.DNI),
            Rol = "Cliente",
            ComercioId = null, // NULL para clientes
            FechaCreacion = DateTime.UtcNow,
            Activo = true
        };

        usuario = await _usuarioRepository.CreateAsync(usuario);
    }

    // Crear vínculo Cliente
    var cliente = new Cliente
    {
        UsuarioId = usuario.Id,
        ComercioId = request.ComercioId,
        EstadoId = 2, // Activo
        OrigenRegistro = 1, // Administración
        Activo = true,
        FechaCreacion = DateTime.UtcNow,
        FechaAprobacion = DateTime.UtcNow
    };

    var clienteCreado = await _clienteRepository.CreateAsync(cliente);
    await _clienteRepository.CrearCuentaCorrienteAsync(clienteCreado.Id);

    var clienteConCuenta = await _clienteRepository.GetByIdAsync(clienteCreado.Id);
    return MapToDto(clienteConCuenta!);
}
```

### AuthService - RegisterClienteAsync (Autogestión)

**CAMBIOS:**
```csharp
public async Task<RegisterResponse> RegisterClienteAsync(RegisterClienteRequest request)
{
    // Validar que comercio existe
    var comercio = await _comercioRepository.GetByIdAsync(request.ComercioId);
    if (comercio == null)
        throw new InvalidOperationException("Comercio no encontrado");

    Usuario usuario;

    // Verificar si usuario existe
    var usuarioExistente = await _usuarioRepository.GetByEmailAsync(request.Email);

    if (usuarioExistente != null)
    {
        // Usuario existe: verificar contraseña
        if (!await _usuarioRepository.ValidatePasswordAsync(request.Email, request.Password))
            throw new InvalidOperationException("Contraseña incorrecta");

        // Verificar que no esté ya vinculado a este comercio
        var yaEsCliente = await _clienteRepository.ExisteVinculoAsync(
            usuarioExistente.Id, request.ComercioId);

        if (yaEsCliente)
            throw new InvalidOperationException(
                "Ya estás registrado en este comercio");

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
            ComercioId = null, // NULL para clientes
            FechaCreacion = DateTime.UtcNow,
            Activo = true
        };

        usuario = await _usuarioRepository.CreateAsync(usuario);
    }

    // Crear vinculación (pendiente de aprobación)
    var cliente = new Cliente
    {
        UsuarioId = usuario.Id,
        ComercioId = request.ComercioId,
        EstadoId = 1, // Pendiente
        OrigenRegistro = 2, // Autogestión
        FechaCreacion = DateTime.UtcNow,
        Activo = true
    };

    await _clienteRepository.CreateAsync(cliente);

    return new RegisterResponse
    {
        Success = true,
        Message = "Solicitud enviada. Pendiente de aprobación por el comercio.",
        RequiereAprobacion = true
    };
}
```

### AuthService - LoginAsync

**CAMBIOS IMPORTANTES:**
```csharp
public async Task<LoginResponse> LoginAsync(LoginRequest request)
{
    var usuario = await _usuarioRepository.GetByEmailAsync(request.Email);
    if (usuario == null)
        throw new UnauthorizedAccessException("Credenciales inválidas");

    if (!await _usuarioRepository.ValidatePasswordAsync(request.Email, request.Password))
        throw new UnauthorizedAccessException("Credenciales inválidas");

    if (usuario.Rol == "Cliente")
    {
        // NUEVO: Cliente puede tener múltiples vinculaciones
        var clientes = await _clienteRepository.GetByUsuarioIdAsync(usuario.Id);
        var clientesActivos = clientes.Where(c => c.EstadoId == 2).ToList();

        if (clientesActivos.Count == 0)
        {
            // No tiene ningún comercio activo
            var clientesPendientes = clientes.Where(c => c.EstadoId == 1).ToList();
            if (clientesPendientes.Any())
                throw new UnauthorizedAccessException(
                    "Tu solicitud está pendiente de aprobación");
            else
                throw new UnauthorizedAccessException("Cuenta inactiva");
        }

        // Generar token con info de comercios
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Rol),
            new Claim("UsuarioId", usuario.Id.ToString())
        };

        // Agregar claim con lista de ComercioIds activos
        claims.Add(new Claim("ComercioIds",
            string.Join(",", clientesActivos.Select(c => c.ComercioId))));

        var token = GenerateJwtToken(claims);

        await _usuarioRepository.UpdateLastAccessAsync(usuario.Id);

        return new LoginResponse
        {
            Token = token,
            RequiereCambioPassword = false,
            Comercios = clientesActivos.Select(c => new ComercioInfo
            {
                Id = c.ComercioId,
                Nombre = c.Comercio.Nombre
            }).ToList()
        };
    }

    // Login de Admin/UsuarioComercio (sin cambios)
    ...
}
```

---

## 🆕 Nuevos Endpoints

### 1. Vincular Usuario a Nuevo Comercio
```csharp
// ClientesController.cs
[HttpPost("vincular")]
[Authorize]
public async Task<ActionResult<ClienteDto>> VincularAComercio(
    [FromBody] VincularComercioRequest request)
{
    try
    {
        var usuarioId = GetUsuarioIdFromToken();
        var resultado = await _clienteService.VincularUsuarioAComercioAsync(
            usuarioId, request.ComercioId, request.RequiereAprobacion);

        return Ok(resultado);
    }
    catch (InvalidOperationException ex)
    {
        return BadRequest(new { message = ex.Message });
    }
}
```

### 2. Obtener Comercios de un Usuario
```csharp
// ClientesController.cs
[HttpGet("mis-comercios")]
[Authorize(Roles = "Cliente")]
public async Task<ActionResult<IEnumerable<MiComercioDto>>> GetMisComercios()
{
    var usuarioId = GetUsuarioIdFromToken();
    var comercios = await _clienteService.GetComerciosDeUsuarioAsync(usuarioId);
    return Ok(comercios);
}
```

### 3. Cambiar Contexto de Comercio
```csharp
// AuthController.cs
[HttpPost("cambiar-comercio")]
[Authorize(Roles = "Cliente")]
public async Task<ActionResult<LoginResponse>> CambiarComercio(
    [FromBody] CambiarComercioRequest request)
{
    var usuarioId = GetUsuarioIdFromToken();
    var token = await _authService.CambiarContextoComercioAsync(
        usuarioId, request.ComercioId);

    return Ok(new { token });
}
```

---

## 📦 Nuevos DTOs

### VincularComercioRequest
```csharp
public class VincularComercioRequest
{
    [Required]
    public int ComercioId { get; set; }

    public bool RequiereAprobacion { get; set; } = true;
}
```

### MiComercioDto
```csharp
public class MiComercioDto
{
    public int ComercioId { get; set; }
    public string NombreComercio { get; set; }
    public int EstadoId { get; set; }
    public string EstadoNombre { get; set; }
    public DateTime FechaVinculacion { get; set; }
    public DateTime? FechaAprobacion { get; set; }
    public decimal Saldo { get; set; }
}
```

### LoginResponse (actualizado)
```csharp
public class LoginResponse
{
    public string Token { get; set; }
    public bool RequiereCambioPassword { get; set; }

    // NUEVO: Lista de comercios si es cliente
    public List<ComercioInfo>? Comercios { get; set; }
}

public class ComercioInfo
{
    public int Id { get; set; }
    public string Nombre { get; set; }
}
```

---

## 🎯 Flujos de Usuario Actualizados

### Flujo 1: Cliente Nuevo se Registra por Autogestión
```
1. Usuario accede a /register/cliente
2. Selecciona Comercio A
3. Ingresa datos + contraseña
4. POST /api/auth/register/cliente
5. Sistema:
   a. Crea Usuario (email único, rol=Cliente)
   b. Crea Cliente (UsuarioId, ComercioId=A, Estado=Pendiente)
6. Cliente recibe: "Pendiente de aprobación"
```

### Flujo 2: Cliente Existente se Vincula a Otro Comercio
```
1. Cliente hace login
2. En su dashboard, ve "Vincularme a otro comercio"
3. Selecciona Comercio B
4. POST /api/clientes/vincular { comercioId: B }
5. Sistema:
   a. Verifica que usuario no esté ya vinculado a B
   b. Crea nuevo Cliente (mismo UsuarioId, ComercioId=B, Estado=Pendiente)
6. Cliente recibe: "Solicitud enviada a Comercio B"
7. Admin de Comercio B aprueba
8. Cliente ahora puede cambiar entre Comercio A y B
```

### Flujo 3: Comercio Agrega Cliente Existente
```
1. Admin de Comercio B accede a "Agregar Cliente"
2. Ingresa email: juan@example.com
3. POST /api/clientes { email: "juan@example.com", comercioId: B }
4. Sistema:
   a. Busca Usuario por email
   b. Si existe: verifica que no esté ya en Comercio B
   c. Si no existe: crea Usuario nuevo
   d. Crea Cliente (vinculación)
   e. Crea CuentaCorriente para Comercio B
5. Cliente queda activo en Comercio B
```

### Flujo 4: Login con Múltiples Comercios
```
1. Cliente hace login con juan@example.com
2. Sistema detecta que tiene 2 vinculaciones activas:
   - Cliente (Comercio A, EstadoId=2)
   - Cliente (Comercio B, EstadoId=2)
3. Retorna token con claim: "ComercioIds: 1,2"
4. Frontend muestra selector de comercio
5. Cliente selecciona Comercio A
6. Frontend usa ComercioId=1 en requests subsiguientes
```

---

## ⚠️ Consideraciones Importantes

### 1. Usuarios Admin y UsuarioComercio
**Problema**: Estos usuarios SÍ pertenecen a un comercio específico
**Solución**: Mantener ComercioId nullable en Usuario
```csharp
// Para Admin y UsuarioComercio
Usuario.ComercioId = 1 // Su comercio

// Para Cliente
Usuario.ComercioId = null // Se relaciona vía Cliente
```

### 2. Datos Personales en Cliente vs Usuario
**Decisión**: Mover TODOS los datos personales a Usuario
**Razón**: Evitar duplicación y mantener consistencia

**Datos en Usuario:**
- Nombre
- Apellido
- Email
- Teléfono (nuevo)
- Dirección (nuevo)
- DNI (nuevo)

**Datos en Cliente (solo vinculación):**
- UsuarioId (required)
- ComercioId
- EstadoId
- OrigenRegistro
- FechaAprobacion
- AprobadoPorUsuarioId
- Alias (opcional)
- NotasComercio (opcional)

### 3. Actualización de Usuario
**Problema**: Si Juan actualiza su teléfono en Comercio A, ¿se actualiza en B?
**Respuesta**: SÍ, porque los datos están en Usuario (único)

**Implementar endpoint:**
```csharp
PUT /api/usuarios/mi-perfil
{
  "nombre": "Juan",
  "apellido": "Pérez",
  "telefono": "123456789"
}
```

### 4. Eliminación de Cliente
**Pregunta**: ¿Eliminar Cliente elimina Usuario?
**Respuesta**: NO
- Eliminar Cliente = soft delete de la vinculación
- Usuario persiste (puede tener otros comercios)
- Solo eliminar Usuario si NO tiene ningún Cliente activo

---

## 📋 Checklist de Implementación

- [ ] 1. Crear backup de base de datos
- [ ] 2. Modificar entidad Usuario (agregar campos de Cliente)
- [ ] 3. Modificar entidad Cliente (quitar campos duplicados)
- [ ] 4. Actualizar DbContext (índices, relaciones)
- [ ] 5. Crear migración de EF Core
- [ ] 6. Ejecutar migración en base de datos
- [ ] 7. Actualizar IClienteRepository (agregar ExisteVinculoAsync)
- [ ] 8. Actualizar ClienteRepository (implementar nuevos métodos)
- [ ] 9. Refactorizar ClienteService.CreateClienteAsync
- [ ] 10. Refactorizar AuthService.RegisterClienteAsync
- [ ] 11. Refactorizar AuthService.LoginAsync
- [ ] 12. Crear VincularUsuarioAComercioAsync en ClienteService
- [ ] 13. Crear GetComerciosDeUsuarioAsync en ClienteService
- [ ] 14. Actualizar ClientesController (nuevos endpoints)
- [ ] 15. Actualizar DTOs (ClienteDto, LoginResponse, etc.)
- [ ] 16. Actualizar frontend (selector de comercio en login)
- [ ] 17. Testear flujos:
  - [ ] Cliente nuevo se registra
  - [ ] Cliente existente se vincula a nuevo comercio
  - [ ] Comercio agrega cliente existente
  - [ ] Login con múltiples comercios
  - [ ] Cambio de contexto entre comercios
- [ ] 18. Actualizar documentación (README.md, resumenDesarrollo.txt)

---

## 🚀 Próximos Pasos

1. **Revisar este plan** y confirmar que es lo que necesitas
2. **Decidir sobre migración de datos**: ¿Preservar clientes existentes?
3. **Implementar cambios** paso a paso
4. **Testear exhaustivamente** antes de producción

---

**Última actualización**: 2025-12-04
**Branch**: hardcore-wright
**Autor**: Claude Code
