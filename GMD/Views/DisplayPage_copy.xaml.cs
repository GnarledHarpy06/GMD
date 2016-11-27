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
    public sealed partial class DisplayPage_copy : Page
    {        
        private DisplayEntry DisplayedEntry = App.CurrentEntry2;

        public DisplayPage_copy()
        {
            this.InitializeComponent();            
            FollowUpRebind();
            DisplayedEntry.PropertyChanged += (s, e) => this.FollowUpRebind();
        }

        /* WARNING
         * NEVER EVER COPY CODES
         * Even though the resource cost isn't that expensive
         * Even though the it is bug free
         * 
         * UNLESS YOU ARE ON A THIGHT DEADLINE
         */

        void FollowUpRebind()
        {
            EntryDefinitionRichTextBlock.Blocks.Clear();

            foreach (var item in DisplayedEntry.Definition)
                EntryDefinitionRichTextBlock.Blocks.Add(item);

            if (App.EntriesManager.FavouriteEntries
                .Where(e => (e.WordStr == DisplayedEntry.WordStr)
                && (e.DictId == DisplayedEntry.DictId))
                .Count() == 0)
                FavouriteAppBarToggleButton.IsChecked = false;
            else
                FavouriteAppBarToggleButton.IsChecked = true;

            Bindings.Update();
            // tfw best practice :p
        }

        private void FavouriteAppBarToggleButton_Checked(object sender, RoutedEventArgs e) =>
                    App.EntriesManager.AddFavouriteEntry(DisplayedEntry.ToEntry());

        private void FavouriteAppBarToggleButton_Unchecked(object sender, RoutedEventArgs e) =>
            App.EntriesManager.RemoveFavouriteEntry(DisplayedEntry.ToEntry());
    }
}
