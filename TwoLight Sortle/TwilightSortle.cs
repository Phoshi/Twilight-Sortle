using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Extensions;

namespace TwoLight_Sortle {
    public partial class TwilightSortle : Form {
        #region Private Form Variables
        private Search search;
        private List<Item> itemList = new List<Item>();
        private Thread detailsUpdateThread;
        private BackgroundWorker workingThread = new BackgroundWorker();
        private bool showingTagPanel;
        private bool showingPreferencesPanel;
        private Dictionary<object, Action> actions = new Dictionary<object, Action>();
        private Settings settings;
        private bool startupComplete;
        private bool saving = true;
        #endregion

        public TwilightSortle() {
            InitializeComponent();
            setupActions();
            mainList.DoubleBuffer();
            fileActionPanel.DoubleBuffer();
            TwoLight_Sortle.Load.FilesCache =
                TwoLight_Sortle.Load.FilesCache.LoadFromDisk<Dictionary<UInt32, Item>>(
                    Path.Combine(Application.UserAppDataPath, "FilesCache"));
            IEnumerable<Item> deletedItems = from kvPair in TwoLight_Sortle.Load.FilesCache
                                      where !kvPair.Value.Exists
                                      select kvPair.Value;
            foreach (Item item in deletedItems) {
                item.Delete();
            }
            TwoLight_Sortle.Load.FilesCache =
                TwoLight_Sortle.Load.FilesCache.Where(kvPair => kvPair.Value.Exists).ToDictionary(kvPair => kvPair.Key,
                                                                                                  kvPair => kvPair.Value);
            Tags.TagList = Tags.TagList.LoadFromDisk<Dictionary<string, Tag>>(Path.Combine(
                Application.UserAppDataPath, "Tags"));
            if (isAdmin()) {
                this.Text = "{0} (Administrator)".With(this.Text);
            }
        }

