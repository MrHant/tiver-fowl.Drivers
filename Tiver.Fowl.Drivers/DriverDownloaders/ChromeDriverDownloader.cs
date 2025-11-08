using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Xml;
using Tiver.Fowl.Drivers.DriverBinaries;

namespace Tiver.Fowl.Drivers.DriverDownloaders
{
    public class ChromeDriverDownloader : IDriverDownloader
    {
        public IDriverBinary Binary { get; set; }

        public Uri LinkForDownloadsPage => new Uri("http://chromedriver.storage.googleapis.com/");

        public DownloadResult DownloadBinary(string versionNumber, string platform)
        {
            if (versionNumber.StartsWith("LATEST_RELEASE"))
            {
                try
                {
                    versionNumber = GetLatestVersion(versionNumber);
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
            
            Binary = new DriverBinary(
                platform switch
                {
                    "win32" => "chromedriver.exe",
                    _ => "chromedriver"
                }
            );
            
            Uri uri;
            try
            {
                uri = GetLinkForVersion(versionNumber, platform);
                if (uri == null)
                {
                    return new DownloadResult
                    {
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
                    if (Binary.GetExistingBinaryVersion().Equals(versionNumber))
                    {
                        return new DownloadResult
                        {
                            Successful = true,
                            PerformedAction = DownloaderAction.NoDownloadNeeded
                        };
                    }
                    else
                    {
                        Binary.RemoveBinaryFiles();
                        var result = DownloadBinary(uri, versionNumber);
                        if (result.Successful)
                        {
                            result.PerformedAction = DownloaderAction.BinaryUpdated;
                        }

                        return result;
                    }
                }
                else
                {
                    return DownloadBinary(uri, versionNumber);
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

        private Uri GetLinkForVersion(string versionNumber, string platform)
        {
            var keys = new List<string>();

            using (var response = Context.HttpClient.GetAsync(LinkForDownloadsPage).Result)
            using (var content = response.Content)
            {
                var result = content.ReadAsStringAsync().Result;
                var doc = new XmlDocument();
                doc.LoadXml(result);
                if (doc.DocumentElement == null)
                {
                    return null;
                }

                var nodes = doc.DocumentElement.SelectNodes(".//*[local-name()='Key']");
                keys.AddRange(from XmlNode node in nodes select node.InnerText);
            }

            var query = keys.SingleOrDefault(k => k.StartsWith($"{versionNumber}/") && k.EndsWith($"{platform}.zip"));

            return query == null
                ? null
                : new Uri(LinkForDownloadsPage, query);
        }

        private DownloadResult DownloadBinary(Uri downloadLink, string versionNumber)
        {
            try
            {
                var bytes = Context.HttpClient.GetByteArrayAsync(downloadLink).Result;
                var tempFile = Path.GetTempFileName();
                File.WriteAllBytes(tempFile, bytes);

                // Extract and track extracted files
                var extractedFiles = new List<string>();
                using (var archive = ZipFile.OpenRead(tempFile))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (!string.IsNullOrEmpty(entry.Name))
                        {
                            var destinationPath = Path.Combine(Context.Configuration.DownloadLocation, entry.FullName);
                            entry.ExtractToFile(destinationPath);
                            extractedFiles.Add(entry.FullName);
                        }
                    }
                }

                File.Delete(tempFile);

                // Write version file with version and extracted files list
                var versionFilePath = Binary.DriverBinaryVersionFilepath;
                var versionFileContent = versionNumber + Environment.NewLine +
                                        Environment.NewLine +
                                        string.Join(Environment.NewLine, extractedFiles);
                File.WriteAllText(versionFilePath, versionFileContent);

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

        private string GetLatestVersion(string version)
        {
            var linkForLatestReleaseFile = new Uri(LinkForDownloadsPage, version);

            using (var response = Context.HttpClient.GetAsync(linkForLatestReleaseFile).Result)
            using (var content = response.Content)
            {
                var rawResult = content.ReadAsStringAsync().Result;
                return rawResult.Trim();
            }
        }
    }
}