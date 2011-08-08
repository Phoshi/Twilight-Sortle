using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;

namespace TwoLight_Sortle {
    /// <summary>
    /// Represents a specific directory for sorting 
    /// </summary>
    class Directory : IEnumerable, IEnumerator {
        #region Private Instance Variables;
        private string[] _filepaths;
        private Dictionary<string, Item> _items;
        private List<string> _validFileTypes;
        private string _sortPath;
        private bool _recursive;
        private bool _enabled;
        #endregion

        #region Public Instance Variables
        public string Path { get; internal set; }
        public List<Tag> Tags {
            get { throw new NotImplementedException(); }
        }
        public List<Item> Items {
            get {
                foreach (Item item in this) {
                }
                return (from kvPair in _items select kvPair.Value).ToList();
            }
        }
        #endregion

        #region Constructors
        public Directory(string path) {
            Path = path;
            _items = new Dictionary<string, Item>();
            _validFileTypes = new List<string> {
                                                   ".png",
                                                   ".jpg",
                                                   ".gif"
                                               };
            updateFilepaths();
        }
        #endregion

        #region Private Methods
        private void updateFilepaths() {
            _filepaths = GetFiles(Path);
            if (_recursive) {
                List<string> subdirectories = new List<string>();
                subdirectories.AddRange(System.IO.Directory.GetDirectories(Path));
                while (subdirectories.Count > 0) {
                    string directory = subdirectories.First();
                    subdirectories.RemoveAt(0);
                    string[] newFiles = GetFiles(directory);
                    string[] newFilePaths = new string[newFiles.Length + _filepaths.Length];
                    Array.Copy(_filepaths, newFilePaths, _filepaths.Length);
                    Array.Copy(newFiles, 0, newFilePaths, _filepaths.Length, newFiles.Length);
                    _filepaths = newFilePaths;
                    subdirectories.AddRange(System.IO.Directory.GetDirectories(directory));
                }
            }

            HashSet<string> lookup = new HashSet<string>(_filepaths);
            foreach (KeyValuePair<string, Item> keyValuePair in _items) {
                if (!lookup.Contains(keyValuePair.Key)) {
                    _items.Remove(keyValuePair.Key);
                }
            }
        }

        private string[] GetFiles(string path) {
            string[] rawFiles = System.IO.Directory.GetFiles(Path);
            rawFiles = rawFiles.Where(file => _validFileTypes.Contains(System.IO.Path.GetExtension(file))).ToArray();
            return rawFiles;
        }
        #endregion

        #region Public Methods
        public void DownloadTo(string url) {
            throw new NotImplementedException();
        }
        #endregion

        #region Enumeration
        private int _position;
        public IEnumerator GetEnumerator() {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public bool MoveNext() {
            _position++;
            if (_position < _filepaths.Length) {
                return true;
            }
            return false;
        }

        public void Reset() {
            _position = 0;
        }

        public object Current {
            get {
                if (!_items.ContainsKey(_filepaths[_position])) {
                    _items[_filepaths[_position]] = Load.Image(_filepaths[_position]);
                }
                return _items[_filepaths[_position]];
            }
        }
        #endregion
    }
}
