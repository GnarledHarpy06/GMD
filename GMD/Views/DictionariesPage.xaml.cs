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
        }

        private void AddDict_Click(object sender, RoutedEventArgs e)
        {
            App.DictsManager.AddDictAsync();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            DictionaryListView.ItemsSource = App.DictsManager.Dicts;
        }
    }
}
