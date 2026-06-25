# Etapa 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos todo el repo (necesario porque el proyecto web referencia BLL y DAL)
COPY . .

RUN dotnet restore "ProyectoGrupalTennis/ProyectoGrupalTennis.csproj"
RUN dotnet publish "ProyectoGrupalTennis/ProyectoGrupalTennis.csproj" -c Release -o /app/publish

# Etapa 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "ProyectoGrupalTennis.dll"]
