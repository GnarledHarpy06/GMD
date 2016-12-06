using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using GMD.Models;

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
            VisualStateManager.GoToState(this, QueryState.Name, true);
            QueryTextBox.Focus(FocusState.Programmatic);
            DictsCheck();
            App.DictsManager.DictDatabaseChanged += (s, f) => DictsCheck();
        }

        private void QueryListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            displayEntry(App.EntriesManager.GetEntry((WordStrByBookName)e.ClickedItem));            
        }

        private async void RecentEntriesListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var tmp = ((RecentEntry)e.ClickedItem).ToEntry();
            if(App.DictsManager.Dicts.Where(d => d.DictID == tmp.DictId).Count() == 1)
                displayEntry(tmp);
            else
            {
                var dialog = new Windows.UI.Popups.MessageDialog("The dictionary for this entry isn't accessible.", "Sorry");
                dialog.Commands.Add(new Windows.UI.Popups.UICommand("Ok") { });
                await dialog.ShowAsync();
            }            
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

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (AdaptiveStates.CurrentState == NarrowState)
                VisualStateManager.GoToState(this, MasterState.Name, true);
            VisualStateManager.GoToState(this, RecentEntriesState.Name, true);
            QueryTextBox.Text = "";
        }

        void DictsCheck()
        {
            if ((App.DictsManager.Dicts.Where(d => d.IsQueried).Count() < 1) && (QueryStates.CurrentState == QueryState))
            {
                NoDictWarning.Visibility = Visibility.Visible;                
            }
            else
                NoDictWarning.Visibility = Visibility.Collapsed;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {            
        }

        private void AdaptiveStates_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e) =>
            this.Frame.Navigate(typeof(DictionariesPage));        
    }    
}
