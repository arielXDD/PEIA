FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto para restaurar dependencias
COPY src/Shared/PEIA.Shared.Kernel/PEIA.Shared.Kernel.csproj src/Shared/PEIA.Shared.Kernel/
COPY src/Shared/PEIA.Shared.Infra/PEIA.Shared.Infra.csproj src/Shared/PEIA.Shared.Infra/
COPY src/Modules/PEIA.Modules.ERP/PEIA.Modules.ERP.csproj src/Modules/PEIA.Modules.ERP/
COPY src/Modules/PEIA.Modules.Inventory/PEIA.Modules.Inventory.csproj src/Modules/PEIA.Modules.Inventory/
COPY src/Modules/PEIA.Modules.Logistics/PEIA.Modules.Logistics.csproj src/Modules/PEIA.Modules.Logistics/
COPY src/Modules/PEIA.Modules.Reports/PEIA.Modules.Reports.csproj src/Modules/PEIA.Modules.Reports/
COPY src/Modules/PEIA.Modules.Automation/PEIA.Modules.Automation.csproj src/Modules/PEIA.Modules.Automation/
COPY src/Modules/PEIA.Modules.Prediction/PEIA.Modules.Prediction.csproj src/Modules/PEIA.Modules.Prediction/
COPY src/PEIA.Web/PEIA.Web.csproj src/PEIA.Web/

RUN dotnet restore src/PEIA.Web/PEIA.Web.csproj

# Copiar todo el codigo fuente
COPY src/ src/

# Publicar la aplicacion
RUN dotnet publish src/PEIA.Web/PEIA.Web.csproj -c Release -o /app/publish --no-restore

# Etapa de ejecucion
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Crear directorio para logs
RUN mkdir -p /app/logs

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "PEIA.Web.dll"]
