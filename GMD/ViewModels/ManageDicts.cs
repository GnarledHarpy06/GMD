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
            getDatabaseConnection();
            updateDictionary();

            DictDatabaseChanged += (s, e) => updateDictionary();
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

        private void getDatabaseConnection()
        {
            if (!File.Exists(path))
            {
                connection = new SQLiteConnection
                    (new SQLitePlatformWinRT(), path);
                connection.CreateTable<Dict>();
                connection.CreateTable<WordStrDBIndex>();
                connection.CreateTable<RecentEntry>();
                connection.CreateTable<FavouritEntry>();
            }
            else
            {
                connection = new SQLiteConnection
                    (new SQLitePlatformWinRT(), path);
            }           
        }        

        private void getDictionaryFromTable()
        {
            var query = connection.Table<Dict>();
            if(query != null)
            foreach (Dict dict in query)
                Dicts.Add(dict);
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

                    try
                    {
                        await (await StorageFolder.GetFolderFromPathAsync($@"{ApplicationData.Current.LocalFolder.Path}\{extractionFolder}\res\")).DeleteAsync();
                    }
                    catch (FileNotFoundException)
                    { }
                    catch (System.Runtime.InteropServices.COMException)
                    { }
                    finally
                    { }
                    
                }

                Dict newDict = new Dict(extractionFolder);
                await newDict.BuildDictionaryAsync();
                connection.Insert(newDict, newDict.GetType());

                string[] wordStrs = newDict.GetKeywordsFromDictAsync();
                WordStrDBIndex[] wordStrDBIndexes = new WordStrDBIndex[newDict.WordCount];

                for (int i = 0; i < newDict.WordCount; i++)                
                    wordStrDBIndexes[i] = new WordStrDBIndex() { WordStr = wordStrs[i], DictId = newDict.DictID };
                
                connection.InsertAll(wordStrDBIndexes);                
                RaiseDictDatabaseChanged("Added");
            }
            finally { }

        }

        public async void RemoveDictAsync(int dictID)
        {
            var pathToDelete = connection.Get<Dict>(dictID).Directory;
            var folderToDelete = await StorageFolder.GetFolderFromPathAsync(pathToDelete);
            await folderToDelete.DeleteAsync();

            connection.Delete<Dict>(dictID);
            connection.Table<WordStrDBIndex>().Delete(p => p.DictId == dictID);
                        
            RaiseDictDatabaseChanged("Removed");
        }

        public void updateDictionary()
        {
            if (Dicts != null)
                Dicts.Clear();
            getDictionaryFromTable();
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
