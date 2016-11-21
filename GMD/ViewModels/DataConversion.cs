using GMD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Documents;

namespace GMD.ViewModels
{
    static class DataConversion
    {
        public static Paragraph[] FormatDefinition(string definition, Dict.TypeSequenceEnum typeSqc)
        {
            string[] formattedDefinition;

            switch (typeSqc)
            {
                case Dict.TypeSequenceEnum.m:
                case Dict.TypeSequenceEnum.l:
                    formattedDefinition = definition.Split('\n');
                    break;
                case Dict.TypeSequenceEnum.g:                    
                case Dict.TypeSequenceEnum.t:                    
                case Dict.TypeSequenceEnum.x:                    
                case Dict.TypeSequenceEnum.y:                    
                case Dict.TypeSequenceEnum.k:                    
                case Dict.TypeSequenceEnum.w:
                case Dict.TypeSequenceEnum.h:
                case Dict.TypeSequenceEnum.n:
                case Dict.TypeSequenceEnum.r:
                case Dict.TypeSequenceEnum.W:
                case Dict.TypeSequenceEnum.P:
                case Dict.TypeSequenceEnum.X:                
                default:
                    formattedDefinition = new string[] { "Sorry the data couldn't be formated correctly", definition };
                    break;

            }

            Paragraph[] paragraphs = new Paragraph[formattedDefinition.Count()];
            for (int i = 0; i < formattedDefinition.Count(); i++)
            {
                paragraphs[i] = new Paragraph();
                paragraphs[i].Inlines.Add(new Run() { Text = formattedDefinition[i] });
            }

            return paragraphs;
        }

        public static string GetString(byte[] self)
        {
            List<char> charList = new List<char>();

            int j = 0; // var to record charArray offset caused by utf16 char
            for (int i = 0; i < self.Length; i++)
            {
                if ((self[i] & 0x80) == 0x00)
                {
                    byte[] _UTF8SingleByte;
                    if(BitConverter.IsLittleEndian)
                        _UTF8SingleByte = new byte[] { self[i], 0 };
                    else
                        _UTF8SingleByte = new byte[] { 0, self[i] };

                    charList.Add(BitConverter.ToChar(_UTF8SingleByte, 0));
                }
                else if ((self[i] & 0xC0) == 0xC0)
                {
                    byte[] _UTF8DoubleBytes = new byte[] { self[i], self[i + 1] };

                    if (BitConverter.IsLittleEndian)
                        charList.Add(Encoding.UTF8.GetChars(_UTF8DoubleBytes).FirstOrDefault()); // hack
                    else
                        charList.Add(Encoding.BigEndianUnicode.GetChars(_UTF8DoubleBytes).FirstOrDefault()); // hack                        

                    i++;
                    j++;
                    // UTF-8 2 bytes char workaround. tfw best practice :p
                }
                else if ((self[i] & 0xE0) == 0xE0)
                {
                    byte[] _UTF8TripleleBytes = new byte[] { self[i], self[i + 1], self[i + 2] };

                    if (BitConverter.IsLittleEndian)
                        charList.Add(Encoding.UTF8.GetChars(_UTF8TripleleBytes).FirstOrDefault()); // hack
                    else
                        charList.Add(Encoding.BigEndianUnicode.GetChars(_UTF8TripleleBytes).FirstOrDefault()); // hack                        

                    i += 2;
                    j += 2;
                    // UTF-8 3 bytes char workaround. tfw best practice :p
                }
                else if ((self[i] & 0xF0) == 0xF0)
                {
                    byte[] _UTF8QuadBytes = new byte[] { self[i], self[i + 1], self[i + 2], self[i + 3] };

                    if (BitConverter.IsLittleEndian)
                        charList.Add(Encoding.UTF8.GetChars(_UTF8QuadBytes).FirstOrDefault()); // hack
                    else
                        charList.Add(Encoding.BigEndianUnicode.GetChars(_UTF8QuadBytes).FirstOrDefault()); // hack                        

                    i += 3;
                    j += 3;
                    // UTF-8 4 bytes char workaround. tfw best practice :p
                }
            }

            return new string(charList.ToArray<char>());            
        }

        public static UInt64 GetUInt64(string Uint64Str)
        {
            byte[] tmp = GetBytes(Uint64Str);

            if (tmp.Length == Uint64Str.Length)
            {
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(tmp);

                return BitConverter.ToUInt64(tmp, 0);
            }
            else return UInt64.MaxValue;
        }

        public static UInt32 GetUInt32(string Uint32Str)
        {
            byte[] tmp = GetBytes(Uint32Str);

            if (tmp.Length == Uint32Str.Length)
            {
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(tmp);

                return BitConverter.ToUInt32(tmp, 0);
            }
            else return UInt32.MaxValue;
        }

