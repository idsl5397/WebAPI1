using Microsoft.AspNetCore.Mvc;
using WebAPI1.Models;
using WebAPI1.Services;

namespace WebAPI1.Controllers;

[Route("[controller]")]
public class SuggestController: ControllerBase
{
    private readonly isha_sys_devContext _db;
    private readonly ILogger<SuggestController> _logger;
    private readonly ISuggestService _suggestService;
    
    public SuggestController(ILogger<SuggestController> logger,isha_sys_devContext db,ISuggestService suggestService)
    {
        _db = db;
        _suggestService = suggestService;
        _logger = logger;
    }

    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SuggestDto>>> GetAll()
    {
        var suggests = await _suggestService.GetAllSuggestsAsync();
        return Ok(suggests);
    }
}