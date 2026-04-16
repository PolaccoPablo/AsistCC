using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SaasACC.Application.Services;
using SaasACC.Domain.Entities;

namespace SaasACC.Infrastructure.Seeds;

/// <summary>
/// Carga datos de prueba realistas en la base de datos.
/// Solo ejecuta si la tabla Comercios está vacía (idempotente).
/// Contraseña para todos los usuarios: Admin123!
/// </summary>
public static class DbSeeder
{
    private static readonly string _hash = PasswordHasher.Hash("Admin123!");

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (await db.Comercios.AnyAsync())
            return;

        var now = DateTime.UtcNow;

        // ─── 1. COMERCIOS ─────────────────────────────────────────────────────────
        var ferreteria = new Comercio
        {
            Nombre = "Ferretería El Maestro",
            Email = "ferreteria@elmaestro.com.ar",
            Telefono = "0351-456-7890",
            Direccion = "Av. San Martín 1450, Córdoba Capital",
            NotificacionesEmail = true,
            NotificacionesWhatsApp = false,
            FechaCreacion = now.AddMonths(-14)
        };
        var panaderia = new Comercio
        {
            Nombre = "Panadería La Espiga Dorada",
            Email = "info@laespigadorada.com.ar",
            Telefono = "0341-234-5678",
            Direccion = "Rivadavia 234, Rosario, Santa Fe",
            NotificacionesEmail = true,
            NotificacionesWhatsApp = true,
            FechaCreacion = now.AddMonths(-10)
        };
        var libreria = new Comercio
        {
            Nombre = "Librería y Papelería El Saber",
            Email = "contacto@elsaber.com.ar",
            Telefono = "0261-789-0123",
            Direccion = "Av. Belgrano 890, Mendoza Capital",
            NotificacionesEmail = false,
            NotificacionesWhatsApp = true,
            FechaCreacion = now.AddMonths(-7)
        };

        db.Comercios.AddRange(ferreteria, panaderia, libreria);
        await db.SaveChangesAsync();

        // ─── 2. USUARIOS STAFF (Admin / UsuarioComercio) ──────────────────────────
        var adminF = new Usuario { Nombre = "Juan", Apellido = "García", Email = "admin@elmaestro.com.ar", PasswordHash = _hash, Rol = "Admin", ComercioId = ferreteria.Id, FechaCreacion = ferreteria.FechaCreacion, UltimoAcceso = now.AddHours(-2) };
        var empF   = new Usuario { Nombre = "María", Apellido = "López", Email = "empleado@elmaestro.com.ar", PasswordHash = _hash, Rol = "UsuarioComercio", ComercioId = ferreteria.Id, FechaCreacion = now.AddMonths(-12), UltimoAcceso = now.AddHours(-1) };
        var adminP = new Usuario { Nombre = "Carlos", Apellido = "Rodríguez", Email = "admin@laespigadorada.com.ar", PasswordHash = _hash, Rol = "Admin", ComercioId = panaderia.Id, FechaCreacion = panaderia.FechaCreacion, UltimoAcceso = now.AddDays(-1) };
        var adminL = new Usuario { Nombre = "Ana", Apellido = "Martínez", Email = "admin@elsaber.com.ar", PasswordHash = _hash, Rol = "Admin", ComercioId = libreria.Id, FechaCreacion = libreria.FechaCreacion, UltimoAcceso = now.AddHours(-3) };
        var empL   = new Usuario { Nombre = "Pablo", Apellido = "Sánchez", Email = "empleado@elsaber.com.ar", PasswordHash = _hash, Rol = "UsuarioComercio", ComercioId = libreria.Id, FechaCreacion = now.AddMonths(-6), UltimoAcceso = now.AddDays(-3) };

        db.Usuarios.AddRange(adminF, empF, adminP, adminL, empL);
        await db.SaveChangesAsync();

        // ─── 3. USUARIOS CLIENTES (ComercioId = null, Rol = "Cliente") ───────────

        // Ferretería – 10 clientes (8 activos, 1 pendiente, 1 inactivo)
        var uF1  = Cu("Roberto",   "Fernández",  "roberto.fernandez@gmail.com",  now.AddMonths(-13));
        var uF2  = Cu("Claudia",   "Gómez",      "claudia.gomez@hotmail.com",    now.AddMonths(-12));
        var uF3  = Cu("Diego",     "Herrera",    "diego.herrera@gmail.com",      now.AddMonths(-11));
        var uF4  = Cu("Lucía",     "Torres",     "lucia.torres@gmail.com",       now.AddMonths(-10));
        var uF5  = Cu("Martín",    "Pérez",      "martin.perez@hotmail.com",     now.AddMonths(-9));
        var uF6  = Cu("Valeria",   "Castro",     "valeria.castro@gmail.com",     now.AddMonths(-8));
        var uF7  = Cu("Federico",  "Morales",    "federico.morales@gmail.com",   now.AddMonths(-7));
        var uF8  = Cu("Gabriela",  "Ruiz",       "gabriela.ruiz@gmail.com",      now.AddMonths(-6));
        var uF9  = Cu("Sebastián", "Díaz",       "sebastian.diaz@hotmail.com",   now.AddMonths(-2)); // pendiente
        var uF10 = Cu("Patricia",  "Álvarez",    "patricia.alvarez@gmail.com",   now.AddMonths(-8)); // inactivo

        // Panadería – 8 clientes (6 activos, 1 pendiente, 1 inactivo)
        var uP1 = Cu("Jorge",     "Romero",  "jorge.romero@gmail.com",        now.AddMonths(-9));
        var uP2 = Cu("Silvia",    "Núñez",   "silvia.nunez@hotmail.com",      now.AddMonths(-8));
        var uP3 = Cu("Andrés",    "Vega",    "andres.vega@gmail.com",         now.AddMonths(-7));
        var uP4 = Cu("Carolina",  "Medina",  "carolina.medina@gmail.com",     now.AddMonths(-6));
        var uP5 = Cu("Ricardo",   "Jiménez", "ricardo.jimenez@hotmail.com",   now.AddMonths(-5));
        var uP6 = Cu("Natalia",   "Flores",  "natalia.flores@gmail.com",      now.AddMonths(-4));
        var uP7 = Cu("Gustavo",   "Ortiz",   "gustavo.ortiz@gmail.com",       now.AddMonths(-1)); // pendiente
        var uP8 = Cu("Laura",     "Molina",  "laura.molina@gmail.com",        now.AddMonths(-6)); // inactivo

