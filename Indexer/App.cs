using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Indexer
{
    public class App
    {
        public void Run()
        {
            Crawler crawler = new Crawler();

            var directoryArray = new DirectoryInfo("maildir").GetDirectories();
            var directories = new List<DirectoryInfo>(directoryArray).OrderBy(d => d.Name).AsEnumerable();
            
            DateTime start = DateTime.Now;
            foreach (var directory in directories)
            {
                crawler.IndexFilesIn(directory, new List<string> { ".txt"});
            }
            
            TimeSpan used = DateTime.Now - start;
            Console.WriteLine("DONE! used " + used.TotalMilliseconds);
        }
    }
}
