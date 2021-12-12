using System;
using System.IO;
using System.Net;

namespace HttpLib.Test
{
    internal class Program
    {
        static void GetDownloadName(string cDisposition, out string name)
        {
            int fNameBegin = cDisposition.IndexOf('"') + 1;
            int fNameEnd = cDisposition.IndexOf('"', fNameBegin);

            string fName = cDisposition.Substring(fNameBegin, fNameEnd - fNameBegin);
            name = fName;
        }

        static void Download(HttpResponse download)
        {
            GetDownloadName(download.Headers["Content-Disposition"], out string name);
            int szStrLen = download.Headers[HttpResponseHeader.ContentLength].Length;
            long size = long.Parse(download.Headers[HttpResponseHeader.ContentLength]);

            Console.Clear();


            Console.WriteLine($"File Name: {name}");
            Console.WriteLine($"Size: {size}\n");

            FileStream fStream = new FileInfo(name).Create();

            long downloaded = 0;
            Console.Write($"Downloaded: {new string('0', szStrLen)}/{size.ToString(new string('0', szStrLen))}");
            Console.CursorLeft -= (szStrLen * 2) + 1;

            byte[] buffer = new byte[1024];
            while (downloaded != size)
            {
                Console.CursorVisible = false;

                int sz = download.HttpStream.BaseStream.Read(buffer);
                downloaded += sz;

                fStream.Write(buffer, 0, sz);
                Console.Write(downloaded.ToString(new string('0', szStrLen)));
                Console.CursorLeft -= szStrLen;
            }

            Console.CursorTop += 2;
            Console.CursorLeft = 0;

            Console.WriteLine("FINISHED !");
        }

        static void Main(string[] args)
        {
            Console.WriteLine("=== Google Drive Download Test ===");
            Console.Write("GDrive Url: ");
            string url = Console.ReadLine();

            HttpStream stream = new();
            GDriveDownloadStart start = new();

            HttpResponse download = start.Start(stream, url);
            Download(download);
        }
    }
}
