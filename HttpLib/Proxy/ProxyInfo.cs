using System.Threading.Tasks;

namespace HttpLib.Proxy
{
    public class ProxyInfo
    {
        public string Host { get; set; }
        public int Port { get; set; }

        public ProxyInfo(string host, int port)
        {
            Host = host;
            Port = port;
        }

        public async Task OpenAsync(HttpStream http, string host, int port)
        {
            http.Url = $"{host}:{port}";

            HttpRequest request = new(http);
            request.Method = "CONNECT";

            HttpResponse response = await request.SendAsync();
            await response.ReadFullBodyAsync();
        }
    }
}
