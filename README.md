# tiver-fowl.Drivers ![.NET](https://img.shields.io/badge/.NET-6-blue)  [![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://raw.githubusercontent.com/MrHant/tiver-fowl/master/LICENSE) [![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2FMrHant%2Ftiver-fowl.Drivers.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2FMrHant%2Ftiver-fowl.Drivers?ref=badge_shield)


Download WebDriver drivers' binaries from official sources

## Branch status

| Branch | Package | CI  |
| ------ | ------- | --- |
| master (stable) | [![NuGet](https://img.shields.io/nuget/v/Tiver.Fowl.Drivers.svg)](https://www.nuget.org/packages/Tiver.Fowl.Drivers) | [![Build status](https://ci.appveyor.com/api/projects/status/s6xak6m2jc2mijnt/branch/master?svg=true)](https://ci.appveyor.com/project/MrHant/tiver-fowl-drivers/branch/master) |
| develop | [![NuGet Pre Release](https://img.shields.io/nuget/vpre/Tiver.Fowl.Drivers.svg)](https://www.nuget.org/packages/Tiver.Fowl.Drivers/absoluteLatest) | [![Build status](https://ci.appveyor.com/api/projects/status/s6xak6m2jc2mijnt/branch/develop?svg=true)](https://ci.appveyor.com/project/MrHant/tiver-fowl-drivers/branch/develop) |

## Installation
* Add ```Tiver.Fowl.Drivers``` nuget package to solution
* Add downloaders configuration to ```app.config``` file. Sample configuration can be found below
* Add calls to execute downloaders. Sample usage can be found below

### .NET support
* Package is targeting .NET 6 and .NET Standard 2.0
* Tests executed on .NET 6, .NET Framework 4.8, .NET Framework 4.7.2, .NET Framework 4.6.2

## Downloaders
* ChromeDriverDownloader - downloads binaries from http://chromedriver.storage.googleapis.com/
  * ```LATEST_RELEASE``` version can be used to download latest driver version
  * ```LATEST_RELEASE_XXX``` version can be used to download latest driver with version XXX

## Sample usage
### Download chrome driver of specific version

```c#
var result = new ChromeDriverDownloader().DownloadBinary("76.0.3809.25", "win32");
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

### Sample config for ChromeDriver v76.0.3809.25

```json
{
  "Tiver.Fowl.Drivers": {
    "HttpTimeout": 120,
    "Drivers": [
      {
        "Name": "chrome",
        "DownloaderType": "ChromeDriverDownloader",
        "Version": "76.0.3809.25",
        "Platform": "win32"
      }
    ]
  }
}
```
