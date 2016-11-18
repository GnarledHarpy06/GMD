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
        public DisplayPage()
        {
            this.InitializeComponent();
            UpdateDisplayedEntry();
            DisplayEntry.CurrentEntryChanged += (s , e) => UpdateDisplayedEntry();
        }

        void UpdateDisplayedEntry()
        {
            Run run = new Run();
            Paragraph p = new Paragraph();
            run.Text = App.CurrentEntry.Definition;
            p.Inlines.Add(run);

            EntryDefinitionRichTextBlock.Blocks.Add(p);
            DictNameTextBlock.Text = App.CurrentEntry.BookName;
            EntryWordStrTextBlock.Text = App.CurrentEntry.wordStr;
        }        
    }
}
