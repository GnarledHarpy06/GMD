using GMD.Models;
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
    public sealed partial class FavoritesPage : Page
    {
        public FavoritesPage()
        {
            this.InitializeComponent();
            FavouriteEntriesListView.ItemsSource = App.EntriesManager.FavouriteEntries;
            DetailFrame.Navigate(typeof(DisplayPage_copy));
        }

        private void FavouriteEntriesListView_ItemClick(object sender, ItemClickEventArgs e) =>
            displayEntry(((FavouriteEntry)e.ClickedItem).ToEntry());        

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
    }
}