        private bool isAdmin() {
            WindowsIdentity wi = WindowsIdentity.GetCurrent();
            WindowsPrincipal wp = new WindowsPrincipal(wi);
            return wp.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void restartWithAdminRights() {
            MessageBox.Show(
                "This action requires administrative rights. Twilight Sortle will now attempt to restart with the required rights.");
            try {
                save();
                saving = false;
                Process.Start(new ProcessStartInfo() {
                    Verb = "runas",
                    FileName = Application.ExecutablePath
                });
            }
            catch (Exception) {
                return;
            }
            this.Close();
        }

        private void setupActions() {
            //Buttons
            actions[addTagButton] = toggleAddTagPanel;
            actions[clearFilterButton] = (() => searchBox.Text = "");
            actions[removeTagButton] = removeSelectedTag;
            actions[deleteFileToolStripMenuItem] = deleteSelectedFiles;
            actions[preferencesSaveButton] = togglePreferences;
            actions[preferencesEnabledCheck] = (() => {
                                                    int index = preferencesGetSelectedDirectory();
                                                    if (index > -1) {
                                                        Directory dir = settings.Directories.ElementAt(index);
                                                        dir.Enabled = !dir.Enabled;
                                                    }
                                                });
            actions[preferencesRecursiveCheck] = (() => {
                                                      int index = preferencesGetSelectedDirectory();
                                                      if (index > -1) {
                                                          Directory dir = settings.Directories.ElementAt(index);
                                                          dir.Recursive = !dir.Recursive;
                                                      }
                                                  });
            actions[moveDirUpButton] = (() => {
                                            int getSelectedDirectory = preferencesGetSelectedDirectory();
                                            if (getSelectedDirectory > 0) {
                                                Directory tempDir = settings.Directories.ElementAt(getSelectedDirectory);
                                                settings.Directories[getSelectedDirectory] =
                                                    settings.Directories[getSelectedDirectory - 1];
                                                settings.Directories[getSelectedDirectory - 1] = tempDir;
                                            }
                                            rebuildDirectoriesList();
                                        });

            actions[moveDirDownButton] = (() => {
                                              int getSelectedDirectory = preferencesGetSelectedDirectory();
                                              int numDirectories = settings.Directories.Count;
                                              if (getSelectedDirectory < numDirectories - 1) {
                                                  Directory tempDir =
                                                      settings.Directories.ElementAt(getSelectedDirectory);
                                                  settings.Directories[getSelectedDirectory] =
                                                      settings.Directories[getSelectedDirectory + 1];
                                                  settings.Directories[getSelectedDirectory + 1] = tempDir;
                                              }
                                              rebuildDirectoriesList();
                                          });

            //Menu Items
            actions[exitToolStripMenuItem] = Close;
            actions[buildTagDatabaseFilenamesToolStripMenuItem] = (() => buildTagDatabaseFromFileNames(itemList));
            actions[byTagToolStripMenuItem] = renameAllVisibleItemsByTags;
            actions[byHashToolStripMenuItem] = renameAllVisibleItemsByHash;
            actions[addTagToolStripMenuItem] = toggleAddTagPanel;
            actions[removeToolStripMenuItem] = removeSelectedTag;
            actions[preferencesToolStripMenuItem] = togglePreferences;
            actions[buildToolStripMenuItem] = updateSortedTree;
            actions[removeToolStripMenuItem] = removeSortedTree;
            actions[imgurToolStripMenuItem] = uploadImages;
            actions[setAsWallpaperToolStripMenuItem] = setSelectedAsWallpaper;
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SystemParametersInfo(
            UInt32 action, UInt32 uParam, String vParam, UInt32 winIni);

        private const UInt32 SPI_SETDESKWALLPAPER = 0x14;
        private const UInt32 SPIF_UPDATEINIFILE = 0x01;
        private const UInt32 SPIF_SENDWININICHANGE = 0x02;

        private void setSelectedAsWallpaper() {
            if (getSelectedItems().Count == 0) {
                return;
            }
            string path = getSelectedItems()[0].Path;
            string newPath = Path.GetTempPath();
            string oldName = Path.GetFileNameWithoutExtension(path);
            string newName = Path.Combine(newPath, oldName + ".bmp");
            Image image = Image.FromFile(path);
            image.Save(newName, ImageFormat.Bmp);
            image.Dispose();
            int result = SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, newName,
                                              SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }

        private void uploadImages() {
            List<Item> selectedItems = getSelectedItems();
            List<string> urls = new List<string>();
            Action action = (() => {
                                 foreach (Item selectedItem in selectedItems) {
                                     urls.Add(selectedItem.UploadedUrl);
                                 }
                             });
            Action after = (() => Clipboard.SetText(String.Join(" ", urls)));
            runLongOperation(action, after, "Uploading {0} files".With(selectedItems.Count));
        }

        private void removeSortedTree() {
            Action action = (() => {
                                 foreach (Directory enabledDirectory in settings.EnabledDirectories) {
                                     if (enabledDirectory.SortPath != null) {
                                         try {
                                             System.IO.Directory.Delete(enabledDirectory.SortPath, true);
                                         }
                                         finally {
                                             enabledDirectory.Items.ForEach(item => item.Invalidated = true);
                                         }
                                     }
                                 }
                             });
            Action after = refreshListingWithSearch;
            runLongOperation(action, after, "Removing sorted tree");
        }

        private void deleteSelectedFiles() {
            List<Item> items = getSelectedItems();
            items.ForEach(item => item.Delete());
            items.ForEach(item=>settings.GetDirectory(item.Directory).UpdateFilepaths());
            refreshListingWithSearch();
        }

        private void updateSearchOptionsMenuCheckedStatus() {
            searchOptions_Click(new object(), new EventArgs());
        }

        private void renameAllVisibleItemsByHash() {
            runLongOperation((() => {
                foreach (Item item in itemList) {
                    item.RenameToHash();
                }
            }), (() => updateMainListing(itemList, true)), "Renaming By Hash");
        }

        private void renameAllVisibleItemsByTags() {
            runLongOperation((() => {
                foreach (Item item in itemList) {
                    item.RenameToTags();
                }
            }), (() => updateMainListing(itemList, true)), "Renaming By Tag");
        }

        private void buildTagDatabaseFromFileNames(List<Item> items) {
            foreach (Item item in items) {
                if (!item.HasTags) {
                    string filename = item.Filename;
                    List<string> tags = filename.Split(' ').ToList();
                    tags.ForEach(tag => item.Add(tag));
                }
            }
        }

        private void removeSelectedTag() {
            runLongOperation(null,
                             (() => {
                                 if (getSelectedTag() != null && getSelectedItems().Count > 0) {
                                     getSelectedItems().ForEach(item => item.Remove(getSelectedTag()));
                                     updateTagList(getSelectedItems().First());
                                     updateMainListing(itemList, true);
                                 }
                              })
                             , "Removing Selected Tag");
        }

        private string getSelectedTag() {
            if (tagList.SelectedItems.Count > 0) {
                return tagList.SelectedItems[0].Text;
            }
            return null;
        }

        private List<Item> allEnabledItems() {
            return
                (from directory in settings.EnabledDirectories select directory.Items).SelectMany(item => item).ToList();
        }

        private void runLongOperation(Action operation, Action callback = null, string reason = "") {
            workingThread = new BackgroundWorker();
            workingThread.DoWork += (((object sender, DoWorkEventArgs e) => {
                                          if (operation != null) {
                                              operation();
                                          }
                                          workingPanel.Invoke((Action) ((() => updateWorkingPanelPosition())));
                                      }));
            if (callback != null) {
                workingThread.RunWorkerCompleted += ((object sender, RunWorkerCompletedEventArgs e) => callback());
            }
            workingThread.RunWorkerCompleted += ((sender, e) => updateWorkingPanelPosition());
            workingThread.RunWorkerAsync();
            updateWorkingPanelPosition(reason);
        }

        private void toggleAddTagPanel() {
            showingTagPanel = !showingTagPanel;
            updateTagPanelPosition();
            if (showingTagPanel) {
                splitListDetails.Enabled = false;
                addTagFilter.Select();
            }
            else {
                splitListDetails.Enabled = true;
            }
        }


        private ListViewItem createEntry(Item image) {
            ListViewItem item = new ListViewItem(image.ToString());
            item.SubItems.Add(image.Filename);
            item.SubItems.Add(image.Directory);
            item.SubItems.Add(image.Filesize);
            item.SubItems.Add("{0}x{1}".With(image.Dimensions.Width, image.Dimensions.Height));
            item.SubItems.Add(image.ExternalUrl);
            return item;
        }

        private void updateMainListing(List<Item> newList, bool scrapIt = false) {
            int i = 0;
            itemList = newList;
            mainList.Invoke((Action)mainList.BeginUpdate);
            if (scrapIt) {
                mainList.Invoke((Action)mainList.Items.Clear);
            }
            for (; i < newList.Count(); i++) {
                if (i >= mainList.Items.Count || i >= newList.Count) {
                    break;
                }
                if (mainList.Items[i].Text == newList[i].ToString()) {
                    mainList.Invoke((Action)(() => mainList.Items[i].SubItems[1].Text = newList[i].Filename));
                    continue;
                }
                else {
                    if ((from item in newList select item.ToString()).Contains(mainList.Items[i].Text)) {
                        mainList.Invoke((Action)(() => mainList.Items.Insert(i, createEntry(newList[i]))));
                        i++;
                    }
                    else {
                        mainList.Invoke((Action)(() => mainList.Items.RemoveAt(i)));
                        i--;
                    }
                }
            }
            if (newList.Count > i) {
                for (; i < newList.Count; i++) {
                    mainList.Invoke((Action)(() => mainList.Items.Add(createEntry(newList[i]))));
                }
            }
            while (mainList.Items.Count > i) {
                mainList.Invoke((Action)(() => mainList.Items.RemoveAt(mainList.Items.Count - 1)));
            }

            bool colourSwitch = false;
            foreach (ListViewItem item in mainList.Items) {
                if (!itemList[item.Index].Invalidated) {
                    item.BackColor = colourSwitch ? Color.White : Color.FromArgb(240, 240, 240);
                }
                else {
                    item.BackColor = Color.Gray;
                    item.ForeColor = Color.White;
                }
                colourSwitch = !colourSwitch;
            }

            mainList.Invoke((Action)(() => mainList.EndUpdate()));
            listIndexLabel.Invoke((Action)updateIndexLabel);
        }

        private void updateIndexLabel() {
            int selectedIndex = 1;
            if (mainList.SelectedIndices.Count > 0) {
                selectedIndex = mainList.SelectedIndices[0] + 1;
            }
            listIndexLabel.Text = "{0}/{1}".With(selectedIndex, itemList.Count);
        }

        private List<Item> getSelectedItems() {
            return (from int selectedIndex in mainList.SelectedIndices select itemList[selectedIndex]).ToList();
        }

        private void updatePreviewImage(int selectedIndex) {
            if (detailsUpdateThread != null) {
                detailsUpdateThread.Abort();
            }
            Action updateDetails = (() => {
                Item selectedItem = itemList[selectedIndex];
                updateTagList(selectedItem);
                previewImage.Image = selectedItem.Image;
            });
            detailsUpdateThread = new Thread((() => updateDetails()));
            detailsUpdateThread.Start();
        }

        private void refreshListingWithSearch() {
            search = new Search(allEnabledItems(), searchBox.Text, settings.SearchOptions);
            updateMainListing(search.Items, true);
        }

        private void save() {
            if (saving) {
                TwoLight_Sortle.Load.FilesCache.SaveToDisk(Path.Combine(Application.UserAppDataPath, "FilesCache"));
                TwoLight_Sortle.Tags.TagList.SaveToDisk(Path.Combine(Application.UserAppDataPath, "Tags"));
                settings.SaveToDisk(Path.Combine(Application.UserAppDataPath, "Settings"));
            }
        }

        #region Working Panel
        private void updateWorkingPanelPosition(string reason = "") {
            if (workingThread.IsBusy) {
                workingPanel.Visible = true;
                workingPanel.Left = (this.Width / 2) - (workingPanel.Width / 2);
                workingPanel.Top = (this.Height / 2) - (workingPanel.Height / 2);
                splitListDetails.Enabled = false;
                preferencesPanel.Enabled = false;
                addTagPanel.Enabled = false;
                workingLabel.Left = workingPanel.Width / 2 - workingLabel.Text.Width(workingLabel.Font) / 2;
                if (reason != "") {
                    workingDescription.Text = reason;
                    workingDescription.Left = (workingPanel.Width / 2 - reason.Width(workingDescription.Font) / 2);
                }
            }
            else {
                workingPanel.Visible = false;
                splitListDetails.Enabled = true;
                preferencesPanel.Enabled = true;
                addTagPanel.Enabled = true;

            }
        }
        #endregion

        #region File Sorting
        private void updateSortedTree() {
            if (!isAdmin()) {
                restartWithAdminRights();
            }
            Action action = (() => {
                                 Dictionary<string, List<string>> aprioriData = new Dictionary<string, List<string>>();
                                 foreach (Item item in itemList) {
                                     aprioriData[item.Path] = (from tag in item.Tags select tag.Name).ToList();
                                 }
                                 Apriori.Apriori apriori = new Apriori.Apriori(aprioriData, 5);
                                 List<List<string>> frequentSets = apriori.getFrequentSets();
                                 var filesToSort = from file in itemList where file.Invalidated select file;
                                 foreach (Item file in filesToSort) {
                                     createSortedTree(file, frequentSets);
                                 }
                             });
            Action after = refreshListingWithSearch;
            runLongOperation(action, after, "Updating Sorted Tree");
        }

        private void createSortedTree(Item item, List<List<string>> frequentSets) {
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
                var fakeCombination = new List<Tag>() {Tags.GetTag("Misc.")};
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

        private List<List<Tag>> getAllCombinations(IEnumerable<Tag> rootList, int depth, List<Tag> remainingList = null) {
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
        #endregion

        #region Preferences Panel
        private void rebuildDirectoriesList() {
            preferencesDirectoryList.Items.Clear();
            foreach (Directory directory in settings.Directories) {
                ListViewItem newItem = new ListViewItem(directory.ToString());
                newItem.ForeColor = directory.Enabled ? Color.Black : Color.Gray;
                preferencesDirectoryList.Items.Add(newItem);
            }
            preferencesDirectoryList.Items.Add("New Directory...");
            preferencesSaveButton.Enabled = settings.Directories.Count > 0;
        }
        private void updatePreferencesSettingsPanel(Directory dir) {
            preferencesValidFileTypesList.Items.Clear();
            preferencesControls.Enabled = true;
            preferencesControls.Text = dir.Path;
            foreach (string validFileType in dir.ValidFileTypes) {
                preferencesValidFileTypesList.Items.Add(validFileType);
            }
            preferencesSortPath.Text = dir.SortPath;
            preferencesRecursiveCheck.Checked = dir.Recursive;
            preferencesEnabledCheck.Checked = dir.Enabled;
        }
        private void updatePreferencesPanel() {
            if (settings.Directories.Count == 0 && startupComplete) {
                showingPreferencesPanel = true;
            }
            if (showingPreferencesPanel) {
                splitListDetails.Enabled = false;
                preferencesPanel.Height = this.ClientRectangle.Height - menuStrip1.Height;
                preferencesPanel.Left = 0;
                preferencesPanel.Top = menuStrip1.Height;
                preferencesPanel.Visible = true;
                rebuildDirectoriesList();
                int index = preferencesGetSelectedDirectory();
                if (index == -1) {
                    index = 0;
                }
                if (settings.Directories.Count > 0) {
                    updatePreferencesSettingsPanel(settings.Directories.ElementAt(index));
                }
                else {
                    preferencesSaveButton.Enabled = false;
                }
            }
            else {
                splitListDetails.Enabled = true;
                preferencesPanel.Visible = false;
            }
        }

        private int preferencesGetSelectedDirectory() {
            if (preferencesDirectoryList.SelectedIndices.Count > 0) {
                int index = preferencesDirectoryList.SelectedIndices[0];
                if (index >= settings.Directories.Count) {
                    return -1;
                }
                return index;
            }
            return -1;
        }

        private void togglePreferences() {
            showingPreferencesPanel = !showingPreferencesPanel;
            updatePreferencesPanel();
            runLongOperation((() => search = new Search(allEnabledItems(), searchBox.Text, settings.SearchOptions)),
                             (() => updateMainListing(search.Items, true)), "Updating Directory Cache");
        }

        private void preferencesDirectoryList_SelectedIndexChanged(object sender, EventArgs e) {
            int index = preferencesGetSelectedDirectory();
            if (index > -1 && index < settings.Directories.Count) {
                updatePreferencesSettingsPanel(settings.Directories.ElementAt(index));
            }
        }

        private void preferencesDirectoryList_DoubleClick(object sender, EventArgs e) {
            Point point = preferencesDirectoryList.PointToClient(MousePosition);
            ListViewItem item = preferencesDirectoryList.GetItemAt(point.X, point.Y);
            if (item == null) {
                return;
            }
            if (item.Index == preferencesDirectoryList.Items.Count - 1) {
                if (folderBrowser.ShowDialog() == DialogResult.OK) {
                    runLongOperation((()=>settings.AddDirectory(folderBrowser.SelectedPath)), rebuildDirectoriesList, "Creating Directory Cache");
                }
            }
            else {
                settings.RemoveDirectory(settings.Directories.ElementAt(item.Index));
                rebuildDirectoriesList();
            }

        }

        #endregion

        #region Tag Panel

        private bool _tabCompleting;
        private void addTagFilter_TextChanged(object sender, EventArgs e) {
            if (!_tabCompleting) {
                string[] lastWords = addTagFilter.Text.Split(' ');
                updateTagAutocompleteList(lastWords.Last(), addTagFilter.Text, lastWords.Length < 1);
                _tagCompletePosition = -1;
            }
        }

        private void updateTagAutocompleteList(string partial, string existingTags, bool showAllTags = false) {
            var tags = (from tag in settings.Tags orderby tag.Count descending select tag);
            var oldTags = existingTags.Split(' ');
            addTagList.BeginUpdate();
            addTagList.Items.Clear();
            foreach (Tag tag in tags) {
                if ((showAllTags || tag.Name.Contains(partial)) && !oldTags.Contains(tag.Name)) {
                    addTagList.Items.Add(tag);
                }
            }
            addTagList.EndUpdate();
        }

        private int _tagCompletePosition = -1;
        private void addTagFilter_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Tab) {
                if (addTagList.Items.Count <= 0) {
                    return;
                }
                if (!e.Shift) {
                    if (_tagCompletePosition < addTagList.Items.Count - 1) {
                        _tagCompletePosition++;
                    }
                    else {
                        _tagCompletePosition = 0;
                    }
                }
                else {
                    _tagCompletePosition--;
                    if (_tagCompletePosition < 0) {
                        _tagCompletePosition = addTagList.Items.Count - 1;
                    }
                }
                string newTag = addTagList.Items[_tagCompletePosition].ToString();
                string newTagFilterInput = "";
                if (addTagFilter.Text.Contains(" ")) {
                    newTagFilterInput = addTagFilter.Text.Substring(0, addTagFilter.Text.LastIndexOf(' ')) + " ";
                }
                _tabCompleting = true;
                addTagFilter.Text = "{0}{1}".With(newTagFilterInput, newTag);
                addTagFilter.Select(addTagFilter.Text.Length, 0);
                _tabCompleting = false;
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Return) {
                List<string> newTags = addTagFilter.Text.Split(' ').ToList();
                getSelectedItems().ForEach(item => newTags.ForEach(item.Add));
                updateTagList(getSelectedItems()[0]);
                refreshListingWithSearch();
                toggleAddTagPanel();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape) {
                toggleAddTagPanel();
                e.SuppressKeyPress = e.Handled = true;
            }
        }


        private void updateTagList(Item selectedItem) {
            tagList.Invoke((Action)(() => tagList.Items.Clear()));
            foreach (Tag tag in selectedItem.Tags) {
                ListViewItem newItem = new ListViewItem(tag.Name);
                newItem.SubItems.Add(tag.Count.ToString());
                tagList.Invoke((Action)(() => tagList.Items.Add(newItem)));
            }
        }

        private void updateTagPanelPosition() {
            if (!showingTagPanel) {
                addTagPanel.Top = this.Bottom;
                _tagCompletePosition = -1;
                addTagFilter.Text = "";
            }
            else {
                addTagPanel.Top = this.Bottom - addTagPanel.Height;
                addTagFilter.Select();
            }
            updateTagAutocompleteList(addTagFilter.Text, "");
            addTagPanel.Width = this.Width;
            addTagPanel.Left = 0;
        }

        #endregion

        #region Form Event Handlers
        private void TwilightSortle_Load(object sender, EventArgs e) {
            settings = new Settings();
            Action action = (() => {
                                 settings =
                                     settings.LoadFromDisk<Settings>(Path.Combine(Application.UserAppDataPath,
                                                                                  "Settings"));
                                 search = new Search(allEnabledItems(), "", settings.SearchOptions);
                                 startupComplete = true;
                             });
            Action callback = (() => { updateMainListing(search.Items); updateSearchOptionsMenuCheckedStatus(); updatePreferencesPanel();});
            runLongOperation(action, callback, "Updating directory cache");
            updateTagPanelPosition();
        }

        private void TwilightSortle_FormClosing(object sender, FormClosingEventArgs e) {
            save();
        }

        private void TwilightSortle_Resize(object sender, EventArgs e) {
            updateTagPanelPosition();
            updatePreferencesPanel();
            updateFileActionsPanel();
            updateWorkingPanelPosition();
        }
        #endregion

        #region Control Event Handlers
        private void searchBox_TextChanged(object sender, EventArgs e) {
            refreshListingWithSearch();
        }

        private void mainList_SelectedIndexChanged(object sender, EventArgs e) {
            if (mainList.SelectedIndices.Count == 1) {
                updatePreviewImage(mainList.SelectedIndices[0]);
                updateIndexLabel();
            }
        }

        private bool resizing;
        private void mainList_SizeChanged(object sender, EventArgs e) {
            if (!resizing) {
                resizing = true;
                ListView view = sender as ListView;
                if (view != null) {
                    float totalWidth = 0;
                    for (int i = 0; i < view.Columns.Count; i++) {
                        totalWidth += Convert.ToInt32(view.Columns[i].Tag);
                    }

                    for (int i = 0; i < view.Columns.Count; i++) {
                        float percentage = Convert.ToInt32(view.Columns[i].Tag) / totalWidth;
                        view.Columns[i].Width = (int)(view.ClientRectangle.Width * percentage);
                    }
                }
            }

            resizing = false;
        }


        private void preferencesSortPath_Enter(object sender, EventArgs e) {
            if (folderBrowser.ShowDialog() == DialogResult.OK) {
                preferencesSortPath.Text = folderBrowser.SelectedPath;
                settings.Directories.ElementAt(preferencesGetSelectedDirectory()).SortPath = folderBrowser.SelectedPath;
            }
        }

        #endregion

        #region Click Handlers
        private void button_Click(object sender, EventArgs e) {
            if (actions.ContainsKey(sender)) {
                actions[sender]();
            }
        }

        private void searchOptions_Click(object sender, EventArgs e) {
            Dictionary<object, SearchState> states = new Dictionary<object, SearchState> {
                                                                                             {
                                                                                                 taggedFilesToolStripMenuItem
                                                                                                 , SearchState.Tagged
                                                                                                 },
                                                                                             {
                                                                                                 untaggedFilesToolStripMenuItem
                                                                                                 , SearchState.Untagged
                                                                                                 },
                                                                                             {
                                                                                                 filepathsToolStripMenuItem
                                                                                                 , SearchState.Filenames
                                                                                                 },
                                                                                             {
                                                                                                 tagsToolStripMenuItem,
                                                                                                 SearchState.Tags
                                                                                                 },
                                                                                             {
                                                                                                 regularExpressionsToolStripMenuItem
                                                                                                 , SearchState.Regex
                                                                                                 },
                                                                                             {
                                                                                                 caseSensitiveToolStripMenuItem
                                                                                                 ,
                                                                                                 SearchState.
                                                                                                 CaseSensitive
                                                                                                 }
                                                                                         };
            if (states.ContainsKey(sender)) {
                settings.SearchOptions ^= states[sender];
            }
            foreach (KeyValuePair<object, SearchState> kvPair in states) {
                ((ToolStripMenuItem)kvPair.Key).Checked = (settings.SearchOptions & kvPair.Value) == kvPair.Value;
            }
            refreshListingWithSearch();
        }
        #endregion

        #region File Downloading
        private List<string> dragged;
        private float fileItemHeight;
        private void TwilightSortle_DragEnter(object sender, DragEventArgs e) {
            dragged = new List<string>();
            if (e.Data.GetDataPresent(DataFormats.Html)) {
                string html = (String) e.Data.GetData(DataFormats.Html);
                MatchCollection matches = Regex.Matches(html, @"<a [^>]*href=[""'](.*?)[""']");
                foreach (Match match in matches) {
                    if (Regex.IsMatch(match.Groups[1].Value, @"\.(jpg|jpeg|png|gif)$")) {
                        dragged.Add(match.Groups[1].Value);
                    }
                }
                if (matches.Count == 0) {
                    matches = Regex.Matches(html, @"<img [^>]*src=""(.*?)""");
                    foreach (Match match in matches) {
                        dragged.Add(match.Groups[1].Value);
                    }
                }
                updateFileActionsPanel();
                //e.Effect = DragDropEffects.Copy;
                fileActionPanel.Visible = true;
            }
            else {
                e.Effect = DragDropEffects.None;
            }
            Console.WriteLine("Main Form Drag Enter");
        }

        private void TwilightSortle_DragLeave(object sender, EventArgs e) {
                fileActionPanel.Visible = false;
        }

        private void updateFileActionsPanel() {
            fileActionPanel.Width = (int) (ClientRectangle.Width * 0.9f);
            fileActionPanel.Left = (int) (ClientRectangle.Width * 0.05f);
        }

        private void fileActionPanel_Paint(object sender, PaintEventArgs e) {
            Console.WriteLine("File Panel Redraw");
            Point clientMousePosition = fileActionPanel.PointToClient(MousePosition);
            float currentHeight = 0f;
            const float spacing = 5.0f;
            Brush brush = new SolidBrush(Color.Black);
            Brush bgBrush = new SolidBrush(Color.LightGray);
            Brush selectedBgBrush = new SolidBrush(Color.LightPink);
            Font font = new Font(this.Font.FontFamily, 16);
            foreach (Directory directory in settings.Directories) {
                float rectHeight = directory.Path.Height(font);
                string sortString = "Not sorting.";
                if (directory.SortPath != null) {
                    sortString = @"Sorting to: ""{0}""".With(directory.SortPath);
                }
                rectHeight += spacing;
                rectHeight += sortString.Height(this.Font);
                rectHeight += spacing;
                fileItemHeight = rectHeight;
                Rectangle backRect = new Rectangle(0, (int) currentHeight, fileActionPanel.Width, (int) rectHeight);
                e.Graphics.FillRectangle(backRect.Contains(clientMousePosition) ? selectedBgBrush : bgBrush, backRect);
                e.Graphics.DrawString(directory.Path, font, brush, 0.0f, currentHeight);
                currentHeight += directory.Path.Height(font);
                currentHeight += spacing;
                e.Graphics.DrawString(sortString, this.Font, brush, 0, currentHeight);
                currentHeight += sortString.Height(this.Font);
                currentHeight += spacing;

            }
        }

        private void TwilightSortle_DragOver(object sender, DragEventArgs e) {
            if (dragged.Count == 0) {
                return;
            }

            if (fileActionPanel.ClientRectangle.Contains(fileActionPanel.PointToClient(MousePosition))) {
                e.Effect = DragDropEffects.Copy;
                fileActionPanel.Invalidate();
            }
            else {
                e.Effect = DragDropEffects.None;
            }
        }

        private void TwilightSortle_DragDrop(object sender, DragEventArgs e) {
            if (e.Effect == DragDropEffects.Copy) {
                Point clientMousePosition = fileActionPanel.PointToClient(MousePosition);
                int index = (int) (clientMousePosition.Y / fileItemHeight);
                Action action = (() => {
                                     dragged.ForEach(url => settings.Directories.ElementAt(index).DownloadTo(url));
                                     settings.Directories.ElementAt(index).UpdateFilepaths();

                                 });
                Action later = refreshListingWithSearch;
                fileActionPanel.Visible = false;
                runLongOperation(action, later,
                                 "Downloading {0} file{1}".With(dragged.Count, dragged.Count == 1 ? "" : "s"));
            }
        }
        #endregion

    }
}
