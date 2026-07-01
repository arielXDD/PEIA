# Ejecución del Proyecto PEIA

Para ejecutar el proyecto **PEIA** de forma local, sigue estos pasos desde una terminal (PowerShell o CMD).

## 1. Navegar a la carpeta del proyecto
Primero, asegúrate de estar en el directorio raíz del proyecto:

```powershell
cd C:\Users\Ariel\Downloads\peia
```

*(Nota: Ajusta la ruta si moviste la carpeta a otra ubicación).*

## 2. Ejecutar la aplicación
Utiliza el CLI de .NET para compilar e iniciar el servidor:

```powershell
dotnet run --project src\PEIA.Web
```
Este comando se encarga de:
- Restaurar paquetes NuGet (si es necesario).
- Compilar la solución.
- Levantar el servidor web de Kestrel de forma automática.

## 3. Acceder a la interfaz web
Una vez que la consola indique que la aplicación ha iniciado (verás un mensaje similar a `Now listening on: https://localhost:5001`), abre tu navegador y visita:

👉 **[https://localhost:5001](https://localhost:5001)**

---

### 💡 Nota sobre la Base de Datos (Primer inicio)
Gracias a la configuración de `SeedData`, la **primera vez que ejecutes el proyecto** el sistema automáticamente:
1. Verificará que la base de datos `peiadb` exista, o la creará si no existe.
2. Aplicará todas las migraciones necesarias para generar las tablas.
3. Creará los roles básicos (Administrador, Logística, Inventario, etc.) y las bodegas por defecto.
4. Generará los 5 usuarios de prueba iniciales.

*Si tienes algún problema de conexión, asegúrate de que tu servicio local de PostgreSQL esté corriendo y de que la contraseña en `src/PEIA.Web/appsettings.Development.json` sea la correcta para el usuario `postgres`.*
