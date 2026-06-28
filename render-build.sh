#!/bin/bash
# Instalar .NET 8 SDK
curl -sSL https://dot.net/v1/dotnet-install.sh > dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0 --install-dir .dotnet

# Añadir al path de la sesión actual
export PATH="$PATH:$PWD/.dotnet"

# SOBRESCRIBIR appsettings.json para garantizar que Blazor llama a tu API en Render
cp Web/wwwroot/appsettings.Production.json Web/wwwroot/appsettings.json

# Publicar el Frontend
dotnet publish Web/Web.csproj -c Release -o output
