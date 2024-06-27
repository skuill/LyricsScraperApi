#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8180
EXPOSE 8181

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["LyricsScraperApi/LyricsScraperApi.csproj", "LyricsScraperApi/"]
RUN dotnet restore "./LyricsScraperApi/LyricsScraperApi.csproj"
COPY . .
WORKDIR "/src/LyricsScraperApi"
RUN dotnet build "./LyricsScraperApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./LyricsScraperApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LyricsScraperApi.dll"]