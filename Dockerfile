# Etapa 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ControlTorneosFutbol.csproj", "."]
RUN dotnet restore "ControlTorneosFutbol.csproj"
COPY . .
RUN dotnet publish "ControlTorneosFutbol.csproj" -c Release -o /app/publish

# Etapa 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ControlTorneosFutbol.dll"]
