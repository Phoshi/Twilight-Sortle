using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TwoLight_Sortle {
    static class Load {
        private static Dictionary<UInt32, Item> _files;

        static Load() {
            _files = new Dictionary<UInt32, Item>();
        }

        public static Item Image(string path) {
            UInt32 hash = Hash(path);
            if (!_files.ContainsKey(hash)) {
                _files[hash] = new Item(path);
            }
            return _files[hash];
        }

        public static UInt32 Hash(string path) {
            Stream filestream = new FileStream(path, FileMode.Open, FileAccess.Read);
            UInt32 hash = HashStream(filestream);
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