        // Librería – 6 clientes (5 activos, 1 pendiente)
        var uL1 = Cu("Marcelo",   "Vargas",   "marcelo.vargas@gmail.com",    now.AddMonths(-6));
        var uL2 = Cu("Sandra",    "Reyes",    "sandra.reyes@hotmail.com",    now.AddMonths(-5));
        var uL3 = Cu("Alejandro", "Cruz",     "alejandro.cruz@gmail.com",    now.AddMonths(-4));
        var uL4 = Cu("Mónica",    "Benítez",  "monica.benitez@gmail.com",    now.AddMonths(-3));
        var uL5 = Cu("Ernesto",   "Mendoza",  "ernesto.mendoza@hotmail.com", now.AddMonths(-2));
        var uL6 = Cu("Verónica",  "Ríos",     "veronica.rios@gmail.com",     now.AddMonths(-1)); // pendiente

        db.Usuarios.AddRange(
            uF1, uF2, uF3, uF4, uF5, uF6, uF7, uF8, uF9, uF10,
            uP1, uP2, uP3, uP4, uP5, uP6, uP7, uP8,
            uL1, uL2, uL3, uL4, uL5, uL6);
        await db.SaveChangesAsync();

        // ─── 4. CLIENTES ─────────────────────────────────────────────────────────

        // Ferretería – activos
        var cF1  = CA(uF1,  ferreteria.Id, "28456789", "11-5555-1234",  "Palermo, CABA",                    adminF.Id, now.AddMonths(-12), now.AddMonths(-13));
        var cF2  = CA(uF2,  ferreteria.Id, "32145678", "0351-456-7891", "Córdoba Capital",                  adminF.Id, now.AddMonths(-11), now.AddMonths(-12));
        var cF3  = CA(uF3,  ferreteria.Id, "25789012", "11-6666-2345",  "Villa Urquiza, CABA",              empF.Id,   now.AddMonths(-10), now.AddMonths(-11));
        var cF4  = CA(uF4,  ferreteria.Id, "30234567", "0351-789-0123", "Nueva Córdoba",                    adminF.Id, now.AddMonths(-9),  now.AddMonths(-10));
        var cF5  = CA(uF5,  ferreteria.Id, "27890123", "11-7777-3456",  "Caballito, CABA",                  adminF.Id, now.AddMonths(-8),  now.AddMonths(-9));
        var cF6  = CA(uF6,  ferreteria.Id, "33456789", "0351-234-5678", "Barrio Gral. Paz, Córdoba",        empF.Id,   now.AddMonths(-7),  now.AddMonths(-8));
        var cF7  = CA(uF7,  ferreteria.Id, "29012345", "11-8888-4567",  "Flores, CABA",                     adminF.Id, now.AddMonths(-6),  now.AddMonths(-7));
        var cF8  = CA(uF8,  ferreteria.Id, "31678901", "0351-567-8901", "Alta Córdoba",                     adminF.Id, now.AddMonths(-5),  now.AddMonths(-6));
        // Ferretería – pendiente (autogestión)
        var cF9  = new Cliente { Nombre = uF9.Nombre, Apellido = uF9.Apellido!, Email = uF9.Email, Telefono = "11-9999-5678", DNI = "35789012", Direccion = "Flores, CABA", UsuarioId = uF9.Id, ComercioId = ferreteria.Id, EstadoId = 1, OrigenRegistro = 2, FechaCreacion = now.AddMonths(-2), Activo = true };
        // Ferretería – inactivo
        var cF10 = new Cliente { Nombre = uF10.Nombre, Apellido = uF10.Apellido!, Email = uF10.Email, Telefono = "0351-890-1234", DNI = "26345678", Direccion = "Barrio Cofico, Córdoba", UsuarioId = uF10.Id, ComercioId = ferreteria.Id, EstadoId = 3, OrigenRegistro = 1, FechaAprobacion = now.AddMonths(-7), AprobadoPorUsuarioId = adminF.Id, FechaCreacion = now.AddMonths(-8), NotasComercio = "Cliente dado de baja por falta de pago reiterada", Activo = true };

        // Panadería – activos
        var cP1 = CA(uP1, panaderia.Id, "24567890", "0341-123-4567", "Rosario Centro",               adminP.Id, now.AddMonths(-8), now.AddMonths(-9));
        var cP2 = CA(uP2, panaderia.Id, "29876543", "0341-234-5678", "Barrio Las Flores, Rosario",   adminP.Id, now.AddMonths(-7), now.AddMonths(-8));
        var cP3 = CA(uP3, panaderia.Id, "27654321", "0341-345-6789", "Rosario Norte",                adminP.Id, now.AddMonths(-6), now.AddMonths(-7));
        var cP4 = CA(uP4, panaderia.Id, "31234567", "0341-456-7890", "Villa Gobernador Gálvez, SF",  adminP.Id, now.AddMonths(-5), now.AddMonths(-6));
        var cP5 = CA(uP5, panaderia.Id, "26789012", "0341-567-8901", "Funes, Santa Fe",              adminP.Id, now.AddMonths(-4), now.AddMonths(-5));
        var cP6 = CA(uP6, panaderia.Id, "34012345", "0341-678-9012", "Granadero Baigorria, SF",      adminP.Id, now.AddMonths(-3), now.AddMonths(-4));
        // Panadería – pendiente
        var cP7 = new Cliente { Nombre = uP7.Nombre, Apellido = uP7.Apellido!, Email = uP7.Email, Telefono = "0341-789-0123", DNI = "38901234", Direccion = "Rosario Sur", UsuarioId = uP7.Id, ComercioId = panaderia.Id, EstadoId = 1, OrigenRegistro = 2, FechaCreacion = now.AddMonths(-1), Activo = true };
        // Panadería – inactivo
        var cP8 = new Cliente { Nombre = uP8.Nombre, Apellido = uP8.Apellido!, Email = uP8.Email, Telefono = "0341-890-1234", DNI = "28901234", Direccion = "Rosario Este", UsuarioId = uP8.Id, ComercioId = panaderia.Id, EstadoId = 3, OrigenRegistro = 1, FechaAprobacion = now.AddMonths(-5), AprobadoPorUsuarioId = adminP.Id, FechaCreacion = now.AddMonths(-6), NotasComercio = "Cerró el negocio - ya no realiza pedidos", Activo = true };

