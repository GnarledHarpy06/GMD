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
            RecentEntriesListView.ItemsSource = App.EntriesManager.ViewedEntries;
            QueryListView.ItemsSource = App.EntriesManager.CollectionMatchedOfKeywordsByBookName;
            DetailFrame.Navigate(typeof(DisplayPage));
        }

        private void QueryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(QueryTextBox.Text == "")
            {
                VisualStateManager.GoToState(this, RecentEntriesState.Name, true);
            }
            else
            {
                App.EntriesManager.QueryKeywords(QueryTextBox.Text);
                VisualStateManager.GoToState(this, QueryState.Name, true);
            }
        }
        
        private void SearchPage_Loaded(object sender, RoutedEventArgs e)
        {   
            Loaded -= SearchPage_Loaded;
            App.EntriesManager.ConstructAsync();
            // tfw best practice :p
        }
                
        private void AdaptiveStates_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {            
        }        

        private void QueryListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            displayEntry(App.EntriesManager.GetEntry((WordStrByBookName)e.ClickedItem));
        }

        private void RecentEntriesListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            displayEntry(((RecentEntry)e.ClickedItem).ToEntry());
        }

        private void displayEntry(Entry clickedEntry)
        {
            App.CurrentEntry.UpdateEntry(clickedEntry);

            if (AdaptiveStates.CurrentState == NarrowState)
            {
                VisualStateManager.GoToState(this, DetailState.Name, true);

                Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += (s, f) =>
                {
                    f.Handled = true;
                    VisualStateManager.GoToState(this, MasterState.Name, true);
                };
            }

            App.EntriesManager.AddRecentEntry(clickedEntry);
        }

        //public delegate void EntryUpdateHandler(object sender, SearchPageEventArgs e);
        //public event EntryUpdateHandler OnCurrentEntryChanged;

        //void RaiseOnCurrentEntryChanged(Entry selectedEntry)
        //{
        //    OnCurrentEntryChanged?.Invoke(this, new SearchPageEventArgs(selectedEntry));
        //}
    }

    //public class SearchPageEventArgs : EventArgs
    //{
    //    public Entry SelectedEntry { get; private set; }

    //    public SearchPageEventArgs(Entry newEntry)
    //    {
    //        SelectedEntry = newEntry;
    //    }        
    //}
}
