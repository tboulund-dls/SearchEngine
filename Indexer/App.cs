using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Indexer
{
    public class App
    {
        public void Run()
        {
            var api = new HttpClient() { BaseAddress = new Uri("http://word-service") };
            api.Send(new HttpRequestMessage(HttpMethod.Delete, "Database"));
            api.Send(new HttpRequestMessage(HttpMethod.Post, "Database"));
            
            Crawler crawler = new Crawler();

            var directoryArray = new DirectoryInfo("maildir").GetDirectories();
            var directories = new List<DirectoryInfo>(directoryArray).OrderBy(d => d.Name).AsEnumerable();
            
            DateTime start = DateTime.Now;
            var tasks = new List<Task>();
            foreach (var directory in directories)
            {
                tasks.AddRange(crawler.IndexFilesIn(directory, new List<string> { ".txt"}));
            }
            Console.WriteLine("Queued everything");
            Task.WhenAll(tasks).Wait();
            
            TimeSpan used = DateTime.Now - start;
            Console.WriteLine("DONE! used " + used.TotalMilliseconds);
            
            Thread.Sleep(30000);
        }
    }
}
