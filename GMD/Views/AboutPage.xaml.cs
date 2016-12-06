using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GMD.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AboutPage : Page
    {
        public AboutPage()
        {
            this.InitializeComponent();
        }

        private async void SurveyButton_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(@"http://www.giovanand.hol.es/gmd-the-survey");
            await Windows.System.Launcher.LaunchUriAsync(uri);
        }
    }
}
