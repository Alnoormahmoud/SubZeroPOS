using System;
using System.Windows;

namespace SubZeroPOS.WPF
{
    public static class ThemeManager
    {
        public const string SubZeroDark = "SubZero Dark";
        public const string Light = "Light";
        public const string BlueDark = "Blue Dark";

        public static void ApplyTheme(string theme)
        {
            string dictionaryPath = theme switch
            {
                Light =>
                    "/SubZeroPOS.WPF;component/Resources/Themes/LightTheme.xaml",

                BlueDark =>
                    "/SubZeroPOS.WPF;component/Resources/Themes/BlueDarkTheme.xaml",

                _ =>
                    "/SubZeroPOS.WPF;component/Resources/Themes/SubZeroDarkTheme.xaml"
            };

            var newDictionary = new ResourceDictionary
            {
                Source = new Uri(dictionaryPath, UriKind.Relative)
            };

            var dictionaries = Application.Current.Resources.MergedDictionaries;

            // Find the currently loaded theme
            int themeIndex = -1;

            for (int i = 0; i < dictionaries.Count; i++)
            {
                var source = dictionaries[i].Source;

                if (source != null &&
                    source.OriginalString.Contains("/Resources/Themes/"))
                {
                    themeIndex = i;
                    break;
                }
            }

            if (themeIndex >= 0)
            {
                // Replace the existing theme at the same position
                dictionaries[themeIndex] = newDictionary;
            }
            else
            {
                // No theme exists yet
                dictionaries.Add(newDictionary);
            }
        }
    }
}