using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ConsoleSearch
{
    public class SearchLogic
    {
        private HttpClient api = new() { BaseAddress = new Uri("http://localhost:5092") };

        Dictionary<string, int> mWords;

        public SearchLogic()
        {
            var response = api.Send(new HttpRequestMessage(HttpMethod.Get, "Word"));
            var content = response.Content.ReadAsStringAsync().Result;
            mWords = JsonSerializer.Deserialize<Dictionary<string, int>>(content);
            //mWords = mDatabase.GetAllWords();
        }

        public int GetIdOf(string word)
        {
            if (mWords.ContainsKey(word))
                return mWords[word];
            return -1;
        }

        public List<KeyValuePair<int, int>> GetDocuments(List<int> wordIds)
        {            
            var response = api.Send(new HttpRequestMessage(HttpMethod.Get, "Document/GetByWordIds"));
            var content = response.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<List<KeyValuePair<int, int>>>(content);
           //return mDatabase.GetDocuments(wordIds);
        }

        public List<string> GetDocumentDetails(List<int> docIds)
        {
            var response = api.Send(new HttpRequestMessage(HttpMethod.Get, "Document/GetByDocIds"));
            var content = response.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<List<string>>(content);
            //return mDatabase.GetDocDetails(docIds);
        }
    }
}