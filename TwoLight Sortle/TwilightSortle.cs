using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Extensions;

namespace TwoLight_Sortle {
    public partial class TwilightSortle : Form {
        private Search search;
        private SearchState searchOptions;
        private Directory dir;
        public TwilightSortle() {
            InitializeComponent();
            mainList.DoubleBuffer();
        }

        private void TwilightSortle_Load(object sender, EventArgs e) {
            dir = new Directory(@"C:\Users\Andrew\Pictures\Ponies");
            searchOptions = SearchState.Filenames | SearchState.Tags | SearchState.Regex;
            search = new Search(dir.Items, "", searchOptions);
            updateMainListing(search.Items);
        }

        private ListViewItem createEntry(Item image) {
            ListViewItem item = new ListViewItem(image.ToString());
            item.SubItems.Add(image.Filename);
            item.SubItems.Add(image.Directory);
            item.SubItems.Add(image.Filesize);
            item.SubItems.Add(image.Dimensions.ToString());
            return item;
        }

        private void updateMainListing(List<Item> newList) {
            int i = 0;
            mainList.BeginUpdate();
            for (; i < newList.Count(); i++) {
                if (i >= mainList.Items.Count || i >= newList.Count) {
                    break;
                }
                if (mainList.Items[i].Text == newList[i].ToString()) {
                    continue;
                }
                else {
                    if ((from item in newList select item.ToString()).Contains(mainList.Items[i].Text)) {
                        mainList.Items.Insert(i, createEntry(newList[i]));
                        i++;
                    }
                    else {
                        mainList.Items.RemoveAt(i);
                        i--;
                    }
                }
            }
            if (newList.Count > i) {
                for (; i < newList.Count; i++) {
                    mainList.Items.Add(createEntry(newList[i]));
                }
            }
            while (mainList.Items.Count > i) {
                mainList.Items.RemoveAt(mainList.Items.Count - 1);
            }
            mainList.EndUpdate();
        }

        private void searchBox_TextChanged(object sender, EventArgs e) {
            search = new Search(dir.Items, searchBox.Text, searchOptions);
            updateMainListing(search.Items);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            this.Close();
        }
    }
}
