using GMD.Models;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Windows.Storage;

namespace GMD.ViewModels
{
    public class ManageEntries
    {
        private byte[][] arrayOfAllQueriedIdxByteArray;        
        private WordStrsIndex[] arrayOfAllQueriedWordStrsIndex;
        public ObservableCollection<FavouriteEntry> FavouriteEntries = new ObservableCollection<FavouriteEntry>();
        public ObservableCollection<RecentEntry> ViewedEntries = new ObservableCollection<RecentEntry>();
        public ObservableCollection<WordStrByBookName> CollectionMatchedOfKeywordsByBookName = 
            new ObservableCollection<WordStrByBookName>();

        private string path = Path.Combine
            (ApplicationData.Current.LocalFolder.Path, "Dicts_db.sqlite");
        private SQLiteConnection connection;

        public ManageEntries()
        {
            getDatabaseConnection(); // ConstructorAsync() to delegate constructor method 
        }

        public void Construct()
        {
            try
            {
                App.DictsManager.DictDatabaseChanged += (s, e) => updatearrayOfAllQueriedKeywordsAsync();
                populatearrayOfAllQueriedWordStrsIndexesAsync();
                populatearrayOfAllQueriedIdxByteArray();
                populateCollectionOfViewedEntries();
                populateCollectionOfFavouriteEntries();
            }
            catch { }
        }

        private void populateCollectionOfFavouriteEntries()
        {
            var dbViewedEntr = connection.Table<FavouriteEntry>();
            foreach (var item in dbViewedEntr)
                FavouriteEntries.Add(item);
        }

        public void AddFavouriteEntry(Entry favouriteEntry)
        {
            if (FavouriteEntries.Where(e => (e.DictId == favouriteEntry.DictId)
            && (e.WordStr == favouriteEntry.WordStr)).Count() == 0)
            {
                FavouriteEntry FE = new FavouriteEntry(favouriteEntry);
                FavouriteEntries.Add(FE);
                connection.Insert(FE);
            }
        }

        public void RemoveFavouriteEntry(Entry favouriteEntry) =>        
            FavouriteEntries.Remove(FavouriteEntries
                .Where(f => (f.DictId == favouriteEntry.DictId) 
                && (f.WordStr == favouriteEntry.WordStr))
                .FirstOrDefault());
        

        private void populateCollectionOfViewedEntries()
        {            
            var dbViewedEntr = connection.Table<RecentEntry>();
            foreach (var item in dbViewedEntr)
                ViewedEntries.Add(item);

            RecentEntry.SimpleSortRecentEntriesOC(ViewedEntries);
        }

        public void AddRecentEntry(Entry viewedentry)
        {
            if (ViewedEntries.Where(e => (e.DictId == viewedentry.DictId) && (e.WordStr == viewedentry.WordStr)).Count() == 0)
            {
                RecentEntry RE = new RecentEntry(viewedentry);
                ViewedEntries.Add(RE);
                connection.Insert(RE);

                RecentEntry.SimpleSortRecentEntriesOC(ViewedEntries);
            }
        }

        private void populatearrayOfAllQueriedIdxByteArray()
        {
            var dicts = connection.Table<Dict>().Where(p => p.IsQueried);            
            arrayOfAllQueriedIdxByteArray = new byte[dicts.Max(p => p.DictID) + 1][];

            foreach (Dict dict in dicts)
            {
                try
                {
                    arrayOfAllQueriedIdxByteArray[dict.DictID] = dict.GetIdxByteArray();
                }
                catch { }
            }
        }

        private void populatearrayOfAllQueriedWordStrsIndexesAsync()
        {
            TableQuery<Dict> dicts = connection.Table<Dict>().Where(p => p.IsQueried);
            TableQuery<WordStrDBIndex> wordStrDBIndexes = connection.Table<WordStrDBIndex>();

            if(wordStrDBIndexes.Count() < 1)
            {
                arrayOfAllQueriedWordStrsIndex = new WordStrsIndex[0];
                return;
            }
            WordStrsIndex[] arrayOfWordStrsIndexes = new WordStrsIndex[dicts.Max(p => p.DictID) + 1];

            foreach(Dict dict in dicts)
            {
                try
                {
                    var wordStrs = wordStrDBIndexes
                        .Where(p => p.DictId == dict.DictID);
                    var _wordStrs = wordStrs.Select(p => p.WordStr);

                    arrayOfWordStrsIndexes[dict.DictID] = new WordStrsIndex(_wordStrs, dict.DictID);
                }
                catch { }                
            }
            arrayOfAllQueriedWordStrsIndex = arrayOfWordStrsIndexes;
        }                

