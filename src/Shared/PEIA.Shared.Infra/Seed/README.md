# PEIA - Seed de Base de Datos

Esta carpeta contiene datos iniciales y datos demo para probar PEIA sin capturar todo manualmente.

## Base normal

La base local normal del proyecto debe apuntar a `peiadb`:

```json
"DefaultConnection": "Host=localhost;Database=peiadb;Username=postgres;Password=4523"
```

Al iniciar `PEIA.Web`, la clase `SeedData.cs` se ejecuta automaticamente y verifica:

1. Roles base del sistema.
2. Centros base.
3. Usuarios iniciales.
4. Categorias, productos, stocks, movimientos, pedidos, notificaciones y reglas base.

Comando:

```powershell
dotnet run --project src\PEIA.Web --launch-profile http
```

## Base demo

Para probar datos mas cargados sin alterar la base original, usa `peiadb_demo` temporalmente en `src/PEIA.Web/appsettings.Development.json`:

```json
"DefaultConnection": "Host=localhost;Database=peiadb_demo;Username=postgres;Password=4523"
```

Cuando termines de probar, regresa la conexion a `peiadb`.

## Seeds separados

Los archivos `SeedData*Demo.cs` estan separados para que el equipo pueda activarlos de forma controlada cuando necesite mas datos de prueba:

- `SeedDataProductosDemo.cs`
- `SeedDataProductosVariadosDemo.cs`
- `SeedDataPedidosVariadosDemo.cs`
- `SeedDataNotificacionesDemo.cs`
- `SeedDataNotificacionesVariadasDemo.cs`
- `SeedDataReglasDemo.cs`
- `SeedDataReglasVariadasDemo.cs`
- `SeedDataConfiguracionVariadaDemo.cs`

Estos archivos no deben asumirse como datos obligatorios de produccion. Son apoyo para demos, pruebas de rendimiento visual y validacion de reportes.

## Archivo local Seed.zip

`Seed.zip` es un archivo local de apoyo y no debe subirse al repositorio. Esta ignorado en `.gitignore`.
