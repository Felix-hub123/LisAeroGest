FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia o ficheiro do projeto e restaura dependências
COPY ["LisAeroGest/LisAeroGest.csproj", "LisAeroGest/"]
RUN dotnet restore "LisAeroGest/LisAeroGest.csproj"

# Copia todo o código e compila a aplicação
COPY . .
WORKDIR "/src/LisAeroGest"
RUN dotnet build "LisAeroGest.csproj" -c Release -o /app/build
RUN dotnet publish "LisAeroGest.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagem final de execução
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "LisAeroGest.dll"]