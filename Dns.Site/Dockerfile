FROM mcr.microsoft.com/dotnet/core/sdk:3.0.100-preview5-buster AS build-dotnet
WORKDIR /build

COPY . .
RUN dotnet restore -s https://api.nuget.org/v3/index.json -s https://nuget.cmim.ru "Dns.Site/Dns.Site.csproj"
RUN dotnet build "Dns.Site/Dns.Site.csproj" -c Release --no-restore -o /app
RUN dotnet publish "Dns.Site/Dns.Site.csproj" -c Release --no-restore -o /app
WORKDIR /app

FROM node:lts-alpine AS build-vue
COPY . .
RUN npm install
RUN npm run publsih


FROM mcr.microsoft.com/dotnet/core/aspnet:3.0.0-preview5-buster-slim AS base
WORKDIR /app
COPY --from=build-dotnet /app .
COPY --from=build-vue /wwwroot /app/
ENTRYPOINT ["dotnet", "Auth.MVC.dll"]