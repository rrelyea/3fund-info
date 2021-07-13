using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace daily.DataProviders
{
    public static class HttpUtility
    {
        public static async Task<string> CallUrl(string fullUrl)
        {
            HttpClient client = new HttpClient();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
            client.DefaultRequestHeaders.Accept.Clear();
            string response = await client.GetStringAsync(fullUrl);
            return response;
        }
    }
}