        // Librería – activos
        var cL1 = CA(uL1, libreria.Id, "30567890", "0261-123-4567", "Ciudad de Mendoza",           adminL.Id, now.AddMonths(-5), now.AddMonths(-6));
        var cL2 = CA(uL2, libreria.Id, "27123456", "0261-234-5678", "Godoy Cruz, Mendoza",         adminL.Id, now.AddMonths(-4), now.AddMonths(-5));
        var cL3 = CA(uL3, libreria.Id, "32890123", "0261-345-6789", "Las Heras, Mendoza",          empL.Id,   now.AddMonths(-3), now.AddMonths(-4));
        var cL4 = CA(uL4, libreria.Id, "28345678", "0261-456-7890", "Guaymallén, Mendoza",         adminL.Id, now.AddMonths(-2), now.AddMonths(-3));
        var cL5 = CA(uL5, libreria.Id, "25901234", "0261-567-8901", "Maipú, Mendoza",              empL.Id,   now.AddMonths(-1), now.AddMonths(-2));
        // Librería – pendiente
        var cL6 = new Cliente { Nombre = uL6.Nombre, Apellido = uL6.Apellido!, Email = uL6.Email, Telefono = "0261-678-9012", DNI = "36678901", Direccion = "Luján de Cuyo, Mendoza", UsuarioId = uL6.Id, ComercioId = libreria.Id, EstadoId = 1, OrigenRegistro = 2, FechaCreacion = now.AddMonths(-1), Activo = true };

        db.Clientes.AddRange(
            cF1, cF2, cF3, cF4, cF5, cF6, cF7, cF8, cF9, cF10,
            cP1, cP2, cP3, cP4, cP5, cP6, cP7, cP8,
            cL1, cL2, cL3, cL4, cL5, cL6);
        await db.SaveChangesAsync();

        // ─── 5. CUENTAS CORRIENTES (solo clientes Activo y Inactivo) ─────────────

        // Ferretería
        var ccF1  = new CuentaCorriente { ClienteId = cF1.Id,  LimiteCredito = 30000m, FechaCreacion = cF1.FechaAprobacion!.Value };
        var ccF2  = new CuentaCorriente { ClienteId = cF2.Id,  LimiteCredito = 15000m, FechaCreacion = cF2.FechaAprobacion!.Value };
        var ccF3  = new CuentaCorriente { ClienteId = cF3.Id,  LimiteCredito = 20000m, FechaCreacion = cF3.FechaAprobacion!.Value };
        var ccF4  = new CuentaCorriente { ClienteId = cF4.Id,  LimiteCredito = 25000m, FechaCreacion = cF4.FechaAprobacion!.Value };
        var ccF5  = new CuentaCorriente { ClienteId = cF5.Id,  LimiteCredito = 10000m, FechaCreacion = cF5.FechaAprobacion!.Value };
        var ccF6  = new CuentaCorriente { ClienteId = cF6.Id,  LimiteCredito = 18000m, FechaCreacion = cF6.FechaAprobacion!.Value };
        var ccF7  = new CuentaCorriente { ClienteId = cF7.Id,  LimiteCredito = 35000m, FechaCreacion = cF7.FechaAprobacion!.Value };
        var ccF8  = new CuentaCorriente { ClienteId = cF8.Id,  LimiteCredito = 12000m, FechaCreacion = cF8.FechaAprobacion!.Value };
        var ccF10 = new CuentaCorriente { ClienteId = cF10.Id, LimiteCredito = 8000m,  FechaCreacion = cF10.FechaAprobacion!.Value, Bloqueada = true, Observaciones = "Cuenta bloqueada por deuda vencida no regularizada" };

        // Panadería
        var ccP1 = new CuentaCorriente { ClienteId = cP1.Id, LimiteCredito = 5000m,  FechaCreacion = cP1.FechaAprobacion!.Value };
        var ccP2 = new CuentaCorriente { ClienteId = cP2.Id, LimiteCredito = 8000m,  FechaCreacion = cP2.FechaAprobacion!.Value };
        var ccP3 = new CuentaCorriente { ClienteId = cP3.Id, LimiteCredito = 6000m,  FechaCreacion = cP3.FechaAprobacion!.Value };
        var ccP4 = new CuentaCorriente { ClienteId = cP4.Id, LimiteCredito = 10000m, FechaCreacion = cP4.FechaAprobacion!.Value };
        var ccP5 = new CuentaCorriente { ClienteId = cP5.Id, LimiteCredito = 7500m,  FechaCreacion = cP5.FechaAprobacion!.Value };
        var ccP6 = new CuentaCorriente { ClienteId = cP6.Id, LimiteCredito = 4000m,  FechaCreacion = cP6.FechaAprobacion!.Value };
        var ccP8 = new CuentaCorriente { ClienteId = cP8.Id, LimiteCredito = 3000m,  FechaCreacion = cP8.FechaAprobacion!.Value, Bloqueada = true, Observaciones = "Cliente inactivo - cuenta cerrada por cierre de negocio" };

        // Librería
        var ccL1 = new CuentaCorriente { ClienteId = cL1.Id, LimiteCredito = 15000m, FechaCreacion = cL1.FechaAprobacion!.Value };
        var ccL2 = new CuentaCorriente { ClienteId = cL2.Id, LimiteCredito = 20000m, FechaCreacion = cL2.FechaAprobacion!.Value };
        var ccL3 = new CuentaCorriente { ClienteId = cL3.Id, LimiteCredito = 10000m, FechaCreacion = cL3.FechaAprobacion!.Value };
        var ccL4 = new CuentaCorriente { ClienteId = cL4.Id, LimiteCredito = 25000m, FechaCreacion = cL4.FechaAprobacion!.Value };
        var ccL5 = new CuentaCorriente { ClienteId = cL5.Id, LimiteCredito = 8000m,  FechaCreacion = cL5.FechaAprobacion!.Value };

        db.CuentasCorrientes.AddRange(
            ccF1, ccF2, ccF3, ccF4, ccF5, ccF6, ccF7, ccF8, ccF10,
            ccP1, ccP2, ccP3, ccP4, ccP5, ccP6, ccP8,
            ccL1, ccL2, ccL3, ccL4, ccL5);
        await db.SaveChangesAsync();

