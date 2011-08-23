using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Extensions {
    public static class listBoxExtensions {

        /// <summary>
        /// Scrolls a list view up or down 
        /// </summary>
        /// <param name="listView">The list view to scroll</param>
        /// <param name="howMuch">How much to scroll. Use negative values to go up.</param>
        public static void Scroll(this ListView listView, int howMuch) {
            int selectedIndex;
            if (listView.SelectedIndices.Count > 0) {
                selectedIndex = listView.SelectedIndices[listView.SelectedIndices.Count - 1];
                listView.Items[selectedIndex].Selected = false;
            }
            else {
                selectedIndex = howMuch > 0 ? -1 : listView.Items.Count;
            }
            if (selectedIndex + howMuch < listView.Items.Count && selectedIndex + howMuch >= 0) {
                listView.Items[selectedIndex + howMuch].Selected = true;
                listView.Items[selectedIndex + howMuch].EnsureVisible();
            }
        }

        /// <summary>
        /// Scrolls a list box up or down 
        /// </summary>
        /// <param name="listBox">The list view to scroll</param>
        /// <param name="howMuch">How much to scroll. Use negative values to go up.</param>
        public static void Scroll(this ListBox listBox, int howMuch) {
            int selectedIndex;
            if (listBox.SelectedIndices.Count > 0) {
                selectedIndex = listBox.SelectedIndices[listBox.SelectedIndices.Count - 1];
            }
            else {
                selectedIndex = howMuch > 0 ? -1 : listBox.Items.Count;
            }
            if (selectedIndex + howMuch < listBox.Items.Count && selectedIndex + howMuch >= 0) {
                listBox.SelectedIndex = selectedIndex + howMuch;
            }
        }
    }
}
