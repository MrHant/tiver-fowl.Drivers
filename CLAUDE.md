# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a .NET library that downloads WebDriver driver binaries (e.g., ChromeDriver) from official sources. It's distributed as a NuGet package (`Tiver.Fowl.Drivers`) targeting .NET 6 and .NET Standard 2.0.

## Build Commands

This project uses standard `dotnet` CLI commands directly. MinVer is used for automatic semantic versioning based on git tags.

### Basic Build Commands

```bash
# Restore dependencies
dotnet restore Tiver.Fowl.Drivers.sln

# Build the solution (Debug)
dotnet build Tiver.Fowl.Drivers.sln --configuration Debug

# Build the solution (Release, as done in CI)
# MinVer automatically determines version from git tags
dotnet build Tiver.Fowl.Drivers.sln --configuration Release

# Run tests
dotnet test Tiver.Fowl.Drivers.sln --configuration Debug

# Create NuGet package
# MinVer automatically sets the package version
dotnet pack Tiver.Fowl.Drivers.sln --configuration Release --output artifacts

# Push to NuGet (requires NUGET_API_KEY environment variable)
dotnet nuget push artifacts/*.nupkg --source https://api.nuget.org/v3/index.json --api-key $NUGET_API_KEY
```

### Running Tests

```bash
dotnet test Tiver.Fowl.Drivers.sln
```

### Running a Single Test

```bash
# Run specific test class
dotnet test --filter "FullyQualifiedName~DownloadersChromeDriver"

# Run specific test method
dotnet test --filter "FullyQualifiedName~DownloadersChromeDriver.DownloadBinaryForChrome"
```

## Architecture

### Core Design Pattern: Strategy + Facade + Factory

The library uses an interface-based strategy pattern with reflection-based factory instantiation and a static facade for convenience.

**Key Components:**

1. **IDriverDownloader** - Strategy interface for all downloaders
   - `DownloadBinary(version, platform) : DownloadResult` - Main operation
   - `Binary : IDriverBinary` - Managed driver file reference
   - `LinkForDownloadsPage : Uri` - Download source URL

1a. **IDriverBinary** - Binary management interface
   - `CheckBinaryExists() : bool` - Checks if binary file exists
   - `GetExistingBinaryVersion() : string` - Gets version from first line of `.version` file
   - `GetExtractedFiles() : string[]` - Gets list of extracted files from `.version` file (lines 3+)
   - `RemoveBinaryFiles()` - Deletes all tracked extracted files and the `.version` file

2. **Downloaders** - Static facade (located at `/Tiver.Fowl.Drivers/Downloaders.cs`)
   - `Get(name) : IDriverDownloader` - Get configured downloader by name
   - `DownloadBinaryFor(name) : DownloadResult` - End-to-end download using config

3. **ChromeDriverDownloader** - Current implementation (located at `/Tiver.Fowl.Drivers/DriverDownloaders/ChromeDriverDownloader.cs`)
   - Downloads from `chromedriver.storage.googleapis.com`
   - Supports `LATEST_RELEASE` and `LATEST_RELEASE_XXX` versions
   - Uses named Mutex (`Global\\ChromeDriverDownloader`) for thread-safety
   - Stores version and list of extracted files in `.version` files to avoid redundant downloads and enable complete cleanup

### Configuration System

Configuration is loaded from `Tiver_config.json` using Microsoft.Extensions.Configuration.

**Configuration Structure:**
```json
{
  "Tiver.Fowl.Drivers": {
    "HttpTimeout": 120,
    "DownloadLocation": "/path/to/drivers",
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

**Context.cs** (`/Tiver.Fowl.Drivers/Configuration/Context.cs`) - Static class that:
- Loads configuration on first access (static constructor)
- Creates shared HttpClient with configured timeout
- Provides defaults if config file is missing

### Downloader Factory Pattern

Downloaders are instantiated via reflection in `Downloaders.cs`:

```csharp
var typeName = $"Tiver.Fowl.Drivers.DriverDownloaders.{driverConfig.DownloaderType}";
var type = Type.GetType(typeName);
var downloader = (IDriverDownloader)Activator.CreateInstance(type);
```

This allows configuration-driven selection without modifying code.

### Thread Safety

- **Named Mutex**: ChromeDriverDownloader uses `Mutex.OpenExisting("Global\\ChromeDriverDownloader")` to prevent concurrent downloads
- **Static HttpClient**: Shared instance in Context class (thread-safe by design)
- **Test Suite**: Includes parallel test class (`DownloadersChromeDriverParallel`) to verify mutex safety

## Project Structure

```
Tiver.Fowl.Drivers/                  # Main library
├── Configuration/
│   ├── Context.cs                    # Static config loader & HttpClient
│   └── DriversConfiguration.cs       # Config models
├── DriverDownloaders/
│   ├── IDriverDownloader.cs          # Strategy interface
│   └── ChromeDriverDownloader.cs     # Chrome implementation
├── DriverBinaries/
│   ├── IDriverBinary.cs              # Binary management interface
│   └── DriverBinary.cs               # File & version tracking
└── Downloaders.cs                    # Static facade/factory

