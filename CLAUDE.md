# LyricsScraperApi (REST API over LyricsScraperNET)

Notes for AI coding agents working in this repository. Personal or machine-specific notes go to `CLAUDE.local.md` (git-ignored).

## Build
- ASP.NET Core on `net10.0` (set in `src/Directory.Build.props`), solution `src/LyricsScraperApi.sln`. Package versions only in `src/Directory.Packages.props` (central package management): `PackageReference` items have no `Version`.
- `dotnet build src/LyricsScraperApi.sln --configuration Release`
- Lint (same as CI): `dotnet format src/LyricsScraperApi.sln --verify-no-changes`. Rules in `.editorconfig` (IDE0005, IDE0130), also checked in the build (`EnforceCodeStyleInBuild`).
- CS1591 (missing XML comment) is off: write XML comments where they help the OpenAPI document (DTOs, controller actions).

## Layout
- One endpoint: `POST api/LyricsScraper` (`Controllers/LyricsScraperController.cs`); health `/api/health`; OpenAPI `/openapi/v1.json` and Swagger UI `/swagger` in every environment.
- Flow: controller → `Services/LyricsScraperService` (maps `SearchResult.Code` to errors) → `ILyricsScraperClient` (singleton, `WithAllProviders()`).
- Request DTOs are polymorphic with System.Text.Json attributes (`[JsonPolymorphic]` on `SearchRequestBaseDto`, discriminator `requestType`). The base stays abstract: the OpenAPI generator emits `anyOf` and the discriminator only for an abstract base. `Formatters/SearchRequestJsonInputFormatter` turns the `NotSupportedException` of a missing `requestType` into a 400.
- Errors: `Models/Errors` (`BaseError` with `Code`, `LyricsScraperErrors`), `Handlers/GlobalErrorHandler` (ProblemDetails with the `errorCode` extension), `Handlers/GlobalExceptionHandler` (unexpected exceptions), `Filters/GlobalRequestValidation` (FluentValidation to ValidationProblemDetails).
- Result pattern in `ResultPattern/`, manual mapping in `Mapping/LyricsScraperProfile.cs`.
- Docs: `README.md` (landing page), `docs/` (api, docker, development, migration guides).

## Tests
- xunit.v3 on Microsoft Testing Platform (`global.json`), FakeItEasy, Shouldly.
- Same as CI, from the repository root:
  - `dotnet test src/LyricsScraperApi.UnitTests --configuration Release -- --coverage --coverage-output-format cobertura`
  - `dotnet test src/LyricsScraperApi.IntegrationTests --configuration Release -- --coverage --coverage-output-format cobertura`
- Integration tests run the real app in memory (`WebApplicationFactory<Program>`, environment Production) with a fake `ILyricsScraperClient`: no network.
- Pass `TestContext.Current.CancellationToken` to async calls in tests (xUnit1051).
- New behavior needs tests: whole objects in asserts, verify the calls of the library client. Mutation check for every fix or new behavior: break it, run the tests, at least one must fail, restore.

## API contract
- `ExternalProviderTypeDto` maps to `LyricsScraperNET.Providers.Models.ExternalProviderType` by number: same members and numbers (`MappingProfileTests` checks it). Never renumber.
- Response and error formats are public API: a change is a major version and goes to `docs/migration-<from>-to-<to>.md`. Update `docs/api.md`, `src/LyricsScraper.http` and `postman/` in the same PR.

## Docker and release
- `src/LyricsScraperApi/Dockerfile`, build context `src`. Multi-platform (`linux/amd64`, `linux/arm64`) without emulation: the SDK stage uses `--platform=$BUILDPLATFORM` and `-a $TARGETARCH`. Keep the `base` stage: Visual Studio fast mode uses it.
- `.github/workflows/cicd.yml`: build and tests on push and PR, Docker build without push; a published GitHub release (plain version tag, e.g. `2.0.0`) pushes `skuill/lyrics-scraper-api` with the tags `<version>`, `<major>.<minor>`, `<major>`, `latest`. No server deploy.

## Conventions
- English only in code, comments, docs and commit messages.
- Branches `feature/<issue>_<desc>`, `bugfix/<issue>_<desc>`; pull requests target `main`; commit messages and PR titles start with the issue numbers, e.g. `#1 ...`.
- Follow existing patterns; do not silently change working configs (CI, Docker, lint, coverage).
- Trust code over docs when they disagree.
- Never open secrets (user secrets, certificates, `.env` values).
- Skip `bin/ obj/ .vs/ TestResults/ logs/` when searching.
