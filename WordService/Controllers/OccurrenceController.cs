using Microsoft.AspNetCore.Mvc;

namespace WordService.Controllers;

[ApiController]
[Route("[controller]")]
public class OccurrenceController : ControllerBase
{
    private Database database = new();

    [HttpPost]
    public void Post(int docId, ISet<int> wordIds)
    {
        database.InsertAllOcc(docId, wordIds);
    }
}