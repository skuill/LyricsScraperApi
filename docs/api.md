# API reference

One endpoint searches lyrics: `POST /api/LyricsScraper`. The full description is the OpenAPI document of the running service:

| What | Address |
|---|---|
| Swagger UI | `/swagger` |
| OpenAPI document (3.1) | `/openapi/v1.json` |
| Health check | `GET /api/health` (`Healthy`, 200) |

Both are available in every environment, also in the Docker image (Production).

## Search request

The JSON property `requestType` selects the kind of search. It can be anywhere in the object.

### By artist and song

```json
{
  "requestType": "ArtistAndSong",
  "artist": "Parkway Drive",
  "song": "Idols And Anchors",
  "provider": "All"
}
```

| Property | Required | Notes |
|---|---|---|
| `requestType` | yes | `ArtistAndSong` |
| `artist` | yes | Not empty. |
| `song` | yes | Not empty. |
| `provider` | no | One of the [providers](#providers), default `All`. |

### By the address of a lyrics page

```json
{
  "requestType": "Uri",
  "uri": "https://genius.com/Parkway-drive-idols-and-anchors-lyrics",
  "provider": "All"
}
```

| Property | Required | Notes |
|---|---|---|
| `requestType` | yes | `Uri` |
| `uri` | yes | Address of a page of one of the providers. |
| `provider` | no | One of the [providers](#providers), default `All`. |

### Providers

`provider` takes the name of a provider of [LyricsScraperNET](https://github.com/skuill/LyricsScraperNET/blob/main/docs/providers.md):

| Value | Site |
|---|---|
| `All` | Every provider, in the order of their priority. |
| `AZLyrics` | azlyrics.com |
| `Genius` | genius.com |
| `SongLyrics` | songlyrics.com |
| `KPopLyrics` | kpoplyrics.net |
| `LyricsFreak` | lyricsfreak.com |
| `Lrclib` | lrclib.net |

Any other value (for example `Musixmatch` or `LyricFind`, removed in 2.0.0) is a 400.

## Responses

### 200: lyrics found

```json
{
  "instrumental": false,
  "externalProvider": "Lrclib",
  "lyricText": "..."
}
```

| Property | Notes |
|---|---|
| `instrumental` | `true` for a song without words; `lyricText` is empty then. |
| `externalProvider` | The provider that found the lyrics. |
| `lyricText` | The lyrics. |

### Errors

Every error is [ProblemDetails](https://www.rfc-editor.org/rfc/rfc9457) (`application/problem+json`). Errors of the search also have `errorCode`; check it in code, not `detail`, which is a message for people.

| Status | `errorCode` | When |
|---|---|---|
| 400 | (none, `errors` per field) | Invalid JSON, unknown `requestType` or `provider`, an empty `artist`, `song` or `uri`. |
| 400 | `Lyric.BadRequest` | The library rejected the request, for example a provider that is not available. |
| 404 | `Lyric.NotFound` | At least one provider answered and none has the lyrics. |
| 503 | `Lyric.ProvidersUnavailable` | No provider could answer (site down, timeout, error). The lyrics may exist: try again later. |
| 500 | `Lyric.ServiceMisconfigured` | The service has no providers to search. |
| 500 | `Lyric.SearchError` | An unexpected result of the library. |
| 500 | (none) | An unexpected error; `detail` is empty outside Development. |

A search error:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "Lyric not found. Search request: Artist: Parkway Drive. Song: Unknown. Provider: All.",
  "instance": "POST /api/LyricsScraper",
  "errorCode": "Lyric.NotFound",
  "traceId": "00-42b14a0776aeee397ee53b758065c952-30c51129770bf948-00"
}
```

A validation error:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "instance": "POST /api/LyricsScraper",
  "errors": {
    "requestType": [ "The requestType is not specified in the request. Available values: ArtistAndSong, Uri." ]
  },
  "traceId": "00-8c30c8228fa636489c5a7f75541878b2-6a55578b222723f3-00"
}
```

`traceId` is also in the logs of the service, so one failed request can be found there.

## Examples

- [`src/LyricsScraper.http`](../src/LyricsScraper.http) for Visual Studio and VS Code (REST Client).
- [Postman collection and environment](../postman).
