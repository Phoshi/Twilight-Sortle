using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TwoLight_Sortle {
    static class TreeHelper {
        public static void createSortedTree(Item item, List<List<string>> frequentSets, Settings settings) {
            var combinations = getAllCombinations(item.Tags, 4);
            List<List<Tag>> validCombinations = new List<List<Tag>>();
            foreach (List<string> frequentSet in frequentSets) {
                foreach (List<Tag> combination in combinations) {
                    List<String> strCombination = (from tag in combination select tag.Name).ToList();
                    if (frequentSet.Intersect(strCombination).Count() == strCombination.Count) {
                        validCombinations.Add(combination);
                    }
                }
            }
            if (validCombinations.Count == 0) {
                var fakeCombination = new List<Tag>() { Tags.GetTag("Misc.") };
                combinations.Add(fakeCombination);
                validCombinations.Add(fakeCombination);
            }
            foreach (List<Tag> combination in combinations) {
                if (validCombinations.Contains(combination)) {
                    string rootSortPath = settings.GetDirectory(item.Directory).SortPath;
                    if (rootSortPath == null) {
                        continue;
                    }
                    string tagsPath = System.IO.Path.Combine((from tag in combination select tag.Name).ToArray());
                    string newPath = Path.Combine(rootSortPath, tagsPath, item.Filename + item.Extension);
                    item.Link(newPath);
                }
            }
        }

        private static List<List<Tag>> getAllCombinations(IEnumerable<Tag> rootList, int depth, List<Tag> remainingList = null) {
            if (remainingList == null) {
                remainingList = new List<Tag>();
            }
            List<List<Tag>> combinations = new List<List<Tag>>();
            if (remainingList.Count > 0) {
                combinations.Add(remainingList);
            }
            foreach (Tag tag in rootList) {
                List<Tag> newList = new List<Tag>(rootList);
                newList.Remove(tag);
                List<Tag> newRemaining = new List<Tag>(remainingList);
                newRemaining.Add(tag);
                if (newRemaining.Count < depth) {
                    combinations.AddRange(getAllCombinations(newList, depth, newRemaining));
                }
            }
            return combinations;
        }
    }
}
