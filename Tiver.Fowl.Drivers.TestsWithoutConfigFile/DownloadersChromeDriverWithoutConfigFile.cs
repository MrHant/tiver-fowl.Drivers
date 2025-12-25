using System.IO;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Tiver.Fowl.Drivers.Configuration;
using Tiver.Fowl.Drivers.DriverDownloaders;

namespace Tiver.Fowl.Drivers.TestsWithoutConfigFile
{
    [TestFixture]
    public class DownloadersChromeDriverWithoutConfigFile
    {
        private static DriversConfiguration Config => new DriversConfiguration();

        private static string DriverFilepath => Path.Combine(Config.DownloadLocation, "chromedriver.exe");

        private static string[] Platforms = { "win32" };
        
        private static string BinaryName(string platform)
        {
            return platform switch
            {
                "win32" => "chromedriver.exe",
                _ => "chromedriver"
            };
        }
        private void DeleteDriverAndVersionFilesIfExist()
        {
            foreach (var platform in Platforms)
            {
                var versionFilepath = Path.Combine(Config.DownloadLocation, $"{BinaryName(platform)}.version");
                if (File.Exists(versionFilepath))
                {
                    var fileList = File.ReadAllLines(versionFilepath)[2..];
                    foreach (var file in fileList)
                    {
                        var filePath = Path.Combine(Config.DownloadLocation, file);
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                    }

                    File.Delete(versionFilepath);
                }
            }
        }

        [SetUp]
        public void SetUp()
        {
            DeleteDriverAndVersionFilesIfExist();
        }

        
        [Test]
        public void Download_v142()
        {
            var downloader = new ChromeDriverDownloader();
            const string versionNumber = "142.0.7444.61";
            var result = downloader.DownloadBinary(versionNumber, "win32");
            ClassicAssert.IsTrue(result.Successful);
            ClassicAssert.AreEqual(DownloaderAction.BinaryDownloaded, result.PerformedAction);
            ClassicAssert.IsNull(result.ErrorMessage);
            var exists = File.Exists(DriverFilepath);
            ClassicAssert.IsTrue(exists);
            exists = downloader.Binary.CheckBinaryExists();
            ClassicAssert.IsTrue(exists);
            ClassicAssert.AreEqual(versionNumber, downloader.Binary.GetExistingBinaryVersion());
        }
    }
}
