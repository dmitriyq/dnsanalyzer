FROM mcr.microsoft.com/dotnet/core/sdk:3.0-alpine AS build
WORKDIR /build

COPY . .
RUN dotnet restore -s https://api.nuget.org/v3/index.json -s https://nuget.cmim.ru "Dns.Resolver.Producer/Dns.Resolver.Producer.csproj"
RUN dotnet build "Dns.Resolver.Producer/Dns.Resolver.Producer.csproj" -c Release --no-restore -o /app
RUN dotnet publish "Dns.Resolver.Producer/Dns.Resolver.Producer.csproj" -c Release --no-restore -o /app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/runtime:3.0-alpine AS base
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Dns.Resolver.Producer.dll"]
