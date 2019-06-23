namespace Tiver.Fowl.Drivers.DriverDownloaders
{
    public class DownloadResult
    {
        public bool Successful;
        public string ErrorMessage;
        public DownloaderAction PerformedAction;
    }

    public enum DownloaderAction
    {
        Unknown,
        BinaryDownloaded,
        BinaryUpdated,
        NoDownloadNeeded,
    }
}