        public Entry GetEntry(WordStrByBookName word)
        {
            Dict dict = connection.Table<Dict>().Where(p => p.BookName == word.BookName).FirstOrDefault();
            byte[] wordStrByteArray = DataConversion.GetBytes(word.WordStr + '\0');
            int offsetwordStr;
            if (dict.idxOffsetBits == Dict.idxOffsetBitsEnum.Uint64)
                offsetwordStr = arrayOfAllQueriedIdxByteArray[dict.DictID].LocateWordStr64(wordStrByteArray);
            else
                offsetwordStr = arrayOfAllQueriedIdxByteArray[dict.DictID].LocateWordStr32(wordStrByteArray);

            // fix this quickhack later

            int wordStrLength = wordStrByteArray.Length - 1;

            int offsetOffset = offsetwordStr + wordStrLength + 1;
            int offsetLength = 4;

            string wordDataOffset;
            uint wordDataSize;

            if (dict.idxOffsetBits == Dict.idxOffsetBitsEnum.Uint64)
            {
                offsetLength = 8;
                int lengthOffset = offsetOffset + offsetLength;
                int lengthLength = 4;
                
                wordDataOffset = DataConversion.GetUInt64(arrayOfAllQueriedIdxByteArray[dict.DictID]
                    .SubsByteArray(offsetOffset, offsetLength)).ToString();
                wordDataSize = DataConversion.GetUInt32(arrayOfAllQueriedIdxByteArray[dict.DictID]
                    .SubsByteArray(lengthOffset, lengthLength));                
            }
            else
            {
                int lengthOffset = offsetOffset + offsetLength;
                int lengthLength = 4;

                wordDataOffset = DataConversion.GetUInt32(arrayOfAllQueriedIdxByteArray[dict.DictID]
                    .SubsByteArray(offsetOffset, offsetLength)).ToString();
                wordDataSize = DataConversion.GetUInt32(arrayOfAllQueriedIdxByteArray[dict.DictID]
                    .SubsByteArray(lengthOffset, lengthLength));
            }

            return new Entry()
            {
                DictId = dict.DictID,
                WordStr = word.WordStr,
                wordDataOffset = wordDataOffset,
                wordDataSize = wordDataSize
            };                        
        }        

        public void QueryKeywords(string keyword)
        {
            if (arrayOfAllQueriedWordStrsIndex == null)
                return;
            CollectionMatchedOfKeywordsByBookName.Clear();            

            int pointer = 0;
            foreach(WordStrsIndex WordStrsIndex in arrayOfAllQueriedWordStrsIndex)
            {
                if (WordStrsIndex != null)
                {
                    WordStrsIndex filteredKeywordIndex = WordStrsIndex.FilterWordStrs(keyword);

                    string bookname = connection.Get<Dict>(WordStrsIndex.DictId).BookName;
                    WordStrByBookName[] arrayOfWordStrByBookName =
                        filteredKeywordIndex.GetWordStrsByBookName(bookname);

                    int minItems = arrayOfWordStrByBookName.Count();
                    for (int i = pointer; i < pointer + minItems; i++)
                        CollectionMatchedOfKeywordsByBookName.Add(arrayOfWordStrByBookName[i]);
                }
            }
        }

        private void getDatabaseConnection()
        {
            if (File.Exists(path))
                connection = new SQLiteConnection(new SQLitePlatformWinRT(), path);
        }

        private void updatearrayOfAllQueriedKeywordsAsync()
        {
            getDatabaseConnection();
            populatearrayOfAllQueriedWordStrsIndexesAsync();
            populatearrayOfAllQueriedIdxByteArray();
        }
    }

    public class WordStrsIndex
    {
        public string[] WordStrs { get; private set; }
        public int DictId { get; private set; }
        //public string BookName { get; private set; } // Later implemented

        public WordStrsIndex() { }

        public WordStrsIndex(System.Collections.Generic.IEnumerable<string> wordStrs, int dictId)
        {
            WordStrs = wordStrs.ToArray();
            DictId = dictId;
        }

        public WordStrsIndex(WordStrDBIndex[] wordStrDBIndexes)
        {
            WordStrs = new string[wordStrDBIndexes.Count()];
            DictId = wordStrDBIndexes[0].DictId;
            for (int i = 0; i < wordStrDBIndexes.Count(); i++)
            {
                WordStrs[i] = wordStrDBIndexes[i].WordStr;
            }
        }

        public WordStrsIndex FilterWordStrs(string keyword)
        {
            /*
            Regex searchPattern = new Regex($@"^({keyword})(.+)?");
            CollectionMatchedOfEntries.Clear();

            var queryMatchingEntries = arrayOfAllQueriedEntries
                .Where(p => searchPattern.Matches(p.wordStr).Count > 0); // RegEx implemetation not used
            */

            string[] queriedWordStrs = this.WordStrs.Where(p => p.IndexOf(keyword) == 0).ToArray();

            int minItems;
            if (queriedWordStrs.Count() > 30)
                minItems = 30;
            else
                minItems = queriedWordStrs.Count();
            string[] filteredWordStrs = new string[minItems];

            for (int i = 0; i < minItems; i++)
                filteredWordStrs[i] = queriedWordStrs[i];

            return new WordStrsIndex(filteredWordStrs, this.DictId);

            /* Query array of keywords
             * Where keyword matches searchPattern
             * or has IndexOf return value 0
             */
        }
    }
}
