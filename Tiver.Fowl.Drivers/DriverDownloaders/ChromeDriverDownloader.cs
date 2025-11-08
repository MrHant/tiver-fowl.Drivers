using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
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

        private Uri KnownGoodVersionsUri => new Uri(LinkForDownloadsPage, "known-good-versions-with-downloads.json");

        private Uri LastKnownGoodVersionsUri => new Uri(LinkForDownloadsPage, "last-known-good-versions-with-downloads.json");

        public DownloadResult DownloadBinary(string versionNumber, string platform)
        {
            Binary = new DriverBinary(GetBinaryNameForPlatform(platform));

            string resolvedVersion;
            Uri downloadLink;

            try
            {
                var knownGoodVersions = GetKnownGoodVersions();
                resolvedVersion = ResolveVersion(versionNumber, knownGoodVersions);
                downloadLink = GetDownloadLinkForVersion(knownGoodVersions, resolvedVersion, platform);
                if (downloadLink == null)
                {
                    return new DownloadResult
                    {
                        Successful = false,
                        ErrorMessage = "Cannot find specified version to download."
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
                    var updatedResult = DownloadBinary(downloadLink, resolvedVersion);
                    if (updatedResult.Successful)
                    {
                        updatedResult.PerformedAction = DownloaderAction.BinaryUpdated;
                    }

                    return updatedResult;
                }
                else
                {
                    return DownloadBinary(downloadLink, resolvedVersion);
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

        private DownloadResult DownloadBinary(Uri downloadLink, string versionNumber)
        {
            try
            {
                var bytes = Context.HttpClient.GetByteArrayAsync(downloadLink).Result;
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

        private static Uri GetDownloadLinkForVersion(KnownGoodVersionsResponse versionsResponse, string version, string platform)
        {
            if (versionsResponse?.Versions == null)
            {
                return null;
            }

            var versionEntry = versionsResponse.Versions.FirstOrDefault(v =>
                string.Equals(v.Version, version, StringComparison.OrdinalIgnoreCase));

            var download = versionEntry?.Downloads?.ChromeDriver?.FirstOrDefault(d =>
                string.Equals(d.Platform, platform, StringComparison.OrdinalIgnoreCase));

            return download?.Url == null ? null : new Uri(download.Url);
        }

        private string ResolveVersion(string requestedVersion, KnownGoodVersionsResponse knownGoodVersions)
        {
            if (!requestedVersion.StartsWith("LATEST_RELEASE", StringComparison.OrdinalIgnoreCase))
            {
                return requestedVersion;
            }

            var suffix = requestedVersion.Substring("LATEST_RELEASE".Length);
            if (string.IsNullOrWhiteSpace(suffix))
            {
                return GetLatestStableVersion();
            }

            if (suffix.StartsWith("_", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(suffix.TrimStart('_'), out var milestone))
            {
                return GetLatestVersionForMilestone(milestone, knownGoodVersions);
            }

            throw new InvalidOperationException($"Unknown latest release format '{requestedVersion}'.");
        }

        private static string GetLatestVersionForMilestone(int milestone, KnownGoodVersionsResponse knownGoodVersions)
        {
            if (knownGoodVersions?.Versions == null)
            {
                throw new InvalidOperationException("Known good versions list is empty.");
            }

            var milestonePrefix = $"{milestone}.";
            var candidate = knownGoodVersions.Versions
                .Where(v => v.Version.StartsWith(milestonePrefix, StringComparison.OrdinalIgnoreCase))
                .OrderBy(v => ParseVersion(v.Version))
                .LastOrDefault();

            if (candidate == null)
            {
                throw new InvalidOperationException($"Cannot find known good version for milestone {milestone}.");
            }

            return candidate.Version;
        }

        private static Version ParseVersion(string value)
        {
            return Version.TryParse(value, out var parsed) ? parsed : new Version(0, 0);
        }

        private string GetLatestStableVersion()
        {
            using var response = Context.HttpClient.GetAsync(LastKnownGoodVersionsUri).Result;
            response.EnsureSuccessStatusCode();
            using var content = response.Content;
            var json = content.ReadAsStringAsync().Result;
            var data = JsonSerializer.Deserialize<LastKnownGoodVersionsResponse>(json, JsonOptions);

            if (data?.Channels == null || !data.Channels.TryGetValue("Stable", out var stable) ||
                string.IsNullOrWhiteSpace(stable.Version))
            {
                throw new InvalidOperationException("Unable to determine last known good stable version.");
            }

            return stable.Version;
        }

        private KnownGoodVersionsResponse GetKnownGoodVersions()
        {
            using var response = Context.HttpClient.GetAsync(KnownGoodVersionsUri).Result;
            response.EnsureSuccessStatusCode();
            using var content = response.Content;
            var json = content.ReadAsStringAsync().Result;
            var data = JsonSerializer.Deserialize<KnownGoodVersionsResponse>(json, JsonOptions);

            if (data?.Versions == null || data.Versions.Count == 0)
            {
                throw new InvalidOperationException("Unable to load known good versions list.");
            }

            return data;
        }

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

        private class LastKnownGoodVersionsResponse
        {
            [JsonPropertyName("channels")]
            public Dictionary<string, ChannelInfo> Channels { get; set; }
        }

        private class ChannelInfo
        {
            [JsonPropertyName("version")]
            public string Version { get; set; }
        }
    }
}
