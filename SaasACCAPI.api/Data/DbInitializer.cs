using Microsoft.EntityFrameworkCore;
using SaasACC.Infrastructure;
using SaasACC.Infrastructure.Repositories;
using SaasACC.Model.Entities;

namespace SaasACCAPI.api.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Asegurar que la base de datos esté creada
        await context.Database.EnsureCreatedAsync();
        
        // Verificar si ya hay datos
        if (await context.Comercios.AnyAsync())
        {
            return; // Ya hay datos, no inicializar
        }
        
        // Crear comercio de prueba
        var comercio = new Comercio
        {
            Nombre = "Comercio Demo",
            Email = "demo@comercio.com",
            Telefono = "123456789",
            Direccion = "Calle Demo 123",
            NotificacionesEmail = true,
            NotificacionesWhatsApp = false
        };
        
        context.Comercios.Add(comercio);
        await context.SaveChangesAsync();
        
        // Crear usuario de prueba
        var usuario = new Usuario
        {
            Nombre = "Usuario Demo",
            Email = "admin@demo.com",
            PasswordHash = UsuarioRepository.HashPassword("123456"), // Contraseña: 123456
            Rol = "Admin",
            ComercioId = comercio.Id,
            UltimoAcceso = null
        };
        
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();
        
        Console.WriteLine("Base de datos inicializada con datos de prueba:");
        Console.WriteLine($"Comercio: {comercio.Nombre} (ID: {comercio.Id})");
        Console.WriteLine($"Usuario: {usuario.Email} - Contraseña: 123456");
    }
}
