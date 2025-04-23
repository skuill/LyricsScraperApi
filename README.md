<h1 align="center">LyricsScraperApi</h1>
<p align="center">
  <a href="https://github.com/skuill/LyricsScraperApi/actions/workflows/cicd.yml" ><img src="https://github.com/skuill/LyricsScraperApi/actions/workflows/cicd.yml/badge.svg"/>
  <a href="https://www.codefactor.io/repository/github/skuill/lyricsscraperapi"><img src="https://www.codefactor.io/repository/github/skuill/lyricsscraperapi/badge" alt="CodeFactor" /></a>
  <a href="https://codecov.io/gh/skuill/LyricsScraperApi" ><img src="https://codecov.io/gh/skuill/LyricsScraperApi/graph/badge.svg?token=7S3JL4G5U0"/>
  <a href="https://hub.docker.com/r/skuill/lyrics-scraper-api">
    <img src="https://img.shields.io/docker/v/skuill/lyrics-scraper-api" alt="Docker Image Tag"/>
  </a>
  <a href="https://hub.docker.com/r/skuill/lyrics-scraper-api">
    <img src="https://img.shields.io/docker/pulls/skuill/lyrics-scraper-api" alt="Docker Pulls"/>
  </a>
 </a>
</p>
<p align="center">
A .NET API service to search for lyrics of a song from the web.
</p>
<img src="https://github.com/skuill/LyricsScraperApi/blob/main/resources/swagger.png">

## Feedback

Feel free to send me feedback on [Telegram](https://t.me/skuill) or [file an issue](https://github.com/skuill/LyricsScraperApi/issues). Feature requests are always welcome.

## Technologies
Project is created with:
* `.NET: 8.0`
* `Microsoft Visual Studio Community 2022`
* `docker`

## Built with
* [LyricsScraperNET](https://github.com/skuill/LyricsScraperNET) - 🎼 A a versatile .NET library that provides an API for searching song lyrics from the web.
* [Serilog](https://serilog.net/) - Simple .NET logging with fully-structured events
* [Shouldly](https://github.com/shouldly/shouldly) - An assertion framework which focuses on giving great error messages when the assertion fails while being simple and terse.
* [FakeItEasy](https://fakeiteasy.github.io/) - A .Net dynamic fake framework for creating all types of fake objects, mocks, stubs etc.
* [FluentValidation](https://fluentvalidation.net/) - A validation library for .NET that uses a fluent interface to construct strongly-typed validation rules.
* [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) - Swagger tools for documenting APIs built on ASP.NET Core
* [xUnit](https://xunit.net/) - A free, open source, community-focused unit testing tool for the .NET Framework.

## Roadmap
Visit [issue board](https://github.com/skuill/LyricsScraperApi/issues).

## Running the Project in Docker

You can pull the Docker image for this project from Docker Hub.

### Pull the Docker image

First, pull the pre-built Docker image from Docker Hub

```bash
docker pull skuill/lyrics-scraper-api
```

### Run the Docker container

Once the image is pulled, run the container using the following command:

```bash
docker run -d -p 8180:8180 -p 8181:8181 -e ASPNETCORE_HTTP_PORTS=8180 -e ASPNETCORE_HTTPS_PORTS=8181 --name lyrics-scraper-api skuill/lyrics-scraper-api
```

### Access the API

Once the container is running, your API will be available at the following addresses:

http://localhost:8180 or https://localhost:8181


The healthcheck is configured here:

https://localhost:8181/api/health