        // ─── 6. MOVIMIENTOS ──────────────────────────────────────────────────────
        var movs = new List<Movimiento>();

        // ── Ferretería – ccF1 – Roberto Fernández (LímC=30.000) ──────────────────
        // Cliente activo, compras regulares de obra, varios saldados y deuda corriente
        movs.AddRange([
            Mv(ccF1.Id, TipoMovimiento.Debe,   1850m, "Pintura látex blanca 20L x2 unidades",              "FA-0001-00001", now.AddMonths(-11), now.AddMonths(-10), pagado: true,  fechaPago: now.AddMonths(-10)),
            Mv(ccF1.Id, TipoMovimiento.Haber,  1850m, "Pago en efectivo – FA-0001-00001",                   null,            now.AddMonths(-10), now.AddMonths(-10), pagado: true,  fechaPago: now.AddMonths(-10)),
            Mv(ccF1.Id, TipoMovimiento.Debe,   3200m, "Cemento x10 bolsas + arena gruesa 5m3",              "FA-0001-00018", now.AddMonths(-10), now.AddMonths(-9),  pagado: true,  fechaPago: now.AddMonths(-9)),
            Mv(ccF1.Id, TipoMovimiento.Haber,  3200m, "Transferencia bancaria CBU – FA-0001-00018",         null,            now.AddMonths(-9),  now.AddMonths(-9),  pagado: true,  fechaPago: now.AddMonths(-9)),
            Mv(ccF1.Id, TipoMovimiento.Debe,    850m, "Set llaves combinadas + alicates",                   "FA-0001-00035", now.AddMonths(-8),  now.AddMonths(-7),  pagado: true,  fechaPago: now.AddMonths(-7)),
            Mv(ccF1.Id, TipoMovimiento.Haber,   850m, "Pago en efectivo",                                   null,            now.AddMonths(-7),  now.AddMonths(-7),  pagado: true,  fechaPago: now.AddMonths(-7)),
            Mv(ccF1.Id, TipoMovimiento.Debe,   4500m, "Caños y conexiones plomería – remodelación baño",    "FA-0001-00052", now.AddMonths(-6),  now.AddMonths(-5),  pagado: true,  fechaPago: now.AddMonths(-5)),
            Mv(ccF1.Id, TipoMovimiento.Haber,  4500m, "Transferencia bancaria",                             null,            now.AddMonths(-5),  now.AddMonths(-5),  pagado: true,  fechaPago: now.AddMonths(-5)),
            Mv(ccF1.Id, TipoMovimiento.Debe,   1200m, "Pintura exterior látex 10L – color teja",            "FA-0001-00071", now.AddMonths(-4),  now.AddMonths(-3),  pagado: true,  fechaPago: now.AddMonths(-3)),
            Mv(ccF1.Id, TipoMovimiento.Haber,  1200m, "Pago en efectivo",                                   null,            now.AddMonths(-3),  now.AddMonths(-3),  pagado: true,  fechaPago: now.AddMonths(-3)),
            Mv(ccF1.Id, TipoMovimiento.Debe,   2800m, "Cableado eléctrico 50m + fichas e interruptores",   "FA-0001-00089", now.AddMonths(-2),  now.AddMonths(-1),  pagado: false, fechaPago: null),
            Mv(ccF1.Id, TipoMovimiento.Debe,    680m, "Tornillos autorroscantes x500 + tarugos Fischer",    "FA-0001-00097", now.AddMonths(-1),  now.AddDays(30),    pagado: false, fechaPago: null),
            Mv(ccF1.Id, TipoMovimiento.Debe,   3900m, "Cerámicos porcellanato 60x60 – 10m2 + adhesivo",    "FA-0001-00108", now.AddDays(-15),   now.AddDays(15),    pagado: false, fechaPago: null),
        ]);

        // ── Ferretería – ccF2 – Claudia Gómez (LímC=15.000) ─────────────────────
        movs.AddRange([
            Mv(ccF2.Id, TipoMovimiento.Debe,    950m, "Pintura base y barniz interior",                     "FA-0001-00008", now.AddMonths(-10), now.AddMonths(-9),  pagado: true,  fechaPago: now.AddMonths(-9)),
            Mv(ccF2.Id, TipoMovimiento.Haber,   950m, "Pago en efectivo",                                   null,            now.AddMonths(-9),  now.AddMonths(-9),  pagado: true,  fechaPago: now.AddMonths(-9)),
            Mv(ccF2.Id, TipoMovimiento.Debe,   2100m, "Taladro eléctrico 750W + caladora + accesorios",     "FA-0001-00029", now.AddMonths(-7),  now.AddMonths(-6),  pagado: true,  fechaPago: now.AddMonths(-5)),
            Mv(ccF2.Id, TipoMovimiento.Haber,  1000m, "Pago parcial en efectivo",                           null,            now.AddMonths(-6),  now.AddMonths(-6),  pagado: true,  fechaPago: now.AddMonths(-6)),
            Mv(ccF2.Id, TipoMovimiento.Haber,  1100m, "Saldo restante – transferencia bancaria",            null,            now.AddMonths(-5),  now.AddMonths(-5),  pagado: true,  fechaPago: now.AddMonths(-5)),
            Mv(ccF2.Id, TipoMovimiento.Debe,   1450m, "Sellador acrílico + masilla para paredes 5kg",       "FA-0001-00061", now.AddMonths(-3),  now.AddMonths(-2),  pagado: false, fechaPago: null),
            Mv(ccF2.Id, TipoMovimiento.Debe,    780m, "Llave de paso 3/4 + ducha teléfono + sifón",         "FA-0001-00083", now.AddMonths(-1),  now.AddDays(30),    pagado: false, fechaPago: null),
        ]);

