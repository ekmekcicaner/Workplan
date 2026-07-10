FROM node:22-alpine AS assets
WORKDIR /src
COPY src/Workplan.Client/package.json ./
RUN npm install
COPY src/Workplan.Client/ ./
RUN npm run tailwind:build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props ./
COPY src/Workplan.Client/Workplan.Client.csproj src/Workplan.Client/
RUN dotnet restore src/Workplan.Client/Workplan.Client.csproj

COPY src/Workplan.Client/ src/Workplan.Client/
COPY --from=assets /src/wwwroot/css/app.css src/Workplan.Client/wwwroot/css/app.css

# Blazor WASM runs in the browser and can't read container env vars, so the
# API base URL is baked in at build time: swap the dev default for the
# Docker-network-published one before publishing.
COPY src/Workplan.Client/wwwroot/appsettings.Docker.json src/Workplan.Client/wwwroot/appsettings.json

RUN dotnet publish src/Workplan.Client/Workplan.Client.csproj -c Release -o /app/publish --no-restore

FROM nginx:alpine AS final
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY src/Workplan.Client/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
