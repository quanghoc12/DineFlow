using DineFlow.Services.Menu;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers.Staff;

[ApiController]
[Route("api/staff/menu")]
public class StaffMenuController : StaffControllerBase
{
    private readonly IMenuCatalogService _menuCatalogService;

    public StaffMenuController(IMenuCatalogService menuCatalogService)
    {
        _menuCatalogService = menuCatalogService;
    }

    [HttpGet]
    public async Task<ActionResult<MenuCatalogDto>> GetCatalog(
        [FromQuery] int? categoryId,
        [FromQuery] string? search,
        [FromQuery] string? salesChannelCode,
        [FromQuery] bool? availableOnly,
        CancellationToken cancellationToken)
    {
        MenuCatalogDto response = await _menuCatalogService.GetCatalogAsync(new MenuCatalogFilter
        {
            CategoryId = categoryId,
            Search = search,
            SalesChannelCode = salesChannelCode,
            AvailableOnly = availableOnly ?? true
        }, cancellationToken);

        return Ok(response);
    }

    [HttpGet("{menuItemId:int}")]
    public async Task<ActionResult<MenuCatalogItemDto>> GetMenuItem(
        int menuItemId,
        [FromQuery] string? salesChannelCode,
        CancellationToken cancellationToken)
    {
        MenuCatalogItemDto? response = await _menuCatalogService.GetMenuItemAsync(menuItemId, salesChannelCode, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }
}
