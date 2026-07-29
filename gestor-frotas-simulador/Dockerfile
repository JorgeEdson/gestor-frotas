FROM mcr.microsoft.com/dotnet/sdk:10.0 AS construcao
WORKDIR /origem

COPY src/GestorFrotas.Simulador/GestorFrotas.Simulador.csproj src/GestorFrotas.Simulador/
RUN dotnet restore src/GestorFrotas.Simulador/GestorFrotas.Simulador.csproj

COPY . .
RUN dotnet publish src/GestorFrotas.Simulador/GestorFrotas.Simulador.csproj --configuration Release --output /aplicacao

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS execucao
WORKDIR /aplicacao
COPY --from=construcao /aplicacao .
ENTRYPOINT ["dotnet", "GestorFrotas.Simulador.dll"]
