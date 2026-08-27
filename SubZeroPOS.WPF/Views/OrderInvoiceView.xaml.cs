using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using SubZeroPOS.Core.DTOs;
using SubZeroPOS.WPF.ViewModels;

namespace SubZeroPOS.WPF.Views
{
    public partial class OrderInvoiceView : UserControl
    {
        public OrderInvoiceView()
        {
            InitializeComponent();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is OrderInvoiceViewModel vm)
            {
                vm.PrintRequested += PrintReceipt;
            }
        }

        private void PrintReceipt()
        {
            if (DataContext is not OrderInvoiceViewModel vm || vm.Order is null) return;

            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() != true) return;

            var document = BuildReceiptDocument(vm.Order);

            // Previous attempt used VisualTreeHelper.GetDescendantBounds() on a
            // paginator page - that always returns the full requested PageSize,
            // not the actual ink extent, so it didn't fix the blank-space issue.
            // Measuring via a FlowDocumentScrollViewer with infinite available
            // height gives the document's true natural content height.
            double measuredHeight = MeasureDocumentHeight(document, document.PageWidth);
            document.PageHeight = measuredHeight + 16;

            IDocumentPaginatorSource paginatorSource = document;
            printDialog.PrintDocument(paginatorSource.DocumentPaginator, "فاتورة ساب زيرو");

