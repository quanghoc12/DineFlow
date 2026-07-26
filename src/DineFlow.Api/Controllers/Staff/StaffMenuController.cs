using DineFlow.Api.Services;
using DineFlow.Services.Menu;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers.Staff;

[ApiController]
[Route("api/staff/menu")]
public class StaffMenuController : StaffControllerBase
{
    private readonly IMenuCatalogService _menuCatalogService;
    private readonly IMenuImageStorageService _menuImageStorageService;

    public StaffMenuController(
        IMenuCatalogService menuCatalogService,
        IMenuImageStorageService menuImageStorageService)
    {
        _menuCatalogService = menuCatalogService;
        _menuImageStorageService = menuImageStorageService;
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

    [HttpPost("images")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<ActionResult<MenuImageUploadResult>> UploadImage(
        [FromForm] IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return BadRequest(new { code = "ImageRequired", message = "Vui lòng chọn ảnh món." });
        }

        try
        {
            MenuImageUploadResult result = await _menuImageStorageService.UploadMenuImageAsync(file, cancellationToken);
            return Ok(result);
        }
        catch (MenuImageUploadException ex)
        {
            return BadRequest(new { code = "ImageUploadFailed", message = ex.Message });
        }
    }
}
