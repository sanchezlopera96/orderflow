# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY Directory.Build.props Directory.Packages.props ./
COPY src/BuildingBlocks/OrderFlow.BuildingBlocks.csproj src/BuildingBlocks/
COPY src/Inventory/Inventory.Domain/OrderFlow.Inventory.Domain.csproj src/Inventory/Inventory.Domain/
COPY src/Inventory/Inventory.Infrastructure/OrderFlow.Inventory.Infrastructure.csproj src/Inventory/Inventory.Infrastructure/
COPY src/Inventory/Inventory.Worker/OrderFlow.Inventory.Worker.csproj src/Inventory/Inventory.Worker/
RUN dotnet restore src/Inventory/Inventory.Worker/OrderFlow.Inventory.Worker.csproj

COPY src/ src/
RUN dotnet publish src/Inventory/Inventory.Worker/OrderFlow.Inventory.Worker.csproj -c Release -o /app /p:UseAppHost=false

# ---- runtime (el worker no expone HTTP: imagen runtime, más liviana que aspnet) ----
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "OrderFlow.Inventory.Worker.dll"]