        public static UInt32 GetUInt32(byte[] tmp)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(tmp);
                return BitConverter.ToUInt32(tmp, 0);
            }
            else
                return BitConverter.ToUInt32(tmp, 0);
        }

        public static UInt64 GetUInt64(byte[] tmp)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(tmp);
                return BitConverter.ToUInt64(tmp, 0);
            }
            else
                return BitConverter.ToUInt64(tmp, 0);
        }

        public static byte[] GetBytes(string charArray)
        {
            List<byte> tmp = new List<byte>();

            foreach(char chr in charArray)
            {
                byte[] chrTmp = new byte[2];
                chrTmp = BitConverter.GetBytes(chr);
                
                if ((chrTmp[0] & 0x80) != 0x80)
                    tmp.Add(chrTmp[0]);
                else
                {
                    byte[] bytes;
                    if (BitConverter.IsLittleEndian)
                        bytes = Encoding.UTF8.GetBytes(new char[] { chr });
                    else
                        bytes = Encoding.BigEndianUnicode.GetBytes(new char[] { chr });

                    foreach (byte _byte in bytes)
                        tmp.Add(_byte);
                }
            }
            return tmp.ToArray<byte>();
        }

        public static byte[] GetBytes(char[] charArray)
        {
            List<byte> tmp = new List<byte>();
            
            foreach (char chr in charArray)
            {
                byte[] chrTmp = new byte[2];
                chrTmp = BitConverter.GetBytes(chr);

                tmp.Add(chrTmp[0]);
                if ((chrTmp[0] & 0x80) == 0x80)
                    tmp.Add(chrTmp[1]);
            }

            return tmp.ToArray<byte>();
        }

        static public WordStrByBookName[] GetWordStrsByBookName(this WordStrsIndex self, string bookName)
        {
            WordStrByBookName[] arrayOfWordStrsByBookName = new WordStrByBookName[self.WordStrs.Count()];
            for (int i = 0; i < self.WordStrs.Count(); i++)
            {
                arrayOfWordStrsByBookName[i] = new WordStrByBookName()
                {
                    WordStr = self.WordStrs[i],
                    BookName = bookName
                };
            }

            return arrayOfWordStrsByBookName;
        }

        //static public WordStrDBIndex[] GetWordStrDBIndexes( this WordStrsIndex self)
        //{
        //    WordStrDBIndex[] arrayOfWordStrDBIndexes = new WordStrDBIndex[self.WordStrs.Count()];
        //    for (int i = 0; i < self.WordStrs.Count(); i++)
        //    {
        //        arrayOfWordStrDBIndexes[i] = new WordStrDBIndex()
        //        {
        //            WordStr = self.WordStrs[i],
        //            DictId = self.DictId
        //        };
        //    }

        //    return arrayOfWordStrDBIndexes;
        //} NOT USED
    }

    static class ByteArrayRocks
    {
        /* By Jb Evain, Nov 12 '08 at 11:21
         * From StackOverflow
         * http://stackoverflow.com/questions/283456/byte-array-pattern-search
         * 
         * With few edits, tweaks, and overloads by Gio
         */
        static readonly int[] Empty = new int[0];

        public static int Locate(this byte[] self, byte[] candidate)
        {
            if (IsEmptyLocate(self, candidate))
                return -1;

            for (int i = 0; i < self.Length; i++)
            {
                if (IsMatch(self, i, candidate))
                    return i;
            }

            return -1;
        }

        public static int Locate(this byte[] self, byte[] candidate, int startPos)
        {
            if (IsEmptyLocate(self, candidate))
                return -1;

            for (int i = startPos; i < self.Length; i++)
            {
                if (IsMatch(self, i, candidate))
                    return i;
            }

            return -1;
        }

        public static int Locate(this byte[] self, byte candidate)
        {
            for (int i = 0; i < self.Length; i++)
            {
                if (self[i] == candidate)
                    return i;
            }
            return -1;
        }

        public static int Locate(this byte[] self, byte candidate, int startPos)
        {
            for (int i = startPos; i < self.Length; i++)
            {
                if (self[i] == candidate)
                    return i;
            }
            return -1;
        }

        public static int[] LocateAny(this byte[] self, byte[] candidate)
        {
            if (IsEmptyLocate(self, candidate))
                return Empty;

            var list = new List<int>();

            for (int i = 0; i < self.Length; i++)
            {
                if (!IsMatch(self, i, candidate))
                    continue;

                list.Add(i);
            }

            return list.Count == 0 ? Empty : list.ToArray();
        }

        public static int[] LocateAny(this byte[] self, byte candidate)
        {
            var list = new List<int>();

            for (int i = 0; i < self.Length; i++)
            {
                if (self[i] != candidate)
                    continue;

                list.Add(i);
            }

            return list.Count == 0 ? Empty : list.ToArray();
        }

        static bool IsMatch(byte[] array, int position, byte[] candidate)
        {
            if (candidate.Length > (array.Length - position))
                return false;

            for (int i = 0; i < candidate.Length; i++)
                if (array[position + i] != candidate[i])
                    return false;

            return true;
        }

        static bool IsEmptyLocate(byte[] array, byte[] candidate)
        {
            return array == null
                || candidate == null
                || array.Length == 0
                || candidate.Length == 0
                || candidate.Length > array.Length;
        }

        public static byte[] Remove(this byte[] self, int pos, int length)
        {
            byte[] newArray = new byte[self.Length - length];
            int j = 0;
            for (int i = 0; i < self.Length; i++)
            {
                if ((i >= pos) && (i < pos + length))
                    continue;

                newArray[j] = self[i];
                j++;
            }

            return newArray;
        }

        public static byte[] SubsByteArray(this byte[] self, int pos, int length)
        {
            byte[] newArray = new byte[length];

            for (int i = pos; i < pos + length; i++)            
                newArray[i - pos] = self[i];

            return newArray;
        }

    }
}
