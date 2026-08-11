using System;

namespace SubZeroPOS.WPF.ViewModels
{
    public class DateFormatOption
    {
        public string FormatString { get; set; } = string.Empty;
        public string PreviewText => DateTime.Now.ToString(FormatString);
        public string DisplayName => $"{PreviewText}";
    }
}
