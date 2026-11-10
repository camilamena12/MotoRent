

# Etapa 1: compilar MotoRent con .NET 8
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY . .

# Eliminar archivos temporales copiados desde Windows
# y restaurar los paquetes dentro de Linux
RUN rm -rf bin obj && dotnet restore MotoRent.Api.csproj --force

RUN dotnet publish MotoRent.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

# Etapa 2: ejecutar la API
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:10000

EXPOSE 10000

ENTRYPOINT ["dotnet", "MotoRent.Api.dll"]