        // ── Ferretería – ccF3 – Diego Herrera (LímC=20.000) ─────────────────────
        movs.AddRange([
            Mv(ccF3.Id, TipoMovimiento.Debe,   5200m, "Materiales construcción – reforma integral cocina",  "FA-0001-00012", now.AddMonths(-9),  now.AddMonths(-8),  pagado: true,  fechaPago: now.AddMonths(-8)),
            Mv(ccF3.Id, TipoMovimiento.Haber,  5200m, "Transferencia Mercado Pago",                         null,            now.AddMonths(-8),  now.AddMonths(-8),  pagado: true,  fechaPago: now.AddMonths(-8)),
            Mv(ccF3.Id, TipoMovimiento.Debe,   3800m, "Cerámicos piso cocina 8m2 + adhesivo Bostik",        "FA-0001-00044", now.AddMonths(-6),  now.AddMonths(-5),  pagado: true,  fechaPago: now.AddMonths(-4)),
            Mv(ccF3.Id, TipoMovimiento.Haber,  2000m, "Pago parcial en efectivo",                           null,            now.AddMonths(-5),  now.AddMonths(-5),  pagado: true,  fechaPago: now.AddMonths(-5)),
            Mv(ccF3.Id, TipoMovimiento.Haber,  1800m, "Pago saldo restante – transferencia",                null,            now.AddMonths(-4),  now.AddMonths(-4),  pagado: true,  fechaPago: now.AddMonths(-4)),
            Mv(ccF3.Id, TipoMovimiento.Debe,   6500m, "Revoque y hormigón – ampliación cochera",            "FA-0001-00091", now.AddMonths(-2),  now.AddMonths(-1),  pagado: false, fechaPago: null),
            Mv(ccF3.Id, TipoMovimiento.Debe,   1100m, "Pintura exterior + rodillo profesional 25cm",        "FA-0001-00112", now.AddMonths(-1),  now.AddDays(30),    pagado: false, fechaPago: null),
        ]);

        // ── Ferretería – ccF4 – Lucía Torres (LímC=25.000) ──────────────────────
        movs.AddRange([
            Mv(ccF4.Id, TipoMovimiento.Debe,   7500m, "Instalación plomería completa – baño principal",     "FA-0001-00020", now.AddMonths(-8),  now.AddMonths(-7),  pagado: true,  fechaPago: now.AddMonths(-7)),
            Mv(ccF4.Id, TipoMovimiento.Haber,  7500m, "Transferencia bancaria",                             null,            now.AddMonths(-7),  now.AddMonths(-7),  pagado: true,  fechaPago: now.AddMonths(-7)),
            Mv(ccF4.Id, TipoMovimiento.Debe,   4200m, "Pintura total departamento 3 amb. – 6 latas 20L",    "FA-0001-00057", now.AddMonths(-5),  now.AddMonths(-4),  pagado: true,  fechaPago: now.AddMonths(-4)),
            Mv(ccF4.Id, TipoMovimiento.Haber,  4200m, "Pago en efectivo",                                   null,            now.AddMonths(-4),  now.AddMonths(-4),  pagado: true,  fechaPago: now.AddMonths(-4)),
            Mv(ccF4.Id, TipoMovimiento.Debe,   2900m, "Tablero eléctrico 12 ctos + cableado 50m",           "FA-0001-00078", now.AddMonths(-3),  now.AddMonths(-2),  pagado: false, fechaPago: null),
            Mv(ccF4.Id, TipoMovimiento.Debe,   1600m, "Malla de acero + hormigón premezclado 0.3m3",        "FA-0001-00099", now.AddMonths(-1),  now.AddDays(30),    pagado: false, fechaPago: null),
        ]);

        // ── Ferretería – ccF5 – Martín Pérez (LímC=10.000) ──────────────────────
        movs.AddRange([
            Mv(ccF5.Id, TipoMovimiento.Debe,   1200m, "Materiales plomería – cocina y baño",                "FA-0001-00031", now.AddMonths(-7),  now.AddMonths(-6),  pagado: true,  fechaPago: now.AddMonths(-6)),
            Mv(ccF5.Id, TipoMovimiento.Haber,  1200m, "Pago en efectivo",                                   null,            now.AddMonths(-6),  now.AddMonths(-6),  pagado: true,  fechaPago: now.AddMonths(-6)),
            Mv(ccF5.Id, TipoMovimiento.Debe,   2400m, "Pintura enduído x4 baldes 5kg + rodillos",           "FA-0001-00063", now.AddMonths(-4),  now.AddMonths(-3),  pagado: true,  fechaPago: now.AddMonths(-3)),
            Mv(ccF5.Id, TipoMovimiento.Haber,  2400m, "Transferencia bancaria",                             null,            now.AddMonths(-3),  now.AddMonths(-3),  pagado: true,  fechaPago: now.AddMonths(-3)),
            Mv(ccF5.Id, TipoMovimiento.Debe,   3100m, "Ventana PVC 1.20x1.00 corrediza c/vidrio",           "FA-0001-00086", now.AddMonths(-2),  now.AddMonths(-1),  pagado: false, fechaPago: null),
        ]);

        // ── Ferretería – ccF6 – Valeria Castro (LímC=18.000) ────────────────────
        movs.AddRange([
            Mv(ccF6.Id, TipoMovimiento.Debe,   4800m, "Revestimiento pared exterior – 8m2 + mortero",       "FA-0001-00040", now.AddMonths(-6),  now.AddMonths(-5),  pagado: true,  fechaPago: now.AddMonths(-5)),
            Mv(ccF6.Id, TipoMovimiento.Haber,  4800m, "Transferencia Mercado Pago",                         null,            now.AddMonths(-5),  now.AddMonths(-5),  pagado: true,  fechaPago: now.AddMonths(-5)),
            Mv(ccF6.Id, TipoMovimiento.Debe,   2200m, "Pintura impermeabilizante techo Sika 20L",            "FA-0001-00059", now.AddMonths(-4),  now.AddMonths(-3),  pagado: true,  fechaPago: now.AddMonths(-3)),
            Mv(ccF6.Id, TipoMovimiento.Haber,  2200m, "Pago en efectivo",                                   null,            now.AddMonths(-3),  now.AddMonths(-3),  pagado: true,  fechaPago: now.AddMonths(-3)),
            Mv(ccF6.Id, TipoMovimiento.Debe,   5600m, "Instalación eléctrica completa – 3 ambientes",       "FA-0001-00094", now.AddMonths(-1),  now.AddDays(30),    pagado: false, fechaPago: null),
        ]);

