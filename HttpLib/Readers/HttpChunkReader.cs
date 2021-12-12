using System;
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
            string[] line = Encoding.UTF8.GetString(sizeBuffer).TrimEnd().Split(';');

            string[] extensions = new string[line.Length - 1];
            Array.Copy(line, 1, extensions, 0, extensions.Length);

            int size = int.Parse(line[0], NumberStyles.HexNumber);

            if (size == 0)
            {
                int eol = 0;
                while (eol != 2)
                    eol += await BaseResponse.HttpStream.BaseStream.ReadAsync(new byte[2]);

                return new(stream, extensions, true);
            }

            byte[] sBuffer = new byte[1];
            int total = 0;
            while (true)
            {
                int read = await BaseResponse.HttpStream.BaseStream.ReadAsync(sBuffer);
                total += read;

                await stream.WriteAsync(sBuffer);
                if (total == size)
                {
                    int eol = 0;
                    while (eol != 2)
                        eol += await BaseResponse.HttpStream.BaseStream.ReadAsync(new byte[2]); //Jump end of line bytes

                    return new(stream, extensions, false);
                }
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
        public HttpChunk(MemoryStream src, string[] extensions, bool end)
        {
            Buffer = src.ToArray();
            Extensions = extensions;
            End = end;

            src.Dispose();
        }

        public string[] Extensions { get; private set; }
        public byte[] Buffer { get; private set; }
        public bool End { get; private set; }
    }
}
