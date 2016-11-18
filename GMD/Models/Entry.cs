using GMD.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace GMD.Models
{
    public class Entry
    {
        public string wordStr { get; set; }
        public int DictId { get; set; }
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

    public class DisplayEntry :  Entry
    {
        public DisplayEntry()
        {
            BookName = "GMD Modular Dictionary";
            wordStr = "WELCOME MY DEAR";
            Definition = "Start by entering keyword in the text box";
        }

        public DisplayEntry(Entry entry)
        {
            Dict entryDict = new ManageDicts().Dicts.Where(p => p.DictID == entry.DictId).FirstOrDefault();
            BookName = entryDict.BookName;
            string dictPath = entryDict.dictPath;

            using (FileStream fileStream = new FileStream(dictPath, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[entry.wordDataSize]; // create buffer
                int seekOffset = checked((int)UInt64.Parse(entry.wordDataOffset));
                int length = checked((int)entry.wordDataSize);

                fileStream.Seek(seekOffset, SeekOrigin.Begin);
                fileStream.Read(buffer, 0, length);

                char[] charArray = new char[length];

                int j = 0; // var to record charArray offset caused by utf16 char
                for (int i = 0; i < length; i++)
                {
                    if ((buffer[i] & 0xf0) == 0xf0)
                    {
                        byte[] _UTF16Char = new byte[2] { buffer[i], buffer[i + 1] };
                        if (BitConverter.IsLittleEndian)
                            Array.Reverse(_UTF16Char);

                        charArray[i - j] = BitConverter.ToChar(_UTF16Char, 0);
                        i++;
                        j++;
                        // UTF16 workaround. tfw best practice :p
                    }
                    else
                        charArray[i - j] = BitConverter.ToChar(new byte[2] { buffer[i], 0 }, 0);
                }
                Definition = new string(charArray);

                RaiseCurrentEntryChanged("Current Entry Changed");
            }
        }

        public string BookName { get; private set; }
        public string Definition { get; private set; }

        public static event System.ComponentModel.PropertyChangedEventHandler CurrentEntryChanged;

        protected virtual void RaiseCurrentEntryChanged(string propertyName)
        {
            CurrentEntryChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
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
