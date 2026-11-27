FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restaurar dependências
COPY ["LisAeroGest/LisAeroGest.csproj", "LisAeroGest/"]
RUN dotnet restore "LisAeroGest/LisAeroGest.csproj"

# Copiar o código da aplicação
COPY LisAeroGest/ LisAeroGest/
WORKDIR /src/LisAeroGest

# Compilar e publicar
RUN dotnet publish "LisAeroGest.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

# Imagem final de execução
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "LisAeroGest.dll"]