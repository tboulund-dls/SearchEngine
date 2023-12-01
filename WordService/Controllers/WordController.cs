using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace WordService.Controllers;

[ApiController]
[Route("[controller]")]
public class WordController : ControllerBase
{
    private Database database = new Database();

    [HttpGet]
    public Dictionary<string, int> Get()
    {
        return database.GetAllWords();
    }

    [HttpPost]
    public void Post(Dictionary<string, int> res)
    {
        database.InsertAllWords(res);
    }
}