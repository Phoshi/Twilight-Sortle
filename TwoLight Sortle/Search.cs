using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Extensions;

namespace TwoLight_Sortle {
    /// <summary>
    /// Represents a search object - actual searching is deferred.
    /// </summary>
    class Search : IEnumerable, IEnumerator{
        private string _search;
        private SearchState _state;

        private List<Item> _allItems;
        private List<Item> _results;

        private int _position;

        public List<Item> Items {
            get { return _results; }
        }
        /// <summary>
        /// Creates a new search on the item list supplied, with the specified options.
        /// Results on the IEnumberable.
        /// </summary>
        /// <param name="allItems">The list of items to search</param>
        /// <param name="searchTerms">The string to search on</param>
        /// <param name="state">Search options</param>
        public Search(List<Item> allItems, string searchTerms, SearchState state) {
            _allItems = allItems;
            _search = searchTerms;
            _state = state;
            _results = new List<Item>();
            doSearch();
        }

        private void doSearch() {
            foreach (Item item in _allItems) {
                if (itemPassesSearch(item)) {
                    _results.Add(item);
                }
            }
        }

        private bool itemPassesSearch(Item item) {
            if (string.IsNullOrWhiteSpace(_search)) {
                return true;
            }
            if ((_state & SearchState.Tagged) == SearchState.Tagged && item.Tags.Count() == 0) {
                return false;
            }
            if ((_state & SearchState.Untagged) == SearchState.Untagged && item.Tags.Count() > 0) {
                return false;
            }
            bool regex, caseSensitive, tags, filenames;
            string searchTerm = item.Filename;
            caseSensitive = ((_state & SearchState.CaseSensitive) == SearchState.CaseSensitive);
            regex = ((_state & SearchState.Regex) == SearchState.Regex);
            tags = ((_state & SearchState.Tags) == SearchState.Tags);
            filenames = ((_state & SearchState.Filenames) == SearchState.Filenames);
            if (!caseSensitive) {
                searchTerm = searchTerm.ToLower();
            }
            if (!(filenames && (regex ? Regex.IsMatch(searchTerm, _search, caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase) : searchTerm.Contains(_search)))) {
                return false;
            }
            if (item.Tags.Count() > 0 && !(tags && item.Tags.Matches(searchTerm, caseSensitive))) {
                return false;
            }

            return true;
        }

        public IEnumerator GetEnumerator() {
            return this;
        }

        public bool MoveNext() {
            _position++;
            if (_position >= _results.Count) {
                return false;
            }
            return true;
        }

        public void Reset() {
            _position = 0;
        }

        public object Current {
            get { return _results[_position]; }
        }
    }

    [Flags]
    enum SearchState {
        Tags = 1,
        Filenames = 1 << 1,
        CaseSensitive = 1 << 2,
        Regex = 1 << 3,
        Tagged = 1 << 4,
        Untagged = 1 << 5
    }
}
