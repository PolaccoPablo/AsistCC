# Script para crear migraciones Entity Framework
# Uso: .\crear-migracion.ps1 -NombreMigracion "NombreDeTuMigracion"

param(
    [Parameter(Mandatory=$true)]
    [string]$NombreMigracion
)

Write-Host "Creando migración: $NombreMigracion" -ForegroundColor Green

# Navegar al proyecto de Infrastructure
Set-Location -Path "SaasACC.Infrastructure"

try {
    # Crear la migración
    Write-Host "Ejecutando: dotnet ef migrations add $NombreMigracion..." -ForegroundColor Yellow
    dotnet ef migrations add $NombreMigracion --startup-project "../SaasACCAPI.api" --context "ApplicationDbContext"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Migración '$NombreMigracion' creada exitosamente!" -ForegroundColor Green
        
        # Preguntar si quiere aplicar la migración
        $aplicar = Read-Host "¿Deseas aplicar la migración a la base de datos? (s/n)"
        
        if ($aplicar -eq 's' -or $aplicar -eq 'S' -or $aplicar -eq 'si' -or $aplicar -eq 'Si') {
            Write-Host "Aplicando migración a la base de datos..." -ForegroundColor Yellow
            dotnet ef database update --startup-project "../SaasACCAPI.api" --context "ApplicationDbContext"
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✅ Migración aplicada exitosamente a la base de datos!" -ForegroundColor Green
            } else {
                Write-Host "❌ Error al aplicar la migración a la base de datos" -ForegroundColor Red
            }
        }
    } else {
        Write-Host "❌ Error al crear la migración" -ForegroundColor Red
    }
}
catch {
    Write-Host "❌ Error: $_" -ForegroundColor Red
}
finally {
    # Volver al directorio original
    Set-Location -Path ".."
}

Write-Host "`nPresiona cualquier tecla para continuar..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
