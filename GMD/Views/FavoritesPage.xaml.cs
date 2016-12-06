using GMD.Models;
using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GMD.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FavoritesPage : Page
    {
        public FavoritesPage()
        {
            this.InitializeComponent();
            FavouriteEntriesListView.ItemsSource = App.EntriesManager.FavouriteEntries;
            EntriesCheck();
            App.EntriesManager.FavouriteEntries.CollectionChanged += (s, e) => EntriesCheck();
            DetailFrame.Navigate(typeof(DisplayPage_copy));
        }

        private async void FavouriteEntriesListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var tmp = ((FavouriteEntry)e.ClickedItem).ToEntry();
            if (App.DictsManager.Dicts.Where(d => d.DictID == tmp.DictId).Count() == 1)
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
            App.CurrentEntry2.UpdateEntry(clickedEntry);

            if (AdaptiveStates.CurrentState == NarrowState)
            {
                VisualStateManager.GoToState(this, DetailState.Name, true);

                Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += (s, f) =>
                {
                    f.Handled = true;
                    VisualStateManager.GoToState(this, MasterState.Name, true);
                };
            }
        }

        void EntriesCheck()
        {
            if (App.EntriesManager.FavouriteEntries.Count() < 1)
            {
                NoEntriesWarning.Visibility = Visibility.Visible;
            }
            else
                NoEntriesWarning.Visibility = Visibility.Collapsed;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (AdaptiveStates.CurrentState == NarrowState)
                VisualStateManager.GoToState(this, MasterState.Name, true);
        }
    }
}
