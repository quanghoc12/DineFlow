using DineFlow.BusinessObjects.Menu;
using DineFlow.Repositories.Menu;

namespace DineFlow.Services.Menu;

public class MenuItemService : IMenuItemService
{
    private readonly IMenuItemRepository _menuItemRepository;

    public MenuItemService() : this(new MenuItemRepository())
    {
    }

    public MenuItemService(IMenuItemRepository menuItemRepository)
    {
        _menuItemRepository = menuItemRepository;
    }

    public List<MenuItem> GetAll() => _menuItemRepository.GetAll();

    public List<MenuItem> Search(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return _menuItemRepository.GetAll();
        }

        return _menuItemRepository.Search(keyword.Trim());
    }

    public MenuItem Create(MenuItem item)
    {
        Validate(item);
        ApplyStockRule(item);
        return _menuItemRepository.Add(item);
    }

    public void Update(MenuItem item)
    {
        Validate(item);
        ApplyStockRule(item);
        _menuItemRepository.Update(item);
    }

    private static void Validate(MenuItem item)
    {
        if (string.IsNullOrWhiteSpace(item.ItemName))
        {
            throw new Exception("Tên món không được để trống.");
        }

        if (item.CategoryId <= 0)
        {
            throw new Exception("Category không hợp lệ.");
        }

        if (item.Price < 0)
        {
            throw new Exception("Giá món không được âm.");
        }

        if (item.TrackStock && item.AvailableQuantity is null or < 0)
        {
            throw new Exception("Món có quản lý stock phải có số lượng >= 0.");
        }
    }

    private static void ApplyStockRule(MenuItem item)
    {
        if (item.TrackStock && item.AvailableQuantity == 0)
        {
            item.IsAvailable = false;
        }
    }
}
