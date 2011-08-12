using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwoLight_Sortle;

namespace Extensions {
    static class listExtensions {
        public static bool Matches(this IEnumerable<Tag> self, string search, bool caseSensitive = false) {
            string[] tagSearches = search.Split(' ');
            int numMatches = 0;
            foreach (Tag tag in self) {
                foreach (string searchTag in tagSearches) {
                    if ((caseSensitive ? tag.Name : tag.Name.ToLower()).Contains(searchTag)) {
                        numMatches++;
                    }
                }
            }
            if (numMatches >= tagSearches.Count()) {
                return true;
            }
            return false;
        }
    }
}
