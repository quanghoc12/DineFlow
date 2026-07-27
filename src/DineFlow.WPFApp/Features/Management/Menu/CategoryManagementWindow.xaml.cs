using DineFlow.BusinessObjects.Menu;
using System.Windows;
using System.Windows.Controls;

namespace DineFlow.WPFApp.Features.Management.Menu;

public partial class CategoryManagementWindow : Window
{
    private readonly MenuManagementViewModel _viewModel;
    private ManagedCategoryDto? _selected;

    public CategoryManagementWindow(IEnumerable<ManagedCategoryDto> categories, MenuManagementViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        CategoryList.ItemsSource = categories.ToList();
    }

    private void CategoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = CategoryList.SelectedItem as ManagedCategoryDto;
        if (_selected is null) return;
        NameTextBox.Text = _selected.CategoryName;
        DescriptionTextBox.Text = _selected.Description ?? string.Empty;
        OrderTextBox.Text = _selected.DisplayOrder.ToString();
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        CategoryList.SelectedItem = null;
        NameTextBox.Clear();
        DescriptionTextBox.Clear();
        OrderTextBox.Text = "0";
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(OrderTextBox.Text, out int order))
        {
            ErrorText.Text = "Thứ tự phải là số nguyên.";
            return;
        }
        await _viewModel.SaveCategoryAsync(new SaveCategoryRequest
        {
            CategoryId = _selected?.CategoryId,
            CategoryName = NameTextBox.Text,
            Description = DescriptionTextBox.Text,
            DisplayOrder = order
        });
        ErrorText.Text = _viewModel.ErrorMessage;
        if (string.IsNullOrEmpty(ErrorText.Text)) Close();
    }

    private async void ToggleButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        await _viewModel.ToggleCategoryAsync(_selected);
        ErrorText.Text = _viewModel.ErrorMessage;
        if (string.IsNullOrEmpty(ErrorText.Text)) Close();
    }
}
