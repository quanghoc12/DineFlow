using DineFlow.Services.Menu;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers.Customer;

[ApiController]
[Route("api/customer/menu")]
public class CustomerMenuController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly IMenuItemService _menuItemService;

    public CustomerMenuController(ICategoryService categoryService, IMenuItemService menuItemService)
    {
        _categoryService = categoryService;
        _menuItemService = menuItemService;
    }

    [HttpGet]
    public IActionResult GetMenu()
    {
        var categories = _categoryService.GetAll().Where(x => x.IsActive).ToList();
        var items = _menuItemService.GetAll().Where(x => x.IsActive).ToList();

        return Ok(new
        {
            categories,
            items
        });
    }
}
