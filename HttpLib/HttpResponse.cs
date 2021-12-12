using HttpLib.Readers;
using HttpLib.Utils;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HttpLib
{
    public class HttpResponse
    {
        /// <summary>
        /// Http Stream instance
        /// </summary>
        public HttpStream HttpStream { get; private set; }

        /// <summary>
        /// Response status code
        /// </summary>
        public string Status { get; private set; }

        /// <summary>
        /// Response headers
        /// </summary>
        public WebHeaderCollection Headers { get; private set; }

        public HttpResponse(HttpStream stream)
        {
            HttpStream = stream;
            Headers = new();
        }
        protected async Task ReadHeaders()
        {
            MemoryStream hStream = new();
            byte[] buffer = new byte[1];
            while (true)
            {
                await HttpStream.BaseStream.ReadAsync(buffer);
                await hStream.WriteAsync(buffer);

                if (hStream.FinishedContent())
                    break;
            }

            string headers = Encoding.UTF8.GetString(hStream.ToArray());
            hStream.Dispose();

            headers.ParseHeaders(Headers);
        }

        /// <summary>
        /// Read headers and cookies
        /// </summary>
        /// <returns></returns>
        public async Task InitializeAsync()
        {
            Status = Encoding.UTF8.GetString(await ReadToEndOfLine()).TrimEnd();
            await ReadHeaders();

            if (Headers[HttpResponseHeader.SetCookie] != null)
                HttpStream.Cookies.SetCookies(new Uri(HttpStream.Url), Headers[HttpResponseHeader.SetCookie]);
        }

        /// <summary>
        /// Read current line
        /// </summary>
        /// <returns>Current line bytes</returns>
        public async Task<byte[]> ReadToEndOfLine()
        {
            MemoryStream ms = new();
            byte[] buffer = new byte[1];
            while (true)
            {
                await HttpStream.BaseStream.ReadAsync(buffer);
                await ms.WriteAsync(buffer);

                if (ms.FinishedLine())
                    break;
            }

            byte[] result = ms.ToArray();
            ms.Dispose();
            return result;
        }

        /// <summary>
        /// Read full body
        /// </summary>
        /// <returns>Body bytes</returns>
        public async Task<byte[]> ReadFullBodyAsync()
        {
            byte[] responseBuffer = new byte[0];
            if (Headers[HttpResponseHeader.TransferEncoding]?.ToLower() == "chunked")
                responseBuffer = await new HttpChunkReader(this).ReadAllChunksAsync();
            else if (Headers[HttpResponseHeader.ContentLength] != null)
            {
                byte[] buffer = new byte[long.Parse(Headers[HttpResponseHeader.ContentLength])];
                await HttpStream.BaseStream.ReadAsync(buffer);

                responseBuffer = buffer;
            }

            return responseBuffer;
        }

        /// <summary>
        /// Read full body
        /// </summary>
        /// <returns>Body bytes</returns>
        public byte[] ReadFullBody() => ReadFullBodyAsync().GetAwaiter().GetResult();
    }
}
