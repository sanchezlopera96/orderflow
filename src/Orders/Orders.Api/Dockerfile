# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# Se copian primero los csproj y los props para aprovechar la caché de restore.
COPY Directory.Build.props Directory.Packages.props ./
COPY src/BuildingBlocks/OrderFlow.BuildingBlocks.csproj src/BuildingBlocks/
COPY src/Orders/Orders.Domain/OrderFlow.Orders.Domain.csproj src/Orders/Orders.Domain/
COPY src/Orders/Orders.Infrastructure/OrderFlow.Orders.Infrastructure.csproj src/Orders/Orders.Infrastructure/
COPY src/Orders/Orders.Api/OrderFlow.Orders.Api.csproj src/Orders/Orders.Api/
RUN dotnet restore src/Orders/Orders.Api/OrderFlow.Orders.Api.csproj

# Luego el resto del código y se publica.
COPY src/ src/
RUN dotnet publish src/Orders/Orders.Api/OrderFlow.Orders.Api.csproj -c Release -o /app /p:UseAppHost=false

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "OrderFlow.Orders.Api.dll"]
