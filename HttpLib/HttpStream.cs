using HttpLib.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace HttpLib
{
    public class HttpStream : IDisposable
    {
        /// <summary>
        /// Is Ssl enabled
        /// </summary>
        public bool Ssl { get; private set; }

        /// <summary>
        /// Set header Connection: keep-alive
        /// </summary>
        public bool KeepAlive { get; set; }

        /// <summary>
        /// Network stream
        /// </summary>
        public Stream BaseStream { get; private set; }

        /// <summary>
        /// Connection socket
        /// </summary>
        public Socket Socket { get; private set; }

        /// <summary>
        /// Current url
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Cookies
        /// </summary>
        public CookieContainer Cookies { get; private set; }

        /// <summary>
        /// Is stream open
        /// </summary>
        public bool IsOpen { get; private set; }

        /// <summary>
        /// Connect to a proxy server when OpenStream is called
        /// </summary>
        public ProxyInstance Proxy { get; set; }

        public HttpStream()
        {
            KeepAlive = true;
            Cookies = new();
        }

        /// <summary>
        /// Open stream
        /// </summary>
        /// <param name="url">Target url</param>
        /// <returns></returns>
        public async Task OpenStreamAsync(string url)
        {
            try
            {
                IsOpen = true;
                url.GetUrlData(out string host, out int port, out bool ssl);

                Socket = new(SocketType.Stream, ProtocolType.Tcp);

                if (Proxy != null)
                {
                    await Socket.ConnectAsync(Proxy.Host, Proxy.Port);
                    BaseStream = new NetworkStream(Socket);
                    await Proxy.OpenAsync(this, host, port);
                }
                else
                {
                    await Socket.ConnectAsync(host, port);
                    BaseStream = new NetworkStream(Socket);
                }

                Url = url;
                Ssl = ssl;
                if (Ssl) BaseStream = BaseStream.CreateSslStream(host);
            }
            catch (Exception ex)
            {
                IsOpen = false;
                Url = null;
                Ssl = false;

                Socket?.Dispose();
                Socket = null;

                BaseStream?.Dispose();
                BaseStream = null;

                Debug.WriteLine(ex);
                throw;
            }
        }

        /// <summary>
        /// Open stream
        /// </summary>
        /// <param name="url">Target url</param>
        public void OpenStream(string url) => OpenStreamAsync(url).GetAwaiter().GetResult();

        /// <summary>
        /// Close stream and socket
        /// </summary>
        public void CloseStream()
        {
            IsOpen = false;
            BaseStream?.Close();
            Socket?.Close();
        }

        /// <summary>
        /// Dispose stream and socket
        /// </summary>
        public void Dispose()
        {
            IsOpen = false;

            BaseStream?.Dispose();
            Socket?.Dispose();

            BaseStream = null;
            Socket = null;
        }
    }
}
