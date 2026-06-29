using DineFlow.Services.Menu;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers.Customer;

[ApiController]
[Route("api/customer/menu")]
public class CustomerMenuController : ControllerBase
{
    private readonly IMenuCatalogService _menuCatalogService;

    public CustomerMenuController(IMenuCatalogService menuCatalogService)
    {
        _menuCatalogService = menuCatalogService;
    }

    [HttpGet]
    public async Task<ActionResult<MenuCatalogDto>> GetCatalog(
        [FromQuery] int? categoryId,
        [FromQuery] string? search,
        [FromQuery] string? salesChannelCode,
        CancellationToken cancellationToken)
    {
        MenuCatalogDto response = await _menuCatalogService.GetCatalogAsync(new MenuCatalogFilter
        {
            CategoryId = categoryId,
            Search = search,
            SalesChannelCode = salesChannelCode,
            AvailableOnly = true
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

        if (response is null || !response.IsAvailable)
        {
            return NotFound();
        }

        return Ok(response);
    }
}
