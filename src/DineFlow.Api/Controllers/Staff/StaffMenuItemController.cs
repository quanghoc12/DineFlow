using DineFlow.BusinessObjects.Menu;
using DineFlow.Services.Menu;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers.Staff;

[ApiController]
[Route("api/staff/menu-items")]
public class StaffMenuItemController : ControllerBase
{
    private readonly IMenuItemService _menuItemService;

    public StaffMenuItemController(IMenuItemService menuItemService)
    {
        _menuItemService = menuItemService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_menuItemService.GetAll());
    }

    [HttpPost]
    public IActionResult Create([FromBody] MenuItem item)
    {
        try
        {
            return Ok(_menuItemService.Create(item));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
