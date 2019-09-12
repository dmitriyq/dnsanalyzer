FROM mcr.microsoft.com/dotnet/core/sdk:3.0-alpine AS build
WORKDIR /build

COPY . .
RUN dotnet restore -s https://api.nuget.org/v3/index.json -s https://nuget.cmim.ru "Dns.Resolver.Aggregator/Dns.Resolver.Aggregator.csproj"
RUN dotnet build "Dns.Resolver.Aggregator/Dns.Resolver.Aggregator.csproj" -c Release --no-restore -o /app
RUN dotnet publish "Dns.Resolver.Aggregator/Dns.Resolver.Aggregator.csproj" -c Release --no-restore -o /app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/runtime:3.0-alpine AS base
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Dns.Resolver.Aggregator.dll"]
