FROM mcr.microsoft.com/dotnet/core/sdk:3.0.100-preview5-buster AS build
WORKDIR /build

COPY . .
RUN dotnet restore -s https://api.nuget.org/v3/index.json -s https://nuget.cmim.ru "Dns.Resolver/Dns.Resolver.csproj"
RUN dotnet build "Dns.Resolver/Dns.Resolver.csproj" -c Release --no-restore -o /app
RUN dotnet publish "Dns.Resolver/Dns.Resolver.csproj" -c Release --no-restore -o /app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/runtime:3.0.0-preview5-buster-slim AS base
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Dns.Resolver.dll"]