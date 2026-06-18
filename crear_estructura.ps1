# ==============================================================
# PEIA - Script de creacion de estructura de solucion .NET 8
# Monolito Modular | MediatR | EF Core | SignalR | ML.NET
# ==============================================================
# COMO USAR:
#   1. Abre PowerShell como Administrador (o en la terminal de VS Code)
#   2. Navega a: cd C:\Users\Ariel\Downloads\peia
#   3. Ejecuta: .\crear_estructura.ps1
# ==============================================================

$raiz = "C:\Users\Ariel\Downloads\peia"
Set-Location $raiz

Write-Host "=== Creando solucion PEIA ===" -ForegroundColor Cyan

# -------------------------------------------------------
# 1. Solucion principal
# -------------------------------------------------------
dotnet new sln -n PEIA --output .
Write-Host "[OK] Solucion PEIA.sln creada" -ForegroundColor Green

# -------------------------------------------------------
# 2. Proyecto Web principal (Blazor Server / Razor Pages)
# -------------------------------------------------------
New-Item -ItemType Directory -Path "src\PEIA.Web" -Force | Out-Null
dotnet new web -n PEIA.Web --output "src\PEIA.Web" --framework net8.0
dotnet sln add "src\PEIA.Web\PEIA.Web.csproj"
Write-Host "[OK] PEIA.Web creado" -ForegroundColor Green

# -------------------------------------------------------
# 3. Shared - Kernel (contratos, DTOs, eventos MediatR)
# -------------------------------------------------------
New-Item -ItemType Directory -Path "src\Shared\PEIA.Shared.Kernel" -Force | Out-Null
dotnet new classlib -n PEIA.Shared.Kernel --output "src\Shared\PEIA.Shared.Kernel" --framework net8.0
dotnet sln add "src\Shared\PEIA.Shared.Kernel\PEIA.Shared.Kernel.csproj"
Write-Host "[OK] PEIA.Shared.Kernel creado" -ForegroundColor Green

# -------------------------------------------------------
# 4. Shared - Infrastructure (EF Core, Identidad, JWT)
# -------------------------------------------------------
New-Item -ItemType Directory -Path "src\Shared\PEIA.Shared.Infra" -Force | Out-Null
dotnet new classlib -n PEIA.Shared.Infra --output "src\Shared\PEIA.Shared.Infra" --framework net8.0
dotnet sln add "src\Shared\PEIA.Shared.Infra\PEIA.Shared.Infra.csproj"
Write-Host "[OK] PEIA.Shared.Infra creado" -ForegroundColor Green

# -------------------------------------------------------
# 5. Modulos del negocio (Monolito Modular)
# -------------------------------------------------------
$modulos = @("ERP", "Inventory", "Logistics", "Reports", "Automation", "Prediction")

foreach ($modulo in $modulos) {
    $ruta = "src\Modules\PEIA.Modules.$modulo"
    New-Item -ItemType Directory -Path $ruta -Force | Out-Null
    dotnet new classlib -n "PEIA.Modules.$modulo" --output $ruta --framework net8.0
    dotnet sln add "$ruta\PEIA.Modules.$modulo.csproj"
    Write-Host "[OK] PEIA.Modules.$modulo creado" -ForegroundColor Green
}

# -------------------------------------------------------
# 6. Proyecto de pruebas
# -------------------------------------------------------
New-Item -ItemType Directory -Path "tests\PEIA.Tests" -Force | Out-Null
dotnet new xunit -n PEIA.Tests --output "tests\PEIA.Tests" --framework net8.0
dotnet sln add "tests\PEIA.Tests\PEIA.Tests.csproj"
Write-Host "[OK] PEIA.Tests creado" -ForegroundColor Green

# -------------------------------------------------------
# 7. Referencias entre proyectos
# -------------------------------------------------------
Write-Host "`n=== Configurando referencias ===" -ForegroundColor Cyan

# Infra depende de Kernel
dotnet add "src\Shared\PEIA.Shared.Infra\PEIA.Shared.Infra.csproj" reference "src\Shared\PEIA.Shared.Kernel\PEIA.Shared.Kernel.csproj"

# Cada modulo depende de Kernel
foreach ($modulo in $modulos) {
    dotnet add "src\Modules\PEIA.Modules.$modulo\PEIA.Modules.$modulo.csproj" reference "src\Shared\PEIA.Shared.Kernel\PEIA.Shared.Kernel.csproj"
}

# Web depende de Infra y todos los modulos
dotnet add "src\PEIA.Web\PEIA.Web.csproj" reference "src\Shared\PEIA.Shared.Infra\PEIA.Shared.Infra.csproj"
foreach ($modulo in $modulos) {
    dotnet add "src\PEIA.Web\PEIA.Web.csproj" reference "src\Modules\PEIA.Modules.$modulo\PEIA.Modules.$modulo.csproj"
}

# Tests depende de Web
dotnet add "tests\PEIA.Tests\PEIA.Tests.csproj" reference "src\PEIA.Web\PEIA.Web.csproj"

Write-Host "[OK] Referencias configuradas" -ForegroundColor Green

# -------------------------------------------------------
# 8. Instalar paquetes NuGet principales
# -------------------------------------------------------
Write-Host "`n=== Instalando paquetes NuGet ===" -ForegroundColor Cyan

