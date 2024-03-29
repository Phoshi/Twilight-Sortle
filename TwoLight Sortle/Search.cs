﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
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
        private SortState _sort;
        private bool _sortAscending;

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
        public Search(List<Item> allItems, string searchTerms, SearchState state, SortState sort = SortState.Filename, bool sortAscending = true) {
            _allItems = allItems;
            _search = searchTerms;
            _state = state;
            _results = new List<Item>();
            _sort = sort;
            _sortAscending = sortAscending;
            doSearch();
        }

        private void doSearch() {
            foreach (Item item in _allItems) {
                if (itemPassesSearch(item)) {
                    try {
                        //A little hacky, but if I do it here then it greatly simplifies background loading while still allowing me to add directories quickly
                        string filesize = item.Filesize;
                        Size size = item.Dimensions;
                    }
                    catch(Exception)
                    {
                        continue;
                    }
                    _results.Add(item);
                }
            }
            switch (_sort) {
                case SortState.Filename:
                    _results = _results.OrderBy(result => result.Filename).ToList();
                    break;
                case SortState.Dimensions:
                    _results = _results.OrderBy(result => result.Span).ToList();
                    break;
                case SortState.Directory:
                    _results = _results.OrderBy(result => result.Directory).ToList();
                    break;
                case SortState.External:
                    _results = _results.OrderBy(result => result.ExternalUrl).ToList();
                    break;
                case SortState.Filesize:
                    _results = _results.OrderBy(result => result.RawFilesize).ToList();
                    break;
                case SortState.Tags:
                    _results = _results.OrderBy(result => result.HasTags ? result.Tags.First().Name : "").OrderBy(result => result.Tags.Count()).ToList();
                    break;
            }

            if (!_sortAscending) {
                _results.Reverse();
            }

        }

        private bool regexIsBroken = false;
        private bool itemPassesSearch(Item item) {
            if ((_state & SearchState.Tagged) != SearchState.Tagged && item.HasTags) {
                return false;
            }
            if ((_state & SearchState.Untagged) != SearchState.Untagged && !item.HasTags) {
                return false;
            }
            if (string.IsNullOrWhiteSpace(_search)) {
                return true;
            }

            string searchTerm = item.Filename;
            bool caseSensitive = ((_state & SearchState.CaseSensitive) == SearchState.CaseSensitive);
            bool regex = ((_state & SearchState.Regex) == SearchState.Regex);
            bool tags = ((_state & SearchState.Tags) == SearchState.Tags);
            bool filenames = ((_state & SearchState.Filenames) == SearchState.Filenames);

            if (!caseSensitive) {
                searchTerm = searchTerm.ToLower();
            }
            bool passesFilename = false;
            bool passesTag = false;
            try {
                if (regexIsBroken || (filenames &&
                     (regex
                          ? Regex.IsMatch(searchTerm, _search,
                                          caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase)
                          : searchTerm.Contains(_search)))) {
                    passesFilename = true;
                }
            }
            catch (ArgumentException) {
                passesFilename = true;
                regexIsBroken = true;
            }
            if ((tags && item.Tags.Matches(_search, caseSensitive))) {
                passesTag = true;
            }

            return passesFilename || passesTag;
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

    enum SortState {
        Filename,
        Tags,
        Dimensions,
        Directory,
        Filesize,
        External,
    }
}
