namespace SubZeroPOS.WPF.ViewModels
{
    /// <summary>
    /// Only two payment methods for now (Cash / Bankak), so a lightweight
    /// fixed list here is simpler than a full DB lookup table.
    /// </summary>
    public class PaymentMethodOption
    {
        public string Code { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
    }
}
