using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using GMD;
using GMD.Models;
using GMD.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
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
            await App.DictsManager.AddDictAsync();
            DictionaryPageProgressRing.IsActive = false;
        }

        private async void RemoveDictAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            DictionaryPageProgressRing.IsActive = true;
            await App.DictsManager.RemoveDictAsync(
                (DictionaryListView.ItemsSource as ObservableCollection<Dict>)
                .ElementAtOrDefault(DictionaryListView.SelectedIndex)
                .DictID);
            DictionaryPageProgressRing.IsActive = false;

            // get DictionaryListView ItemSource, get the selectedItem through selectedIndex
            // get the dictID from selectedItem
            // RemoveDictAsync(DictId)
        }

        private void DictionaryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DictionaryListView.SelectedIndex != -1)
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
    }
}
