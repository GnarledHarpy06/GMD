using GMD.Models;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;

namespace GMD.ViewModels
{
    public class ManageEntries
    {
        private WordStrsIndex[] arrayOfAllQueriedWordStrsIndex;
        public ObservableCollection<WordStrByBookName> CollectionMatchedOfKeywordsByBookName = 
            new ObservableCollection<WordStrByBookName>();

        private string path = Path.Combine
            (ApplicationData.Current.LocalFolder.Path, "Dicts_db.sqlite");
        private SQLiteConnection connection;

        public ManageEntries()
        {
            getDatabaseConnection(); // ConstructorAsync() to delegate constructor method 
        }

        public async Task ConstructAsync()
        {
            try
            {
                App.DictsManager.DictDatabaseChanged += (s, e) => updatearrayOfAllQueriedKeywordsAsync();
                populatearrayOfAllQueriedWordStrsIndexesAsync();                
            }
            catch { }
        }

        private void populatearrayOfAllQueriedWordStrsIndexesAsync()
        {
            var dicts = connection.Table<Dict>().Where(p => p.IsQueried);
            var wordStrDBIndexes = connection.Table<WordStrDBIndex>().ToArray();
            WordStrsIndex[] arrayOfWordStrsIndexes = new WordStrsIndex[dicts.Max(p => p.DictID) + 1];

            foreach(Dict dict in dicts)
            {
                try
                {
                    string[] wordStrs = wordStrDBIndexes
                        .Where(p => p.DictId == dict.DictID)
                        .Select(p => p.WordStr).ToArray();
                    arrayOfWordStrsIndexes[dict.DictID] = new WordStrsIndex(wordStrs, dict.DictID);
                }
                catch { }                
            }
            arrayOfAllQueriedWordStrsIndex = arrayOfWordStrsIndexes;
        }                

        public async Task<Entry> GetEntryAsync(WordStrByBookName word)
        {
            Dict dict = connection.Table<Dict>().Where(p => p.BookName == word.BookName).FirstOrDefault();
            string idxStr =  await dict.GetDictStrAsync();

            int offsetwordStr = idxStr.IndexOf(word.WordStr + '\0');
            int wordStrLength = word.WordStr.Length;

            int offsetOffset = offsetwordStr + wordStrLength + 1;
            int offsetLength = 4;

            if (dict.idxOffsetBits == Dict.idxOffsetBitsEnum.Uint64)            
                offsetLength = 8;
                        
            int lengthOffset = offsetOffset + offsetLength;
            int lengthLength = 4;            

            return new Entry()
            {
                DictId = dict.DictID,
                wordStr = idxStr.Substring(offsetwordStr, wordStrLength),
                wordDataOffset = getUInt32(idxStr.Substring(offsetOffset, offsetLength)).ToString(),
                wordDataSize = getUInt32(idxStr.Substring(lengthOffset, lengthLength))
            };
        }

        private static UInt64 getUInt64(string Uint64Str)
        {
            byte[] tmp = new byte[8];
            for (int i = 0; i < 8; i++)
                tmp[i] = BitConverter.GetBytes(Uint64Str[i])[0];

            if (BitConverter.IsLittleEndian)
                Array.Reverse(tmp);

            return BitConverter.ToUInt64(tmp, 0);
        }

        private static UInt32 getUInt32(string Uint32Str)
        {
            byte[] tmp = new byte[4];
            for (int i = 0; i < 4; i++)
                tmp[i] = BitConverter.GetBytes(Uint32Str[i])[0];

            if (BitConverter.IsLittleEndian)
                Array.Reverse(tmp);

            return BitConverter.ToUInt32(tmp, 0);
        }

        public void QueryKeywords(string keyword)
        {
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
            try
            {
                connection = new SQLiteConnection(new SQLitePlatformWinRT(), path);
            }
            catch { }
        }

        private void updatearrayOfAllQueriedKeywordsAsync() =>
            populatearrayOfAllQueriedWordStrsIndexesAsync();
    }

    public class WordStrsIndex
    {
        public string[] WordStrs { get; private set; }
        public int DictId { get; private set; }

        public WordStrsIndex() { }

        public WordStrsIndex(string[] wordStrs, int dictId)
        {
            WordStrs = wordStrs;
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
                .Where(p => searchPattern.Matches(p.wordStr).Count > 0); // RegEx implemetation
            */

            string[] queriedWordStrs = this.WordStrs.Where(p => p.IndexOf(keyword) == 0).ToArray();

            int minItems;
            if (queriedWordStrs.Count() > 50)
                minItems = 50;
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

        public WordStrByBookName[] GetWordStrsByBookName(string bookName)
        {
            WordStrByBookName[] arrayOfWordStrsByBookName = new WordStrByBookName[WordStrs.Count()];
            for (int i = 0; i < WordStrs.Count(); i++)
            {
                arrayOfWordStrsByBookName[i] = new WordStrByBookName()
                {
                    WordStr = WordStrs[i],
                    BookName = bookName
                };
            }

            return arrayOfWordStrsByBookName;
        }

        public WordStrDBIndex[] GetWordStrDBIndexes()
        {
            WordStrDBIndex[] arrayOfWordStrDBIndexes = new WordStrDBIndex[WordStrs.Count()];
            for (int i = 0; i < WordStrs.Count(); i++)
            {
                arrayOfWordStrDBIndexes[i] = new WordStrDBIndex()
                {
                    WordStr = WordStrs[i],
                    DictId = DictId
                };
            }

            return arrayOfWordStrDBIndexes;
        }
    }

    public class SimpleWordStrBaseClass
    {
        public string WordStr { get; set; }
    }

    public class WordStrDBIndex : SimpleWordStrBaseClass
    {
        public int DictId { get; set; }
    }

    public class WordStrByBookName : SimpleWordStrBaseClass
    {
        public string BookName { get; set; }
    }

    /* Two classes above only serves for mundane purpose
     * Better fix it later
     */


}
