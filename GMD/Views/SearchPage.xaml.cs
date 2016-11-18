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
using GMD.ViewModels;
using GMD.Models;
using System.Threading.Tasks;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GMD.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchPage : Page
    {
        public SearchPage()
        {
            this.InitializeComponent();
            QueryListView.ItemsSource = App.EntriesManager.CollectionMatchedOfKeywordsByBookName;
        }

        private void QueryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            App.EntriesManager.QueryKeywords(QueryTextBox.Text);
        }
        
        private async void SearchPage_Loaded(object sender, RoutedEventArgs e)
        {   
            Loaded -= SearchPage_Loaded; // tfw best practice :p
            await App.EntriesManager.ConstructAsync();
        }

        private async void QueryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(QueryListView.SelectedItem != null)
              App.CurrentEntry = new DisplayEntry( await App.EntriesManager.GetEntryAsync((WordStrByBookName)QueryListView.SelectedItem));
                // tfw best practice :p            
        }
    }
}
