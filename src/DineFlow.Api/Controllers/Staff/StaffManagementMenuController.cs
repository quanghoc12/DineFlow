using DineFlow.BusinessObjects.Menu;
using DineFlow.Services.Menu;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers.Staff;

[ApiController]
[Route("api/staff/management/menu")]
public sealed class StaffManagementMenuController : StaffControllerBase
{
    private readonly IMenuManagementService _menuManagementService;

    public StaffManagementMenuController(IMenuManagementService menuManagementService)
    {
        _menuManagementService = menuManagementService;
    }

    [HttpGet("categories")]
    public async Task<ActionResult<IReadOnlyList<ManagedCategoryDto>>> GetCategories(CancellationToken cancellationToken)
    {
        return Ok(await _menuManagementService.GetCategoriesAsync(cancellationToken));
    }

    [HttpPost("categories")]
    public async Task<IActionResult> SaveCategory([FromBody] SaveCategoryRequest request, CancellationToken cancellationToken)
    {
        await _menuManagementService.SaveCategoryAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPatch("categories/{categoryId:int}/active")]
    public async Task<IActionResult> SetCategoryActive(int categoryId, [FromBody] SetCategoryActiveRequest request, CancellationToken cancellationToken)
    {
        await _menuManagementService.SetCategoryActiveAsync(categoryId, request.IsActive, cancellationToken);
        return NoContent();
    }

    [HttpDelete("categories/{categoryId:int}")]
    public async Task<IActionResult> DeleteCategory(int categoryId, CancellationToken cancellationToken)
    {
        await _menuManagementService.DeleteCategoryAsync(categoryId, cancellationToken);
        return NoContent();
    }

    [HttpGet("items")]
    public async Task<ActionResult<IReadOnlyList<ManagedMenuItemDto>>> GetItems(CancellationToken cancellationToken)
    {
        return Ok(await _menuManagementService.GetItemsAsync(cancellationToken));
    }

    [HttpPost("items")]
    public async Task<IActionResult> SaveItem([FromBody] SaveMenuItemRequest request, CancellationToken cancellationToken)
    {
        await _menuManagementService.SaveItemAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPatch("items/{itemId:int}/availability")]
    public async Task<IActionResult> SetItemAvailability(int itemId, [FromBody] SetItemAvailabilityRequest request, CancellationToken cancellationToken)
    {
        await _menuManagementService.SetItemAvailabilityAsync(itemId, request.IsAvailable, cancellationToken);
        return NoContent();
    }

    [HttpPatch("items/{itemId:int}/deleted")]
    public async Task<IActionResult> SetItemDeleted(int itemId, [FromBody] SetItemDeletedRequest request, CancellationToken cancellationToken)
    {
        await _menuManagementService.SetItemDeletedAsync(itemId, request.IsDeleted, cancellationToken);
        return NoContent();
    }

    [HttpGet("choice-groups")]
    public async Task<ActionResult<IReadOnlyList<ManagedChoiceGroupDto>>> GetChoiceGroups(CancellationToken cancellationToken)
    {
        return Ok(await _menuManagementService.GetChoiceGroupsAsync(cancellationToken));
    }

    [HttpPost("choice-groups")]
    public async Task<IActionResult> SaveChoiceGroup([FromBody] SaveChoiceGroupRequest request, CancellationToken cancellationToken)
    {
        await _menuManagementService.SaveChoiceGroupAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPatch("choice-groups/{choiceGroupId:int}/availability")]
    public async Task<IActionResult> SetChoiceGroupAvailability(int choiceGroupId, [FromBody] SetChoiceGroupAvailabilityRequest request, CancellationToken cancellationToken)
    {
        await _menuManagementService.SetChoiceGroupAvailabilityAsync(choiceGroupId, request.IsAvailable, cancellationToken);
        return NoContent();
    }

    [HttpPost("choice-items")]
    public async Task<IActionResult> SaveChoiceItem([FromBody] SaveChoiceItemRequest request, CancellationToken cancellationToken)
    {
        await _menuManagementService.SaveChoiceItemAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPatch("choice-items/{choiceItemId:int}/availability")]
    public async Task<IActionResult> SetChoiceItemAvailability(int choiceItemId, [FromBody] SetChoiceItemAvailabilityRequest request, CancellationToken cancellationToken)
    {
        await _menuManagementService.SetChoiceItemAvailabilityAsync(choiceItemId, request.IsAvailable, cancellationToken);
        return NoContent();
    }

    [HttpPost("choice-group-assignments")]
    public async Task<IActionResult> AssignChoiceGroup([FromBody] AssignChoiceGroupRequest request, CancellationToken cancellationToken)
    {
        await _menuManagementService.AssignChoiceGroupAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("items/{menuItemId:int}/choice-groups/{choiceGroupId:int}")]
    public async Task<IActionResult> RemoveChoiceGroupAssignment(int menuItemId, int choiceGroupId, CancellationToken cancellationToken)
    {
        await _menuManagementService.RemoveChoiceGroupAssignmentAsync(menuItemId, choiceGroupId, cancellationToken);
        return NoContent();
    }

    [HttpGet("sales-channels")]
    public async Task<ActionResult<IReadOnlyList<ManagedSalesChannelDto>>> GetSalesChannels(CancellationToken cancellationToken)
    {
        return Ok(await _menuManagementService.GetSalesChannelsAsync(cancellationToken));
    }

    [HttpPost("sales-channels")]
    public async Task<IActionResult> SaveSalesChannel([FromBody] SaveSalesChannelRequest request, CancellationToken cancellationToken)
    {
        await _menuManagementService.SaveSalesChannelAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPatch("sales-channels/{salesChannelId:int}/active")]
    public async Task<IActionResult> SetSalesChannelActive(int salesChannelId, [FromBody] SetSalesChannelActiveRequest request, CancellationToken cancellationToken)
    {
        await _menuManagementService.SetSalesChannelActiveAsync(salesChannelId, request.IsActive, cancellationToken);
        return NoContent();
    }

    [HttpDelete("sales-channels/{salesChannelId:int}")]
    public async Task<IActionResult> DeleteSalesChannel(int salesChannelId, CancellationToken cancellationToken)
    {
        await _menuManagementService.DeleteSalesChannelAsync(salesChannelId, cancellationToken);
        return NoContent();
    }

    [HttpPost("item-channel-prices")]
    public async Task<IActionResult> SaveMenuItemChannelPrice([FromBody] SaveChannelPriceRequest request, CancellationToken cancellationToken)
    {
        await _menuManagementService.SaveMenuItemChannelPriceAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPost("choice-item-channel-prices")]
    public async Task<IActionResult> SaveChoiceItemChannelPrice([FromBody] SaveChannelPriceRequest request, CancellationToken cancellationToken)
    {
        await _menuManagementService.SaveChoiceItemChannelPriceAsync(request, cancellationToken);
        return NoContent();
    }
}

public sealed class SetCategoryActiveRequest { public bool IsActive { get; set; } }
public sealed class SetItemAvailabilityRequest { public bool IsAvailable { get; set; } }
public sealed class SetItemDeletedRequest { public bool IsDeleted { get; set; } }
public sealed class SetChoiceGroupAvailabilityRequest { public bool IsAvailable { get; set; } }
public sealed class SetChoiceItemAvailabilityRequest { public bool IsAvailable { get; set; } }
public sealed class SetSalesChannelActiveRequest { public bool IsActive { get; set; } }