# Infra: EF Core + SQL Server + Identity
dotnet add "src\Shared\PEIA.Shared.Infra\PEIA.Shared.Infra.csproj" package Microsoft.EntityFrameworkCore --version 8.0.0
dotnet add "src\Shared\PEIA.Shared.Infra\PEIA.Shared.Infra.csproj" package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.0
dotnet add "src\Shared\PEIA.Shared.Infra\PEIA.Shared.Infra.csproj" package Microsoft.EntityFrameworkCore.Tools --version 8.0.0
dotnet add "src\Shared\PEIA.Shared.Infra\PEIA.Shared.Infra.csproj" package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 8.0.0

# Kernel: MediatR
dotnet add "src\Shared\PEIA.Shared.Kernel\PEIA.Shared.Kernel.csproj" package MediatR --version 12.2.0

# Web: JWT + SignalR + Swagger
dotnet add "src\PEIA.Web\PEIA.Web.csproj" package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0
dotnet add "src\PEIA.Web\PEIA.Web.csproj" package Microsoft.AspNetCore.SignalR --version 1.1.0
dotnet add "src\PEIA.Web\PEIA.Web.csproj" package Swashbuckle.AspNetCore --version 6.5.0

# Prediccion: ML.NET
dotnet add "src\Modules\PEIA.Modules.Prediction\PEIA.Modules.Prediction.csproj" package Microsoft.ML --version 3.0.0

Write-Host "[OK] Paquetes NuGet instalados" -ForegroundColor Green

# -------------------------------------------------------
# 9. Carpetas internas por modulo
# -------------------------------------------------------
Write-Host "`n=== Creando estructura interna de modulos ===" -ForegroundColor Cyan

$subcarpetas = @("Application", "Domain", "Infrastructure", "Interfaces")

foreach ($modulo in $modulos) {
    foreach ($sub in $subcarpetas) {
        New-Item -ItemType Directory -Path "src\Modules\PEIA.Modules.$modulo\$sub" -Force | Out-Null
    }
    Write-Host "[OK] Subcarpetas de PEIA.Modules.$modulo creadas" -ForegroundColor Green
}

# Carpetas del Kernel
$kernelCarpetas = @("Abstractions", "DTOs", "Events", "Exceptions", "Extensions")
foreach ($carpeta in $kernelCarpetas) {
    New-Item -ItemType Directory -Path "src\Shared\PEIA.Shared.Kernel\$carpeta" -Force | Out-Null
}
Write-Host "[OK] Subcarpetas de Shared.Kernel creadas" -ForegroundColor Green

# Carpetas de Infra
$infraCarpetas = @("Data", "Identity", "Migrations", "Repositories", "Seed")
foreach ($carpeta in $infraCarpetas) {
    New-Item -ItemType Directory -Path "src\Shared\PEIA.Shared.Infra\$carpeta" -Force | Out-Null
}
Write-Host "[OK] Subcarpetas de Shared.Infra creadas" -ForegroundColor Green

# -------------------------------------------------------
# 10. Archivos base importantes
# -------------------------------------------------------
Write-Host "`n=== Creando archivos base ===" -ForegroundColor Cyan

# .gitignore
@"
# .NET
bin/
obj/
*.user
*.suo
.vs/
*.log

# Environment
.env
appsettings.Development.json

# NuGet
*.nupkg
packages/
"@ | Out-File -FilePath ".gitignore" -Encoding UTF8
Write-Host "[OK] .gitignore creado" -ForegroundColor Green

# README.md basico
@"
# PEIA - Plataforma Empresarial Inteligente para Almacenes

## Tecnologias
- .NET 8 | C# | Blazor Server
- EF Core + SQL Server
- ASP.NET Identity + JWT
- MediatR (Monolito Modular)
- SignalR (Tiempo real)
- ML.NET (Predicciones)

## Estructura
- src/PEIA.Web           -> Host principal (UI)
- src/Shared/Kernel      -> Contratos y DTOs compartidos
- src/Shared/Infra       -> Base de datos e identidad
- src/Modules/ERP        -> Usuarios, roles, centros
- src/Modules/Inventory  -> Inventario multi-centro
- src/Modules/Logistics  -> Logistica y SLAs
- src/Modules/Reports    -> Reportes y dashboard
- src/Modules/Automation -> Notificaciones (SignalR)
- src/Modules/Prediction -> ML.NET y camaras
- tests/PEIA.Tests       -> Pruebas unitarias

## Equipo
- Ariel Guevara Balderas  -> Lider / Scrum Master / UX / ERP
- Julian David Sierra     -> Inventario / BD / Multi-centro
- Jose Manuel Villa       -> Logistica / Rastreo / SLAs
- Jennifer Munoz          -> Frontend / Dashboard / Reportes
- Mariano Sanchez         -> Automatizacion / DevOps
- Jose Cruz Ramirez       -> Prediccion / Camaras / QA
"@ | Out-File -FilePath "README.md" -Encoding UTF8
Write-Host "[OK] README.md creado" -ForegroundColor Green

# -------------------------------------------------------
# RESUMEN FINAL
# -------------------------------------------------------
Write-Host "`n=================================================" -ForegroundColor Cyan
Write-Host "  PEIA - Estructura creada exitosamente!" -ForegroundColor Green
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Siguiente paso: Abrir PEIA.sln en Visual Studio" -ForegroundColor Yellow
Write-Host "o ejecutar: dotnet build" -ForegroundColor Yellow
