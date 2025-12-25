# Changelog

## [Unreleased]
- xxx

## [0.7.0]
- Added: New Chrome downloader (Fixes #6).
- Changed: Target framework updated to .NET 10; dependency bumps.
- Added: Dev container for development.
- Changed: Switched from Nuke to `dotnet` CLI for builds.
- Changed: Updated .NET SDK version.
- Removed: .NET Framework targets.

## [0.5.3] - 2022-09-16
- Fixed: Numbered Latest versions download for Chrome.

## [0.5.2] - 2022-09-14
- Removed: .NET 5 target from tests.

## [0.5.1] - 2022-09-14
- Added: Support for numbered “latest” versions.
- Added: .NET Standard 2.0 target; adjusted tests accordingly.

## [0.5.0] - 2022-01-09
- Changed: Updated target framework to .NET 6 and refreshed dependencies.
- Added: README included in NuGet package.
- Removed: Obsolete files.

## [0.4.3] - 2021-03-01
- Changed: Default HTTP timeout set to 120 seconds.

## [0.4.2] - 2021-02-27
- No user-impacting changes recorded.

## [0.4.1] - 2021-02-27
- Changed: Target framework updated to .NET 5.0; dependency updates.
- Fixed: Missing file reference.

## [0.4.0] - 2020-05-11
- Added: Platform specification in `ChromeDriverDownloader`.
- Fixed/Build: VS target set to 2019; nuspec/CI fixes.

## [0.3.2] - 2020-02-05
- Fixed: Similar versions detection error and `NullReferenceException`.

## [0.3.1] - 2020-02-05
- No user-impacting changes recorded.

## [0.3.0] - 2019-06-23
- Added: `Downloaders` class to simplify configuration-driven downloads.
- Changed: Updated ChromeDriver to recent versions; static `HttpClient` usage.
- Changed: Namespace adjustments and HTTP exception handling improvements.
- Removed: `configurationSectionGroup`.

## [v0.2.0] - 2018-10-10
- Added: `BinaryUpdated` action and `DownloaderAction`.
- Added: `System.Configuration` in NuGet package.
- Changed: `bool` return replaced by `DownloadResult` class; improved invalid version handling.

## [v0.1.4] - 2017-09-18
- Added: NuGet push task; NUnit tests; Cake/AppVeyor build integration.
- Fixed: AppVeyor and GitVersion configuration.
- Changed: GitVersion develop tag.

## [v0.1.3] - 2017-09-01
- No user-impacting changes recorded.

## [v0.1.2] - 2017-08-30
- Added: List of downloaders and initial README.

## [v0.1.1] - 2017-08-08
- Changed: Targeted .NET Framework 4.5; added package to solution.

## [v0.1.0] - 2017-08-07
- Added: Initial package and `DriverCollection` configuration.