        // ── Ferretería – ccF7 – Federico Morales (LímC=35.000) ──────────────────
        movs.AddRange([
            Mv(ccF7.Id, TipoMovimiento.Debe,  12000m, "Materiales construcción – reforma integral 4 amb.", "FA-0001-00047", now.AddMonths(-5),  now.AddMonths(-4),  pagado: true,  fechaPago: now.AddMonths(-3)),
            Mv(ccF7.Id, TipoMovimiento.Haber,  6000m, "Pago parcial – transferencia bancaria",             null,            now.AddMonths(-4),  now.AddMonths(-4),  pagado: true,  fechaPago: now.AddMonths(-4)),
            Mv(ccF7.Id, TipoMovimiento.Haber,  6000m, "Pago saldo – cheque diferido al cobro",             null,            now.AddMonths(-3),  now.AddMonths(-3),  pagado: true,  fechaPago: now.AddMonths(-3)),
            Mv(ccF7.Id, TipoMovimiento.Debe,   8500m, "Ladrillo cerámico 2000u + bloque hormigón 500u",    "FA-0001-00076", now.AddMonths(-3),  now.AddMonths(-2),  pagado: true,  fechaPago: now.AddMonths(-2)),
            Mv(ccF7.Id, TipoMovimiento.Haber,  8500m, "Transferencia bancaria",                            null,            now.AddMonths(-2),  now.AddMonths(-2),  pagado: true,  fechaPago: now.AddMonths(-2)),
            Mv(ccF7.Id, TipoMovimiento.Debe,  15000m, "Materiales segundo piso – columnas, vigas, losa",  "FA-0001-00104", now.AddMonths(-1),  now.AddDays(60),    pagado: false, fechaPago: null),
        ]);

        // ── Ferretería – ccF8 – Gabriela Ruiz (LímC=12.000) ─────────────────────
        movs.AddRange([
            Mv(ccF8.Id, TipoMovimiento.Debe,   2100m, "Esmalte sintético rejas + pintura anticorrosiva",   "FA-0001-00055", now.AddMonths(-4),  now.AddMonths(-3),  pagado: true,  fechaPago: now.AddMonths(-3)),
            Mv(ccF8.Id, TipoMovimiento.Haber,  2100m, "Pago en efectivo",                                  null,            now.AddMonths(-3),  now.AddMonths(-3),  pagado: true,  fechaPago: now.AddMonths(-3)),
            Mv(ccF8.Id, TipoMovimiento.Debe,   3500m, "Grifería baño monocomando + accesorios inox",       "FA-0001-00080", now.AddMonths(-2),  now.AddMonths(-1),  pagado: false, fechaPago: null),
            Mv(ccF8.Id, TipoMovimiento.Debe,   1800m, "Cerámicos pared baño 6m2 + adhesivo Porcelfix",     "FA-0001-00103", now.AddMonths(-1),  now.AddDays(30),    pagado: false, fechaPago: null),
        ]);

        // ── Ferretería – ccF10 – Patricia Álvarez (INACTIVO/BLOQUEADA) ───────────
        movs.AddRange([
            Mv(ccF10.Id, TipoMovimiento.Debe,  3200m, "Materiales varios obra – piso y paredes",           "FA-0001-00016", now.AddMonths(-6),  now.AddMonths(-5),  pagado: true,  fechaPago: now.AddMonths(-5)),
            Mv(ccF10.Id, TipoMovimiento.Haber, 3200m, "Pago en efectivo",                                  null,            now.AddMonths(-5),  now.AddMonths(-5),  pagado: true,  fechaPago: now.AddMonths(-5)),
            Mv(ccF10.Id, TipoMovimiento.Debe,  5800m, "Pintura total casa + impermeabilizante techo",      "FA-0001-00038", now.AddMonths(-4),  now.AddMonths(-3),  pagado: false, fechaPago: null, obs: "Deuda vencida – cliente no responde comunicaciones"),
            Mv(ccF10.Id, TipoMovimiento.Debe,  2400m, "Herramientas eléctricas – amoladora + taladro",     "FA-0001-00054", now.AddMonths(-3),  now.AddMonths(-2),  pagado: false, fechaPago: null, obs: "En gestión de cobro judicial"),
        ]);

        // ── Panadería – ccP1 – Jorge Romero (LímC=5.000) ─────────────────────────
        movs.AddRange([
            Mv(ccP1.Id, TipoMovimiento.Debe,    480m, "Pan artesanal semanal x4 semanas – marzo",          "FB-0001-00001", now.AddMonths(-7),  now.AddMonths(-6),  pagado: true,  fechaPago: now.AddMonths(-6)),
            Mv(ccP1.Id, TipoMovimiento.Haber,   480m, "Pago en efectivo",                                  null,            now.AddMonths(-6),  now.AddMonths(-6),  pagado: true,  fechaPago: now.AddMonths(-6)),
            Mv(ccP1.Id, TipoMovimiento.Debe,    520m, "Pan artesanal semanal x4 semanas – abril",          "FB-0001-00009", now.AddMonths(-6),  now.AddMonths(-5),  pagado: true,  fechaPago: now.AddMonths(-5)),
            Mv(ccP1.Id, TipoMovimiento.Haber,   520m, "Pago en efectivo",                                  null,            now.AddMonths(-5),  now.AddMonths(-5),  pagado: true,  fechaPago: now.AddMonths(-5)),
            Mv(ccP1.Id, TipoMovimiento.Debe,   1200m, "Torta cumpleaños 3 pisos personalizada + facturas", "FB-0001-00017", now.AddMonths(-4),  now.AddMonths(-3),  pagado: true,  fechaPago: now.AddMonths(-3)),
            Mv(ccP1.Id, TipoMovimiento.Haber,  1200m, "Transferencia bancaria",                            null,            now.AddMonths(-3),  now.AddMonths(-3),  pagado: true,  fechaPago: now.AddMonths(-3)),
            Mv(ccP1.Id, TipoMovimiento.Debe,    560m, "Pan artesanal semanal x4 semanas – junio",          "FB-0001-00025", now.AddMonths(-2),  now.AddMonths(-1),  pagado: false, fechaPago: null),
            Mv(ccP1.Id, TipoMovimiento.Debe,    590m, "Pan artesanal + medialunas semanal x4 – julio",     "FB-0001-00033", now.AddMonths(-1),  now.AddDays(30),    pagado: false, fechaPago: null),
        ]);

