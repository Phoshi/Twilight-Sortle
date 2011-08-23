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
using KeyBindings;
using UACHelperFunctions;

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
        private List<List<object>> queuedActions = new List<List<object>>();
        private KeyBindings.KeyBindings globals;
        #endregion

        public TwilightSortle() {
            InitializeComponent();
            setupActions();
            mainList.DoubleBuffer();
            mainList.ContextMenuStrip = new ContextMenuStrip();
            mainList.ContextMenuStrip.Opening += new CancelEventHandler(ContextMenuStrip_Opening);
            fileActionPanel.DoubleBuffer();
            beginLoad();
            if (UACHelper.IsAdmin()) {
                this.Text = "{0} (Administrator)".With(this.Text);
            }
        }

        void ContextMenuStrip_Opening(object sender, CancelEventArgs e) {
            ContextMenuStrip strip = mainList.ContextMenuStrip;
            strip.Items.Clear();
            strip.Items.Add("Open", null, ((s, eArgs)=>Process.Start(getSelectedItems()[0].Path)));
            strip.Items.Add("Open Containing Folder", null,
                            ((s, eArgs) => Process.Start("explorer.exe", "/select,{0}".With(getSelectedItems()[0].Path))));
            if (getSelectedItems().Any(item => item.ExternalUrl != "")) {
                strip.Items.Add("Copy external URL(s)", null, ((s, eA) => {
                                                             IEnumerable<string> result =
                                                                 from item in getSelectedItems()
                                                                 where item.ExternalUrl != ""
                                                                 select item.ExternalUrl;
                                                             Clipboard.SetText(String.Join(" ", result));
                                                         }));
            }
            strip.Items.Add("Upload", null, ((s, eA)=>uploadImages()));
        }

        private void restartWithAdminRights() {
            MessageBox.Show("This operation requires elevated priviledges - now attempting to aquire them");
            save();
            saving = false;
            UACHelper.LaunchWithAdminRights();
            Application.Exit();
        }
        private void buildQuickFolderAccessList() {
            foldersToolStripMenuItem.DropDownItems.Clear();
            settings.Directories.ForEach(directory => {
                                             ToolStripMenuItem item = new ToolStripMenuItem(directory.Path);
                                             item.Checked = directory.Enabled;
                                             item.CheckOnClick = true;
                                             item.MouseDown += ((sender, e) => runLongOperation((() => {
                                                                                                     if (e.Button ==
                                                                                                         MouseButtons.
                                                                                                             Right) {
                                                                                                         settings.
                                                                                                             Directories
                                                                                                             .ForEach(
                                                                                                                 dir =>
                                                                                                                 dir.
                                                                                                                     Enabled
                                                                                                                 = false);
                                                                                                         directory.
                                                                                                             Enabled =
                                                                                                             true;
                                                                                                         mainMenuStrip.Invoke(
                                                                                                             (Action)(() => buildQuickFolderAccessList()));
                                                                                                     }
                                                                                                     else {
                                                                                                         directory.
                                                                                                             Enabled =
                                                                                                             !directory.
                                                                                                                  Enabled;
                                                                                                     }
                                                                                                 }),
                                                                                                refreshListingWithSearch,
                                                                                                "Updating Directory Settings"));
                                             foldersToolStripMenuItem.DropDownItems.Add(item);
                                         });
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

            //Hotkeys
            globals = new KeyBindings.KeyBindings(this);
            globals.bind("qq", Close);
            globals.bind("<C-f>", searchBox.Select);
            globals.bind("Rt", renameAllVisibleItemsByTags);
            globals.bind("Rh", renameAllVisibleItemsByHash);
            globals.bind("<C-o>b", updateSortedTree);
            globals.bind("<C-o>d", removeSortedTree);
            globals.bind("<C-s>", save);

            KeyBindings.KeyBindings searchBindings = new KeyBindings.KeyBindings(searchBox);
            searchBindings.bind(new[] {Keys.Down}, mainList.Select);

            KeyBindings.KeyBindings mainListBindings = new KeyBindings.KeyBindings(mainList);
            mainListBindings.bind("j", (() => mainList.Scroll(1)));
            mainListBindings.bind("k", (() => mainList.Scroll(-1)));
            mainListBindings.bind("a", toggleAddTagPanel);
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SystemParametersInfo(
            UInt32 action, UInt32 uParam, String vParam, UInt32 winIni);

        private const UInt32 SPI_SETDESKWALLPAPER = 0x14;
        private const UInt32 SPIF_UPDATEINIFILE = 0x01;
        private const UInt32 SPIF_SENDWININICHANGE = 0x02;

        private void setSelectedAsWallpaper() {
            string newName = null;
            if (getSelectedItems().Count == 0) {
                return;
            }
            string path = getSelectedItems()[0].Path;
            Action action = (() => {
                                 string newPath = Path.GetTempPath();
                                 string oldName = Path.GetFileNameWithoutExtension(path);
                                 newName = Path.Combine(newPath, oldName + ".bmp");
                                 Image image = Image.FromFile(path);
                                 image.Save(newName, ImageFormat.Bmp);
                                 image.Dispose();
                             });
            Action actualAction = (() => SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, newName,
                                                              SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE));
            runLongOperation(action, null, "Preparing image");
            runLongOperation(actualAction, null, "Setting wallpaper");
        }

        private void uploadImages() {
            List<Item> selectedItems = getSelectedItems();
            if (selectedItems.Count > 0) {
                List<string> urls = new List<string>();
                Action action = (() => {
                                     foreach (Item selectedItem in selectedItems) {
                                         urls.Add(selectedItem.UploadedUrl);
                                     }
                                 });
                Action after = (() => Clipboard.SetText(String.Join(" ", urls)));
                runLongOperation(action, after, "Uploading {0} file(s).\nURL(s) will be placed on clipboard.".With(selectedItems.Count));
            }
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
            Action action = (() => {
                                 items.ForEach(item => item.Delete());
                                 items.ForEach(item => settings.GetDirectory(item.Directory).UpdateFilepaths());
                             });
            runLongOperation(action, refreshListingWithSearch, "Deleting files");
        }

        private void updateSearchOptionsMenuCheckedStatus() {
            searchOptions_Click(new object(), new EventArgs());
        }

        private void renameAllVisibleItemsByHash() {
            runLongOperation((() => {
                foreach (Item item in allEnabledItems()) {
                    item.RenameToHash();
                }
            }), refreshListingWithSearch, "Renaming By Hash");
        }

        private void renameAllVisibleItemsByTags() {
            runLongOperation((() => {
                foreach (Item item in allEnabledItems()) {
                    item.RenameToTags();
                }
            }), refreshListingWithSearch, "Renaming By Tag");
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
                                     refreshListingWithSearch();
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
            if (workingThread.IsBusy) {
                List<object> newQueueItem = new List<object>() {operation, callback, reason};
                queuedActions.Add(newQueueItem);
                return;
            }
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
            workingThread.RunWorkerCompleted += ((sender, e) => runQueuedOperations());
            workingThread.RunWorkerCompleted += ((sender, e) => updateWorkingPanelPosition());
            workingThread.RunWorkerAsync();
            updateWorkingPanelPosition(reason);
        }

        private void runQueuedOperations() {
            if (queuedActions.Count > 0) {
                List<object> actions = queuedActions.First();
                queuedActions.RemoveAt(0);
                Action action = actions[0] as Action;
                Action callback = actions[1] as Action;
                string description = actions[2] as string;
                runLongOperation(action, callback, description);
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
                if (!itemList[item.Index].Invalidated || settings.GetDirectory(itemList[item.Index].Directory).SortPath == null) {
                    item.BackColor = colourSwitch ? Color.White : Color.FromArgb(240, 240, 240);
                }
                else {
                    item.BackColor = colourSwitch ? Color.Gray : Color.Black;
                    item.ForeColor = colourSwitch ? Color.White : Color.LightGray;
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
            listIndexLabel.Text = "{0}/{1}".With(selectedIndex, allEnabledItems().Count);
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
            search = new Search(allEnabledItems(), searchBox.Text, settings.SearchOptions, settings.SortOptions, settings.SortDescending);
            updateMainListing(search.Items, true);
        }

        private void beginLoad(bool async = true) {
            Action action = (() => {
                TwoLight_Sortle.Load.FilesCache =
                    TwoLight_Sortle.Load.FilesCache.LoadFromDisk<Dictionary<UInt32, Item>>(
                        Path.Combine(Application.UserAppDataPath, "FilesCache"));
            });
            Action deleteAction = (() => {
                IEnumerable<Item> deletedItems = from kvPair in TwoLight_Sortle.Load.FilesCache
                                                 where !kvPair.Value.Exists
                                                 select kvPair.Value;
                foreach (Item item in deletedItems) {
                    item.Delete();
                }
                TwoLight_Sortle.Load.FilesCache =
                    TwoLight_Sortle.Load.FilesCache.Where(kvPair => kvPair.Value.Exists).
                        ToDictionary(
                            kvPair => kvPair.Key,
                            kvPair => kvPair.Value);
            });
            Action filesAction = (() => {
                Tags.TagList = Tags.TagList.LoadFromDisk<Dictionary<string, Tag>>(Path.Combine(
                    Application.UserAppDataPath, "Tags"));
                TwoLight_Sortle.Load.FilesCache.ToList().ForEach(item=>item.Value.Tags.ToList().ForEach(tag=>tag.AddedImage(item.Value)));
            });
            settings = new Settings();
            Action loadSettingsAction = (() => {
                settings =
                    settings.LoadFromDisk<Settings>(Path.Combine(Application.UserAppDataPath,
                                                                 "Settings"));
            });
            Action callback = (() => {
                updateMainListing(search.Items);
                updateSearchOptionsMenuCheckedStatus();
                updatePreferencesPanel();
                buildQuickFolderAccessList();
            });
            Action dirStateAction = (() => {
                search = new Search(allEnabledItems(), "", settings.SearchOptions);
                startupComplete = true;
            });
            if (async) {
                runLongOperation(action, null, "Verifying File Cache Integrity");
                runLongOperation(deleteAction, null, "Reticulating Splines");
                runLongOperation(filesAction, null, "Loading Tags Cache");
                runLongOperation(loadSettingsAction, null, "Loading Directory Data");
                runLongOperation(dirStateAction, callback, "Generating Directory State");
            }
            else {
                action();
                deleteAction();
                filesAction();
                loadSettingsAction();
                dirStateAction();
                callback();

            }
        }

        private void save() {
            if (saving) {
                TwoLight_Sortle.Load.FilesCache.SaveToDisk(
                    Path.Combine(Application.UserAppDataPath, "FilesCache"));
                TwoLight_Sortle.Tags.TagList.SaveToDisk(
                    Path.Combine(Application.UserAppDataPath, "Tags"));
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
                workingPanel.Width = 300;
                if (reason.Width(workingDescription.Font) > workingPanel.Width) {
                    workingPanel.Width = reason.Width(workingDescription.Font) + 30;
                    updateWorkingPanelPosition();
                }
                if (reason != "") {
                    workingDescription.Text = reason;
                    workingDescription.Left = (workingPanel.Width / 2 - reason.Width(workingDescription.Font) / 2);
                }
                workingPanel.Refresh();
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
            if (!UACHelper.IsAdmin()) {
                runLongOperation(save, null, "Saving data...");
                Action runOperationAction = (() => UACHelper.RunApplicationAsAdmin(Application.ExecutablePath, "CreateTree", true));
                runLongOperation(runOperationAction, (() => beginLoad()), "Updating Sorted Tree");
                return;
            }
            List<List<string>> frequentSets = null;
            IEnumerable<Item> filesToSort = null;
            Action action = (() => {
                                 Dictionary<string, List<string>> aprioriData = new Dictionary<string, List<string>>();
                                 foreach (Item item in allEnabledItems()) {
                                     aprioriData[item.Path] = (from tag in item.Tags select tag.Name).ToList();
                                 }
                                 Apriori.Apriori apriori = new Apriori.Apriori(aprioriData, 5);
                                 frequentSets = apriori.getFrequentSets();
                                 filesToSort = from file in allEnabledItems() where file.Invalidated select file;
                             });
            runLongOperation(action, null, "Processing tag data");
            Action makeTree = (() => {
                                   foreach (Item file in filesToSort) {
                                       TreeHelper.createSortedTree(file, frequentSets, settings);
                                   }
                               });
            Action after = refreshListingWithSearch;
            runLongOperation(makeTree, after, "Updating sorted tree");
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
            preferencesDirectoryList.Invalidate();
            panel1.Invalidate();
            buildQuickFolderAccessList();
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
                preferencesPanel.Height = this.ClientRectangle.Height - mainMenuStrip.Height;
                preferencesPanel.Left = 0;
                preferencesPanel.Top = mainMenuStrip.Height;
                //splitListDetails.Left = preferencesPanel.Right;
                //splitListDetails.Width = this.Width - preferencesPanel.Left;
                preferencesPanel.Visible = true;
                preferencesPanel.Invalidate();
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
                //splitListDetails.Left = 0;
                //splitListDetails.Width = this.Width;
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
                             (() => updateMainListing(search.Items, true)), "Finalising Directory Cache");
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

        private void toggleAddTagPanel() {
            if (getSelectedItems().Count > 0) {
                showingTagPanel = !showingTagPanel;
                updateTagPanelPosition();
                if (showingTagPanel) {
                    splitListDetails.Enabled = false;
                    addTagFilter.Select();
                }
                else {
                    splitListDetails.Enabled = true;
                    mainList.Select();
                }
            }
        }

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
                toggleAddTagPanel();
                refreshListingWithSearch();
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
                addTagPanel.Visible = false;
                _tagCompletePosition = -1;
                addTagFilter.Text = "";
                addTagPictureBox.Visible = false;
            }
            else {
                addTagPanel.Top = this.Height - addTagPanel.Height;
                addTagPanel.Visible = true;
                addTagPictureBox.Visible = true;
                updateTagImageBox();
                addTagFilter.Select();
            }
            updateTagAutocompleteList(addTagFilter.Text, "");
            addTagPanel.Width = this.Width;
            addTagPanel.Left = 0;
        }

        private void updateTagImageBox() {
            Image image = getSelectedItems()[0].Image;
            if (image.Width > this.Width || image.Height > (this.Height - addTagPanel.Height)) {
                addTagPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            }
            else {
                addTagPictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
            }

            addTagPictureBox.Width = this.Width;
            addTagPictureBox.Height = this.Height - addTagPanel.Height;
            addTagPictureBox.Left = (this.Width / 2) - (addTagPictureBox.Width / 2);
            addTagPictureBox.Top = ((this.Height - addTagPanel.Height) / 2) - (addTagPictureBox.Height / 2);
            addTagPictureBox.Image = image;
        }
        #endregion

        #region Form Event Handlers
        private void TwilightSortle_Load(object sender, EventArgs e) {
            updateTagPanelPosition();
        }

        private void TwilightSortle_FormClosed(object sender, FormClosedEventArgs e) {
            runLongOperation(null, null, "Saving");
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

        private void mainList_MouseDoubleClick(object sender, MouseEventArgs e) {
            if (getSelectedItems().Count > 0) {
                Process.Start(getSelectedItems()[0].Path);
            }
        }

        private void mainList_ItemDrag(object sender, ItemDragEventArgs e) {
            string[] files = (from item in getSelectedItems() select item.Path).ToArray();
            if (files.Count() > 0) {
                DoDragDrop(new DataObject(DataFormats.FileDrop, files), DragDropEffects.Copy | DragDropEffects.Move);
            }
        }

        private void mainList_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Right) {
                Point mousePosition = mainList.PointToClient(MousePosition);
                ListViewItem item = mainList.GetItemAt(mousePosition.X, mousePosition.Y);
                if (item != null) {
                    mainList.ContextMenuStrip.Show(MousePosition);
                }
            }
        }

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
                    view.BeginUpdate();
                    float totalWidth = 0;
                    for (int i = 0; i < view.Columns.Count; i++) {
                        totalWidth += Convert.ToInt32(view.Columns[i].Tag);
                    }

                    for (int i = 0; i < view.Columns.Count; i++) {
                        float percentage = Convert.ToInt32(view.Columns[i].Tag) / totalWidth;
                        view.Columns[i].Width = (int)(view.ClientRectangle.Width * percentage);
                    }
                    view.EndUpdate();
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

        #region File Downloading and Handling
        private List<string> dragged;
        private bool requiresDownload;
        private float fileItemHeight;
        private void TwilightSortle_DragEnter(object sender, DragEventArgs e) {
            dragged = new List<string>();
            if (e.Data.GetDataPresent(DataFormats.Html)) {
                string html = (String) e.Data.GetData(DataFormats.Html);
                MatchCollection matches = Regex.Matches(html, @"<a [^>]*href=[""'](.*?)[""']");
                foreach (Match match in matches) {
                    dragged.Add(match.Groups[1].Value);
                }
                if (matches.Count == 0) {
                    matches = Regex.Matches(html, @"<img [^>]*src=""(.*?)""");
                    foreach (Match match in matches) {
                        dragged.Add(match.Groups[1].Value);
                    }
                }
                updateFileActionsPanel();
                requiresDownload = true;
                //e.Effect = DragDropEffects.Copy;
                fileActionPanel.Visible = true;
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                dragged = ((string[])e.Data.GetData(DataFormats.FileDrop)).ToList();
                requiresDownload = false;
                updateFileActionsPanel();
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
                e.Graphics.DrawString(directory.ToString(), font, brush, 0.0f, currentHeight);
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
                e.Effect = requiresDownload ? DragDropEffects.Copy : DragDropEffects.Move;
                fileActionPanel.Invalidate();
            }
            else {
                e.Effect = DragDropEffects.None;
            }
        }

        private void TwilightSortle_DragDrop(object sender, DragEventArgs e) {
            if (e.Effect == DragDropEffects.None) {
                return;
            }
            Point clientMousePosition = fileActionPanel.PointToClient(MousePosition);
            int index = (int) (clientMousePosition.Y / fileItemHeight);
            if (requiresDownload) {
                Action action = (() => {
                                     dragged.ForEach(url => settings.Directories.ElementAt(index).DownloadTo(url));
                                     settings.Directories.ElementAt(index).UpdateFilepaths();

                                 });
                Action later = refreshListingWithSearch;
                fileActionPanel.Visible = false;
                runLongOperation(action, later,
                                 "Downloading {0} file{1}\n{2}".With(dragged.Count, dragged.Count == 1 ? "" : "s",
                                                                     String.Join(", ", dragged)));
            }
            else {
                Action action = (() => {
                                     dragged.ForEach(((filepath) => {
                                                          string fileName = Path.GetFileName(filepath);
                                                          string newPath =
                                                              Path.Combine(settings.Directories.ElementAt(index).Path,
                                                                           fileName);
                                                          while (File.Exists(newPath)) {
                                                              string extension = Path.GetExtension(newPath);
                                                              fileName = Path.GetFileNameWithoutExtension(newPath);
                                                              newPath =
                                                                  Path.Combine(
                                                                      settings.Directories.ElementAt(index).Path,
                                                                      "{0}.{1}".With(fileName, extension));
                                                          }
                                                          File.Move(filepath, newPath);
                                                      }));
                                     settings.Directories.ForEach(dir => dir.UpdateFilepaths());
                                 });
                Action later = refreshListingWithSearch;
                fileActionPanel.Visible = false;
                runLongOperation(action, later, "Moving Files");
            }
        }

        #endregion

        private void mainList_ColumnClick(object sender, ColumnClickEventArgs e) {
            Dictionary<int, SortState> states = new Dictionary<int, SortState>() {
                                                                                     {0, SortState.Tags},
                                                                                     {1, SortState.Filename},
                                                                                     {2, SortState.Directory},
                                                                                     {3, SortState.Filesize},
                                                                                     {4, SortState.Dimensions},
                                                                                     {5, SortState.External}
                                                                                 };
            if (states.ContainsKey(e.Column)) {
                if (states[e.Column] == settings.SortOptions) {
                    settings.SortDescending = !settings.SortDescending;
                }
                else {
                    settings.SortOptions = states[e.Column];
                    settings.SortDescending = true;
                }
            }
            refreshListingWithSearch();
        }
    }
}
