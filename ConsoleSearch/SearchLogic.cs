using System;
using System.Collections.Generic;

namespace ConsoleSearch
{
    public class SearchLogic
    {
        Database mDatabase = new();

        Dictionary<string, int> mWords;

        public SearchLogic()
        {
            mWords = mDatabase.GetAllWords();
        }

        public int GetIdOf(string word)
        {
            if (mWords.ContainsKey(word))
                return mWords[word];
            return -1;
        }

        public Dictionary<int, int> GetDocuments(List<int> wordIds)
        {
            return mDatabase.GetDocuments(wordIds);
        }

        public List<string> GetDocumentDetails(List<int> docIds)
        {
            return mDatabase.GetDocDetails(docIds);
        }
    }
}