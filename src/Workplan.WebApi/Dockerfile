FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props ./
COPY src/Workplan.SharedKernel/Workplan.SharedKernel.csproj src/Workplan.SharedKernel/
COPY src/Workplan.Domain/Workplan.Domain.csproj src/Workplan.Domain/
COPY src/Workplan.Application/Workplan.Application.csproj src/Workplan.Application/
COPY src/Workplan.Infrastructure/Workplan.Infrastructure.csproj src/Workplan.Infrastructure/
COPY src/Workplan.WebApi/Workplan.WebApi.csproj src/Workplan.WebApi/
RUN dotnet restore src/Workplan.WebApi/Workplan.WebApi.csproj

COPY src/Workplan.SharedKernel/ src/Workplan.SharedKernel/
COPY src/Workplan.Domain/ src/Workplan.Domain/
COPY src/Workplan.Application/ src/Workplan.Application/
COPY src/Workplan.Infrastructure/ src/Workplan.Infrastructure/
COPY src/Workplan.WebApi/ src/Workplan.WebApi/

RUN dotnet publish src/Workplan.WebApi/Workplan.WebApi.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Npgsql probes for Kerberos/GSSAPI support on first connect; without this lib it logs a
# scary (but harmless) "Cannot load library libgssapi_krb5.so.2" line and falls back fine.
RUN apt-get update && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Workplan.WebApi.dll"]