        // ── Panadería – ccP2 – Silvia Núñez (LímC=8.000) ────────────────────────
        movs.AddRange([
            Mv(ccP2.Id, TipoMovimiento.Debe,   2500m, "Pedido mensual panadería – familia 5 personas",     "FB-0001-00004", now.AddMonths(-6),  now.AddMonths(-5),  pagado: true,  fechaPago: now.AddMonths(-5)),
            Mv(ccP2.Id, TipoMovimiento.Haber,  2500m, "Transferencia Mercado Pago",                        null,            now.AddMonths(-5),  now.AddMonths(-5),  pagado: true,  fechaPago: now.AddMonths(-5)),
            Mv(ccP2.Id, TipoMovimiento.Debe,   1800m, "Catering evento social – empanadas x50 + tortas",   "FB-0001-00013", now.AddMonths(-4),  now.AddMonths(-3),  pagado: true,  fechaPago: now.AddMonths(-3)),
            Mv(ccP2.Id, TipoMovimiento.Haber,  1800m, "Pago en efectivo",                                  null,            now.AddMonths(-3),  now.AddMonths(-3),  pagado: true,  fechaPago: now.AddMonths(-3)),
            Mv(ccP2.Id, TipoMovimiento.Debe,   2800m, "Pedido mensual panadería – julio",                  "FB-0001-00022", now.AddMonths(-1),  now.AddDays(30),    pagado: false, fechaPago: null),
        ]);

        // ── Panadería – ccP3 – Andrés Vega (LímC=6.000) ─────────────────────────
        movs.AddRange([
            Mv(ccP3.Id, TipoMovimiento.Debe,   1100m, "Panadería quincenal – pan integral + facturas",     "FB-0001-00006", now.AddMonths(-5),  now.AddMonths(-4),  pagado: true,  fechaPago: now.AddMonths(-4)),
            Mv(ccP3.Id, TipoMovimiento.Haber,  1100m, "Pago en efectivo",                                  null,            now.AddMonths(-4),  now.AddMonths(-4),  pagado: true,  fechaPago: now.AddMonths(-4)),
            Mv(ccP3.Id, TipoMovimiento.Debe,   1100m, "Panadería quincenal – pan + bizcochos",             "FB-0001-00018", now.AddMonths(-3),  now.AddMonths(-2),  pagado: true,  fechaPago: now.AddMonths(-2)),
            Mv(ccP3.Id, TipoMovimiento.Haber,  1100m, "Transferencia bancaria",                            null,            now.AddMonths(-2),  now.AddMonths(-2),  pagado: true,  fechaPago: now.AddMonths(-2)),
            Mv(ccP3.Id, TipoMovimiento.Debe,   2400m, "Pan dulce x6 + budines x4 – pedido especial",       "FB-0001-00031", now.AddMonths(-1),  now.AddDays(15),    pagado: false, fechaPago: null),
        ]);

        // ── Panadería – ccP4 – Carolina Medina (LímC=10.000) ────────────────────
        movs.AddRange([
            Mv(ccP4.Id, TipoMovimiento.Debe,   3500m, "Catering casamiento – facturas y pan artesanal x200","FB-0001-00010", now.AddMonths(-4),  now.AddMonths(-3),  pagado: true,  fechaPago: now.AddMonths(-3)),
            Mv(ccP4.Id, TipoMovimiento.Haber,  3500m, "Transferencia bancaria",                            null,            now.AddMonths(-3),  now.AddMonths(-3),  pagado: true,  fechaPago: now.AddMonths(-3)),
            Mv(ccP4.Id, TipoMovimiento.Debe,   4200m, "Pedido mensual empresa – desayunos de trabajo",     "FB-0001-00027", now.AddMonths(-2),  now.AddMonths(-1),  pagado: false, fechaPago: null),
            Mv(ccP4.Id, TipoMovimiento.Debe,    650m, "Torta cumpleaños personalizada – 20 porciones",     "FB-0001-00036", now.AddMonths(-1),  now.AddDays(30),    pagado: false, fechaPago: null),
        ]);

        // ── Panadería – ccP5 – Ricardo Jiménez (LímC=7.500) ─────────────────────
        movs.AddRange([
            Mv(ccP5.Id, TipoMovimiento.Debe,    890m, "Pan integral + medialunas de manteca semanal",      "FB-0001-00014", now.AddMonths(-3),  now.AddMonths(-2),  pagado: true,  fechaPago: now.AddMonths(-2)),
            Mv(ccP5.Id, TipoMovimiento.Haber,   890m, "Pago en efectivo",                                  null,            now.AddMonths(-2),  now.AddMonths(-2),  pagado: true,  fechaPago: now.AddMonths(-2)),
            Mv(ccP5.Id, TipoMovimiento.Debe,   1750m, "Pedido familiar mensual – julio",                   "FB-0001-00029", now.AddMonths(-1),  now.AddDays(30),    pagado: false, fechaPago: null),
        ]);

        // ── Panadería – ccP6 – Natalia Flores (LímC=4.000) ──────────────────────
        movs.AddRange([
            Mv(ccP6.Id, TipoMovimiento.Debe,    420m, "Primera compra – pan artesanal de campo",           "FB-0001-00019", now.AddMonths(-2),  now.AddMonths(-1),  pagado: true,  fechaPago: now.AddMonths(-1)),
            Mv(ccP6.Id, TipoMovimiento.Haber,   420m, "Pago en efectivo",                                  null,            now.AddMonths(-1),  now.AddMonths(-1),  pagado: true,  fechaPago: now.AddMonths(-1)),
            Mv(ccP6.Id, TipoMovimiento.Debe,    980m, "Pan + facturas + medialunas – pedido mensual",      "FB-0001-00032", now.AddMonths(-1),  now.AddDays(30),    pagado: false, fechaPago: null),
        ]);

        // ── Panadería – ccP8 – Laura Molina (INACTIVO) ───────────────────────────
        movs.AddRange([
            Mv(ccP8.Id, TipoMovimiento.Debe,   1200m, "Pedido panadería mensual – octubre",                "FB-0001-00007", now.AddMonths(-5),  now.AddMonths(-4),  pagado: true,  fechaPago: now.AddMonths(-4)),
            Mv(ccP8.Id, TipoMovimiento.Haber,  1200m, "Pago en efectivo",                                  null,            now.AddMonths(-4),  now.AddMonths(-4),  pagado: true,  fechaPago: now.AddMonths(-4)),
            Mv(ccP8.Id, TipoMovimiento.Debe,   2800m, "Pedido especial evento empresa – noviembre",        "FB-0001-00016", now.AddMonths(-3),  now.AddMonths(-2),  pagado: false, fechaPago: null, obs: "Cliente inactivo – cobro pendiente por cierre"),
        ]);

