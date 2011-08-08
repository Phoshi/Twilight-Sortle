using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwoLight_Sortle;

namespace Extensions {
    static class listExtensions {
        public static bool Matches(this IEnumerable<Tag> self, string search, bool caseSensitive = false) {
            string[] tagSearches = search.Split(' ');
            foreach (Tag tag in self) {
                foreach (string searchTag in tagSearches) {
                    if ((caseSensitive ? tag.Name : tag.Name.ToLower()).Contains(searchTag)) {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
