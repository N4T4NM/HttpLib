using HttpLib.Utils;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HttpLib
{
    public class HttpRequest : IDisposable
    {
        /// <summary>
        /// Request method
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Request headers
        /// </summary>
        public WebHeaderCollection Headers { get; private set; }

        /// <summary>
        /// Request body
        /// </summary>
        public Stream BodyStream { get; private set; }

        /// <summary>
        /// Http Stream instance
        /// </summary>
        public HttpStream HttpStream { get; private set; }

        public HttpRequest(HttpStream stream)
        {
            Method = "GET";

            Headers = new();
            BodyStream = new MemoryStream();
            HttpStream = stream;
        }

        /// <summary>
        /// Build http request headers and body
        /// </summary>
        /// <returns>Built packet</returns>
        public byte[] Build()
        {
            if (HttpStream.Cookies?.GetCookies(new Uri(HttpStream.Url)).Count > 0)
                Headers[HttpRequestHeader.Cookie] = HttpStream.Cookies.GetCookieHeader(new Uri(HttpStream.Url));
            if (HttpStream.KeepAlive)
                Headers[HttpRequestHeader.Connection] = "kee-alive";

            Headers[HttpRequestHeader.ContentLength] = BodyStream.Length.ToString();
            Headers[HttpRequestHeader.Host] = HttpStream.Url.GetHost();

            byte[] start = Encoding.UTF8.GetBytes($"{Method} {HttpStream.Url} HTTP/1.1\r\n");
            byte[] headers = Headers.ToByteArray();
            byte[] body = (BodyStream as MemoryStream).ToArray();

            byte[] packet = new byte[start.Length + headers.Length + body.Length];
            Array.Copy(start, 0, packet, 0, start.Length);
            Array.Copy(headers, 0, packet, start.Length, headers.Length);
            Array.Copy(body, 0, packet, start.Length + headers.Length, body.Length);

            return packet;
        }

        /// <summary>
        /// Send request
        /// </summary>
        /// <param name="dispose">Dispose request body stream after request</param>
        /// <returns>Http response</returns>
        public async Task<HttpResponse> SendAsync(bool dispose=true)
        {
            await HttpStream.BaseStream.WriteAsync(Build());
            HttpResponse response = new(HttpStream);
            await response.InitializeAsync();

            if (dispose)
                this.Dispose();
            return response;
        }

        /// <summary>
        /// Send request
        /// </summary>
        /// <param name="dispose">Dispose request body stream after request</param>
        /// <returns>Http response</returns>
        public HttpResponse Send(bool dispose=true) => SendAsync(dispose).GetAwaiter().GetResult();

        /// <summary>
        /// Dispose body stream
        /// </summary>
        public void Dispose()
        {
            BodyStream.Dispose();
        }
    }
}
