using CommunityToolkit.Mvvm.ComponentModel;

namespace SubZeroPOS.WPF.ViewModels
{
    /// <summary>
    /// UI-facing cart row. CartItemDto (in Core) is a plain data class used to
    /// send the order to the service layer - it doesn't implement
    /// INotifyPropertyChanged, so WPF can't detect in-place changes like
    /// "user tapped the same item again, bump Quantity from 1 to 2".
    /// This wrapper lives only in WPF and IS observable, so the cart list
    /// and line totals refresh live. It gets converted to CartItemDto only
    /// when the order is actually submitted.
    /// </summary>
    public partial class CartLineItem : ObservableObject
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(LineTotal))]
        private int quantity;

        public decimal LineTotal => UnitPrice * Quantity;
    }
}