            // Also save a real PDF copy and open it, so the person can confirm
            // what was printed immediately, in a normal PDF viewer (Edge).
            // We deliberately do NOT ask PdfSharp to draw the Arabic text
            // itself - PdfSharp has no Arabic glyph-shaping support, so hand-
            // drawn Arabic text would render as disconnected/incorrect
            // characters. Instead we rasterize the SAME on-screen receipt
            // (ReceiptBorder), which WPF already renders with fully correct
            // Arabic shaping, and embed that image into the PDF page. This
            // guarantees the PDF looks identical to what's on screen.
            // Wrapped in try/catch since this is a convenience feature only -
            // failure here should never block or break the actual print,
            // which already succeeded above.
            try
            {
                OpenReceiptAsPdf(vm.Order.OrderId);
            }
            catch
            {
                // Non-critical - the actual print already succeeded above.
            }
        }

        private void OpenReceiptAsPdf(int orderId)
        {
            ReceiptBorder.UpdateLayout();

            double width = ReceiptBorder.ActualWidth;
            double height = ReceiptBorder.ActualHeight;
            if (width <= 0 || height <= 0) return;

            double dpiScale = 2.0;
            int pixelWidth = (int)(width * dpiScale);
            int pixelHeight = (int)(height * dpiScale);

            var renderBitmap = new RenderTargetBitmap(
                pixelWidth, pixelHeight, 96 * dpiScale, 96 * dpiScale, PixelFormats.Pbgra32);
            renderBitmap.Render(ReceiptBorder);

            // Encode to PNG bytes so PdfSharp's XImage can read it.
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
            byte[] pngBytes;
            using (var ms = new MemoryStream())
            {
                encoder.Save(ms);
                pngBytes = ms.ToArray();
            }

            using var pdfDocument = new PdfDocument();
            var page = pdfDocument.AddPage();

            // WPF's ActualWidth/Height are in device-independent pixels
            // (1/96 inch). PDF points are 1/72 inch, so convert: pt = dip * 0.75.
            page.Width = XUnit.FromPoint(width * 0.75);
            page.Height = XUnit.FromPoint(height * 0.75);

            using var gfx = XGraphics.FromPdfPage(page);
            using var imageStream = new MemoryStream(pngBytes);
            var ximage = XImage.FromStream(imageStream);
            gfx.DrawImage(ximage, 0, 0, page.Width.Point, page.Height.Point);

            string tempPath = Path.Combine(Path.GetTempPath(), $"SubZero_Invoice_{orderId}.pdf");
            if (File.Exists(tempPath))
                File.Delete(tempPath);
            pdfDocument.Save(tempPath);

            // Try to open explicitly in Microsoft Edge (common install paths).
            // Falls back to the OS default PDF handler if Edge isn't found -
            // this never throws, so it can't break the print flow above.
            string[] possibleEdgePaths =
            {
                @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
                @"C:\Program Files\Microsoft\Edge\Application\msedge.exe"
            };

            string? edgePath = Array.Find(possibleEdgePaths, File.Exists);

            if (edgePath != null)
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(edgePath, $"\"{tempPath}\"")
                {
                    UseShellExecute = true
                });
            }
            else
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(tempPath)
                {
                    UseShellExecute = true
                });
            }
        }

        private static double MeasureDocumentHeight(FlowDocument doc, double width)
        {
            var host = new FlowDocumentScrollViewer
            {
                Document = doc,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled
            };

            host.Measure(new Size(width, double.PositiveInfinity));
            double height = host.DesiredSize.Height;

            // Release the document from this temporary host so it can be
            // reassigned to the print paginator afterward - a FlowDocument can
            // only be "owned" by one container at a time.
            host.Document = null;

            return height;
        }

        private FlowDocument BuildReceiptDocument(OrderInvoiceDto order)
        {
            var doc = new FlowDocument
            {
                FlowDirection = FlowDirection.RightToLeft,
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 13,
                PagePadding = new Thickness(16),
                PageWidth = 320,
                ColumnWidth = 320
            };

            // Logo, if the file exists next to the exe - printing shouldn't
            // fail just because the logo is missing on a fresh install.
            // Uses the full lockup (icon + Arabic + English name already
            // baked into the image) to match the on-screen invoice exactly -
            // so no separate restaurant-name text line is needed below it.
            try
            {
                var logoUri = new Uri("pack://siteoforigin:,,,/Images/Branding/logo.png", UriKind.Absolute);
                var logoBitmap = new System.Windows.Media.Imaging.BitmapImage(logoUri);
                var logoImage = new System.Windows.Controls.Image
                {
                    Source = logoBitmap,
                    Width = 140,
                    Stretch = Stretch.Uniform
                };
                var logoContainer = new BlockUIContainer(logoImage)
                {
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 8)
                };
                doc.Blocks.Add(logoContainer);
            }
            catch
            {
                // Logo file missing/unreadable - fall back to plain text name
                // so the receipt still identifies the restaurant.
                doc.Blocks.Add(Centered(order.RestaurantName, 17, FontWeights.Bold));
            }

            if (!string.IsNullOrWhiteSpace(order.RestaurantAddress))
                doc.Blocks.Add(Centered(order.RestaurantAddress, 11, FontWeights.Normal, Brushes.Gray));
            if (!string.IsNullOrWhiteSpace(order.RestaurantPhone))
                doc.Blocks.Add(Centered(order.RestaurantPhone, 11, FontWeights.Normal, Brushes.Gray, bottomMargin: 10));

            doc.Blocks.Add(Divider());

            // Meta info as simple right-aligned "label: value" lines instead of
            // a 2-column table - the table approach produced uneven, off-center
            // spacing since columns auto-sized to content rather than the full
            // page width.
            if (order.ShowCashierName)
                AddLine(doc, "الكاشير", order.CashierName);
            AddLine(doc, "التاريخ والوقت", order.OrderDateFormatted);
            AddLine(doc, "نوع الطلب", order.OrderTypeNameAr);
            if (!string.IsNullOrWhiteSpace(order.CustomerName))
                AddLine(doc, "اسم الزبون", order.CustomerName);
            AddLine(doc, "طريقة الدفع", order.PaymentMethodNameAr);

            doc.Blocks.Add(Divider());

            var items = NewTable();
            foreach (var item in order.Items)
                AddRow(items, $"{item.Quantity} × {item.ItemName}", item.LineTotal.ToString("#,##0"));
            if (order.HasDeliveryFee)
                AddRow(items, "رسوم التوصيل", order.DeliveryFee.ToString("#,##0"));
            doc.Blocks.Add(items);

            doc.Blocks.Add(Divider());

            var totalTable = NewTable();
            AddRow(totalTable, "الإجمالي", $"{order.TotalAmount:#,##0} {order.CurrencySymbol}", bold: true);
            doc.Blocks.Add(totalTable);

            if (!string.IsNullOrWhiteSpace(order.FooterPrimary) || !string.IsNullOrWhiteSpace(order.FooterSecondary))
            {
                doc.Blocks.Add(Divider());
                if (!string.IsNullOrWhiteSpace(order.FooterPrimary))
                    doc.Blocks.Add(Centered(order.FooterPrimary, 13, FontWeights.Medium));
                if (!string.IsNullOrWhiteSpace(order.FooterSecondary))
                    doc.Blocks.Add(Centered(order.FooterSecondary, 10, FontWeights.Normal, Brushes.Gray));
            }

            return doc;
        }

        private static void AddLine(FlowDocument doc, string label, string value)
        {
            var p = new Paragraph { TextAlignment = TextAlignment.Right, FontSize = 11, Margin = new Thickness(0, 0, 0, 3) };
            p.Inlines.Add(new Run($"{label}: ") { Foreground = Brushes.Gray });
            p.Inlines.Add(new Run(value) { Foreground = Brushes.Black });
            doc.Blocks.Add(p);
        }

        private static Paragraph Centered(string text, double size, FontWeight weight, Brush? foreground = null, double bottomMargin = 2)
        {
            return new Paragraph(new Run(text))
            {
                TextAlignment = TextAlignment.Center,
                FontSize = size,
                FontWeight = weight,
                Foreground = foreground ?? Brushes.Black,
                Margin = new Thickness(0, 0, 0, bottomMargin)
            };
        }

        private static Paragraph Divider()
        {
            return new Paragraph
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Margin = new Thickness(0, 6, 0, 6)
            };
        }

        private static Table NewTable()
        {
            var table = new Table();
            // Star widths force the table to span the FULL page width with
            // proportional columns - fixing the "not centered/aligned" look
            // that Auto-width columns produced (they shrink-wrapped to content
            // instead of spanning evenly).
            table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
            table.RowGroups.Add(new TableRowGroup());
            return table;
        }

        private static void AddRow(Table table, string label, string value, bool bold = false)
        {
            var row = new TableRow();
            var weight = bold ? FontWeights.Bold : FontWeights.Normal;
            var size = bold ? 15.0 : 12.0;

            row.Cells.Add(new TableCell(new Paragraph(new Run(label)) { FontSize = size, FontWeight = weight })
            { TextAlignment = TextAlignment.Right });
            row.Cells.Add(new TableCell(new Paragraph(new Run(value)) { FontSize = size, FontWeight = weight, Foreground = bold ? Brushes.Black : Brushes.DarkSlateGray })
            { TextAlignment = TextAlignment.Left });

            table.RowGroups[0].Rows.Add(row);
        }

  
    }
}
