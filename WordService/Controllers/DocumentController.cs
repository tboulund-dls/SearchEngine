using Microsoft.AspNetCore.Mvc;

namespace WordService.Controllers;

[ApiController]
[Route("[controller]")]
public class DocumentController : ControllerBase
{
    private Database database = new();

    [HttpGet("GetByDocIds")]
    public List<string> GetByDocIds(List<int> docIds)
    {
        return database.GetDocDetails(docIds);
    }
    
    [HttpGet("GetByWordIds")]
    public List<KeyValuePair<int, int>> GetByWordIds(List<int> wordIds)
    {
        return database.GetDocuments(wordIds);
    }

    [HttpPost]
    public void Post(int id, string url)
    {
        database.InsertDocument(id, url);
    }
}