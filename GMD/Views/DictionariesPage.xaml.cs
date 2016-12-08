using System;
using System.Linq;
using GMD.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GMD.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DictionariesPage : Page
    {
        ObservableCollection<Dict> localDicts = new ObservableCollection<Dict>();

        public DictionariesPage()
        {
            this.InitializeComponent();
            UpdateDictionary();
            App.DictsManager.DictDatabaseChanged += (s, e) => UpdateDictionary();            
        }

        private void UpdateDictionary()
        {
            localDicts.Clear();
            foreach (Dict newDict in App.DictsManager.Dicts)
                localDicts.Add(newDict);
            DictsCheck();
        }

        private async void AddDictAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            DictionaryPageProgressRing.IsActive = true;
            try
            {
                await App.DictsManager.AddDictAsync();
            }
            catch (Exception f)
            {
                System.Diagnostics.Debug.WriteLine(f.Message);
                var dialog = new Windows.UI.Popups.MessageDialog("We couldn't load the dictionary", "Sorry");
                await dialog.ShowAsync();
            }
            DictionaryPageProgressRing.IsActive = false;
        }

        private async void RemoveDictAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            DictionaryPageProgressRing.IsActive = true;
            await App.DictsManager.RemoveDictAsync(
                ((Dict)DictionaryListView.SelectedItem)
                .DictID);
            DictionaryPageProgressRing.IsActive = false;

            // get DictionaryListView ItemSource
            // get the dict by casting the SelectedItem
            // get the dictID from selectedItem
            // RemoveDictAsync(DictId)
        }

        private void DictionaryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DictionaryListView.SelectedItem != null)
                RemoveDictAppBarButton.IsEnabled = true;
            else
                RemoveDictAppBarButton.IsEnabled = false;
        }        

        private void AdaptiveStates_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            
        }

        void DictsCheck()
        {
            if (App.DictsManager.Dicts.Where(d => d.IsQueried).Count() < 1)
                NoDictWarning.Visibility = Visibility.Visible;
            else
                NoDictWarning.Visibility = Visibility.Collapsed;
        }

        private async void DownloadAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(@"http://www.giovanand.hol.es/gmd-library");
            await Windows.System.Launcher.LaunchUriAsync(uri);
        }
    }
}
