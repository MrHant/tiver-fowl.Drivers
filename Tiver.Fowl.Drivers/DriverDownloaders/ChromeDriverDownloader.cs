using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Tiver.Fowl.Drivers.DriverBinaries;

namespace Tiver.Fowl.Drivers.DriverDownloaders
{
    public class ChromeDriverDownloader : IDriverDownloader
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public IDriverBinary Binary { get; set; }

        public Uri LinkForDownloadsPage => new Uri("https://googlechromelabs.github.io/chrome-for-testing/");

        // Fallback endpoint (only used if lightweight endpoints fail)
        private Uri KnownGoodVersionsUri => new Uri(LinkForDownloadsPage, "known-good-versions-with-downloads.json");

        public DownloadResult DownloadBinary(string versionNumber, string platform)
        {
            // Synchronous wrapper for async implementation
            return DownloadBinaryAsync(versionNumber, platform).GetAwaiter().GetResult();
        }

        private async Task<DownloadResult> DownloadBinaryAsync(string versionNumber, string platform)
        {
            Binary = new DriverBinary(GetBinaryNameForPlatform(platform));

            string resolvedVersion;
            Uri downloadLink;

            try
            {
                // Use new lightweight API to resolve version and get download URL
                resolvedVersion = await ResolveVersionAsync(versionNumber);
                downloadLink = await GetDownloadUrlAsync(resolvedVersion, platform);

                if (downloadLink == null)
                {
                    return new DownloadResult
                    {
                        Successful = false,
                        ErrorMessage = $"Cannot find download URL for version {resolvedVersion} and platform {platform}."
                    };
                }
            }
            catch (Exception ex)
            {
                var message = ex.GetAllExceptionsMessages();
                return new DownloadResult
                {
                    Successful = false,
                    ErrorMessage = message
                };
            }

            // Pre-download the binary bytes before acquiring mutex
            // This keeps async operations outside the mutex to avoid thread-affinity issues
            byte[] downloadedBytes = null;
            try
            {
                downloadedBytes = await Context.HttpClient.GetByteArrayAsync(downloadLink);
            }
            catch (Exception ex)
            {
                var message = ex.GetAllExceptionsMessages();
                return new DownloadResult
                {
                    Successful = false,
                    ErrorMessage = message
                };
            }

            // Now acquire mutex for file system operations only
            using var mutex = new Mutex(false, "Global\\ChromeDriverDownloader");
            try
            {
                mutex.WaitOne(TimeSpan.FromSeconds(Context.Configuration.HttpTimeout + 10));
                if (Binary.CheckBinaryExists())
                {
                    var existingVersion = Binary.GetExistingBinaryVersion();
                    if (!string.IsNullOrEmpty(existingVersion) &&
                        existingVersion.Equals(resolvedVersion, StringComparison.OrdinalIgnoreCase))
                    {
                        return new DownloadResult
                        {
                            Successful = true,
                            PerformedAction = DownloaderAction.NoDownloadNeeded
                        };
                    }

                    Binary.RemoveBinaryFiles();
                    var updatedResult = SaveDownloadedBinary(downloadedBytes, resolvedVersion);
                    if (updatedResult.Successful)
                    {
                        updatedResult.PerformedAction = DownloaderAction.BinaryUpdated;
                    }

                    return updatedResult;
                }
                else
                {
                    return SaveDownloadedBinary(downloadedBytes, resolvedVersion);
                }
            }
            catch (Exception ex)
            {
                var message = ex.GetAllExceptionsMessages();
                return new DownloadResult
                {
                    Successful = false,
                    ErrorMessage = message
                };
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        private static string GetBinaryNameForPlatform(string platform)
        {
            return (platform ?? string.Empty).StartsWith("win", StringComparison.OrdinalIgnoreCase)
                ? "chromedriver.exe"
                : "chromedriver";
        }

        private DownloadResult SaveDownloadedBinary(byte[] bytes, string versionNumber)
        {
            try
            {
                var tempFile = Path.GetTempFileName();
                File.WriteAllBytes(tempFile, bytes);

                var extractedFiles = ExtractArchive(tempFile);

                File.Delete(tempFile);

                var versionFileContent = versionNumber + Environment.NewLine + Environment.NewLine +
                                         string.Join(Environment.NewLine, extractedFiles);
                File.WriteAllText(Binary.DriverBinaryVersionFilepath, versionFileContent);

                return new DownloadResult
                {
                    Successful = true,
                    PerformedAction = DownloaderAction.BinaryDownloaded
                };
            }
            catch (Exception ex)
            {
                var message = ex.GetAllExceptionsMessages();
                return new DownloadResult
                {
                    Successful = false,
                    ErrorMessage = message
                };
            }
        }

        private List<string> ExtractArchive(string archivePath)
        {
            var extractedFiles = new List<string>();

            using var archive = ZipFile.OpenRead(archivePath);
            foreach (var entry in archive.Entries)
            {
                if (string.IsNullOrEmpty(entry.Name))
                {
                    continue;
                }

                var destinationFileName = entry.Name;

                var destinationPath = Path.Combine(Context.Configuration.DownloadLocation, destinationFileName);
                var directory = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                entry.ExtractToFile(destinationPath);
                extractedFiles.Add(destinationFileName);
            }

            return extractedFiles;
        }

        /// <summary>
        /// Resolves version string to actual version number using lightweight text endpoints.
        /// Supports patterns: LATEST_RELEASE_STABLE, LATEST_RELEASE_BETA, LATEST_RELEASE_DEV,
        /// LATEST_RELEASE_CANARY, LATEST_RELEASE_XXX (milestone), or specific version numbers.
        /// </summary>
        private async Task<string> ResolveVersionAsync(string requestedVersion)
        {
            if (!requestedVersion.StartsWith("LATEST_RELEASE", StringComparison.OrdinalIgnoreCase))
            {
                // Specific version number - return as-is
                return requestedVersion;
            }

            // Use lightweight text endpoints for version resolution
            var suffix = requestedVersion.Substring("LATEST_RELEASE".Length);

            if (string.IsNullOrWhiteSpace(suffix))
            {
                // Default LATEST_RELEASE -> use STABLE channel
                return await FetchVersionFromTextEndpointAsync("LATEST_RELEASE_STABLE");
            }

            if (suffix.StartsWith("_", StringComparison.OrdinalIgnoreCase))
            {
                var suffixValue = suffix.TrimStart('_').ToUpperInvariant();

                // Check if it's a channel (STABLE, BETA, DEV, CANARY)
                if (suffixValue == "STABLE" || suffixValue == "BETA" ||
                    suffixValue == "DEV" || suffixValue == "CANARY")
                {
                    return await FetchVersionFromTextEndpointAsync($"LATEST_RELEASE_{suffixValue}");
                }

                // Check if it's a milestone number (e.g., LATEST_RELEASE_116)
                if (int.TryParse(suffixValue, out _))
                {
                    return await FetchVersionFromTextEndpointAsync($"LATEST_RELEASE_{suffixValue}");
                }
            }

            throw new InvalidOperationException(
                $"Unknown version pattern '{requestedVersion}'. " +
                "Supported patterns: LATEST_RELEASE_STABLE, LATEST_RELEASE_BETA, LATEST_RELEASE_DEV, " +
                "LATEST_RELEASE_CANARY, LATEST_RELEASE_XXX (milestone), or specific version numbers.");
        }

        /// <summary>
        /// Fetches version number from lightweight text endpoint.
        /// These endpoints return just the version number as plain text (e.g., "142.0.7444.61").
        /// </summary>
        private async Task<string> FetchVersionFromTextEndpointAsync(string endpointName)
        {
            try
            {
                var uri = new Uri(LinkForDownloadsPage, endpointName);
                var response = await Context.HttpClient.GetAsync(uri);
                response.EnsureSuccessStatusCode();
                var version = await response.Content.ReadAsStringAsync();
                return version.Trim();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to fetch version from endpoint '{endpointName}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets download URL for specific version and platform using individual version JSON endpoint.
        /// Uses {version}.json endpoint which is much smaller than full manifest files.
        /// Falls back to known-good-versions if individual version file is not found.
        /// </summary>
        private async Task<Uri> GetDownloadUrlAsync(string version, string platform)
        {
            try
            {
                // Try individual version JSON endpoint first (lightweight)
                var versionJsonUri = new Uri(LinkForDownloadsPage, $"{version}.json");
                var response = await Context.HttpClient.GetAsync(versionJsonUri);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var versionData = JsonSerializer.Deserialize<VersionJsonResponse>(json, JsonOptions);

                    var download = versionData?.Downloads?.ChromeDriver?.FirstOrDefault(d =>
                        string.Equals(d.Platform, platform, StringComparison.OrdinalIgnoreCase));

                    if (download?.Url != null)
                    {
                        return new Uri(download.Url);
                    }
                }
            }
            catch
            {
                // Fall through to fallback
            }

            // Fallback: use known-good-versions manifest (for older versions or errors)
            return await GetDownloadUrlFromKnownGoodVersionsAsync(version, platform);
        }

        /// <summary>
        /// Fallback method that uses the full known-good-versions manifest.
        /// Only used when individual version JSON endpoint is not available.
        /// </summary>
        private async Task<Uri> GetDownloadUrlFromKnownGoodVersionsAsync(string version, string platform)
        {
            try
            {
                var response = await Context.HttpClient.GetAsync(KnownGoodVersionsUri);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<KnownGoodVersionsResponse>(json, JsonOptions);

                if (data?.Versions == null)
                {
                    return null;
                }

                var versionEntry = data.Versions.FirstOrDefault(v =>
                    string.Equals(v.Version, version, StringComparison.OrdinalIgnoreCase));

                var download = versionEntry?.Downloads?.ChromeDriver?.FirstOrDefault(d =>
                    string.Equals(d.Platform, platform, StringComparison.OrdinalIgnoreCase));

                return download?.Url == null ? null : new Uri(download.Url);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to get download URL from fallback manifest for version {version}: {ex.Message}", ex);
            }
        }

        // Response model for individual version JSON endpoint (e.g., 142.0.7444.61.json)
        // This is a lightweight alternative to downloading the full known-good-versions manifest
        private class VersionJsonResponse
        {
            [JsonPropertyName("downloads")]
            public ChromeDriverDownloads Downloads { get; set; }
        }

        // Response model for known-good-versions-with-downloads.json (fallback only)
        private class KnownGoodVersionsResponse
        {
            [JsonPropertyName("versions")]
            public List<KnownGoodVersion> Versions { get; set; }
        }

        private class KnownGoodVersion
        {
            [JsonPropertyName("version")]
            public string Version { get; set; }

            [JsonPropertyName("downloads")]
            public ChromeDriverDownloads Downloads { get; set; }
        }

        // Shared download structure
        private class ChromeDriverDownloads
        {
            [JsonPropertyName("chromedriver")]
            public List<ChromeDriverDownload> ChromeDriver { get; set; }
        }

        private class ChromeDriverDownload
        {
            [JsonPropertyName("platform")]
            public string Platform { get; set; }

            [JsonPropertyName("url")]
            public string Url { get; set; }
        }
    }
}
