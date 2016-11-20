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
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GMD.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DisplayPage : Page
    {
        private DisplayEntry DisplayedEntry = new DisplayEntry();

        public DisplayPage()
        {
            this.InitializeComponent();
            App.CurrentEntry.PropertyChanged += (s, e) => this.DisplayedEntry.UpdateEntry(App.CurrentEntry);
            DisplayedEntry.PropertyChanged += (s, e) => this.FollowUpRichTextBlockRebind();
        }
        
        void FollowUpRichTextBlockRebind()
        {
            EntryDefinitionRichTextBlock.Blocks.Clear();
            foreach (Paragraph p in this.DisplayedEntry.Definition)
                EntryDefinitionRichTextBlock.Blocks.Add(p);
            // tfw best practice :p
        }
    }
}
