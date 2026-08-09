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

        // Switched from PrintVisual/RenderTargetBitmap (produced blank pages
        // with some print drivers - a known WPF quirk with RTL visuals) to
        // FlowDocument printing, which is WPF's actual intended mechanism for
        // printing formatted/RTL text reliably.
        private void PrintReceipt()
        {
            if (DataContext is not OrderInvoiceViewModel vm || vm.Order is null) return;

            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() != true) return;

            var document = BuildReceiptDocument(vm.Order);

            // FlowDocument defaults to full Letter-page height (11in) if PageHeight
            // isn't set, leaving a huge blank area below a short receipt. Estimate
            // the actual content height from what we know we're printing (header +
            // meta rows + one row per item + footer) and size the page to fit,
            // similar to how a real receipt printer only prints as much paper as
            // the content needs.
            double estimatedHeight = 170; // header + meta rows + total + margins
            estimatedHeight += vm.Order.Items.Count * 20;
            if (vm.Order.HasDeliveryFee) estimatedHeight += 20;
            if (!string.IsNullOrWhiteSpace(vm.Order.CustomerName)) estimatedHeight += 18;
            if (!string.IsNullOrWhiteSpace(vm.Order.FooterPrimary)) estimatedHeight += 22;
            if (!string.IsNullOrWhiteSpace(vm.Order.FooterSecondary)) estimatedHeight += 30;
            document.PageHeight = estimatedHeight;

            IDocumentPaginatorSource paginatorSource = document;
            printDialog.PrintDocument(paginatorSource.DocumentPaginator, "فاتورة سوب زيرو");
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

            var meta = NewTable();
            AddRow(meta, "الكاشير", order.CashierName);
            AddRow(meta, "التاريخ والوقت", order.OrderDate.ToString("yyyy-MM-dd HH:mm"));
            AddRow(meta, "نوع الطلب", order.OrderTypeNameAr);
            if (!string.IsNullOrWhiteSpace(order.CustomerName))
                AddRow(meta, "اسم الزبون", order.CustomerName);
            AddRow(meta, "طريقة الدفع", order.PaymentMethodNameAr);
            doc.Blocks.Add(meta);

            doc.Blocks.Add(Divider());

            var items = NewTable();
            foreach (var item in order.Items)
                AddRow(items, $"{item.Quantity} × {item.ItemName}", item.LineTotal.ToString("0.000"));
            if (order.HasDeliveryFee)
                AddRow(items, "رسوم التوصيل", order.DeliveryFee.ToString("0.000"));
            doc.Blocks.Add(items);

            doc.Blocks.Add(Divider());

            var totalTable = NewTable();
            AddRow(totalTable, "الإجمالي", order.TotalAmount.ToString("0.000"), bold: true);
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
            table.Columns.Add(new TableColumn());
            table.Columns.Add(new TableColumn());
            table.RowGroups.Add(new TableRowGroup());
            return table;
        }

        private static void AddRow(Table table, string label, string value, bool bold = false)
        {
            var row = new TableRow();
            var weight = bold ? FontWeights.Bold : FontWeights.Normal;
            var size = bold ? 15.0 : 12.0;

            // First cell added lands on the right in RightToLeft flow -
            // put the value there (matches on-screen layout: label -> right, value -> left... 
            // actually we want label on the right like the on-screen UI, so label first).
            row.Cells.Add(new TableCell(new Paragraph(new Run(label)) { FontSize = size, FontWeight = weight })
            { TextAlignment = TextAlignment.Right });
            row.Cells.Add(new TableCell(new Paragraph(new Run(value)) { FontSize = size, FontWeight = weight, Foreground = bold ? Brushes.Black : Brushes.DarkSlateGray })
            { TextAlignment = TextAlignment.Left });

            table.RowGroups[0].Rows.Add(row);
        }
    }
}
