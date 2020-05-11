# tiver-fowl.Drivers ![.NET Core](https://img.shields.io/badge/.NET%20Core-3.1-blue) [![NuGet](https://img.shields.io/nuget/v/Tiver.Fowl.Drivers.svg)](https://www.nuget.org/packages/Tiver.Fowl.Drivers/) [![Codacy Badge](https://img.shields.io/codacy/grade/2ecdd0d0d6af44e480bb899af695e442/master.svg)](https://www.codacy.com/app/mr.hant/tiver-fowl.Drivers?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=MrHant/tiver-fowl.Drivers&amp;utm_campaign=Badge_Grade) [![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://raw.githubusercontent.com/MrHant/tiver-fowl/master/LICENSE)


Download WebDriver drivers' binaries from official sources
## Installation
* Add ```Tiver.Fowl.Drivers``` nuget package to solution
* Add downloaders configuration to ```app.config``` file. Sample configuration can be found below
* Add calls to execute downloaders. Sample usage can be found below

## Downloaders
* ChromeDriverDownloader - downloads binaries from http://chromedriver.storage.googleapis.com/
  * ```LATEST_RELEASE``` version can be used to download latest driver version

## Sample usage
### Download chrome driver of specific version

```c#
var result = new ChromeDriverDownloader().DownloadBinary("76.0.3809.25", "win32");
```

### Download configured "chrome" driver

```c#
var result = Downloaders.DownloadBinaryFor("chrome");
```



## Sample configuration

Configuration is stored in file `Tiver_config.json`

### Config for ChromeDriver v76.0.3809.25

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
