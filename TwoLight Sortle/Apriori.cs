using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Apriori {
    class Apriori {
        private Dictionary<string, List<string>> data;
        private int minimum;
        public List<List<string>> freqSets;
        public Apriori(Dictionary<string, List<string>> newData, int min = 10) {
            data = newData;
            minimum = min;
        }

        private List<string> singlise(string toSingleOut) {
            List<string> singular = new List<string>();
            singular.Add(toSingleOut);
            return singular;
        }

        public List<List<string>> getFrequentSets() {
            HashSet<string> tags = new HashSet<string>();
            foreach (KeyValuePair<string, List<string>> item in data) {
                foreach (string tag in item.Value) {
                    tags.Add(tag);
                }
            }

            HashSet<List<string>> itemsets = new HashSet<List<string>>((from entry in tags select singlise(entry)));
            List<List<string>> frequentSets = new List<List<string>>();

            while (itemsets.Count > 0) {
                List<List<string>> frequentTemp = new List<List<string>>();
                Dictionary<List<string>, int> count = new Dictionary<List<string>, int>();

                foreach (KeyValuePair<string, List<string>> dataRow in data) {
                    foreach (List<string> itemset in itemsets) {
                        int match = itemset.Count(item => dataRow.Value.Contains(item));
                        if (match == itemset.Count) {
                            count[itemset] = count.GetValueOrDefault(itemset, 0) + 1;
                            if (count[itemset] == minimum) {
                                frequentTemp.Add(itemset);
                            }
                        }
                    }
                }

                frequentSets.AddRange(frequentTemp);
                itemsets = new HashSet<List<string>>(new StringListComparer());
                
                for (int i = 0; i < frequentTemp.Count; i++) {
                    for (int j = i + 1; j < frequentTemp.Count; j++) {
                        List<string> combined = frequentTemp[i].Union(frequentTemp[j]).OrderBy(item => item).ToList();
                        if (combined.Count == frequentTemp[i].Count + 1) {
                            itemsets.Add(combined);
                        }
                    }
                }
            }

            return frequentSets;
        }

        public Dictionary<List<string>[], double> generateStrongRules(List<List<string>> frequentItems) {
            Dictionary<List<string>[], double> confidences = new Dictionary<List<string>[], double>();
            Dictionary<List<string>[], double> supports = new Dictionary<List<string>[], double>();
            foreach (List<string> frequentItem in frequentItems) {
                var subsets = from m in Enumerable.Range(0, 1 << frequentItem.Count)
                              select
                                  from i in Enumerable.Range(0, frequentItem.Count)
                                  where (m & (1 << i)) != 0
                                  select frequentItem[i];
                foreach (IEnumerable<string> enumerable in subsets.Where(subset => subset.Count()!=0).Where(subset => subset.Count()!=frequentItem.Count)) {
                    IEnumerable<string> counterpart = new List<string>(frequentItem).Where(item => !enumerable.Contains(item));
                    double confidence = (double)getNumberOfOccurancesOfSet(frequentItem, frequentItems) /
                                        getNumberOfOccurancesOfSet(enumerable, frequentItems);
                    double support = (double)getNumberOfOccurancesOfSet(frequentItem, frequentItems) / data.Count;
                    List<string>[] key = new List<string>[2];
                    key[0] = enumerable.ToList();
                    key[1] = counterpart.ToList();
                    confidences[key] = confidence;
                    supports[key] = support;
                }
            }
            double totalSupport = 0;
            foreach (double value in supports.Values) {
                totalSupport += value;
            }
            double averageSupport = totalSupport / supports.Count;
            Dictionary<List<string>[], double> returnConfidences = new Dictionary<List<string>[], double>();
            foreach (KeyValuePair<List<string>[], double> keyValuePair in confidences) {
                if (supports[keyValuePair.Key] > averageSupport*0.8f) {
                    returnConfidences[keyValuePair.Key] = keyValuePair.Value;
                }
            }
            return returnConfidences;
        }

        public int getNumberOfOccurancesOfSet(IEnumerable<string> set, List<List<string>> frequentSets) {
            int occurances = 0;
            foreach (List<string> frequentSet in data.Values) {
                if (frequentSet.Intersect(set).Count() == set.Count()) {
                    occurances++;
                }
            }
            return occurances;
        }

        class StringListComparer : IEqualityComparer<List<string>> {
            public bool Equals(List<string> list1, List<string> list2) {
                bool result = list1.SequenceEqual(list2);
                return result;
            }

            public int GetHashCode(List<string> list) {
                int hash = 0;
                int i = 0;
                foreach (string item in list) {
                    hash ^= item.GetHashCode() + i;
                    i++;
                }
                return hash;
            }
        }

    }
    static class extensionMethods {
        public static value GetValueOrDefault<key, value>(this Dictionary<key, value> dict, key Key, value defaultValue) {
            value ret;
            bool found = dict.TryGetValue(Key, out ret);
            if (found) { return ret; }
            return defaultValue;
        }
    }
}
