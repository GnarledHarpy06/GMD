using GMD.ViewModels;
using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Windows.Storage;
using SharpCompress.Archives;
using SharpCompress.Archives.GZip;
using System.Threading.Tasks;

namespace GMD.Models
{
    public class Dict
    {
        [PrimaryKey, AutoIncrement, Unique]
        public int DictID { get; private set; }
        [Unique]
        public string FolderName { get; private set; }
        [Unique]
        public string Directory { get; private set; }
        public long DictSize { get; private set; }
        public bool IsQueried { get; set; }

        public string BookName { get; private set; }
        public int WordCount { get; private set; }
        public int IdxFileSize { get; private set; }
        public string Author { get; private set; }
        public string Email { get; private set; }
        public string Website { get; private set; }
        public string Description { get; private set; }
        public string Date { get; private set; }

        public int synWordCount { get; private set; } // unsupported
        public idxOffsetBitsEnum idxOffsetBits { get; private set; }
        public string dictType { get; private set; }
        public TypeSequenceEnum sameTypeSequence { get; private set; }

        public string ifoPath { get; private set; }
        public string dictPath { get; private set; }
        public string idxPath { get; private set; }
        public string synPath { get; private set; } // unsupported

        public string ifoOftPath { get; private set; } // unsupported
        public string synOftPath { get; private set; } // unsupported

        // private string ifoCltPath { get; set; } // unsupported
        // private string synCltPath { get; set; } // unsupported

        public enum idxOffsetBitsEnum
        {
            Uint32, Uint64
        }

        public enum TypeSequenceEnum
        {
            m, l, g, t, x, y, k, w, h, n, r, W, P, X
        }

        public Dict() { }

        public Dict(string extractionFolder)
        {
            FolderName = extractionFolder;
            Directory = Path.Combine
                (ApplicationData.Current.LocalFolder.Path, FolderName);
        }

        public async Task BuildDictionaryAsync()
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
            {
                dictPath = getPathByFileType(".dict");

                if (String.IsNullOrEmpty(dictPath))
                {
                    dictPath = await extractAsync(".dict.dz");
                }

            }
            catch { }

            try
            {
                idxPath = getPathByFileType(".idx");

                if (String.IsNullOrEmpty(idxPath))
                {
                    idxPath = await extractAsync(".idx.dz");
                }
            }
            catch { }

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

            try
            {
                if (int.Parse(getStringValue(lines, "idxoffsetbits")) == 64)
                    idxOffsetBits = idxOffsetBitsEnum.Uint64;
                else
                    idxOffsetBits = idxOffsetBitsEnum.Uint32;
            }
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

            try
            {
                char chr = getStringValue(lines, "sametypesequence").FirstOrDefault();
                switch (chr)
                {
                    case 'm':
                        sameTypeSequence = TypeSequenceEnum.m;
                        break;
                    case 'l':
                        sameTypeSequence = TypeSequenceEnum.l;
                        break;
                    case 'g':
                        sameTypeSequence = TypeSequenceEnum.g;
                        break;
                    case 't':
                        sameTypeSequence = TypeSequenceEnum.t;
                        break;
                    case 'x':
                        sameTypeSequence = TypeSequenceEnum.x;
                        break;
                    case 'y':
                        sameTypeSequence = TypeSequenceEnum.y;
                        break;
                    case 'k':
                        sameTypeSequence = TypeSequenceEnum.k;
                        break;
                    case 'w':
                        sameTypeSequence = TypeSequenceEnum.w;
                        break;
                    case 'h':
                        sameTypeSequence = TypeSequenceEnum.h;
                        break;
                    case 'n':
                        sameTypeSequence = TypeSequenceEnum.n;
                        break;
                    case 'r':
                        sameTypeSequence = TypeSequenceEnum.r;
                        break;
                    case 'W':
                        sameTypeSequence = TypeSequenceEnum.W;
                        break;
                    case 'P':
                        sameTypeSequence = TypeSequenceEnum.P;
                        break;
                    case 'X':                        
                    default:
                        sameTypeSequence = TypeSequenceEnum.X;
                        break;
                }
            }
            catch { }

            try { dictType = getStringValue(lines, "dicttype"); }
            catch { }

            DictSize = new DirectoryInfo(Directory).EnumerateFiles().Sum(file => file.Length);
            IsQueried = true;
        }
        
        public byte[] GetIdxByteArray()
        {
            return File.ReadAllBytes(this.idxPath);
        }

        public async Task<string> GetIdxStrAsync()
        {
            using (Stream stream = await
                        (await StorageFile.GetFileFromPathAsync(this.idxPath))
                        .OpenStreamForReadAsync())
            using (StreamReader streamReader = new StreamReader(stream))
            {
                return await streamReader.ReadToEndAsync();
            }
        }

        public string[] GetKeywordsFromDictAsync()
        {
            byte[] idxStr = this.GetIdxByteArray();
            string[] arrayOfKeywords = new string[this.WordCount];

            int offsetLength;
            if (this.idxOffsetBits == Dict.idxOffsetBitsEnum.Uint64)
                offsetLength = 8;
            else
                offsetLength = 4;

            int pointer = 0;

            if (BitConverter.IsLittleEndian)
            {
                for (int i = 0; i < this.WordCount; i++)
                {
                    int pointer2 = idxStr.Locate(0x00, pointer);
                    byte[] wordStrByteArray = idxStr.SubsByteArray(pointer, pointer2 - pointer);
                    arrayOfKeywords[i] = Encoding.UTF8.GetString(wordStrByteArray);

                    pointer += wordStrByteArray.Length + offsetLength + 4 + 1;
                }
            }
            else
            {
                for (int i = 0; i < this.WordCount; i++)
                {
                    int pointer2 = idxStr.Locate(0x00, pointer);
                    byte[] wordStrByteArray = idxStr.SubsByteArray(pointer, pointer2 - pointer);
                    arrayOfKeywords[i] = Encoding.BigEndianUnicode.GetString(wordStrByteArray);

                    pointer += wordStrByteArray.Length + offsetLength + 4 + 1;
                }
            }
            return arrayOfKeywords;
        }
        
        private static string getStringValue(List<string> lines, string valueType)
        {

            string line = lines.Where(p => p.Contains($"{valueType}=")).FirstOrDefault();
            return line.Remove(0, $"{valueType}=".Length);
        }

        private string getPathByFileType(string extension)
        {
            var files = System.IO.Directory.EnumerateFiles(Directory, "*" + extension, SearchOption.TopDirectoryOnly);

            foreach (string currentFile in files)
            {
                string _extension = currentFile.Remove(0, currentFile.Length - extension.Length);
                if (_extension == extension)
                {
                    return currentFile;
                }
            }
            return null;
        }
        
        private async Task<String> extractAsync(string extension)
        {
            string path = getPathByFileType(extension);
            string primaryExtension = extension.Remove(0, extension.LastIndexOf("."));
            string writePath = path.Replace(
                    extension,
                    extension.Remove(extension.IndexOf(primaryExtension),
                    extension.Length - extension.IndexOf(primaryExtension)));
            
            using (Stream stream = await (await StorageFile.GetFileFromPathAsync(path)).OpenStreamForReadAsync())
            using (var archive = GZipArchive.Open(stream))
            {
                var entry = archive.Entries.First();
                entry.WriteToFile(writePath);
            }

            return writePath;
        }        
    }
}
