FROM mcr.microsoft.com/dotnet/core/sdk:3.0.100-preview5-buster AS build
WORKDIR /build

COPY . .
RUN dotnet restore -s https://api.nuget.org/v3/index.json -s https://nuget.cmim.ru "Dns.Analyzer/Dns.Analyzer.csproj"
RUN dotnet build "Dns.Analyzer/Dns.Analyzer.csproj" -c Release --no-restore -o /app
RUN dotnet publish "Dns.Analyzer/Dns.Analyzer.csproj" -c Release --no-restore -o /app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/runtime:3.0.0-preview5-buster-slim AS base
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Dns.Analyzer.dll"]