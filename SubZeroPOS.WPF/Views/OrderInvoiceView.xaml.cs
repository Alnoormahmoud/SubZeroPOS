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

        // =========================================================
        // DATA CONTEXT
        // =========================================================

        private void OnDataContextChanged(
            object sender,
            DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is OrderInvoiceViewModel vm)
            {
                vm.PrintRequested += PrintReceipt;
            }
        }

        // =========================================================
        // PRINT RECEIPT
        // =========================================================

        private void PrintReceipt()
        {
            if (DataContext is not OrderInvoiceViewModel vm ||
                vm.Order is null)
            {
                return;
            }

            var printDialog = new PrintDialog();

            if (printDialog.ShowDialog() != true)
                return;

            // Build the professional RTL receipt.
            var document = BuildReceiptDocument(vm.Order);

            // Measure the natural height of the document.
            double measuredHeight =
                MeasureDocumentHeight(
                    document,
                    document.PageWidth);

            document.PageHeight =
                measuredHeight + 16;

            // Print.
            IDocumentPaginatorSource paginatorSource =
                document;

            printDialog.PrintDocument(
                paginatorSource.DocumentPaginator,
                "فاتورة ساب زيرو");

            // Open PDF only if enabled in settings.
            if (vm.Order.OpenPdfAfterPrinting)
            {
                try
                {
                    OpenReceiptAsPdf(vm.Order);
                }
                catch
                {
                    // PDF generation is optional.
                    // Printing has already succeeded.
                }
            }
        }

        // =========================================================
        // PDF
        // =========================================================

        private void OpenReceiptAsPdf(OrderInvoiceDto order)
        {
            ReceiptBorder.UpdateLayout();

            double width = ReceiptBorder.ActualWidth;
            double height = ReceiptBorder.ActualHeight;

            if (width <= 0 || height <= 0)
                return;

            double dpiScale = 2.0;

            int pixelWidth =
                (int)(width * dpiScale);

            int pixelHeight =
                (int)(height * dpiScale);

            var renderBitmap =
                new RenderTargetBitmap(
                    pixelWidth,
                    pixelHeight,
                    96 * dpiScale,
                    96 * dpiScale,
                    PixelFormats.Pbgra32);

            renderBitmap.Render(ReceiptBorder);

            // Convert WPF visual to PNG.
            var encoder = new PngBitmapEncoder();

            encoder.Frames.Add(
                BitmapFrame.Create(renderBitmap));

            byte[] pngBytes;

            using (var ms = new MemoryStream())
            {
                encoder.Save(ms);
                pngBytes = ms.ToArray();
            }

            using var pdfDocument =
                new PdfDocument();

            var page =
                pdfDocument.AddPage();

            // WPF uses DIP (96 DPI).
            // PDF uses points (72 DPI).
            page.Width =
                XUnit.FromPoint(width * 0.75);

            page.Height =
                XUnit.FromPoint(height * 0.75);

            using var gfx =
                XGraphics.FromPdfPage(page);

            using var imageStream =
                new MemoryStream(pngBytes);

            var ximage =
                XImage.FromStream(imageStream);

            gfx.DrawImage(
                ximage,
                0,
                0,
                page.Width.Point,
                page.Height.Point);

            string tempPath =
                Path.Combine(
                    Path.GetTempPath(),
                    $"SubZero_Invoice_{order.OrderId}.pdf");

            if (File.Exists(tempPath))
                File.Delete(tempPath);

            pdfDocument.Save(tempPath);

            // Open PDF.
            string[] possibleEdgePaths =
            {
                @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
                @"C:\Program Files\Microsoft\Edge\Application\msedge.exe"
            };

            string? edgePath =
                Array.Find(
                    possibleEdgePaths,
                    File.Exists);

            if (edgePath != null)
            {
                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo(
                        edgePath,
                        $"\"{tempPath}\"")
                    {
                        UseShellExecute = true
                    });
            }
            else
            {
                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo(
                        tempPath)
                    {
                        UseShellExecute = true
                    });
            }
        }

        // =========================================================
        // MEASURE DOCUMENT
        // =========================================================

        private static double MeasureDocumentHeight(
            FlowDocument doc,
            double width)
        {
            var host =
                new FlowDocumentScrollViewer
                {
                    Document = doc,

                    HorizontalScrollBarVisibility =
                        ScrollBarVisibility.Disabled,

                    VerticalScrollBarVisibility =
                        ScrollBarVisibility.Disabled
                };

            host.Measure(
                new Size(
                    width,
                    double.PositiveInfinity));

            double height =
                host.DesiredSize.Height;

            // Release the document.
            host.Document = null;

            return height;
        }

        // =========================================================
        // BUILD RECEIPT
        // =========================================================

        private FlowDocument BuildReceiptDocument(
            OrderInvoiceDto order)
        {
            // 58mm receipt ≈ 232 DIP
            // 80mm receipt ≈ 302 DIP
            double pageWidth =
                order.ReceiptPaperWidthMm <= 58
                    ? 232
                    : 302;

            var doc =
                new FlowDocument
                {
                    FlowDirection =
                        FlowDirection.RightToLeft,

                    FontFamily =
                        new FontFamily("Segoe UI"),

                    FontSize = 12,

                    PagePadding =
                        new Thickness(
                            10,
                            12,
                            10,
                            12),

                    PageWidth =
                        pageWidth,

                    ColumnWidth =
                        pageWidth,

                    TextAlignment =
                        TextAlignment.Right
                };

            // =====================================================
            // LOGO
            // =====================================================

            if (order.ShowLogoOnInvoice)
            {
                try
                {
                    var logoUri =
                        new Uri(
                            "pack://siteoforigin:,,,/Images/Branding/logo.png",
                            UriKind.Absolute);

                    var logoBitmap =
                        new BitmapImage(logoUri);

                    var logoImage =
                        new Image
                        {
                            Source = logoBitmap,

                            Width =
                                order.ReceiptPaperWidthMm <= 58
                                    ? 115
                                    : 140,

                            Stretch =
                                Stretch.Uniform
                        };

                    var logoContainer =
                        new BlockUIContainer(
                            logoImage)
                        {
                            TextAlignment =
                                TextAlignment.Center,

                            Margin =
                                new Thickness(
                                    0,
                                    0,
                                    0,
                                    6)
                        };

                    doc.Blocks.Add(
                        logoContainer);
                }
                catch
                {
                    // If logo doesn't exist,
                    // show restaurant name instead.
                    doc.Blocks.Add(
                        Centered(
                            order.RestaurantName,
                            17,
                            FontWeights.Bold));
                }
            }
            else
            {
                doc.Blocks.Add(
                    Centered(
                        order.RestaurantName,
                        17,
                        FontWeights.Bold));
            }

            // =====================================================
            // RESTAURANT ADDRESS
            // =====================================================

            if (!string.IsNullOrWhiteSpace(
                    order.RestaurantAddress))
            {
                doc.Blocks.Add(
                    Centered(
                        order.RestaurantAddress,
                        10,
                        FontWeights.Normal,
                        Brushes.Gray));
            }

            // =====================================================
            // RESTAURANT PHONE
            // =====================================================

            if (!string.IsNullOrWhiteSpace(
                    order.RestaurantPhone))
            {
                doc.Blocks.Add(
                    Centered(
                        order.RestaurantPhone,
                        10,
                        FontWeights.Normal,
                        Brushes.Gray,
                        8));
            }

            // =====================================================
            // TOP SEPARATOR
            // =====================================================

            doc.Blocks.Add(
                Divider());

            // =====================================================
            // INVOICE TITLE
            // =====================================================

            doc.Blocks.Add(
                Centered(
                    "فاتورة بيع",
                    16,
                    FontWeights.Bold,
                    Brushes.Black,
                    8));

            // =====================================================
            // ORDER DETAILS
            //
            // Everything starts from the RIGHT.
            // Labels and values have the same bold styling.
            // =====================================================

            if (order.ShowOrderNumberOnInvoice)
            {
                AddLine(
                    doc,
                    "رقم الفاتورة",
                    order.OrderId.ToString());
            }

            if (order.ShowCashierName)
            {
                AddLine(
                    doc,
                    "الكاشير",
                    order.CashierName);
            }

            AddLine(
                doc,
                "التاريخ والوقت",
                order.OrderDateFormatted);

            AddLine(
                doc,
                "نوع الطلب",
                order.OrderTypeNameAr);

            if (order.ShowCustomerNameOnInvoice &&
                !string.IsNullOrWhiteSpace(
                    order.CustomerName))
            {
                AddLine(
                    doc,
                    "اسم الزبون",
                    order.CustomerName);
            }

            AddLine(
                doc,
                "طريقة الدفع",
                order.PaymentMethodNameAr);

            // =====================================================
            // IMPORTANT SEPARATOR
            //
            // This is the line between:
            // Order details
            // and
            // Items
            // =====================================================

            doc.Blocks.Add(
                Divider());

            // =====================================================
            // ITEMS TITLE
            // =====================================================

            doc.Blocks.Add(
                Centered(
                    "تفاصيل الطلب",
                    13,
                    FontWeights.Bold,
                    Brushes.Black,
                    6));

            // =====================================================
            // ITEMS TABLE
            // =====================================================

            var itemsTable =
                CreateItemsTable(order);

            doc.Blocks.Add(
                itemsTable);

            // =====================================================
            // DELIVERY FEE
            // =====================================================

            if (order.HasDeliveryFee)
            {
                doc.Blocks.Add(
                    Divider());

                AddAmountLine(
                    doc,
                    "رسوم التوصيل",
                    order.DeliveryFee,
                    order.CurrencySymbol);
            }

            // =====================================================
            // TOTAL
            // =====================================================

            doc.Blocks.Add(
                Divider());

            AddTotalLine(
                doc,
                "الإجمالي",
                order.TotalAmount,
                order.CurrencySymbol);

            // =====================================================
            // NOTES
            // =====================================================

            if (!string.IsNullOrWhiteSpace(
                    order.Notes))
            {
                doc.Blocks.Add(
                    Divider());

                doc.Blocks.Add(
                    RightAlignedText(
                        "ملاحظات",
                        11,
                        FontWeights.Bold,
                        Brushes.Black,
                        2));

                doc.Blocks.Add(
                    RightAlignedText(
                        order.Notes,
                        10,
                        FontWeights.Normal,
                        Brushes.Black,
                        6));
            }

            // =====================================================
            // FOOTER
            // =====================================================

            if (!string.IsNullOrWhiteSpace(
                    order.FooterPrimary) ||
                !string.IsNullOrWhiteSpace(
                    order.FooterSecondary))
            {
                doc.Blocks.Add(
                    Divider());

                if (!string.IsNullOrWhiteSpace(
                        order.FooterPrimary))
                {
                    doc.Blocks.Add(
                        Centered(
                            order.FooterPrimary,
                            12,
                            FontWeights.Bold,
                            Brushes.Black,
                            3));
                }

                if (!string.IsNullOrWhiteSpace(
                        order.FooterSecondary))
                {
                    doc.Blocks.Add(
                        Centered(
                            order.FooterSecondary,
                            9,
                            FontWeights.Normal,
                            Brushes.Gray,
                            3));
                }
            }

            return doc;
        }

        // =========================================================
        // ORDER DETAIL LINE
        //
        // Example:
        //
        // رقم الفاتورة: 105
        // الكاشير: أحمد
        // نوع الطلب: سفري
        //
        // All aligned to the RIGHT.
        // =========================================================

        private static void AddLine(
            FlowDocument doc,
            string label,
            string value)
        {
            var paragraph =
                new Paragraph
                {
                    FlowDirection =
                        FlowDirection.RightToLeft,

                    TextAlignment =
                        TextAlignment.Right,

                    FontSize = 11,

                    Margin =
                        new Thickness(
                            0,
                            0,
                            0,
                            4)
                };

            paragraph.Inlines.Add(
                new Run(
                    label + ": ")
                {
                    FontWeight =
                        FontWeights.Bold,

                    Foreground =
                        Brushes.Black
                });

            paragraph.Inlines.Add(
                new Run(
                    value ?? string.Empty)
                {
                    FontWeight =
                        FontWeights.Bold,

                    Foreground =
                        Brushes.Black
                });

            doc.Blocks.Add(
                paragraph);
        }

        // =========================================================
        // ITEMS TABLE
        //
        // RTL layout:
        //
        // الصنف | الكمية | السعر | الإجمالي
        //
        // =========================================================

        private static Table CreateItemsTable(
            OrderInvoiceDto order)
        {
            var table =
                new Table
                {
                    FlowDirection =
                        FlowDirection.RightToLeft,

                    CellSpacing = 0,

                    TextAlignment =
                        TextAlignment.Right
                };

            // -----------------------------------------------------
            // COLUMN WIDTHS
            // -----------------------------------------------------

            table.Columns.Add(
                new TableColumn
                {
                    Width =
                        new GridLength(
                            1.5,
                            GridUnitType.Star)
                });

            table.Columns.Add(
                new TableColumn
                {
                    Width =
                        new GridLength(
                            0.8,
                            GridUnitType.Star)
                });

            table.Columns.Add(
                new TableColumn
                {
                    Width =
                        new GridLength(
                            0.8,
                            GridUnitType.Star)
                });

            table.Columns.Add(
                new TableColumn
                {
                    Width =
                        new GridLength(
                            1.2,
                            GridUnitType.Star)
                });

            var group =
                new TableRowGroup();

            table.RowGroups.Add(
                group);

            // =====================================================
            // HEADER
            // =====================================================

            var header =
                new TableRow
                {
                    Background =
                        Brushes.LightGray
                };

            header.Cells.Add(
                CreateCell(
                    "الصنف",
                    true,
                    TextAlignment.Right));

            header.Cells.Add(
                CreateCell(
                    "الكمية",
                    true,
                    TextAlignment.Center));

            header.Cells.Add(
                CreateCell(
                    "السعر",
                    true,
                    TextAlignment.Center));

            header.Cells.Add(
                CreateCell(
                    "الإجمالي",
                    true,
                    TextAlignment.Left));

            group.Rows.Add(
                header);

            // =====================================================
            // ITEMS
            // =====================================================

            foreach (var item in order.Items)
            {
                var row =
                    new TableRow();

                row.Cells.Add(
                    CreateCell(
                        item.ItemName,
                        false,
                        TextAlignment.Right));

                row.Cells.Add(
                    CreateCell(
                        item.Quantity.ToString(),
                        false,
                        TextAlignment.Center));

                row.Cells.Add(
                    CreateCell(
                        item.UnitPrice.ToString(
                            "#,##0"),
                        false,
                        TextAlignment.Center));

                row.Cells.Add(
                    CreateCell(
                        item.LineTotal.ToString(
                            "#,##0"),
                        false,
                        TextAlignment.Left));

                group.Rows.Add(
                    row);
            }

            return table;
        }

        // =========================================================
        // TABLE CELL
        // =========================================================

        private static TableCell CreateCell(
            string text,
            bool header,
            TextAlignment alignment)
        {
            var paragraph =
                new Paragraph(
                    new Run(text))
                {
                    FlowDirection =
                        FlowDirection.RightToLeft,

                    TextAlignment =
                        alignment,

                    FontSize =
                        header
                            ? 9.5
                            : 10.5,

                    FontWeight =
                        header
                            ? FontWeights.Bold
                            : FontWeights.Normal,

                    Margin =
                        new Thickness(
                            2,
                            3,
                            2,
                            3)
                };

            return new TableCell(
                paragraph)
            {
                FlowDirection =
                    FlowDirection.RightToLeft,

                TextAlignment =
                    alignment,

                BorderBrush =
                    Brushes.LightGray,

                BorderThickness =
                    new Thickness(
                        0,
                        0,
                        0,
                        0.5)
            };
        }

        // =========================================================
        // AMOUNT LINE
        // =========================================================

        private static void AddAmountLine(
            FlowDocument doc,
            string label,
            decimal amount,
            string currency)
        {
            var paragraph =
                new Paragraph
                {
                    FlowDirection =
                        FlowDirection.RightToLeft,

                    TextAlignment =
                        TextAlignment.Right,

                    FontSize = 11,

                    FontWeight =
                        FontWeights.Bold,

                    Margin =
                        new Thickness(
                            0,
                            0,
                            0,
                            5)
                };

            paragraph.Inlines.Add(
                new Run(
                    label + ": ")
                {
                    FontWeight =
                        FontWeights.Bold,

                    Foreground =
                        Brushes.Black
                });

            paragraph.Inlines.Add(
                new Run(
                    $"{amount:#,##0} {currency}")
                {
                    FontWeight =
                        FontWeights.Bold,

                    Foreground =
                        Brushes.Black
                });

            doc.Blocks.Add(
                paragraph);
        }

        // =========================================================
        // TOTAL
        // =========================================================

        private static void AddTotalLine(
            FlowDocument doc,
            string label,
            decimal amount,
            string currency)
        {
            var paragraph =
                new Paragraph
                {
                    FlowDirection =
                        FlowDirection.RightToLeft,

                    TextAlignment =
                        TextAlignment.Right,

                    FontSize = 16,

                    FontWeight =
                        FontWeights.Bold,

                    Margin =
                        new Thickness(
                            0,
                            4,
                            0,
                            8)
                };

            paragraph.Inlines.Add(
                new Run(
                    label + ": ")
                {
                    FontWeight =
                        FontWeights.Bold
                });

            paragraph.Inlines.Add(
                new Run(
                    $"{amount:#,##0} {currency}")
                {
                    FontWeight =
                        FontWeights.Bold
                });

            doc.Blocks.Add(
                paragraph);
        }

        // =========================================================
        // RIGHT-ALIGNED TEXT
        // =========================================================

        private static Paragraph RightAlignedText(
            string text,
            double fontSize,
            FontWeight fontWeight,
            Brush foreground,
            double bottomMargin = 3)
        {
            return new Paragraph(
                new Run(text))
            {
                FlowDirection =
                    FlowDirection.RightToLeft,

                TextAlignment =
                    TextAlignment.Right,

                FontSize =
                    fontSize,

                FontWeight =
                    fontWeight,

                Foreground =
                    foreground,

                Margin =
                    new Thickness(
                        0,
                        0,
                        0,
                        bottomMargin)
            };
        }

        // =========================================================
        // CENTERED TEXT
        // =========================================================

        private static Paragraph Centered(
            string text,
            double size,
            FontWeight weight,
            Brush? foreground = null,
            double bottomMargin = 2)
        {
            return new Paragraph(
                new Run(text))
            {
                FlowDirection =
                    FlowDirection.RightToLeft,

                TextAlignment =
                    TextAlignment.Center,

                FontSize =
                    size,

                FontWeight =
                    weight,

                Foreground =
                    foreground ??
                    Brushes.Black,

                Margin =
                    new Thickness(
                        0,
                        0,
                        0,
                        bottomMargin)
            };
        }

        // =========================================================
        // SEPARATOR
        // =========================================================

        private static Paragraph Divider()
        {
            return new Paragraph
            {
                FlowDirection =
                    FlowDirection.RightToLeft,

                BorderBrush =
                    Brushes.Gray,

                BorderThickness =
                    new Thickness(
                        0,
                        0,
                        0,
                        1),

                Margin =
                    new Thickness(
                        0,
                        7,
                        0,
                        7),

                Padding =
                    new Thickness(0)
            };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
 