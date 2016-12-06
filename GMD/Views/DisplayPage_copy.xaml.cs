using GMD.Models;
using GMD.ViewModels;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GMD.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DisplayPage_copy : Page
    {
        /* WARNING
         * NEVER EVER COPY CODES
         * Even though the resource cost isn't that expensive
         * Even though the it is bug free
         * 
         * UNLESS YOU ARE ON A THIGHT DEADLINE
         */

        private DisplayEntry DisplayedEntry = App.CurrentEntry2;

        public DisplayPage_copy()
        {
            this.InitializeComponent();            
            FollowUpRebind();
            DisplayedEntry.PropertyChanged += (s, e) => this.FollowUpRebind();
        }       

        void FollowUpRebind()
        {
            EntryDefinitionRichTextBlock.FontSize = Settings.GetDefinitionFontSize(); // tfw best practice :p
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
