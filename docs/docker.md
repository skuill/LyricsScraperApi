# Docker

The image is [`skuill/lyrics-scraper-api`](https://hub.docker.com/r/skuill/lyrics-scraper-api) on Docker Hub, for `linux/amd64` and `linux/arm64`.

## Run

```bash
docker run -d -p 8180:8180 --name lyrics-scraper-api skuill/lyrics-scraper-api:2
```

Then open http://localhost:8180/swagger, or check http://localhost:8180/api/health.

## Image tags

A release `2.1.3` is pushed as:

| Tag | Use |
|---|---|
| `2.1.3` | Exactly this release. |
| `2.1` | Latest patch of 2.1. |
| `2` | Latest release of 2.x: new features and fixes, no breaking changes. |
| `latest` | Latest release; can be a new major version with breaking changes. |

## Settings

| Variable | Default | Notes |
|---|---|---|
| `ASPNETCORE_HTTP_PORTS` | `8180` | Port (or ports, `;`-separated) inside the container. The health check uses the first one. An invalid value does not stop the start: ASP.NET Core falls back to port 80 and the container becomes `unhealthy`. |
| `ASPNETCORE_HTTPS_PORTS` | not set | HTTPS also needs a certificate, see [Hosting ASP.NET Core images with Docker over HTTPS](https://learn.microsoft.com/aspnet/core/security/docker-https). Port `8181` is exposed for it. |
| `ASPNETCORE_ENVIRONMENT` | `Production` | `Development` adds the exception message to 500 responses. |
| `Serilog__MinimumLevel__Default` | `Information` | Log level of the service. ASP.NET Core and .NET (`Microsoft`, `System`) log from `Warning`: to see their details, for example `-e Serilog__MinimumLevel__Override__Microsoft=Information`. Any setting of `appsettings.json` can be set this way (`__` between sections). |

The container runs as the non-root user `app`.

## Logs

- Console: `docker logs lyrics-scraper-api`. A failed search lists what each provider did, for example `Providers: Lrclib: Failed (600 ms): HTTP 503; AZLyrics: NotFound (297 ms); ...`.
- Files: `/app/logs/webapi-<date>.log` inside the container, a new file every day and at 1 MB, the last 20 files are kept. They are lost with the container; to keep them, mount a volume: `-v lyrics-scraper-logs:/app/logs`.

## Health check

The image has a `HEALTHCHECK` that calls `/api/health` every 30 seconds, so `docker ps` shows `healthy` or `unhealthy`:

```bash
docker inspect --format "{{.State.Health.Status}}" lyrics-scraper-api
```

## Build locally

From the `src` folder (the build context):

```bash
docker build -f LyricsScraperApi/Dockerfile -t lyrics-scraper-api .
```

The SDK stage runs on the platform of your machine and compiles for the target one, so another platform builds without emulation:

```bash
docker buildx build --platform linux/arm64 -f LyricsScraperApi/Dockerfile -t lyrics-scraper-api:arm64 .
```

The build argument `VERSION` (default `0.0.0-dev`) sets the version of the application, shown in the OpenAPI document. CI passes the release tag.

## Visual Studio

The solution has a Docker Compose project (`src/docker-compose.dcproj`): select the "Docker Compose" profile and start debugging. `docker-compose.override.yml` maps the ports `8180` (HTTP) and `8181` (HTTPS) and mounts your user secrets and development certificate.
