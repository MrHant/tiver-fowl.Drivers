namespace Tiver.Fowl.Drivers.Downloaders
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
        NewFileDownloaded,
        NoDownloadedNeeded,        
        BinaryUpdated
    }
}