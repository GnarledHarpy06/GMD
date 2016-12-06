using GMD.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
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
        }

        private async void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.SetRequestedTheme((Settings.ThemeSettings) ThemeComboBox.SelectedIndex);
            var dialog = new MessageDialog("The new theme will only be applied next time you launch the app. ", "Remember");
            
            await dialog.ShowAsync();            
        }

        private void DefinitionFontSizeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) =>
            Settings.SetDefinitionFontSize((int)Math.Round(e.NewValue));

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            DefinitionFontSizeSlider.Value = Settings.GetDefinitionFontSize();
            ThemeComboBox.SelectedIndex = Settings.GetRequestedThemeSetting();
            this.DefinitionFontSizeSlider.ValueChanged += (s, f) => DefinitionFontSizeSlider_ValueChanged(s, f);
            this.ThemeComboBox.SelectionChanged += (s, f) => ThemeComboBox_SelectionChanged(s, f);
        }        
    }
}
