# PEIA - Seed de Base de Datos (Datos de Prueba)

Si estás ejecutando el proyecto por primera vez, el sistema está configurado para poblar (seed) automáticamente la base de datos con datos de prueba, lo que te permitirá ver cómo funcionan las métricas, las gráficas (con todos sus colores y variaciones), y las notificaciones en tiempo real en los tableros.

## ¿Cómo ejecutar el Seed?

¡Es automático! Al ejecutar el proyecto web principal (`PEIA.Web`) en el entorno de Desarrollo, el sistema leerá los archivos de esta carpeta (`Seed`) e insertará los registros necesarios:

1. Asegúrate de tener configurada correctamente tu conexión a la base de datos en `appsettings.Development.json`.
2. Ejecuta el proyecto desde Visual Studio (o usando `dotnet run` en el proyecto `PEIA.Web`).
3. La aplicación ejecutará los métodos de la clase `SeedData.cs` que a su vez llama a los otros archivos (como `SeedDataPedidosVariadosDemo.cs`).
4. Ingresa a la aplicación (con los usuarios demo generados, como `admin@peia.com`) y podrás ver los datos en las gráficas inmediatamente.

### Nota sobre los Pedidos y Gráficas
Recientemente se incrementó el número de pedidos y se volvió aleatorio su estado (Creado, Asignado, En Ruta, Entregado, Cancelado) para asegurar que en la gráfica de **"Estado de pedidos"** (Donut Chart) se muestren todos los colores adecuadamente en lugar de concentrarse en uno solo.
