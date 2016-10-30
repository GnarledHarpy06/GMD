using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;
using SQLite.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace GMD.ViewModels
{
    public class ManageDicts
    {
        private static string path = Path.Combine
            (ApplicationData.Current.LocalFolder.Path, "Dicts_db.sqlite");

        private SQLiteConnection connection = ManageDicts.GetDatabaseConnection();

        private static SQLiteConnection GetDatabaseConnection()
        {
            SQLiteConnection _connection;

            if (!System.IO.File.Exists(path))
            {
                _connection = new SQLiteConnection
                    (new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), path);
                _connection.CreateTable<Dict>();
            }
            else
            {
                _connection = new SQLiteConnection
                    (new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), path);                
            }

            return _connection;
        }

        private async Task<StorageFile> selectAFileAsync()
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.List;
            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            openPicker.FileTypeFilter.Add(".tar");
            openPicker.FileTypeFilter.Add(".gz");
            openPicker.FileTypeFilter.Add(".bz2");
            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                return file;
            }
            else
            {
                return null;
            }
        }

        async void AddDict()
        {
            StorageFile file = await selectAFileAsync();

            
            using (Stream stream = await file.OpenStreamForReadAsync())
            using (var reader = ReaderFactory.Open(stream))
            {
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
        }
    }    
}
