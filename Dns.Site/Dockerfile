FROM mcr.microsoft.com/dotnet/core/sdk:3.0.100-preview5-bionic AS build
WORKDIR /build

COPY . .
RUN sudo apt update && sudo apt install node-typescript -y
RUN dotnet restore -s https://api.nuget.org/v3/index.json -s https://nuget.cmim.ru "Dns.Site/Dns.Site.csproj"
RUN dotnet build "Dns.Site/Dns.Site.csproj" -c Release --no-restore -o /app
RUN dotnet publish "Dns.Site/Dns.Site.csproj" -c Release --no-restore -o /app
RUN npm run publsih
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/aspnet:3.0.0-preview5-bionic AS base
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Auth.MVC.dll"]