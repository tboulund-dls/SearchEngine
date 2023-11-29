using System;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;

namespace Indexer
{
    class Program
    {
        static void Main(string[] args)
        {
            DecompressGzipFile("enron/mini.tar.gz", "mails.tar");
            Directory.Delete("maildir", true);
            TarFile.ExtractToDirectory("mails.tar", ".", false);
            
            Console.WriteLine("Done");
            
            //new App().Run();
            new Renamer().Crawl(new DirectoryInfo("maildir"));
            new App().Run();
        }
        
        static void DecompressGzipFile(string compressedFilePath, string decompressedFilePath)
        {
            using (FileStream compressedFileStream = File.OpenRead(compressedFilePath))
            {
                using (FileStream decompressedFileStream = File.Create(decompressedFilePath))
                {
                    using (GZipStream gzipStream = new GZipStream(compressedFileStream, CompressionMode.Decompress))
                    {
                        gzipStream.CopyTo(decompressedFileStream);
                    }
                }
            }
        }
    }
}