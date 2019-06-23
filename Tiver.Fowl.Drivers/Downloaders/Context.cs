using System.Net.Http;

namespace Tiver.Fowl.Drivers.Downloaders
{
    public static class Context
    {
        public static readonly HttpClient HttpClient = new HttpClient();
    }
}