using GMD.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GMD.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            this.DefinitionFontSizeSlider.ValueChanged += (s, e) => DefinitionFontSizeSlider_ValueChanged(s, e);
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            Settings.SetRequestedTheme((Settings.ThemeSettings) ThemeComboBox.SelectedIndex);

        private void DefinitionFontSizeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) =>
            Settings.SetDefinitionFontSize((int)Math.Round(e.NewValue));

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            DefinitionFontSizeSlider.Value = Settings.GetDefinitionFontSize();
            ThemeComboBox.SelectedIndex = Settings.GetRequestedThemeSetting();            
        }        
    }
}
