using System;
using System.Net;
using System.Text;

namespace HttpLib.Test
{
    internal class Program
    {
        static void SetHeaders(WebHeaderCollection headers)
        {
            headers["Upgrade-Insecure-Requests"] = "1";
            headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.93 Safari/537.36";
            headers["Accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
            headers["Sec-GPC"] = "1";
            headers["Sec-Fetch-Site"] = "same-origin";
            headers["Sec-Fetch-Mode"] = "navigate";
            headers["Sec-Fetch-User"] = "?1";
            headers["Sec-Fetch-Dest"] = "document";
            headers["Accept-Language"] = "pt-BR,pt;q=0.9,en-US;q=0.8,en;q=0.7";
        }

        static void Main(string[] args)
        {
            string url = "https://www.google.com";
            HttpStream stream = new();
            stream.Proxy = new("127.0.0.1", 8888);
            stream.OpenStream(url);

            HttpRequest req = new(stream);
            SetHeaders(req.Headers);

            HttpResponse res = req.Send();

            Console.WriteLine(Encoding.UTF8.GetString(res.Headers.ToByteArray()));
            Console.WriteLine(Encoding.UTF8.GetString(res.ReadFullBody()));
        }
    }
}
