using GMD.Models;
using SharpCompress.Readers;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;
using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using System.ComponentModel;

namespace GMD.ViewModels
{
    public class ManageDicts
    {
        public ManageDicts()
        {
            GetDatabaseConnection();
            GetDictionaryFromTable();

            DictDatabaseChanged += (s, e) => UpdateDictionary();
        }

        private string path = Path.Combine
            (ApplicationData.Current.LocalFolder.Path, "Dicts_db.sqlite");
        private SQLiteConnection connection;
        public ObservableCollection<Dict> Dicts = new ObservableCollection<Dict>();

        //private static SQLiteConnection GetDatabaseConnection(string path)
        //{
        //    SQLiteConnection _connection;

        //    if (!File.Exists(path))
        //    {
        //        _connection = new SQLiteConnection
        //            (new SQLitePlatformWinRT(), path);
        //        _connection.CreateTable<Dict>();
        //    }
        //    else
        //    {
        //        _connection = new SQLiteConnection
        //            (new SQLitePlatformWinRT(), path);
        //    }

        //    return _connection;
        //}

        private void GetDatabaseConnection()
        {
            if (!File.Exists(path))
            {
                connection = new SQLiteConnection
                    (new SQLitePlatformWinRT(), path);
                connection.CreateTable<Dict>();
            }
            else
            {
                connection = new SQLiteConnection
                    (new SQLitePlatformWinRT(), path);
            }           
        }

        public void GetDictionaryFromTable()
        {
            var query = connection.Table<Dict>();
            if(query != null)
            foreach (Dict dict in query)
                Dicts.Add(dict);
        }

        public void UpdateDictionary()
        {
            if(Dicts != null)
                Dicts.Clear();
            GetDictionaryFromTable();
        }

        private async Task<StorageFile> pickAFileAsync()
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.List;
            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            openPicker.FileTypeFilter.Add(".tar");
            openPicker.FileTypeFilter.Add(".gz");
            openPicker.FileTypeFilter.Add(".bz2");
            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
                return file;
            else            
                return null;            
        }

        public async void AddDictAsync()
        {
            StorageFile file = await pickAFileAsync();
            string extractionFolder;

            try
            {
                using (Stream stream = await file.OpenStreamForReadAsync())
                using (var reader = ReaderFactory.Open(stream))
                {
                    var fileName = file.DisplayName;
                    extractionFolder = fileName.Remove(fileName.LastIndexOf("."),
                        fileName.Length - fileName.LastIndexOf("."));

                    while (reader.MoveToNextEntry())
                    {
                        if (!reader.Entry.IsDirectory)
                        {
                            reader.WriteEntryToDirectory(ApplicationData.Current.LocalFolder.Path, new ExtractionOptions()
                            {
                                ExtractFullPath = true,
                                Overwrite = true
                            });
                        }
                    }
                }

                Dict newDict = new Dict(extractionFolder);
                await newDict.BuildDictionaryAsync();

                connection.Insert(newDict);
                RaiseDictDatabaseChanged("Added");
            }
            finally { }

        }

        public event PropertyChangedEventHandler DictDatabaseChanged;

        protected virtual void RaiseDictDatabaseChanged(string propertyName)
        {
            //var handler = this.DictDatabaseChanged;
            //if (handler != null)
            //{
            //    handler(this, new PropertyChangedEventArgs(propertyName));
            //}

            DictDatabaseChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
