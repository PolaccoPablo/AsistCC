# Script para aplicar migraciones Entity Framework
# Uso: .\aplicar-migraciones.ps1

Write-Host "Aplicando migraciones a la base de datos..." -ForegroundColor Green

# Navegar al proyecto de Infrastructure
Set-Location -Path "SaasACC.Infrastructure"

try {
    # Mostrar migraciones pendientes
    Write-Host "Verificando migraciones pendientes..." -ForegroundColor Yellow
    dotnet ef migrations list --startup-project "../SaasACCAPI.api" --context "ApplicationDbContext"
    
    Write-Host "`nAplicando migraciones..." -ForegroundColor Yellow
    dotnet ef database update --startup-project "../SaasACCAPI.api" --context "ApplicationDbContext"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Todas las migraciones se aplicaron exitosamente!" -ForegroundColor Green
    } else {
        Write-Host "❌ Error al aplicar las migraciones" -ForegroundColor Red
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
