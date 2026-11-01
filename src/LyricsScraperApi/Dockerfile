#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
USER app
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8180
EXPOSE 8180
EXPOSE 8181
# wget comes with Alpine (BusyBox). The port is the first one of ASPNETCORE_HTTP_PORTS, so an override with -e still works.
HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
    CMD wget -q -O /dev/null "http://localhost:${ASPNETCORE_HTTP_PORTS%%;*}/api/health" || exit 1

# The SDK runs on the platform of the build machine and compiles for the target one (-a), so a multi-platform
# image (linux/amd64, linux/arm64) does not need emulation. See https://aka.ms/dotnet/containers/multiplatform
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
ARG TARGETARCH
WORKDIR /src
COPY ["Directory.Build.props", "Directory.Packages.props", "./"]
COPY ["LyricsScraperApi/LyricsScraperApi.csproj", "LyricsScraperApi/"]
RUN dotnet restore "./LyricsScraperApi/LyricsScraperApi.csproj" -a $TARGETARCH
COPY . .
WORKDIR "/src/LyricsScraperApi"

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG TARGETARCH
# The release tag (CI passes it), shown in the OpenAPI document.
ARG VERSION=0.0.0-dev
RUN dotnet publish "./LyricsScraperApi.csproj" -c $BUILD_CONFIGURATION -a $TARGETARCH --no-restore -o /app/publish /p:UseAppHost=false /p:Version=$VERSION

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LyricsScraperApi.dll"]
