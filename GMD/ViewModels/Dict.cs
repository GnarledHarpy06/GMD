using SharpCompress.Readers;
using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Windows.Storage;

namespace GMD.ViewModels
{
    class Dict
    {
        [PrimaryKey, AutoIncrement, Unique]
        private int dictID { get; set; }
        [Unique]
        public string FolderName { get; private set; }
        public string Directory { get; private set; }

        public string BookName { get; private set; }
        public int WordCount { get; private set; }
        public int IdxFileSize { get; private set; }
        public string Author { get; private set; }
        public string Email { get; private set; }
        public string Website { get; private set; }
        public string Description { get; private set; }
        public string Date { get; private set; }

        public int synWordCount { get; private set; } // unsupported
        public byte idxOffsetBits { get; private set; }
        public string dictType { get; private set; }
        public string sameTypeSequence { get; private set; }

        private string ifoPath { get; set; }
        private string dictPath { get; set; }
        private string idxPath { get; set; }
        private string synPath { get; set; }

        private string ifoOftPath { get; set; }
        private string synOftPath { get; set; }

        // private string ifoCltPath { get; set; } // unsupported
        // private string synCltPath { get; set; } // unsupported

        public Dict() { }

        public Dict(string fileName)
        {
            FolderName = fileName;
            Directory = Path.Combine
                (ApplicationData.Current.LocalFolder.Path, FolderName);
        }

        public async void BuildDictionaryAsync()
        {
            try
            {
                ifoPath = getPathByFileType(".ifo");
                synPath = getPathByFileType(".syn");

                ifoOftPath = getPathByFileType(".ifo.oft");
                synOftPath = getPathByFileType(".syn.oft");
            }
            finally { }

            try
            { dictPath = getPathByFileType(".dict"); }
            catch
            {
                try
                { extract("dict.dz"); }
                catch { extract("dict.gz"); }

                dictPath = getPathByFileType(".dict");
            }

            try
            { idxPath = getPathByFileType(".idx"); }
            catch
            {
                extract("idx.gz");

                idxPath = getPathByFileType(".idx");
            }

            List<String> lines;

            using (MemoryStream memStream = new MemoryStream())
            using (Stream stream = await
                (await StorageFile.GetFileFromPathAsync(ifoPath))
                .OpenStreamForReadAsync())
            {
                await stream.CopyToAsync(memStream);
                byte[] _byteBuffer = memStream.ToArray();
                string ifo = Encoding.UTF8.GetString(_byteBuffer, 0, _byteBuffer.Length);
                lines = ifo.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                
                /*  This code reads data from .ifo file as a stream
                 *  copys them to a memory stream
                 *  turns them into byteArray
                 *  turns them into a single string
                 *  assign each line to lines List<String>
                 *  and dispose the unused stream, memStream, and string from memory
                 */
            }

            try
            {
                BookName = getStringValue(lines, "bookname");
                WordCount = int.Parse(getStringValue(lines, "wordcount"));
                IdxFileSize = int.Parse(getStringValue(lines, "idxfilesize"));
            }
            catch { BookName = FolderName; }

            try { idxOffsetBits = byte.Parse(getStringValue(lines, "idxoffsetbits")); }
            catch { }

            try { synWordCount = int.Parse(getStringValue(lines, "synwordcount")); }
            catch { }

            try { Author = getStringValue(lines, "author"); }
            catch { }

            try { Email = getStringValue(lines, "email"); }
            catch { }

            try { Website = getStringValue(lines, "website"); }
            catch { }

            try { Description = getStringValue(lines, "description"); }
            catch { }

            try { Date = getStringValue(lines, "date"); }
            catch { }

            try { sameTypeSequence = getStringValue(lines, "sametypesequence"); }
            catch { }

            try { dictType = getStringValue(lines, "dicttype"); }
            catch { }
        }

        private static string getStringValue(List<string> lines, string valueType)
        {
            return lines.Select(line => line.Contains($"{valueType}=") ?
                    line.Remove(0, line.Length - $"{valueType}=".Length) : null)
                    .ToString();
        }

        private string getPathByFileType(string extension)
        {
            var files = System.IO.Directory.EnumerateFiles(Directory, extension, SearchOption.TopDirectoryOnly);

            foreach (string currentFile in files)
            {
                string _fileName = currentFile.Remove(0, currentFile.Length - extension.Length);
                if (_fileName == extension)
                {
                    return _fileName;
                }
            }
            return null;
        }
        
        private async void extract(string extension)
        {
            string path = getPathByFileType(extension);
            StorageFile file = await StorageFile.GetFileFromPathAsync(path);

            using (Stream stream = await file.OpenStreamForReadAsync())
            using (var reader = ReaderFactory.Open(stream))
            {
                reader.WriteEntryToDirectory(Directory, new ExtractionOptions()
                {
                    ExtractFullPath = true,
                    Overwrite = true
                });
            }
        }
    }

    //public class Lemma
    //{
    //    [PrimaryKey, AutoIncrement]
    //    private int lemmaId { get; set; }
        
    //    private int dictId { get; set; }
    //    public string wordStr { get; set; }
    //    public string wordDataOft { get; set; }
    //    public string wordDataSize { get; set; }

    //}
}
