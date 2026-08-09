using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
            // Force a full layout pass first - guards against ActualWidth/Height
            // being stale or zero if this is called right after navigation.
            ReceiptBorder.UpdateLayout();

            double width = ReceiptBorder.ActualWidth;
            double height = ReceiptBorder.ActualHeight;

            if (width <= 0 || height <= 0)
            {
                MessageBox.Show("تعذر تحضير الفاتورة للطباعة. حاول مرة أخرى.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() != true) return;

            double dpiScale = 2.0;
            int pixelWidth = (int)(width * dpiScale);
            int pixelHeight = (int)(height * dpiScale);

            var renderBitmap = new RenderTargetBitmap(
                pixelWidth, pixelHeight, 96 * dpiScale, 96 * dpiScale, PixelFormats.Pbgra32);
            renderBitmap.Render(ReceiptBorder);

            var drawingVisual = new DrawingVisual();
            using (DrawingContext dc = drawingVisual.RenderOpen())
            {
                // Some PDF/virtual print drivers mishandle transparent (alpha)
                // pixels from RenderTargetBitmap and render them as blank white.
                // Painting an explicit opaque white rectangle first, then the
                // receipt image on top, guarantees the page is never fully
                // transparent regardless of how the driver composites it.
                dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, width, height));
                dc.DrawImage(renderBitmap, new Rect(0, 0, width, height));
            }

            printDialog.PrintVisual(drawingVisual, "فاتورة سوب زيرو");
        }
    }
}
