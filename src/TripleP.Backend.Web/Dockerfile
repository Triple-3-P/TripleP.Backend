# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
# Npgsql probes GSSAPI on Linux; without this package every PostgreSQL connection logs a native load error.
USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TripleP.Backend.Abstractions/TripleP.Backend.Abstractions.csproj", "TripleP.Backend.Abstractions/"]
COPY ["TripleP.Backend.Database/TripleP.Backend.Database.csproj", "TripleP.Backend.Database/"]
COPY ["TripleP.Backend.Logic/TripleP.Backend.Logic.csproj", "TripleP.Backend.Logic/"]
COPY ["TripleP.Backend.RestApi/TripleP.Backend.RestApi.csproj", "TripleP.Backend.RestApi/"]
COPY ["TripleP.Backend.Web/TripleP.Backend.Web.csproj", "TripleP.Backend.Web/"]
RUN dotnet restore "./TripleP.Backend.Web/TripleP.Backend.Web.csproj"
COPY . .
WORKDIR "/src/TripleP.Backend.Web"
RUN dotnet build "./TripleP.Backend.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TripleP.Backend.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage builds a framework-dependent EF Core migrations bundle. Docker compose runs it in the
# app container before the application starts; the bundle uses the .NET runtime from the base image.
# Keep EF_TOOL_VERSION in sync with dotnet-tools.json.
FROM build AS migrations
ARG BUILD_CONFIGURATION=Release
ARG EF_TOOL_VERSION=10.0.13
RUN dotnet tool install --tool-path /tools dotnet-ef --version $EF_TOOL_VERSION
RUN /tools/dotnet-ef migrations bundle \
    --project "/src/TripleP.Backend.Database/TripleP.Backend.Database.csproj" \
    --startup-project "/src/TripleP.Backend.Web/TripleP.Backend.Web.csproj" \
    --configuration $BUILD_CONFIGURATION \
    --output /app/efbundle

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=migrations /app/efbundle ./efbundle
ENTRYPOINT ["dotnet", "TripleP.Backend.Web.dll"]