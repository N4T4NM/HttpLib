using System.IO;
using System.Net.Security;

namespace HttpLib.Utils
{
    public static class UrlUtility
    {
        public static string GetHost(this string url)
        {
            int begin = url.StartsWith("http") ? url.IndexOf('/') + 2 : 0;
            int end = url.Substring(begin).Contains(':') ? url.IndexOf(':', begin) : url.IndexOf('/', begin);

            if (end == -1)
                end = url.Length;

            return url.Substring(begin, end - begin);
        }
        public static int GetPort(this string url)
        {
            int begin = url.IndexOf('/') + 2;
            if (!url.Substring(begin).Contains(':'))
                return url.StartsWith("https://") ? 443 : 80;

            int portBegin = url.IndexOf(':', begin) + 1;
            int portEnd = url.Substring(begin).Contains('/') ? url.IndexOf('/', begin) : url.Length;

            return int.Parse(url.Substring(portBegin, portEnd - portBegin));
        }
        public static bool IsSsl(this string url)
            => url.StartsWith("https://");
        public static void GetUrlData(this string url, out string host, out int port, out bool ssl)
        {
            host = url.GetHost();
            port = url.GetPort();
            ssl = url.IsSsl();
        }
        public static SslStream CreateSslStream(this Stream stream, string host)
        {
            SslStream sslStream = new SslStream(stream);
            sslStream.AuthenticateAsClient(host);

            return sslStream;
        }
    }
}
