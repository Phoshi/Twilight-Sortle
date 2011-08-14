using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Runtime.Serialization;
using Extensions;

namespace TwoLight_Sortle {
    /// <summary>
    /// Represents a specific directory for sorting 
    /// </summary>
    [Serializable]
    class Directory : IEnumerable, IEnumerator, ISerializable {
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
            get {
                assureFilesLoaded();
                return (from kvPair in _items select kvPair.Value.Tags).SelectMany(tag => tag).Distinct().ToList();
            }
        }
        public List<Item> Items {
            get {
                assureFilesLoaded();
                return (from kvPair in _items select kvPair.Value).ToList();
            }
        }
        public bool Enabled {
            get { return _enabled; }
            set { _enabled = value; }
        }
        public bool Recursive {
            get { return _recursive; }
            set { _recursive = value; }
        }
        public List<string> ValidFileTypes {
            get { return _validFileTypes; }
        }
        public String SortPath {
            get { return _sortPath; }
            set { _sortPath = value; }
        }

        #endregion

        #region Constructors
        public Directory(string path) {
            Path = path;
            _items = new Dictionary<string, Item>();
            _enabled = true;
            _validFileTypes = new List<string> {
                                                   ".png",
                                                   ".jpg",
                                                   ".gif"
                                               };
            UpdateFilepaths();
            assureFilesLoaded();
        }

        public Directory(SerializationInfo info, StreamingContext context) {
            _items = new Dictionary<string, Item>();
            Path = info.GetString("Path");
            _validFileTypes = (List<string>) info.GetValue("_validFileTypes", typeof (List<String>));
            _sortPath = info.GetString("_sortPath");
            _recursive = info.GetBoolean("_recursive");
            _enabled = info.GetBoolean("_enabled");
            UpdateFilepaths();
            assureFilesLoaded();
        }
        #endregion

        #region Private Methods
        public void UpdateFilepaths() {
            _filepaths = GetFiles(Path).Where(path=> _validFileTypes.Contains(System.IO.Path.GetExtension(path))).ToArray();
            if (_recursive) {
                List<string> subdirectories = new List<string>();
                subdirectories.AddRange(System.IO.Directory.GetDirectories(Path));
                while (subdirectories.Count > 0) {
                    string directory = subdirectories.First();
                    subdirectories.RemoveAt(0);
                    string[] newFiles = GetFiles(directory).Where(path=> _validFileTypes.Contains(System.IO.Path.GetExtension(path))).ToArray();
                    string[] newFilePaths = new string[newFiles.Length + _filepaths.Length];
                    Array.Copy(_filepaths, newFilePaths, _filepaths.Length);
                    Array.Copy(newFiles, 0, newFilePaths, _filepaths.Length, newFiles.Length);
                    _filepaths = newFilePaths;
                    subdirectories.AddRange(System.IO.Directory.GetDirectories(directory));
                }
            }

            HashSet<string> lookup = new HashSet<string>(_filepaths);
            foreach (KeyValuePair<string, Item> keyValuePair in new Dictionary<string, Item>(_items)) {
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

        private void assureFilesLoaded() {
            foreach (Item item in this) { //Iteration when everything is loaded is a very cheap action, but even so, consider improving
            }
        }

        #endregion

        #region Public Methods
        public void DownloadTo(string url) {
            WebClient client = new WebClient();
            string filename = System.IO.Path.GetFileName(url);
            if (!String.IsNullOrWhiteSpace(filename)) {
                filename = System.IO.Path.Combine(this.Path, filename);
            }
            else {
                filename = System.IO.Path.Combine(this.Path, DateTime.Now.Ticks.ToString());
            }
            client.DownloadFile(url, filename);
            Item newImage = Load.Image(filename);
            newImage.ExternalUrl = url;
        }
        #endregion

        #region Enumeration

        private int _position = -1;
        public IEnumerator GetEnumerator() {
            _position = -1;
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public bool MoveNext() {
            if (++_position < _filepaths.Length) {
                return true;
            }
            return false;
        }

        public void Reset() {
            _position = -1;
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

        public override bool Equals(object obj) {
            Directory otherDir = obj as Directory;
            if (otherDir == null) {
                return base.Equals(obj);
            }
            return otherDir.Path == Path;
        }
        public override string ToString() {
            return @"""{0}"" - {1} item{2}".With(Path, _filepaths.Count(), _filepaths.Count()==1 ? "" : "s");
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("Path", Path);
            info.AddValue("_validFileTypes", _validFileTypes);
            info.AddValue("_sortPath", _sortPath);
            info.AddValue("_recursive", _recursive);
            info.AddValue("_enabled", _enabled);

        }
    }
}
