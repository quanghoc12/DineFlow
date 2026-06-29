using DineFlow.BusinessObjects.Menu;
using System.Windows;

namespace DineFlow.WPFApp.Features.Management.Menu;

public partial class SalesChannelEditorWindow : Window
{
    private readonly ManagedSalesChannelDto? _channel;
    private readonly List<ManagedSalesChannelDto> _existingChannels;

    public SalesChannelEditorWindow(IEnumerable<ManagedSalesChannelDto> existingChannels, ManagedSalesChannelDto? channel = null)
    {
        InitializeComponent();
        _channel = channel;
        _existingChannels = existingChannels.ToList();

        if (channel is null)
        {
            return;
        }

        HeadingText.Text = "Chỉnh sửa kênh bán";
        ToggleButton.Visibility = Visibility.Visible;

        bool isDefault = channel.ChannelCode == "TAI_QUAN" || channel.ChannelName.ToLower().Contains("tại quán");
        if (!isDefault)
        {
            DeleteButton.Visibility = Visibility.Visible;
        }

        CodeTextBox.Text = channel.ChannelCode;
        // Don't allow changing code of existing channel easily to maintain database consistencies
        CodeTextBox.IsEnabled = false;
        NameTextBox.Text = channel.ChannelName;
    }

    public SaveSalesChannelRequest Request { get; private set; } = new();
    public bool ToggleActiveRequested { get; private set; }
    public bool DeleteRequested { get; private set; }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        string code = CodeTextBox.Text.Trim();
        string name = NameTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(code))
        {
            ErrorText.Text = "Mã kênh bán không được để trống.";
            return;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            ErrorText.Text = "Tên kênh bán không được để trống.";
            return;
        }

        // Validate duplicates for new channels
        if (_channel is null)
        {
            bool codeExists = _existingChannels.Any(x => x.ChannelCode.Equals(code, StringComparison.OrdinalIgnoreCase));
            if (codeExists)
            {
                ErrorText.Text = "Mã kênh bán đã tồn tại.";
                return;
            }
        }

        Request = new SaveSalesChannelRequest
        {
            SalesChannelId = _channel?.SalesChannelId,
            ChannelCode = code,
            ChannelName = name
        };

        DialogResult = true;
    }

    private void ToggleButton_Click(object sender, RoutedEventArgs e)
    {
        if (_channel is null) return;
        ToggleActiveRequested = true;
        DialogResult = true;
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_channel is null) return;
        DeleteRequested = true;
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        DragMove();
    }
}
