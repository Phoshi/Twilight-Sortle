using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwoLight_Sortle;

namespace TwoLight_Sortle {
    /// <summary>
    /// Represents a tag an image can have
    /// </summary>
    class Tag {
        private string _name;

        public string Name {
            get { return _name; }
        }

        public int Count {
            get { throw new NotImplementedException(); }
        }

        public List<Item> Files {
            get { throw new NotImplementedException(); }
        }

        public Tag(string name) {
            _name = name;
        }

        public override string ToString() {
            return _name;
        }

    }
    static class Tags {
        private static Dictionary<string, Tag> _tags = new Dictionary<string, Tag>();
        public static Tag GetTag(string name) {
            if (!_tags.ContainsKey(name)) {
                _tags[name] = new Tag(name);
            }
            return _tags[name];
        }
    }
}
