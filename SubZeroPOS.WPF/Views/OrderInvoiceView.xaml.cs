using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
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
            printDialog.PrintDocument(paginatorSource.DocumentPaginator, "فاتورة سوب زيرو");

            // Also save + open a viewable copy automatically, so the person
            // sees confirmation of what was printed without having to hunt
            // for the output file themselves. Wrapped in try/catch since this
            // is a convenience feature - failure here should never block or
            // break the actual print, which already succeeded above.
            try
            {
                string tempPath = System.IO.Path.Combine(
                    System.IO.Path.GetTempPath(), $"SubZero_Invoice_{vm.Order.OrderId}.xps");

                if (System.IO.File.Exists(tempPath))
                    System.IO.File.Delete(tempPath);

                using (var xpsDoc = new System.Windows.Xps.Packaging.XpsDocument(tempPath, System.IO.FileAccess.ReadWrite))
                {
                    var xpsWriter = System.Windows.Xps.Packaging.XpsDocument.CreateXpsDocumentWriter(xpsDoc);
                    xpsWriter.Write(paginatorSource.DocumentPaginator);
                }

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(tempPath)
                {
                    UseShellExecute = true
                });
            }
            catch
            {
                // Non-critical - the actual print already succeeded above.
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

            doc.Blocks.Add(Centered(order.RestaurantName, 17, FontWeights.Bold));
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
                AddRow(items, $"{item.Quantity} × {item.ItemName}", item.LineTotal.ToString("0.000"));
            if (order.HasDeliveryFee)
                AddRow(items, "رسوم التوصيل", order.DeliveryFee.ToString("0.000"));
            doc.Blocks.Add(items);

            doc.Blocks.Add(Divider());

            var totalTable = NewTable();
            AddRow(totalTable, "الإجمالي", $"{order.TotalAmount:0.000} {order.CurrencySymbol}", bold: true);
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
