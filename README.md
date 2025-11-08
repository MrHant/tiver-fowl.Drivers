# tiver-fowl.Drivers ![.NET](https://img.shields.io/badge/.NET-9-blue) 

Download WebDriver drivers' binaries from official sources

## Branch status

| Branch | Package | CI  |
| ------ | ------- | --- |
| master (stable) | [![NuGet](https://img.shields.io/nuget/v/Tiver.Fowl.Drivers.svg)](https://www.nuget.org/packages/Tiver.Fowl.Drivers) | [![Test & Publish NuGet package](https://github.com/MrHant/tiver-fowl.Drivers/actions/workflows/dotnet.yml/badge.svg?branch=master)](https://github.com/MrHant/tiver-fowl.Drivers/actions/workflows/dotnet.yml) |
| develop | [![NuGet Pre Release](https://img.shields.io/nuget/vpre/Tiver.Fowl.Drivers.svg)](https://www.nuget.org/packages/Tiver.Fowl.Drivers/absoluteLatest) | [![Test & Publish NuGet package](https://github.com/MrHant/tiver-fowl.Drivers/actions/workflows/dotnet.yml/badge.svg?branch=develop)](https://github.com/MrHant/tiver-fowl.Drivers/actions/workflows/dotnet.yml) |

## Installation
* Add ```Tiver.Fowl.Drivers``` nuget package to solution
* Add downloaders configuration to ```app.config``` file. Sample configuration can be found below
* Add calls to execute downloaders. Sample usage can be found below

### .NET support
* Package is targeting .NET 9 and .NET Standard 2.0
* Tests executed on .NET 9

## Downloaders
* **ChromeDriverDownloader** - downloads binaries from the Chrome for Testing feeds (https://googlechromelabs.github.io/chrome-for-testing/).
  * ```LATEST_RELEASE``` version resolves to the latest stable Chrome for Testing build.
  * ```LATEST_RELEASE_XXX``` version resolves to the newest known-good build for milestone ```XXX```.
  * *Note:* For chromedriver v114 or older - use *ChromeDriverOldDownloader*
* **ChromeDriverOldDownloader** - downloader for older versions of chromedriver. Uses http://chromedriver.storage.googleapis.com/ and should only be used when you explicitly need a driver version <= v114.

## Sample usage
### Download chrome driver of specific version

```c#
var result = new ChromeDriverDownloader().DownloadBinary("142.0.7444.61", "win32");
```

### Download configured "chrome" driver

```c#
var result = Downloaders.DownloadBinaryFor("chrome");
```

## Configuration

Configuration is stored in file `Tiver_config.json`. 

Top-level element is an object with key "Tiver.Fowl.Drivers".

### Config values:

* **HttpTimeout** - maximum time for HTTP requests, in seconds. Default value - 120 seconds.
* **DownloadLocation** - location which would be used to store downloaded drivers. Default value - location of executing assembly
* **Drivers** - array of drivers configurations
  * **Name** - reference name for driver configuration
  * **DownloaderType** - which downloader to use
  * **Version** - required version of driver
  * **Platform** - required platform of driver

### Sample config for ChromeDriver v142.0.7444.61

```json
{
  "Tiver.Fowl.Drivers": {
    "HttpTimeout": 120,
    "Drivers": [
      {
        "Name": "chrome",
        "DownloaderType": "ChromeDriverDownloader",
        "Version": "142.0.7444.61",
        "Platform": "win32"
      }
    ]
  }
}
```
