# Ambiente de datos demo

La configuracion local normal debe seguir apuntando a la base original:

```json
"DefaultConnection": "Host=localhost;Database=peiadb;Username=postgres;Password=4523"
```

Para probar datos cargados sin alterar la base original, cambia temporalmente `src/PEIA.Web/appsettings.Development.json` a:

```json
"DefaultConnection": "Host=localhost;Database=peiadb_demo;Username=postgres;Password=4523"
```

Luego inicia la aplicacion:

```powershell
dotnet run --project src\PEIA.Web --launch-profile http
```

Cuando termines de probar, regresa la conexion a `peiadb`.

Los seed adicionales estan separados en `src/Shared/PEIA.Shared.Infra/Seed` para poder activarlos de forma controlada cuando el equipo lo necesite.
