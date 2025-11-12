# ====================================================================
# Dockerfile - Backend ULTRA SIMPLES
# Remove --no-build que está causando erro
# ====================================================================

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar tudo
COPY . .

# Restaurar e publicar em um comando só
WORKDIR "/src/BookManager.API"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=build /src/BookManager.API/Reports ./Reports
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "BookManager.API.dll"]