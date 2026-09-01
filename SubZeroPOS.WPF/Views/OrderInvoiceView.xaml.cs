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

        private void OnDataContextChanged(object sender,DependencyPropertyChangedEventArgs e)
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
            double measuredHeight =  MeasureDocumentHeight( document, document.PageWidth);

            document.PageHeight = measuredHeight + 16;

            // Print.
            IDocumentPaginatorSource paginatorSource = document;

            printDialog.PrintDocument(paginatorSource.DocumentPaginator, "فاتورة ساب زيرو");

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

            int pixelWidth = (int)(width * dpiScale);

            int pixelHeight = (int)(height * dpiScale);

            var renderBitmap = new RenderTargetBitmap(pixelWidth, pixelHeight, 96 * dpiScale, 96 * dpiScale, PixelFormats.Pbgra32);

            renderBitmap.Render(ReceiptBorder);

            // Convert WPF visual to PNG.
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

            // WPF uses DIP (96 DPI).
            // PDF uses points (72 DPI).
            page.Width = XUnit.FromPoint(width * 0.75);

            page.Height = XUnit.FromPoint(height * 0.75);

            using var gfx = XGraphics.FromPdfPage(page);

            using var imageStream = new MemoryStream(pngBytes);

            var ximage = XImage.FromStream(imageStream);

            gfx.DrawImage(ximage, 0, 0, page.Width.Point, page.Height.Point);

            string tempPath = Path.Combine(Path.GetTempPath(), $"SubZero_Invoice_{order.OrderId}.pdf");

            if (File.Exists(tempPath)) File.Delete(tempPath);

            pdfDocument.Save(tempPath);

            // Open PDF.
            string[] possibleEdgePaths =
            {
                @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
                @"C:\Program Files\Microsoft\Edge\Application\msedge.exe"
            };

            string? edgePath = Array.Find(possibleEdgePaths, File.Exists);

            if (edgePath != null)
            {
                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo(edgePath, $"\"{tempPath}\"")
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

        // =========================================================
        // MEASURE DOCUMENT
        // =========================================================

        private static double MeasureDocumentHeight(FlowDocument doc,double width)
        {
            var host = new FlowDocumentScrollViewer
            {
                Document = doc,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled
            };

            host.Measure(new Size(width, double.PositiveInfinity));

            double height = host.DesiredSize.Height;

            // Release the document.
            host.Document = null;

            return height;
        }

        // =========================================================
        // BUILD RECEIPT
        // =========================================================

        private FlowDocument BuildReceiptDocument(OrderInvoiceDto order)
        {
            // ==========================================
            // Calculate receipt width
            // ==========================================

            double pageWidth = order.ReceiptPaperWidthMm == 58 ? 220 : 320;


            var doc = new FlowDocument
            {
                FlowDirection = FlowDirection.RightToLeft,
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 12,
                PagePadding = new Thickness(12),
                PageWidth = pageWidth,
                ColumnWidth =  pageWidth
            };

            // ==========================================
            // Restaurant logo
            // ==========================================

            if (order.ShowLogoOnInvoice)
            {
                try
                {
                    var logoUri = new Uri("pack://siteoforigin:,,,/Images/Branding/logo.png",UriKind.Absolute);

                    var logoBitmap = new BitmapImage(logoUri);

                    var logoImage = new Image { Source = logoBitmap, Width = 140, Stretch = Stretch.Uniform };



                    var logoContainer = new BlockUIContainer(logoImage) { TextAlignment = TextAlignment.Center, Margin = new Thickness(0, 0, 0, 8) };


                    doc.Blocks.Add(logoContainer);
                }
                catch
                {
                    doc.Blocks.Add(Centered(order.RestaurantName, 17, FontWeights.Bold));
                }
            }
            else
            {
                doc.Blocks.Add(Centered( order.RestaurantName,17,FontWeights.Bold));
            }


            // ==========================================
            // Restaurant information
            // ==========================================

            if (!string.IsNullOrWhiteSpace(order.RestaurantAddress))
            {
                doc.Blocks.Add(Centered(order.RestaurantAddress,10,FontWeights.Normal,Brushes.Gray));
            }
            if (!string.IsNullOrWhiteSpace(order.RestaurantPhone))
            {
                doc.Blocks.Add(  Centered(order.RestaurantPhone,10,FontWeights.Normal,Brushes.Gray,bottomMargin: 8));
            }


            // ==========================================
            // Separator
            // ==========================================

            doc.Blocks.Add(Divider());


            // ==========================================
            // Invoice title
            // ==========================================

          //  doc.Blocks.Add(Centered("فاتورة بيع", 15,FontWeights.Bold,Brushes.Black,bottomMargin: 6));


            // ==========================================
            // ORDER DETAILS
            // ==========================================

            var detailsTable = CreateDetailsTable();


            // Order number
            if (order.ShowOrderNumberOnInvoice)
            {
                AddDetailRow(detailsTable, "رقم الفاتورة", order.OrderId.ToString(),true);
            }


            // Date
            AddDetailRow(detailsTable, "تاريخ العمليه",order.OrderDateFormatted,true);


            // Customer name
            if (order.ShowCustomerNameOnInvoice && !string.IsNullOrWhiteSpace(order.CustomerName))
            {
                AddDetailRow( detailsTable,  "اسم العميل", order.CustomerName,true);
            }

            // Cashier
            if (order.ShowCashierName &&!string.IsNullOrWhiteSpace(order.CashierName))
            {
                AddDetailRow(detailsTable, "الكاشير", order.CashierName,true);
            }


            // Order type
            if (!string.IsNullOrWhiteSpace(order.OrderTypeNameAr))
            {
                AddDetailRow( detailsTable,"نوع الطلب",order.OrderTypeNameAr,true);
            }


            // Payment method
            if (!string.IsNullOrWhiteSpace(order.PaymentMethodNameAr))
            {
                AddDetailRow(  detailsTable, "طريقة الدفع", order.PaymentMethodNameAr,true);
            }


            doc.Blocks.Add( detailsTable);


            // ==========================================
            // Separator before items
            // ==========================================

            doc.Blocks.Add( Divider());


            // ==========================================
            // ITEMS HEADER
            // ==========================================

            var headerTable = CreateItemsTable();


            AddItemsHeader(headerTable);


            doc.Blocks.Add( headerTable);


            // ==========================================
            // ITEMS
            // ==========================================

            var itemsTable = CreateItemsTable();


            foreach (var item in order.Items)
            {
                AddItemRow(itemsTable, item.ItemName, item.Quantity, item.UnitPrice, item.LineTotal);
            }
            doc.Blocks.Add(itemsTable);


            // ==========================================
            // Delivery fee
            // ==========================================

            if (order.HasDeliveryFee)
            {
                var deliveryTable =     CreateDetailsTable();

                AddDetailRowForTotalandDelivary(deliveryTable, "رسوم التوصيل", $"{order.DeliveryFee:N0} {order.CurrencySymbol}", true);

                doc.Blocks.Add(deliveryTable);
            }


            // ==========================================
            // Notes
            // ==========================================

            if (!string.IsNullOrWhiteSpace(order.Notes)) 
            {
                doc.Blocks.Add(Divider());


                var notesTitle = new Paragraph
                {
                    TextAlignment = TextAlignment.Left,
                    FontWeight = FontWeights.Bold,
                    FontSize = 11,
                    Margin = new Thickness(0, 0, 0, 2)

                };

                notesTitle.Inlines.Add(new Run("ملاحظات"));

                doc.Blocks.Add(notesTitle);

                var notes = new Paragraph(new Run(order.Notes))
                {
                    TextAlignment = TextAlignment.Left,
                    FontSize = 11,
                    Margin = new Thickness(0, 0, 0, 5)
                };

                doc.Blocks.Add(notes);
            }

            // ==========================================
            // Separator before total
            // ==========================================

            doc.Blocks.Add(Divider());


            // ==========================================
            // TOTAL
            // ==========================================

            var totalTable = CreateDetailsTable();

            AddDetailRowForTotalandDelivary(totalTable, "الإجمالي", $"{order.TotalAmount:N0} {order.CurrencySymbol}", true);

            doc.Blocks.Add(totalTable);


            // ==========================================
            // Footer
            // ==========================================

            if (!string.IsNullOrWhiteSpace(order.FooterPrimary) || !string.IsNullOrWhiteSpace(order.FooterSecondary)) 
            {
                doc.Blocks.Add(Divider());

                if (!string.IsNullOrWhiteSpace(order.FooterPrimary)) 
                {
                    doc.Blocks.Add(Centered(order.FooterPrimary, 12, FontWeights.Bold, Brushes.Black));
                }

                if (!string.IsNullOrWhiteSpace(order.FooterSecondary)) 
                {
                    doc.Blocks.Add(Centered(order.FooterSecondary, 10, FontWeights.Normal, Brushes.Gray));
                }
            }

            return doc;
        }

        private static Table CreateDetailsTable()
        {
            var table =
                new Table
                {
                    FlowDirection = FlowDirection.RightToLeft,
                    CellSpacing = 0,
                    Margin = new Thickness(0,0,0,0)
                };


            // Label column
            table.Columns.Add(
                new TableColumn
                {
                    Width =  new GridLength(1,GridUnitType.Star)
                });


            // Value column
            table.Columns.Add(
                new TableColumn
                {
                    Width = new GridLength(1,GridUnitType.Star)
                });

            table.RowGroups.Add(new TableRowGroup());

            return table;
        }

        private static void AddDetailRowForTotalandDelivary(Table table, string label, string? value, bool bold = false)
        {
            var row = new TableRow();


            // ==========================================
            // Label
            // ==========================================

            var labelParagraph =
                new Paragraph(
                    new Run(label))
                {
                    TextAlignment = TextAlignment.Center,

                    FontSize = bold ? 14 : 11,

                    FontWeight = bold ? FontWeights.Bold : FontWeights.Normal,

                    Margin = new Thickness(0)
                };


            var labelCell = new TableCell(labelParagraph)
            {
                Padding = new Thickness(0, 2, 5, 2),

                TextAlignment = TextAlignment.Right
            };

            // ==========================================
            // Value
            // ==========================================

            var valueParagraph = new Paragraph(new Run(value ?? string.Empty))
            {
                TextAlignment = TextAlignment.Center,

                FontSize = bold ? 14 : 11,

                FontWeight = bold ? FontWeights.Bold : FontWeights.Normal,

                Margin = new Thickness(0)
            };


            var valueCell =
                new TableCell(valueParagraph)
                {
                    Padding =
                        new Thickness(5, 2, 0, 2),
                    TextAlignment = TextAlignment.Left
                };


            row.Cells.Add(labelCell);

            row.Cells.Add(valueCell);


            table.RowGroups[0].Rows.Add(row);
        }
        private static void AddDetailRow(Table table,string label,string? value,bool bold = false)
        {
            var row = new TableRow();


            // ==========================================
            // Label
            // ==========================================

            var labelParagraph =
                new Paragraph(
                    new Run(label))
                {
                    TextAlignment = TextAlignment.Justify,

                    FontSize = bold ? 12 : 11,

                    FontWeight = bold ? FontWeights.Bold : FontWeights.Normal,

                    Margin = new Thickness(0)
                };


            var labelCell = new TableCell(labelParagraph)
            {
                Padding = new Thickness(0, 2, 5, 2),

                TextAlignment = TextAlignment.Right
            };

            // ==========================================
            // Value
            // ==========================================

            var valueParagraph = new Paragraph(new Run(value ?? string.Empty))
            {
                TextAlignment = TextAlignment.Left,

                FontSize = bold ? 12 : 11,

                FontWeight = bold ? FontWeights.Bold : FontWeights.Normal,

                Margin = new Thickness(0)
            };


            var valueCell =
                new TableCell(valueParagraph)
                {
                    Padding =
                        new Thickness(5, 2, 0, 2),
                    TextAlignment = TextAlignment.Left
                };


            row.Cells.Add(labelCell);

            row.Cells.Add(valueCell);


            table.RowGroups[0].Rows.Add(row);
        }

        private static void AddItemsHeader(Table table)
        {
            var row = new TableRow();

            row.Cells.Add(CreateHeaderCell("الصنف", TextAlignment.Justify));

            row.Cells.Add(CreateHeaderCell("الكمية", TextAlignment.Justify));

            row.Cells.Add(CreateHeaderCell("السعر", TextAlignment.Justify));

            row.Cells.Add(CreateHeaderCell("الإجمالي", TextAlignment.Justify));

            table.RowGroups[0].Rows.Add(row);
        }

        private static TableCell CreateHeaderCell(string text,TextAlignment alignment)
        {
            var paragraph = new Paragraph(new Run(text))
            {
                FontWeight = FontWeights.Bold,
                FontSize = 10,
                TextAlignment = alignment,
                Margin = new Thickness(0)
            };

            return new TableCell(paragraph)
            {
                Padding = new Thickness(2, 3, 2, 3),

                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(0, 0, 0, 1)

            };
        }

        private static void AddItemRow(Table table, string itemName,decimal quantity, decimal unitPrice,decimal lineTotal)
        {
            var row = new TableRow();

            // ==========================================
            // Item name
            // ==========================================

            row.Cells.Add(CreateItemCell(itemName, TextAlignment.Justify));

            // ==========================================
            // Quantity
            // ==========================================

            row.Cells.Add(CreateItemCell(quantity.ToString("N0"), TextAlignment.Justify));

            // ==========================================
            // Unit price
            // ==========================================

            row.Cells.Add(CreateItemCell(unitPrice.ToString("N0"), TextAlignment.Justify));

            // ==========================================
            // Line total
            // ==========================================

            row.Cells.Add(CreateItemCell(lineTotal.ToString("N0"), TextAlignment.Justify));

            table.RowGroups[0].Rows.Add(row);
        }

        private static TableCell CreateItemCell(string text,TextAlignment alignment)
        {
            var paragraph = new Paragraph(new Run(text))
            {
                TextAlignment = alignment,
                FontSize = 10,
                Margin = new Thickness(0)
            };

            return new TableCell(paragraph)
            {
                Padding = new Thickness(2, 3, 2, 3)

            };
        }

        private static Table CreateItemsTable()
        {
            var table = new Table
            {
                FlowDirection = FlowDirection.RightToLeft,
                CellSpacing = 0,
                Margin = new Thickness(0, 0, 0, 4)

            };

            // ==========================================
            // Item name
            // ==========================================

            table.Columns.Add(new TableColumn
            {
                Width = new GridLength(2.2, GridUnitType.Star)
            });

            // ==========================================
            // Quantity
            // ==========================================

            table.Columns.Add(new TableColumn
            {
                Width = new GridLength(0.8, GridUnitType.Star)
            });

            // ==========================================
            // Unit price
            // ==========================================

            table.Columns.Add(new TableColumn
            {
                Width = new GridLength(1.1, GridUnitType.Star)
            });

            // ==========================================
            // Total
            // ==========================================

            table.Columns.Add(new TableColumn
            {
                Width = new GridLength(1.1, GridUnitType.Star)
            });

            table.RowGroups.Add(new TableRowGroup());

            return table;
        }

        // =========================================================
        // CENTERED TEXT
        // =========================================================

        private static Paragraph Centered(string text,double size,FontWeight weight,Brush? foreground = null,double bottomMargin = 2)
        {
            return new Paragraph(new Run(text))
            {
                FlowDirection = FlowDirection.RightToLeft,

                TextAlignment =  TextAlignment.Center,

                FontSize = size,

                FontWeight =   weight,

                Foreground = foreground ??Brushes.Black, Margin =  new Thickness(0,0,0,bottomMargin)
            };
        }

        // =========================================================
        // SEPARATOR
        // =========================================================

        private static Paragraph Divider()
        {
            return new Paragraph
            {
                FlowDirection = FlowDirection.RightToLeft,

                BorderBrush = Brushes.Gray,

                BorderThickness =new Thickness(0,0,0, 1),
      
                Margin =  new Thickness(0,7,0,7),
          
                Padding = new Thickness(0)
            };
        }
    }
}
 