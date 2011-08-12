using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TwoLight_Sortle;

namespace TwoLight_Sortle{
    /// <summary>
    /// Represents a tag an image can have
    /// </summary>
    [Serializable]
    class Tag : ISerializable{
        private string _name;
        private HashSet<Item> _items;

        public string Name {
            get { return _name; }
        }

        public int Count {
            get { return _items.Count; }
        }

        public List<Item> Files {
            get { return _items.ToList(); }
        }

        public void AddedImage(Item item) {
            _items.Add(item);
        }
        public void RemovedImage(Item item) {
            _items.Remove(item);
        }
        public Tag(string name) {
            _items = new HashSet<Item>();
            _name = name;
        }
        public Tag(SerializationInfo info, StreamingContext context) {
            _items = (HashSet<Item>) info.GetValue("_items", typeof(HashSet<Item>));
            _name = info.GetString("_name");
        }
        public override bool Equals(object obj) {
            Tag otherTag = obj as Tag;
            if (otherTag != null) {
                return otherTag._name == _name;
            }
            return base.Equals(obj);
        }
        public override int GetHashCode() {
            return _name.GetHashCode();
        }
        public override string ToString() {
            return _name;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("_name", _name);
            info.AddValue("_items", _items, _items.GetType());
        }
    }
    static class Tags {
        public static Dictionary<string, Tag> TagList = new Dictionary<string, Tag>();
        public static Tag GetTag(string name) {
            if (!TagList.ContainsKey(name)) {
                TagList[name] = new Tag(name);
            }
            return TagList[name];
        }
    }
}
