using System;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;

namespace GMD.ViewModels
{
    public class Settings
    {
        private static ApplicationDataContainer _settings = ApplicationData.Current.RoamingSettings;

        private static string keyTheme = "requestedTheme";
        private static string keyDefinitionFontSize = "definitionFontSize";        

        private static int defaultThemeSetting = 2;
        private static int defaultDefinitionFontSize = 16;

        public enum ThemeSettings
        {
            Dark,
            Light,
            SystemSetting
        }

        private static Object getSetting(string key)
        {
            return _settings.Values[key];
        }

        public static void SetRequestedTheme(ThemeSettings selectedSetting) =>
            _settings.Values[keyTheme] = (int)selectedSetting;

        public static int GetRequestedThemeSetting()
        {
            try
            {
                return (int)getSetting(keyTheme);
            }
            catch (NullReferenceException)
            {
                return defaultThemeSetting;
            }
        }

        public static ApplicationTheme GetRequestedTheme()
        {
            switch (GetRequestedThemeSetting())
            {
                case 0:
                    return ApplicationTheme.Light;
                case 1:
                    return ApplicationTheme.Dark;
                case 3:
                default:
                    return Application.Current.RequestedTheme;
            }
        }

        public static void SetDefinitionFontSize(int definitionFontSize) =>
            _settings.Values[keyDefinitionFontSize] = definitionFontSize;

        public static int GetDefinitionFontSize()
        {
            try
            {
                return (int)getSetting(keyDefinitionFontSize);
            }
            catch (NullReferenceException)
            {
                return defaultDefinitionFontSize;
            }
        }
        
    }
}
