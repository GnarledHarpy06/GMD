using GMD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMD.ViewModels
{
    static class DataConversion
    {
        public static string GetString(byte[] self)
        {
            List<char> charList = new List<char>();

            int j = 0; // var to record charArray offset caused by utf16 char
            for (int i = 0; i < self.Length; i++)
            {
                if ((self[i] & 0x80) == 0x80)
                {
                    byte[] _UTF16Char = new byte[2] { self[i], self[i + 1] };
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(_UTF16Char);

                    charList.Add(BitConverter.ToChar(_UTF16Char, 0));
                    i++;
                    j++;
                    // UTF16 workaround. tfw best practice :p
                }
                else
                    charList.Add(BitConverter.ToChar(new byte[2] { self[i], 0 }, 0));
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

                tmp.Add(chrTmp[0]);
                if ((chrTmp[0] & 0x80) == 0x80)
                    tmp.Add(chrTmp[1]);                
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
