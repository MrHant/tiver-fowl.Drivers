# tiver-fowl.Drivers [![NuGet](https://img.shields.io/nuget/v/Tiver.Fowl.Drivers.svg)](https://www.nuget.org/packages/Tiver.Fowl.Drivers/) [![Codacy Badge](https://img.shields.io/codacy/grade/2ecdd0d0d6af44e480bb899af695e442/master.svg)](https://www.codacy.com/app/mr.hant/tiver-fowl.Drivers?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=MrHant/tiver-fowl.Drivers&amp;utm_campaign=Badge_Grade) [![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://raw.githubusercontent.com/MrHant/tiver-fowl/master/LICENSE)


Download WebDriver drivers' binaries from official sources
## Installation
* Add ```Tiver.Fowl.Drivers``` nuget package to solution
* Add downloaders configuration to ```app.config``` file. Sample configuration can be found below
* Add calls to execute downloaders. Sample usage can be found below


## Sample usage
### Download configured "chrome" driver

```c#
var config = (IDriversConfiguration) ConfigurationManager.GetSection("driversConfigurationGroup/driversConfiguration");
var driverConfig = config.Instances.Cast<DriverElement>().Single(d => d.Name.Equals("chrome"));
var downloader = (IDriverDownloader)Activator.CreateInstance(
    "Tiver.Fowl.Drivers", 
    $"Tiver.Fowl.Drivers.Downloaders.{driverConfig.DownloaderType}")
        .Unwrap();

var result = downloader.DownloadBinary(driverConfig.Version);
```



## Sample configuration

### Downloading ChromeDriver v2.9
```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <!-- Configuration section-handler declaration area. -->
  <configSections>
    <sectionGroup name="driversConfigurationGroup">
      <section
        name="driversConfiguration"
        type="Tiver.Fowl.Drivers.Configuration.DriversConfigurationSection, Tiver.Fowl.Drivers"
        allowLocation="true"
        allowDefinition="Everywhere" />
    </sectionGroup>
  </configSections>

  <!-- Configuration section settings area. -->
  <driversConfigurationGroup>
    <driversConfiguration>
      <add name="chrome" downloaderType="ChromeDriverDownloader" version="2.9" />
    </driversConfiguration>
  </driversConfigurationGroup>
</configuration>
```
