using System;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace HttpLib.Test
{
    public class GDriveDownloadStart
    {
        void SetHeaders(WebHeaderCollection headers)
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
            headers["Referer"] = "https://drive.google.com";
        }

        string GetDownloadUrl(HttpStream stream)
        {
            HttpRequest bodyRequest = new(stream);
            SetHeaders(bodyRequest.Headers);

            HttpResponse response = bodyRequest.Send();

            string body = Encoding.UTF8.GetString(response.ReadFullBody());

            int start = body.IndexOf("href=\"/uc?export=") + 7;
            int end = body.IndexOf('"', start) - start;

            string url = body.Substring(start, end).Replace("&amp;", "&");

            Debug.WriteLine(url);
            return url;
        }

        string GetGoogleUserContent(HttpStream stream, string url)
        {
            stream.Url = url;
            HttpRequest req = new(stream);
            SetHeaders(req.Headers);

            HttpResponse response = req.Send();
            response.ReadFullBody();

            url = response.Headers[HttpResponseHeader.Location];
            Debug.WriteLine(url);
            return url;
        }

        string GetGoogleDocs(HttpStream stream, string url)
        {
            stream.CloseStream();
            stream.OpenStream(url);

            HttpRequest req = new(stream);
            SetHeaders(req.Headers);

            HttpResponse response = req.Send();

            url = response.Headers[HttpResponseHeader.Location];
            Debug.WriteLine(url);
            return url;
        }

        string GetFileUrl(HttpStream stream, string url)
        {
            stream.CloseStream();
            stream.OpenStream(url);

            HttpRequest req = new(stream);
            SetHeaders(req.Headers);

            HttpResponse response = req.Send();

            url = response.Headers[HttpResponseHeader.Location];
            Debug.WriteLine(url);
            return url;
        }


        public HttpResponse Start(HttpStream stream, string url)
        {
            stream.OpenStream(url);

            Console.WriteLine("Getting start download url...");
            url = GetDownloadUrl(stream);

            Console.WriteLine("Requesting Google User Content url...");
            url = GetGoogleUserContent(stream, $"https://drive.google.com/u/0/{url}");

            Console.WriteLine("Requesting Google Docs url...");
            url = GetGoogleDocs(stream, url);

            Console.WriteLine("Requesting file url...");
            url = GetFileUrl(stream, url);

            Console.WriteLine("Starting download...");
            stream.Url = url;
            HttpRequest req = new(stream);
            SetHeaders(req.Headers);

            HttpResponse response = req.Send();
            return response;
        }
    }
}
