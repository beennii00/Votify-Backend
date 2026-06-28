FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copiar todo el repositorio
COPY . ./

# Restaurar y publicar la API
WORKDIR /app/API
RUN dotnet restore
RUN dotnet publish -c Release -o /out

# Configurar la imagen base para producción
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /out .

# Configurar el puerto
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "API.dll"]
