using GMD.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using Windows.UI.Xaml.Documents;
using SQLite.Net.Attributes;

namespace GMD.Models
{
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

    public class Entry : WordStrDBIndex
    {
        public string wordDataOffset { get; set; }
        public Int64 wordDataSize { get; set; }

        public Entry ToEntry()
        {
            return new Entry
            {
                DictId = this.DictId,
                WordStr = this.WordStr,
                wordDataOffset = this.wordDataOffset,
                wordDataSize = this.wordDataSize
            };
        }
    }

    //public class EntriesGrouping : System.Linq.IGrouping<string, Entry>
    //{
    //    public ObservableCollection<Entry> Entries;
    //    public string BookName { private get ; set; }

    //    public string Key
    //    { get { return BookName; } }

    //    public IEnumerator<Entry> GetEnumerator() =>
    //        Entries.GetEnumerator();

    //    IEnumerator IEnumerable.GetEnumerator() =>
    //        Entries.GetEnumerator();
    //}

    public class DisplayEntry : Entry, INotifyPropertyChanged
    {
        public DisplayEntry()
        {
            BookName = "GMD Modular Dictionary";
            WordStr = "WELCOME MY DEAR";
            Definition = new List<Paragraph>();
            var p = new Paragraph();
            p.Inlines.Add(new Run { Text = "Start by entering keyword in the text box" });
            Definition.Add(p);
        }

        public DisplayEntry(Entry entry)
        {
            Dict entryDict = new ManageDicts().Dicts.Where(p => p.DictID == entry.DictId).FirstOrDefault();
            DictId = entryDict.DictID;
            BookName = entryDict.BookName;
            WordStr = entry.WordStr;
            wordDataOffset = entry.wordDataOffset;
            wordDataSize = entry.wordDataSize;
            
            string dictPath = entryDict.dictPath;

            using (FileStream fileStream = new FileStream(dictPath, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[entry.wordDataSize]; // create buffer
                int seekOffset = checked((int)UInt64.Parse(entry.wordDataOffset));
                int length = checked((int)entry.wordDataSize);

                fileStream.Seek(seekOffset, SeekOrigin.Begin);
                fileStream.Read(buffer, 0, length);

                string charString = DataConversion.GetString(buffer);

                Definition = new List<Paragraph>();
                Paragraph[] paragraphs = DataConversion.FormatDefinition(charString, entryDict.sameTypeSequence);

                foreach (Paragraph paragraph in paragraphs)                
                    Definition.Add(paragraph);                
            }
        }

        public void UpdateEntry(Entry entry)
        {
            DisplayEntry newEntry = new DisplayEntry(entry);
            this.WordStr = newEntry.WordStr;
            this.BookName = newEntry.BookName;
            this.DictId = newEntry.DictId;
            this.wordDataOffset = newEntry.wordDataOffset;
            this.wordDataSize = newEntry.wordDataSize;

            this.Definition.Clear();
            foreach(Paragraph p in newEntry.Definition)
                this.Definition.Add(p);                       

            RaisePropertyChanged("Current Entry Changed");
        }

        public void UpdateEntry(DisplayEntry newEntry)
        {
            this.DictId = newEntry.DictId;
            this.BookName = newEntry.BookName;
            this.WordStr = newEntry.WordStr;
            this.wordDataOffset = newEntry.wordDataOffset;
            this.wordDataSize = newEntry.wordDataSize;

            this.Definition.Clear();
            foreach (Paragraph p in newEntry.Definition)
                this.Definition.Add(p);

            RaisePropertyChanged("Current Entry Changed");
        }

        public string BookName { get; set; }
        public List<Paragraph> Definition { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RecentEntry : Entry
    {
        [PrimaryKey, AutoIncrement, Unique]
        public int RecentlemmaID { get; private set; }

        public RecentEntry() { }
        public RecentEntry(Entry recentEntry)
        {
            DictId = recentEntry.DictId;
            WordStr = recentEntry.WordStr;
            wordDataOffset = recentEntry.wordDataOffset;
            wordDataSize = recentEntry.wordDataSize;
        }        

        public static void SimpleSortRecentEntriesOC(System.Collections.ObjectModel.ObservableCollection<RecentEntry> recententries)
        {
            /* It won't be a proper sorting algorithm
             * since this specifications have a number of exact rules
             * we can use to simplify things :p
             */

            var tmpArray = recententries.ToArray();
            int totalItems = tmpArray.Length;
            for (int i = 0; i < totalItems; i++)
            {
                recententries.RemoveAt(totalItems - tmpArray[i].RecentlemmaID);
                recententries.Insert(totalItems - tmpArray[i].RecentlemmaID, tmpArray[i]);
            }
        }
    }

    public class FavouriteEntry : Entry
    {
        [PrimaryKey, AutoIncrement, Unique]
        private int favouritLemmaID { get; set; }
        public FavouriteEntry() { }
        public FavouriteEntry(Entry recentEntry)
        {
            DictId = recentEntry.DictId;
            WordStr = recentEntry.WordStr;
            wordDataOffset = recentEntry.wordDataOffset;
            wordDataSize = recentEntry.wordDataSize;
        }        
    }
}
