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
A .NET-based Web API that allows users to search for lyrics of a song from multiple external providers (like AZLyrics, Genius, SongLyrics, etc.). It supports both artist/song and direct URL-based search requests.
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

Once the image is pulled, run the container using the following command (using HTTP for example):

```bash
docker run -d -p 8180:8180 -p 8181:8181 -e ASPNETCORE_HTTP_PORTS=8180 --name lyrics-scraper-api skuill/lyrics-scraper-api
```

### Access the API

Once the container is running, your API will be available at the following addresses:

http://localhost:8180 (or https://localhost:8181 if environment ASPNETCORE_HTTPS_PORTS is set)


The healthcheck is configured here:

http://localhost:8180/api/health

### Using Postman to test API

The Postman environment and collection available in folder [postman](https://github.com/skuill/LyricsScraperApi/tree/main/postman)

## Support
If you want to support this project or my work in general, you can donate via the links below.

This is always optional! Thank you! 😉

 * [!["Buy Me A Coffee"](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/skuill)
 * [!["PayPal Donate Button"](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://paypal.me/VitalyKvashin)
 * Crypto. Send only on the networks listed, a coin sent on another network may be lost:

| Network | Coins | Address |
|---|---|---|
| Bitcoin | BTC | `bc1q02d7p4qhjhjn5mnu99wdfc2j06d364jt8qeq0c` |
| Ethereum, Base, Arbitrum, Polygon, BNB Chain | ETH, USDT, USDC | `0xa56541B7DF6263ab84c3e1B69bD37f2a6d848412` |
| Solana | SOL, USDT, USDC | `BZcuV721p9XPsdreiKY9rsmxZDVf8qHNRk3EoPp25nrS` |
| Tron | USDT (TRC-20) | `TGyoTufYzVPU6igcQmFGwH1SFrNes9q3UP` |
