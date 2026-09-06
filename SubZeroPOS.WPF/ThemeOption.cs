public class ThemeOption
{
    public string Value { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public override string ToString()
    {
        return DisplayName;
    }
}