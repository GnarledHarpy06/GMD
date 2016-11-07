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
        public DictionariesPage()
        {
            this.InitializeComponent();
            DictionaryListView.ItemsSource = App.DictsManager.Dicts;
        }        

        private void AddDictAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            App.DictsManager.AddDictAsync();
        }

        private void RemoveDictAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            App.DictsManager.RemoveDictAsync(
                (DictionaryListView.ItemsSource as ObservableCollection<Dict>)
                .ElementAtOrDefault(DictionaryListView.SelectedIndex)
                .DictID);

            // get DictionaryListView ItemSource, get the selectedItem through selectedIndex
            // get the dictID from selectedItem
            // RemoveDictAsync(DictId)
        }

        private void DictionaryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!DictionaryListView.SelectedIndex.Equals(null))
                RemoveDictAppBarButton.IsEnabled = true;
            else
                RemoveDictAppBarButton.IsEnabled = false;
        }
    }
}
