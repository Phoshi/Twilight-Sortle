using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TwoLight_Sortle {
    static class Load {
        public static Dictionary<UInt32, Item> FilesCache = new Dictionary<UInt32, Item>();

        static Load() {
        }

        public static Item Image(string path) {
            UInt32 hash = Hash(path);
            if (!FilesCache.ContainsKey(hash)) {
                FilesCache[hash] = new Item(path);
            }
            FilesCache[hash].Path = path; //To handle moving files around and stuff
            return FilesCache[hash];
        }

        public static UInt32 Hash(string path) {
            Stream filestream = new FileStream(path, FileMode.Open, FileAccess.Read);
            UInt32 hash = HashStream(filestream);
            filestream.Close();
            return hash;
        }
        private static UInt32 HashStream(Stream stream) {
            MurmurHash2Unsafe hasher = new MurmurHash2Unsafe();
            byte[] file = new byte[stream.Length];
            stream.Read(file, 0, (int) stream.Length);
            UInt32 hash = hasher.Hash(file);
            return hash;
        }
    }
}