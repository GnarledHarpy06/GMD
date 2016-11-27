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
        private byte[][] arrayOfAllQueriedIdxByteArray;        
        private WordStrsIndex[] arrayOfAllQueriedWordStrsIndex;
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

        public void ConstructAsync()
        {
            try
            {
                App.DictsManager.DictDatabaseChanged += (s, e) => updatearrayOfAllQueriedKeywordsAsync();
                populatearrayOfAllQueriedWordStrsIndexesAsync();
                populatearrayOfAllQueriedIdxByteArray();
                populateCollectionOfViewedEntry();
            }
            catch { }
        }

        private void populateCollectionOfViewedEntry()
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
            var dicts = connection.Table<Dict>().Where(p => p.IsQueried);
            var wordStrDBIndexes = connection.Table<WordStrDBIndex>().ToArray();

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
                    string[] wordStrs = wordStrDBIndexes
                        .Where(p => p.DictId == dict.DictID)
                        .Select(p => p.WordStr).ToArray();
                    arrayOfWordStrsIndexes[dict.DictID] = new WordStrsIndex(wordStrs, dict.DictID);
                }
                catch { }                
            }
            arrayOfAllQueriedWordStrsIndex = arrayOfWordStrsIndexes;
        }                

        public Entry GetEntry(WordStrByBookName word)
        {
            Dict dict = connection.Table<Dict>().Where(p => p.BookName == word.BookName).FirstOrDefault();
            byte[] wordStrByteArray = DataConversion.GetBytes(word.WordStr + '\0');
            int offsetwordStr = arrayOfAllQueriedIdxByteArray[dict.DictID].Locate(wordStrByteArray);
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
                
                wordDataOffset = DataConversion.GetUInt64(arrayOfAllQueriedIdxByteArray[dict.DictID].SubsByteArray(offsetOffset, offsetLength)).ToString();
                wordDataSize = DataConversion.GetUInt32(arrayOfAllQueriedIdxByteArray[dict.DictID].SubsByteArray(lengthOffset, lengthLength));
                
            }
            else
            {
                int lengthOffset = offsetOffset + offsetLength;
                int lengthLength = 4;

                wordDataOffset = DataConversion.GetUInt32(arrayOfAllQueriedIdxByteArray[dict.DictID].SubsByteArray(offsetOffset, offsetLength)).ToString();
                wordDataSize = DataConversion.GetUInt32(arrayOfAllQueriedIdxByteArray[dict.DictID].SubsByteArray(lengthOffset, lengthLength));
            }

            return new Entry()
            {
                DictId = dict.DictID,
                WordStr = word.WordStr,
                wordDataOffset = wordDataOffset,
                wordDataSize = wordDataSize
            };                        
        }

        public void QueryKeywords2(string keyword)
        {
            /* A failed attempt to optimize query
            */

            if (arrayOfAllQueriedWordStrsIndex == null)
                return;
            var keywordByteArray = DataConversion.GetBytes(keyword);
            List<WordStrByBookName> listWSBK = new List<WordStrByBookName>();

            for (int i = 0; i < arrayOfAllQueriedIdxByteArray.Count(); i++)
            {
                if(arrayOfAllQueriedIdxByteArray[i] != null)
                {
                    string[] arrayOfKeywords = new string[50];

                    int offsetLength;
                    string bookName = connection.Get<Dict>(i).BookName;
                    if (connection.Get<Dict>(i).idxOffsetBits == Dict.idxOffsetBitsEnum.Uint64)
                        offsetLength = 8;
                    else
                        offsetLength = 4;

                    int pointer = 0;

                    if (BitConverter.IsLittleEndian && offsetLength == 4)
                    {
                        for (int j = 0; i < 50; j++)
                        {
                            int pointer2 = arrayOfAllQueriedIdxByteArray[i].QueryLocate32(keywordByteArray, pointer);
                            if (pointer2 == -1) break;
                            int pointer3 = arrayOfAllQueriedIdxByteArray[i].Locate(0x00, pointer2);

                            byte[] wordStrByteArray = arrayOfAllQueriedIdxByteArray[i].SubsByteArray(pointer2, pointer3);
                            arrayOfKeywords[j] = Encoding.UTF8.GetString(wordStrByteArray);

                            pointer = pointer3 + offsetLength + 4 + 1;
                        }
                    }
                    else if (BitConverter.IsLittleEndian && offsetLength == 8)
                    {
                        for (int j = 0; i < 50; j++)
                        {
                            int pointer2 = arrayOfAllQueriedIdxByteArray[i].QueryLocate64(keywordByteArray, pointer);
                            if (pointer2 == -1) break;
                            int pointer3 = arrayOfAllQueriedIdxByteArray[i].Locate(0x00, pointer2);

                            byte[] wordStrByteArray = arrayOfAllQueriedIdxByteArray[i].SubsByteArray(pointer2, pointer3);
                            arrayOfKeywords[j] = Encoding.UTF8.GetString(wordStrByteArray);

                            pointer = pointer3 + offsetLength + 8 + 1;
                        }
                    }
                    else if(!BitConverter.IsLittleEndian && offsetLength == 4)
                    {
                        for (int j = 0; i < 50; i++)
                        {
                            int pointer2 = arrayOfAllQueriedIdxByteArray[i].QueryLocate32(keywordByteArray, pointer);
                            if (pointer2 == -1) break;
                            int pointer3 = arrayOfAllQueriedIdxByteArray[i].Locate(0x00, pointer2);

                            byte[] wordStrByteArray = arrayOfAllQueriedIdxByteArray[i].SubsByteArray(pointer2, pointer3);
                            arrayOfKeywords[j] = Encoding.BigEndianUnicode.GetString(wordStrByteArray);

                            pointer = pointer3 + offsetLength + 4 + 1;
                        }
                    }
                    else
                    {
                        for (int j = 0; i < 50; i++)
                        {
                            int pointer2 = arrayOfAllQueriedIdxByteArray[i].QueryLocate64(keywordByteArray, pointer);
                            if (pointer2 == -1) break;
                            int pointer3 = arrayOfAllQueriedIdxByteArray[i].Locate(0x00, pointer2);

                            byte[] wordStrByteArray = arrayOfAllQueriedIdxByteArray[i].SubsByteArray(pointer2, pointer3);
                            arrayOfKeywords[j] = Encoding.BigEndianUnicode.GetString(wordStrByteArray);

                            pointer = pointer3 + offsetLength + 8 + 1;
                        }
                    }

                    foreach (var item in arrayOfKeywords.Where(p => p != null).ToArray())
                        listWSBK.Add(new WordStrByBookName() { WordStr = item, BookName = bookName });
                }
            }
            listWSBK.Sort();            
            foreach (var item in listWSBK)
                CollectionMatchedOfKeywordsByBookName.Add(item);
            CollectionMatchedOfKeywordsByBookName.Clear();
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
            try
            {
                connection = new SQLiteConnection(new SQLitePlatformWinRT(), path);
            }
            catch { }
        }

        private void updatearrayOfAllQueriedKeywordsAsync()
        {
            populatearrayOfAllQueriedWordStrsIndexesAsync();
            populatearrayOfAllQueriedIdxByteArray();
        }
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
