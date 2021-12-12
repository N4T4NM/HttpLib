using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace HttpLib.Readers
{
    public class HttpChunkReader
    {
        public HttpResponse BaseResponse { get; private set; }

        public HttpChunkReader(HttpResponse response)
        {
            BaseResponse = response;
        }

        public async Task<HttpChunk> ReadChunkAsync()
        {
            MemoryStream stream = new();

            byte[] sizeBuffer = await BaseResponse.ReadToEndOfLine();
            string sizeString = Encoding.UTF8.GetString(sizeBuffer);
            int size = int.Parse(sizeString, NumberStyles.HexNumber);

            if (size == 0)
            {
                byte[] end = new byte[2];
                await BaseResponse.HttpStream.BaseStream.ReadAsync(end);
                await stream.WriteAsync(end);

                return new(stream, true);
            }

            byte[] sBuffer = new byte[1];
            int total = 0;
            while (true)
            {
                int read = await BaseResponse.HttpStream.BaseStream.ReadAsync(sBuffer);
                total += read;

                await stream.WriteAsync(sBuffer);
                if (total == size + 2)
                    return new(stream, false);
            }
        }
        public HttpChunk ReadChunk() => ReadChunkAsync().GetAwaiter().GetResult();

        public async Task<byte[]> ReadAllChunksAsync()
        {
            MemoryStream ms = new();

            while (true)
            {
                HttpChunk chunk = await ReadChunkAsync();
                ms.Write(chunk.Buffer);
                if (chunk.End)
                    break;
            }

            byte[] chunks = ms.ToArray();
            ms.Dispose();

            return chunks;
        }
        public byte[] ReadAllChunks() => ReadAllChunksAsync().GetAwaiter().GetResult();
    }

    public class HttpChunk
    {
        public HttpChunk(MemoryStream src, bool end)
        {
            Buffer = src.ToArray();
            End = end;

            src.Dispose();
        }

        public byte[] Buffer { get; private set; }
        public bool End { get; private set; }
    }
}
