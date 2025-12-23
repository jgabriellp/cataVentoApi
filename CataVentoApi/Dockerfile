# Esta fase é usada durante a execução no VS no modo rápido
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Esta fase é usada para compilar o projeto de serviço
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
# Corrigido: Copia o arquivo .csproj da subpasta
COPY ["CataVentoApi/CataVentoApi.csproj", "CataVentoApi/"] 
RUN dotnet restore "./CataVentoApi/CataVentoApi.csproj"
COPY . .
WORKDIR "/src/CataVentoApi"
RUN dotnet build "./CataVentoApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Esta fase é usada para publicar
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CataVentoApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Esta fase é usada na produção
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CataVentoApi.dll"]