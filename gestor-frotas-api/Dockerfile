FROM mcr.microsoft.com/dotnet/sdk:10.0 AS construcao
WORKDIR /origem

COPY src/GestorFrotas.Api/GestorFrotas.Api.csproj src/GestorFrotas.Api/
RUN dotnet restore src/GestorFrotas.Api/GestorFrotas.Api.csproj

COPY . .
RUN dotnet publish src/GestorFrotas.Api/GestorFrotas.Api.csproj --configuration Release --output /aplicacao

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS execucao
WORKDIR /aplicacao
COPY --from=construcao /aplicacao .
EXPOSE 8080
ENTRYPOINT ["dotnet", "GestorFrotas.Api.dll"]
