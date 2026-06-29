using DineFlow.BusinessObjects.Tables;
using QRCoder;
using System.IO;
using System.Windows;
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
    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