        // ── Librería – ccL1 – Marcelo Vargas (LímC=15.000) ──────────────────────
        movs.AddRange([
            Mv(ccL1.Id, TipoMovimiento.Debe,   3200m, "Útiles escolares inicio año – lista completa x1",   "FC-0001-00001", now.AddMonths(-4),  now.AddMonths(-3),  pagado: true,  fechaPago: now.AddMonths(-3)),
            Mv(ccL1.Id, TipoMovimiento.Haber,  3200m, "Transferencia bancaria",                            null,            now.AddMonths(-3),  now.AddMonths(-3),  pagado: true,  fechaPago: now.AddMonths(-3)),
            Mv(ccL1.Id, TipoMovimiento.Debe,   5800m, "Libros universitarios ingeniería 1er año x5 mat.",  "FC-0001-00012", now.AddMonths(-3),  now.AddMonths(-2),  pagado: true,  fechaPago: now.AddMonths(-2)),
            Mv(ccL1.Id, TipoMovimiento.Haber,  5800m, "Pago en efectivo",                                  null,            now.AddMonths(-2),  now.AddMonths(-2),  pagado: true,  fechaPago: now.AddMonths(-2)),
            Mv(ccL1.Id, TipoMovimiento.Debe,   2100m, "Material de oficina – empresa: resmas, tóner",      "FC-0001-00021", now.AddMonths(-1),  now.AddDays(30),    pagado: false, fechaPago: null),
        ]);

        // ── Librería – ccL2 – Sandra Reyes (LímC=20.000) ────────────────────────
        movs.AddRange([
            Mv(ccL2.Id, TipoMovimiento.Debe,   8500m, "Insumos papelería empresa – pedido trimestral",     "FC-0001-00005", now.AddMonths(-3),  now.AddMonths(-2),  pagado: true,  fechaPago: now.AddMonths(-2)),
            Mv(ccL2.Id, TipoMovimiento.Haber,  8500m, "Transferencia bancaria empresa",                    null,            now.AddMonths(-2),  now.AddMonths(-2),  pagado: true,  fechaPago: now.AddMonths(-2)),
            Mv(ccL2.Id, TipoMovimiento.Debe,   6200m, "Resmas papel A4 x30 + tóner HP + cartuchos",        "FC-0001-00018", now.AddMonths(-2),  now.AddMonths(-1),  pagado: false, fechaPago: null),
            Mv(ccL2.Id, TipoMovimiento.Debe,   3800m, "Carpetas ejecutivas x20 + biblioratos + fichas",    "FC-0001-00025", now.AddMonths(-1),  now.AddDays(30),    pagado: false, fechaPago: null),
        ]);

        // ── Librería – ccL3 – Alejandro Cruz (LímC=10.000) ──────────────────────
        movs.AddRange([
            Mv(ccL3.Id, TipoMovimiento.Debe,   4500m, "Libros de texto secundario x3 hijos – año escolar", "FC-0001-00008", now.AddMonths(-2),  now.AddMonths(-1),  pagado: true,  fechaPago: now.AddMonths(-1)),
            Mv(ccL3.Id, TipoMovimiento.Haber,  4500m, "Pago en efectivo",                                  null,            now.AddMonths(-1),  now.AddMonths(-1),  pagado: true,  fechaPago: now.AddMonths(-1)),
            Mv(ccL3.Id, TipoMovimiento.Debe,   2300m, "Útiles escolares x3 + mochilas escolares x2",       "FC-0001-00019", now.AddMonths(-1),  now.AddDays(30),    pagado: false, fechaPago: null),
        ]);

        // ── Librería – ccL4 – Mónica Benítez (LímC=25.000) ──────────────────────
        // Jardín de infantes – compras institucionales grandes
        movs.AddRange([
            Mv(ccL4.Id, TipoMovimiento.Debe,  11000m, "Material didáctico jardín – inicio ciclo lectivo",  "FC-0001-00011", now.AddMonths(-2),  now.AddMonths(-1),  pagado: false, fechaPago: null),
            Mv(ccL4.Id, TipoMovimiento.Debe,   4500m, "Libros cuentos x15 + plastilinas + témperas",       "FC-0001-00023", now.AddMonths(-1),  now.AddDays(30),    pagado: false, fechaPago: null),
        ]);

        // ── Librería – ccL5 – Ernesto Mendoza (LímC=8.000) ──────────────────────
        movs.AddRange([
            Mv(ccL5.Id, TipoMovimiento.Debe,   2800m, "Libros carrera universitaria Derecho – 3 materias", "FC-0001-00015", now.AddMonths(-1),  now.AddDays(30),    pagado: false, fechaPago: null),
        ]);

        db.Movimientos.AddRange(movs);
        await db.SaveChangesAsync();
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────────

    private static Usuario Cu(string nombre, string apellido, string email, DateTime fechaCreacion) =>
        new() { Nombre = nombre, Apellido = apellido, Email = email, PasswordHash = _hash, Rol = "Cliente", FechaCreacion = fechaCreacion };

    private static Cliente CA(Usuario u, int comercioId, string dni, string tel, string dir, int aprobadoPorId, DateTime fechaAprobacion, DateTime fechaCreacion) =>
        new() { Nombre = u.Nombre, Apellido = u.Apellido!, Email = u.Email, DNI = dni, Telefono = tel, Direccion = dir, UsuarioId = u.Id, ComercioId = comercioId, EstadoId = 2, OrigenRegistro = 1, FechaAprobacion = fechaAprobacion, AprobadoPorUsuarioId = aprobadoPorId, FechaCreacion = fechaCreacion, Activo = true };

    private static Movimiento Mv(int ccId, TipoMovimiento tipo, decimal importe, string desc, string? comprobante, DateTime fecha, DateTime vencimiento, bool pagado, DateTime? fechaPago, string? obs = null) =>
        new() { CuentaCorrienteId = ccId, TipoMovimiento = tipo, Importe = importe, Descripcion = desc, Comprobante = comprobante, FechaCreacion = fecha, FechaVencimiento = vencimiento, Pagado = pagado, FechaPago = fechaPago, ObservacionesPago = obs, Activo = true };
}
