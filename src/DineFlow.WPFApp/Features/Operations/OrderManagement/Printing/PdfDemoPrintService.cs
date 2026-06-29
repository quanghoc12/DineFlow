using System.Globalization;
using System.IO;
using System.Text;
namespace DineFlow.WPFApp.Features.Operations.OrderManagement;

internal sealed class PdfDemoPrintService
{
    private readonly string _outputDirectory = ResolveWorkspaceDocumentDirectory();
    private string BillDirectory => Path.Combine(_outputDirectory, "Bill");
    private string KitchenDirectory => Path.Combine(_outputDirectory, "Bep");

    public string PrintKitchenOrder(
        TableCard? table,
        BillPreview bill,
        IReadOnlyList<(BillLinePreview Line, int Quantity)> lines)
    {
        List<string> content =
        [
            "PHIEU BEP",
            $"Ban: {TableName(table)}",
            $"Bill: {bill.DisplayName}",
            $"Thoi gian: {DateTime.Now:HH:mm dd/MM/yyyy}",
            "",
            "MON CAN CHE BIEN"
        ];

        foreach ((BillLinePreview line, int quantity) in lines.Where(x => x.Quantity > 0))
        {
            content.Add($"{quantity} x {line.ItemName}");
            content.Add($"  {line.ChoiceSummary}");
        }

        return WritePdf(KitchenDirectory, "bep-order-moi", content);
    }

    public string PrintKitchenCancel(
        TableCard? table,
        BillPreview bill,
        BillLinePreview line,
        int cancelQuantity,
        string reason)
    {
        List<string> content =
        [
            "PHIEU HUY BEP",
            $"Ban: {TableName(table)}",
            $"Bill: {bill.DisplayName}",
            $"Thoi gian: {DateTime.Now:HH:mm dd/MM/yyyy}",
            "",
            $"Huy: {cancelQuantity} x {line.ItemName}",
            $"Da thong bao bep: {line.NotifiedQuantity}",
            $"Ly do: {reason}",
            "",
            line.ChoiceSummary
        ];

        return WritePdf(KitchenDirectory, "bep-huy-mon", content);
    }

    public string PrintKitchenBillCancel(
        TableCard? table,
        BillPreview bill,
        string reason)
    {
        List<string> content =
        [
            "PHIEU HUY BILL CHO BEP",
            $"Ban: {TableName(table)}",
            $"Bill: {bill.DisplayName}",
            $"Thoi gian: {DateTime.Now:HH:mm dd/MM/yyyy}",
            $"Ly do: {reason}",
            "",
            "MON TRONG BILL"
        ];

        AddBillLines(content, bill);
        content.Add("");
        content.Add($"Tong tien bill huy: {Money(bill.Total)}");

        return WritePdf(KitchenDirectory, "bep-huy-bill", content);
    }

    public string PrintTemporaryBill(TableCard? table, BillPreview bill)
    {
        List<string> content =
        [
            "PHIEU TAM TINH",
            $"Ban: {TableName(table)}",
            $"Bill: {bill.DisplayName}",
            $"Thoi gian: {DateTime.Now:HH:mm dd/MM/yyyy}",
            "",
            "CHI TIET MON"
        ];

        AddBillLines(content, bill);
        content.Add("");
        content.Add($"Tong tien: {Money(bill.Total)}");

        return WritePdf(BillDirectory, "bill-tam-tinh", content);
    }

    public string PrintPaymentReceipt(
        TableCard? table,
        BillPreview bill,
        decimal paidAmount,
        string paymentMethods)
    {
        List<string> content =
        [
            "PHIEU THANH TOAN",
            $"Ban: {TableName(table)}",
            $"Bill: {bill.DisplayName}",
            $"Thoi gian: {DateTime.Now:HH:mm dd/MM/yyyy}",
            "",
            "CHI TIET MON"
        ];

        AddBillLines(content, bill);
        content.Add("");
        content.Add($"Tong tien: {Money(bill.Total)}");
        content.Add($"Khach thanh toan: {Money(paidAmount)}");
        content.Add($"Phuong thuc: {paymentMethods}");

        return WritePdf(BillDirectory, "bill-thanh-toan", content);
    }

    private static void AddBillLines(List<string> content, BillPreview bill)
    {
        foreach (BillLinePreview line in bill.Lines)
        {
            content.Add($"{line.Quantity} x {line.ItemName}  {Money(line.Total)}");
            content.Add($"  {line.ChoiceSummary}");
        }
    }

    private string WritePdf(string directory, string prefix, IReadOnlyList<string> lines)
    {
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, $"{prefix}-{DateTime.Now:yyyyMMdd-HHmmss-fff}.pdf");
        File.WriteAllBytes(path, BuildSimplePdf(lines));

        return path;
    }

    private static byte[] BuildSimplePdf(IReadOnlyList<string> lines)
    {
        static string Escape(string value)
        {
            return NormalizeText(value)
                .Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("(", "\\(", StringComparison.Ordinal)
                .Replace(")", "\\)", StringComparison.Ordinal);
        }

        StringBuilder content = new();
        content.AppendLine("BT");
        content.AppendLine("/F1 13 Tf");
        content.AppendLine("50 790 Td");

        foreach (string line in lines)
        {
            content.Append('(').Append(Escape(line)).AppendLine(") Tj");
            content.AppendLine("0 -20 Td");
        }

        content.AppendLine("ET");
        byte[] contentBytes = Encoding.ASCII.GetBytes(content.ToString());

        List<string> objects =
        [
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
            $"<< /Length {contentBytes.Length} >>\nstream\n{content}endstream"
        ];

        using MemoryStream stream = new();
        void Write(string value)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
        }

        Write("%PDF-1.4\n");
        List<long> offsets = [0];
        for (int i = 0; i < objects.Count; i++)
        {
            offsets.Add(stream.Position);
            Write($"{i + 1} 0 obj\n{objects[i]}\nendobj\n");
        }

        long xrefPosition = stream.Position;
        Write($"xref\n0 {objects.Count + 1}\n");
        Write("0000000000 65535 f \n");
        foreach (long offset in offsets.Skip(1))
        {
            Write($"{offset:0000000000} 00000 n \n");
        }

        Write($"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xrefPosition}\n%%EOF");
        return stream.ToArray();
    }

    private static string NormalizeText(string value)
    {
        string normalized = value.Normalize(NormalizationForm.FormD);
        StringBuilder builder = new();
        foreach (char character in normalized)
        {
            UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder
            .ToString()
            .Normalize(NormalizationForm.FormC)
            .Replace('đ', 'd')
            .Replace('Đ', 'D');
    }

    private static string TableName(TableCard? table)
    {
        return table is null ? "Chua chon ban" : $"{table.TableName} / {table.Area}";
    }

    private static string Money(decimal value)
    {
        return value.ToString("N0", CultureInfo.CurrentCulture);
    }

    private static string ResolveWorkspaceDocumentDirectory()
    {
        string? workspaceRoot = FindWorkspaceRoot(AppContext.BaseDirectory)
            ?? FindWorkspaceRoot(Directory.GetCurrentDirectory());

        return Path.Combine(
            workspaceRoot ?? Directory.GetCurrentDirectory(),
            "document");
    }

    private static string? FindWorkspaceRoot(string startPath)
    {
        DirectoryInfo? directory = new(startPath);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "DineFlow.sln")) &&
                Directory.Exists(Path.Combine(directory.FullName, "src")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }
}
