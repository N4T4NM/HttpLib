using System;
using System.IO;
using System.Net;
using System.Text;

namespace HttpLib.Utils
{
    public static class HeaderUtility
    {
        public static bool FinishedContent(this byte[] content)
        {
            if (content.Length < 4)
                return false;

            byte[] last = new byte[4];
            Array.Copy(content, content.Length - 4, last, 0, last.Length);

            if (Encoding.UTF8.GetString(last) == "\r\n\r\n")
                return true;
            return false;
        }
        public static bool FinishedContent(this Stream stream)
        {
            long start = stream.Position;
            if (stream.Length < 4)
                return false;

            byte[] last = new byte[4];
            stream.Position = stream.Length - 4;
            stream.Read(last);

            stream.Position = start;

            if (Encoding.UTF8.GetString(last) == "\r\n\r\n")
                return true;
            return false;
        }

        public static bool FinishedLine(this byte[] content)
        {
            if (content.Length < 2)
                return false;

            byte[] last = new byte[2];
            Array.Copy(content, content.Length - 2, last, 0, last.Length);

            if (Encoding.UTF8.GetString(last) == "\r\n")
                return true;
            return false;
        }
        public static bool FinishedLine(this Stream stream)
        {
            long start = stream.Position;
            if (stream.Length < 2)
                return false;

            byte[] last = new byte[2];
            stream.Position = stream.Length - 2;
            stream.Read(last);

            stream.Position = start;

            if (Encoding.UTF8.GetString(last) == "\r\n")
                return true;
            return false;
        }

        public static void ParseHeaders(this string raw, WebHeaderCollection headers)
        {
            foreach (string line in raw.Split("\n"))
            {
                string clean = line.TrimEnd();
                if (clean.Length == 0)
                    continue;

                string key = clean.Substring(0, clean.IndexOf(':'));
                string value = clean.Substring(clean.IndexOf(':') + 2);

                headers.Add(key, value);
            }
        }
    }
}
