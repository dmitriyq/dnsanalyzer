FROM mcr.microsoft.com/dotnet/core/sdk:3.0.100-preview5-bionic AS build
WORKDIR /build

COPY . .
WORKDIR /build/Dns.Site
RUN curl -sL https://deb.nodesource.com/setup_10.x | bash -
RUN apt update && apt install nodejs -y
RUN npm install -g typescript && npm install -g @vue/cli && npm install vue
WORKDIR /build/Dns.Site
RUN ./node_modules/.bin/vue-cli-service build
WORKDIR /build
RUN dotnet restore -s https://api.nuget.org/v3/index.json -s https://nuget.cmim.ru "Dns.Site/Dns.Site.csproj"
RUN dotnet build "Dns.Site/Dns.Site.csproj" -c Release --no-restore -o /app
RUN dotnet publish "Dns.Site/Dns.Site.csproj" -c Release --no-restore -o /app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/aspnet:3.0.0-preview5-bionic AS base
# install System.Drawing native dependencies
RUN apt-get update \
    && apt-get install -y --allow-unauthenticated \
        libc6-dev \
        libgdiplus \
        libx11-dev \
     && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Dns.Site.dll"]