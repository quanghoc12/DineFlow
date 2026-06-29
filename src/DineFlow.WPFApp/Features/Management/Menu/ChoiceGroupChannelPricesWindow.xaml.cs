using DineFlow.BusinessObjects.Menu;
using DineFlow.WPFApp.ViewModels;
using System.Windows;

namespace DineFlow.WPFApp.Features.Management.Menu;

public partial class ChoiceGroupChannelPricesWindow : Window
{
    private readonly ManagedChoiceGroupDto _choiceGroup;
    private readonly MenuManagementViewModel _viewModel;

    public ChoiceGroupChannelPricesWindow(
        ManagedChoiceGroupDto choiceGroup,
        ManagedSalesChannelDto channel,
        MenuManagementViewModel viewModel)
    {
        InitializeComponent();

        _choiceGroup = choiceGroup;
        SelectedChannel = channel;
        _viewModel = viewModel;

        TitleText.Text = $"Nhóm: {choiceGroup.GroupName}";
        SubtitleText.Text = $"Cấu hình giá các lựa chọn phụ trên kênh: {channel.ChannelName}";

        LoadData();
    }

    public ManagedSalesChannelDto SelectedChannel { get; }

    private void LoadData()
    {
        // Bind directly to the choice items of the selected group
        ChoiceItemsGrid.ItemsSource = _choiceGroup.Items.OrderBy(x => x.ChoiceName).ToList();
    }

    private async void EditChoicePriceRow_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not ManagedChoiceItemDto choiceItem) return;

        decimal currentExtra = _viewModel.GetChoiceItemChannelExtraPrice(choiceItem, SelectedChannel);

        ChoiceItemChannelPriceEditorWindow dialog = new(
            choiceItem.ChoiceName,
            choiceItem.ExtraPrice,
            currentExtra,
            SelectedChannel.ChannelName)
        {
            Owner = this
        };

        if (dialog.ShowDialog() == true)
        {
            await _viewModel.SaveChoiceItemChannelPriceAsync(new SaveChannelPriceRequest
            {
                MenuItemId = 0,
                ChoiceItemId = choiceItem.ChoiceItemId,
                SalesChannelId = SelectedChannel.SalesChannelId,
                ChannelExtraPrice = dialog.ResultPrice
            });

            if (string.IsNullOrEmpty(_viewModel.ErrorMessage))
            {
                // Reload local item extra price data from viewmodel
                await _viewModel.LoadAsync();

                // Refresh the items source
                var freshGroup = _viewModel.ChoiceGroups.FirstOrDefault(x => x.ChoiceGroupId == _choiceGroup.ChoiceGroupId);
                if (freshGroup != null)
                {
                    ChoiceItemsGrid.ItemsSource = freshGroup.Items.OrderBy(x => x.ChoiceName).ToList();
                }
            }
            else
            {
                MessageBox.Show(_viewModel.ErrorMessage, "Lỗi lưu giá");
            }
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        DragMove();
    }
}
