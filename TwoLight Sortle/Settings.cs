using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace TwoLight_Sortle{
    [Serializable]
    class Settings : ISerializable{
        private List<Directory> _directories;
        private SearchState _searchState;

        public List<Directory> Directories {
            get { return _directories; }
        }

        public List<Directory> EnabledDirectories {
            get { return (from dir in _directories where dir.Enabled select dir).ToList(); }
        }

        public List<Tag> Tags {
            get {
                var tags = from dir in _directories select dir.Tags;
                return tags.SelectMany(tag => tag).Distinct().ToList();
            }
        }

        public SearchState SearchOptions {
            get { return _searchState; }
            set { _searchState = value; }
        }

        public Settings() {
            _directories = new List<Directory>();
            _searchState = SearchState.Tagged | SearchState.Untagged | SearchState.Tags | SearchState.Filenames;
        }
        public Settings(SerializationInfo info, StreamingContext context) {
            _directories = (List<Directory>) info.GetValue("_directories", typeof(List<Directory>));
            _searchState = (SearchState) info.GetValue("_searchState", typeof (SearchState));
        }

        public void AddDirectory(string path) {
            Directory newDirectory = new Directory(path);
            if (!_directories.Contains(newDirectory)) {
                _directories.Add(newDirectory);
            }
        }
        public Directory GetDirectory(string path) {
            return Directories.FirstOrDefault(directory => directory.Path == path);
        }

        public void RemoveDirectory(Directory directory) {
            _directories.Remove(directory);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("_directories", _directories);
            info.AddValue("_searchState", _searchState);
        }
    }
}
