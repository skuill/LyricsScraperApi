# Migrating from 1.x to 2.0

2.0 moves the service to .NET 10 and LyricsScraperNET 3.0 ([#1](https://github.com/skuill/LyricsScraperApi/issues/1)). The route and the search request stay the same; the error responses and the provider list change.

## Checklist

1. Docker: the service listens on port `8180` in the container by default. If you published `8080` (`-p 8180:8080`) without `ASPNETCORE_HTTP_PORTS`, change it to `-p 8180:8180`. See [Docker](#docker).
2. Do not send `Musixmatch` or `LyricFind` as `provider`: they are a 400 now. `Lrclib` is new.
3. Read the error code from `errorCode`, not from `title`: `title` is the standard HTTP phrase now ("Not Found").
4. Read validation errors from `errors` as an object of field name to messages (`{"artist": ["..."]}`), not as a list of `{field, message}`.
5. Handle 503 `Lyric.ProvidersUnavailable`: no provider could answer, retry later. In 1.x this was a 404.
6. Remove the handling of 403 `Lyric.RegionRestricted`: it is never returned.
7. The OpenAPI document moved from `/swagger/v1/swagger.json` to `/openapi/v1.json`. Swagger UI stays at `/swagger` and is available in the Docker image too.

## Providers

| Provider | 1.x | 2.0 |
|---|---|---|
| `Musixmatch` (3) | Searched, but the site returned decoy lyrics. | Removed: a request with it is a 400. |
| `LyricFind` (5) | Searched, but blocked by a bot check. | Removed: a request with it is a 400. |
| `Lrclib` (8) | Did not exist (new in LyricsScraperNET 3.0). | Added, searched first with `All`. |

The numbers of the other providers did not change. Details: [LyricsScraperNET providers](https://github.com/skuill/LyricsScraperNET/blob/main/docs/providers.md).

## Errors

Every error is ProblemDetails now (`application/problem+json`), with `instance` and `traceId`. Full list: [API reference](api.md#errors).

| Case | 1.x | 2.0 |
|---|---|---|
| Lyrics not found | 404, `title`: `Lyric.NotFound` | 404, `title`: `Not Found`, `errorCode`: `Lyric.NotFound` |
| Every provider failed (site down, timeout) | 404 `Lyric.NotFound` | 503, `errorCode`: `Lyric.ProvidersUnavailable` |
| Request rejected by the library | 400, `title`: `Lyric.BadRequest` | 400, `errorCode`: `Lyric.BadRequest` |
| No providers configured | 404 `Lyric.NotFound` | 500, `errorCode`: `Lyric.ServiceMisconfigured` |
| Lyrics not available in the region | 403 `Lyric.RegionRestricted` | Never returned (it came only from LyricFind). |
| Unexpected exception | 500, `type`: exception class, `detail`: exception message | 500, standard `type`; `detail` only in Development |

Validation errors, 1.x:

```json
{ "title": "Validation Failed", "status": 400, "errors": [ { "field": "Artist", "message": "The Artist is not specified in the request." } ] }
```

2.0 (the same format as the other 400 errors of ASP.NET Core, field names as in the request JSON):

```json
{ "title": "One or more validation errors occurred.", "status": 400, "errors": { "artist": [ "The Artist is not specified in the request." ] } }
```

## Request

The request body is the same, `requestType` can still be anywhere in the object. A request without it returns the error `errors.requestType` with the available values.

## Docker

| | 1.x | 2.0 |
|---|---|---|
| Runtime | .NET 8 (Alpine) | .NET 10 (Alpine) |
| Port in the container | `8080` unless `ASPNETCORE_HTTP_PORTS` was set (the README passed `8180`) | `8180` |
| Platforms | `linux/amd64` | `linux/amd64`, `linux/arm64` |
| Tags | `<version>`, `latest` | `<version>`, `<major>.<minor>`, `<major>`, `latest` |
| Health check | None | `HEALTHCHECK` on `/api/health` |
| Log level | `Debug` for everything, also ASP.NET Core internals | `Information` for the service, `Warning` for `Microsoft` and `System` (see [settings](docker.md#settings)) |
| HTTP to HTTPS redirect | When `ASPNETCORE_HTTPS_PORTS` was set | None: API clients usually do not follow redirects; use HTTPS directly or a reverse proxy |
| Exit code when the start fails | 0 | 1 |

Use the tag `2` to get fixes without breaking changes. More: [docker.md](docker.md).
