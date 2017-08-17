using System.Net.Http;

namespace Autotrader
{
    public static class HttpHelper
    {
        public static HttpClient CreateClient()
        {
            var handler = new HttpClientHandler()
            {
                AllowAutoRedirect = false
            };
            var hc = new HttpClient(handler);
            hc.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.90 Safari/537.36");

            return hc;
        }
    }
}
