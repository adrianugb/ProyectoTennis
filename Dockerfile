# Etapa 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "ProyectoGrupalTennis/ProyectoGrupalTennis.csproj"
RUN dotnet publish "ProyectoGrupalTennis/ProyectoGrupalTennis.csproj" -c Release -o /app/publish

# Etapa 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
RUN apt-get update && apt-get install -y ca-certificates && update-ca-certificates
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "ProyectoGrupalTennis.dll"]
