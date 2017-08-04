using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Xml;
using Tiver.Fowl.Drivers.Configuration;

namespace Tiver.Fowl.Drivers.Downloaders
{
    public class ChromeDriverDownloader : IDriverDownloader
    {
        public Uri LinkForDownloadsPage => new Uri("http://chromedriver.storage.googleapis.com/");

        public string DriverBinaryFilename => "chromedriver.exe";

        readonly IDriversConfiguration _config = (DriversConfigurationSection)ConfigurationManager.GetSection("driversConfigurationGroup/driversConfiguration");

        public bool CheckBinaryExists()
        {
            return File.Exists(Path.Combine(_config.DownloadLocation, DriverBinaryFilename));
        }

        public Uri GetLinkForVersion(string versionNumber)
        {
            var keys = new List<string>();

            using (var client = new HttpClient())
            using (var response = client.GetAsync(LinkForDownloadsPage).Result)
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

            var query = keys.SingleOrDefault(k => k.StartsWith(versionNumber) && k.EndsWith("win32.zip"));

            return query == null 
                ? null 
                : new Uri(LinkForDownloadsPage, query);
        }

        public bool DownloadBinary(Uri downloadLink)
        {
            try
            {
                string tempFile;
                using (var client = new HttpClient())
                {
                    var bytes = client.GetByteArrayAsync(downloadLink).Result;
                    tempFile = Path.GetTempFileName();
                    File.WriteAllBytes(tempFile, bytes);
                }

                ZipFile.ExtractToDirectory(tempFile, _config.DownloadLocation);
                File.Delete(tempFile);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
