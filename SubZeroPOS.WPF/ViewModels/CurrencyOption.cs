namespace SubZeroPOS.WPF.ViewModels
{
    public class CurrencyOption
    {
        public string Code { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public string DisplayName => $"{Symbol} ({Code})";
    }
}