Tiver.Fowl.Drivers.Tests/            # Integration tests with config
├── Tiver_config.json                 # Test configuration
├── DownloadersChromeDriver.cs        # Main test suite (10 tests)
└── DownloadersChromeDriverParallel.cs # Parallel safety tests

Tiver.Fowl.Drivers.TestsWithoutConfigFile/ # Tests without config
└── DownloadersChromeDriverWithoutConfigFile.cs

.config/
└── dotnet-tools.json                 # Local dotnet tools manifest
```

## Adding a New Downloader

To add support for a new WebDriver (e.g., GeckoDriver):

1. Create `/Tiver.Fowl.Drivers/DriverDownloaders/GeckoDriverDownloader.cs`
2. Implement `IDriverDownloader` interface
3. Use `Context.Configuration.HttpTimeout` for HTTP requests
4. Use `Context.HttpClient` for downloads
5. Follow version file pattern (`.version` files):
   - Extract archive entries manually and track each file
   - Write version file with format: version number, empty line, list of extracted files
   - Do NOT use `overwrite: true` when extracting to prevent conflicts in parallel scenarios
6. Add configuration to `Tiver_config.json`:
   ```json
   {
     "Name": "gecko",
     "DownloaderType": "GeckoDriverDownloader",
     "Version": "x.x.x",
     "Platform": "linux64"
   }
   ```

## Version Management

- **MinVer**: Automatic semantic versioning based on git tags
- **Package Installation**: MinVer is installed as a NuGet package reference in the main project
- **Build Integration**: MinVer automatically calculates and applies version during build via MSBuild properties (`AssemblyVersion`, `FileVersion`, `InformationalVersion`, `PackageVersion`)
- **Version Calculation**: MinVer determines version from git tags following SemVer 2.0. Use git tags like `1.0.0` to set versions. Pre-release versions are automatically generated for commits without tags.
- **Configuration**: MinVer can be configured via MSBuild properties (e.g., `MinVerTagPrefix`, `MinVerMinimumMajorMinor`)

## Test Patterns

- **NUnit 3** framework
- **SetUp/OneTimeSetUp**: Clean up downloaded files before tests
- **ValueSource**: Cross-platform testing with `[ValueSource(nameof(Platforms))]`
- **Parallel Tests**: Use `[Parallelizable(ParallelScope.All)]` to verify mutex safety
- **Test Configuration**: `Tiver_config.json` is copied to test output directory

## Important Conventions

1. **Naming**: All downloader classes must end with `DownloaderType` suffix (e.g., `ChromeDriverDownloader`)
2. **Namespace**: Full namespace is `Tiver.Fowl.Drivers.DriverDownloaders.{DownloaderType}`
3. **Version Files**: Stored as `{binaryname}.version` with the following format:
   - Line 1: Version number (e.g., `76.0.3809.25`)
   - Line 2: Empty line
   - Lines 3+: List of extracted files from the archive (one per line)

   Example:
   ```
   76.0.3809.25

   chromedriver.exe
   LICENSE.chromedriver
   ```
4. **Lock Files**: Reference pattern is `{binaryname}.lock` for synchronization
5. **Platform Naming**: Use official platform names (e.g., `win32`, `linux64`, `mac64`)
6. **Archive Extraction**: When extracting ZIP archives, do NOT use overwrite mode to prevent file conflicts during parallel downloads

## Dependencies

**Main Library:**
- Microsoft.Extensions.Configuration (v6.0.0) - JSON configuration
- Microsoft.Extensions.Configuration.Binder (v6.0.0) - Config binding
- Microsoft.Extensions.Configuration.Json (v6.0.0) - JSON provider
- MinVer (v6.0.0) - Automatic semantic versioning

**Testing:**
- NUnit (v3.13.2) - Test framework
- NUnit3TestAdapter (v4.2.0) - Test runner
- Appveyor.TestLogger (v2.0.0) - CI integration

## CI/CD

- **Platform**: AppVeyor
- **Configuration**: `appveyor.yml`
- **Build Image**: Visual Studio 2019
- **Targets**: Test, Push (on successful build)
- **Artifacts**: NuGet packages (.nupkg) and symbols (.snupkg)
