using DineFlow.BusinessObjects.Menu;
using DineFlow.Repositories.Menu;

namespace DineFlow.Services.Menu;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService() : this(new CategoryRepository())
    {
    }

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public List<Category> GetAll() => _categoryRepository.GetAll();

    public Category Create(Category category)
    {
        Validate(category);
        return _categoryRepository.Add(category);
    }

    public void Update(Category category)
    {
        Validate(category);
        _categoryRepository.Update(category);
    }

    private static void Validate(Category category)
    {
        if (string.IsNullOrWhiteSpace(category.CategoryName))
        {
            throw new Exception("Tên category không được để trống.");
        }

        if (category.DisplayOrder < 0)
        {
            throw new Exception("DisplayOrder không được âm.");
        }
    }
}
