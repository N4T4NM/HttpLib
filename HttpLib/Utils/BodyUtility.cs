using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace HttpLib.Utils
{
    public static class BodyUtility
    {
        public static async Task<byte[]> ReadChunkedStreamAsync(this HttpResponse response)
        {
            MemoryStream ms = new();
            while (true)
            {
                byte[] sizeBuffer = await response.ReadToEndOfLine();
                string sizeStr = Encoding.UTF8.GetString(sizeBuffer, 0, sizeBuffer.Length - 2);
                int size = int.Parse(sizeStr, System.Globalization.NumberStyles.HexNumber);

                if (size == 0)
                {
                    byte[] end = new byte[2];
                    await response.HttpStream.BaseStream.ReadAsync(end);
                    await ms.WriteAsync(end);
                    break;
                }

                byte[] sBuffer = new byte[1];
                int total = 0;
                while (true)
                {
                    int read = await response.HttpStream.BaseStream.ReadAsync(sBuffer);
                    total += read;

                    await ms.WriteAsync(sBuffer);
                    if (total == size + 2)
                        break;
                }
            }

            byte[] result = ms.ToArray();
            ms.Dispose();
            return result;
        }
    }
}
