using GMD.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Documents;

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
            Definition = new ObservableCollection<Paragraph>();
            var p = new Paragraph();
            p.Inlines.Add(new Run { Text = "Start by entering keyword in the text box" });
            Definition.Add(p);
        }

        public DisplayEntry(Entry entry)
        {
            Dict entryDict = new ManageDicts().Dicts.Where(p => p.DictID == entry.DictId).FirstOrDefault();
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

                var paragraphs = charString.Split('\n');
                Definition = new ObservableCollection<Paragraph>();

                foreach(string paragraph in paragraphs)
                {
                    Run run = new Run();
                    Paragraph newParagraph = new Paragraph();
                    run.Text = paragraph;
                    newParagraph.Inlines.Add(run);
                    Definition.Add(newParagraph);
                }
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
            this.WordStr = newEntry.WordStr;
            this.BookName = newEntry.BookName;
            this.DictId = newEntry.DictId;
            this.wordDataOffset = newEntry.wordDataOffset;
            this.wordDataSize = newEntry.wordDataSize;

            this.Definition.Clear();
            foreach (Paragraph p in newEntry.Definition)
                this.Definition.Add(p);

            RaisePropertyChanged("Current Entry Changed");
        }

        public string BookName { get; set; }
        public ObservableCollection<Paragraph> Definition { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class RecentEntry : Entry
    {
        private int recentlemmaID { get; set; }
    }

    public class FavouritEntry : Entry
    {
        private int favoritLemmaID { get; set; }
    }
}
