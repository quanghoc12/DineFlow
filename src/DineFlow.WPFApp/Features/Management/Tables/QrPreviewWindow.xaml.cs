using DineFlow.BusinessObjects.Tables;
using QRCoder;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DineFlow.WPFApp.Features.Management.Tables;

public partial class QrPreviewWindow : Window
{
    public QrPreviewWindow(ManagedTableDto table)
    {
        InitializeComponent();
        TitleText.Text = table.TableName;
        AreaText.Text = table.Area;
        UrlText.Text = table.QrUrl;

        using QRCodeGenerator generator = new();
        using QRCodeData data = generator.CreateQrCode(table.QrUrl, QRCodeGenerator.ECCLevel.Q);
        byte[] bytes = new PngByteQRCode(data).GetGraphic(15);
        using MemoryStream stream = new(bytes);
        BitmapImage image = new();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.StreamSource = stream;
        image.EndInit();
        image.Freeze();
        QrImage.Source = image;
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e) => Clipboard.SetText(UrlText.Text);

    private void PrintButton_Click(object sender, RoutedEventArgs e)
    {
        PrintDialog dialog = new();
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        StackPanel document = new()
        {
            Width = Math.Max(320, dialog.PrintableAreaWidth - 60),
            Margin = new Thickness(30)
        };
        document.Children.Add(new TextBlock
        {
            Text = TitleText.Text,
            FontSize = 24,
            FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Center
        });
        document.Children.Add(new TextBlock
        {
            Text = AreaText.Text,
            Margin = new Thickness(0, 6, 0, 18),
            TextAlignment = TextAlignment.Center
        });
        document.Children.Add(new Image
        {
            Source = QrImage.Source,
            Width = 280,
            Height = 280,
            Stretch = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Center
        });
        document.Children.Add(new TextBlock
        {
            Text = UrlText.Text,
            Margin = new Thickness(0, 18, 0, 0),
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center
        });
        document.Measure(new Size(dialog.PrintableAreaWidth, dialog.PrintableAreaHeight));
        document.Arrange(new Rect(new Point(0, 0), document.DesiredSize));
        dialog.PrintVisual(document, $"QR {TitleText.Text}");
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
