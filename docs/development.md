# Development

## Requirements

- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0).
- Docker, only to build or run the image.

## Layout

| Path | What |
|---|---|
| `src/LyricsScraperApi.sln` | Solution. |
| `src/LyricsScraperApi/` | The service: `Controllers/`, `Services/` (calls LyricsScraperNET), `Models/` (DTOs and errors), `Mapping/`, `Validators/` (FluentValidation), `Filters/`, `Formatters/`, `Handlers/` (errors and exceptions to ProblemDetails). |
| `src/LyricsScraperApi.UnitTests/` | Unit tests, folders mirror the service. |
| `src/LyricsScraperApi.IntegrationTests/` | The real app in memory (`WebApplicationFactory`) with a fake library client. |
| `src/Directory.Build.props` | Target framework and build settings of all projects. |
| `src/Directory.Packages.props` | All package versions ([central package management](https://learn.microsoft.com/nuget/consume-packages/central-package-management)). |
| `global.json` | Runs the tests on Microsoft Testing Platform. |
| `.editorconfig` | Code style rules checked in the build and in CI. |

## Run

```bash
dotnet run --project src/LyricsScraperApi
```

Then open http://localhost:5277/swagger. With Docker: [docker.md](docker.md).

## Test

Tests use xUnit v3 on [Microsoft Testing Platform](https://learn.microsoft.com/dotnet/core/testing/microsoft-testing-platform-intro), FakeItEasy and Shouldly. Same commands as CI, from the repository root:

```bash
dotnet test src/LyricsScraperApi.UnitTests --configuration Release -- --coverage --coverage-output-format cobertura
dotnet test src/LyricsScraperApi.IntegrationTests --configuration Release -- --coverage --coverage-output-format cobertura
```

New behavior comes with tests: FakeItEasy for the library client, whole objects in the asserts, calls of the client verified.

## Code style

```bash
dotnet format src/LyricsScraperApi.sln --verify-no-changes
```

CI fails when this command finds something. The rules are in `.editorconfig` (for now IDE0005 unused usings and IDE0130 namespace equals folder); `dotnet format src/LyricsScraperApi.sln` fixes them.

## CI and release

`.github/workflows/cicd.yml`:

| Event | Jobs |
|---|---|
| Push or pull request to `main` | Lint, build, unit and integration tests with coverage to Codecov, Docker build for `linux/amd64` and `linux/arm64` without push. |
| GitHub release published | The same, then push of the image to Docker Hub with the tags from [docker.md](docker.md#image-tags). |

To release:

1. Merge the changes to `main`.
2. Create a GitHub release with a new tag, the plain version (for example `2.0.0`), target `main`, and publish it.
3. Check the workflow run and the new tags on Docker Hub.

The tag is the version: it goes to the image tags and, through the build argument `VERSION`, to the application.
