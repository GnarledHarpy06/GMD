using GMD.Models;
using SharpCompress.Readers;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;
using System;
using System.IO;
using System.Collections.ObjectModel;
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

        private void getDatabaseConnection()
        {
            if (!File.Exists(path))
            {
                connection = new SQLiteConnection
                    (new SQLitePlatformWinRT(), path);
                connection.CreateTable<Dict>();
                connection.CreateTable<WordStrDBIndex>();
                connection.CreateTable<RecentEntry>();
                connection.CreateTable<FavouriteEntry>();
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

        public async Task AddDictAsync()
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

                    if (Directory.Exists($@"{ApplicationData.Current.LocalFolder.Path}\{extractionFolder}\res\"))
                    {
                        try
                        {
                            await (await StorageFolder.GetFolderFromPathAsync(
                                $@"{ApplicationData.Current.LocalFolder.Path}\{extractionFolder}\res\"))
                                .DeleteAsync();
                        }
                        catch (Exception) { }
                    }
                }

                Dict newDict = new Dict(extractionFolder);
                await newDict.BuildDictionaryAsync();
                connection.Insert(newDict, newDict.GetType());

                /* WARNING
                 * dict object have to inserted into database first
                 * to make the DictId incrementally unique,
                 * or otherwise the primary key will always be 0                 
                 */

                WordStrDBIndex[] wordStrDBIndexes = new WordStrDBIndex[newDict.WordCount];
                string[] wordStrs = newDict.GetKeywordsFromDict();

                for (int i = 0; i < newDict.WordCount; i++)
                    wordStrDBIndexes[i] = new WordStrDBIndex()
                    {
                        WordStr = wordStrs[i],
                        DictId = newDict.DictID
                    };
                
                connection.InsertAll(wordStrDBIndexes);
                RaiseDictDatabaseChanged("Added");
            }
            catch (Exception e) { System.Diagnostics.Debug.WriteLine(e.Message); }
        }        

        public async Task RemoveDictAsync(int dictID)
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

        protected virtual void RaiseDictDatabaseChanged(string propertyName) =>
            DictDatabaseChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));        
    }
}
