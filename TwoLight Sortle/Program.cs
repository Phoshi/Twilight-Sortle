using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Extensions;

namespace TwoLight_Sortle {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Count() > 1) {
                if (UACHelperFunctions.UACHelper.IsAdmin()) {
                    Load.FilesCache = Load.FilesCache.LoadFromDisk<Dictionary<UInt32, Item>>(
                        Path.Combine(Application.UserAppDataPath, "FilesCache"));
                    Settings settings = new Settings();
                    settings =
                        settings.LoadFromDisk<Settings>(Path.Combine(Application.UserAppDataPath,
                                                                     "Settings"));
                    Dictionary<string, List<string>> aprioriData = new Dictionary<string, List<string>>();
                    var allEnabledItems = (from directory in settings.EnabledDirectories select directory.Items).SelectMany(item => item).ToList();
                    foreach (Item item in allEnabledItems) {
                        aprioriData[item.Path] = (from tag in item.Tags select tag.Name).ToList();
                    }
                    Apriori.Apriori apriori = new Apriori.Apriori(aprioriData, 5);
                    var frequentSets = apriori.getFrequentSets();
                    var filesToSort = from file in allEnabledItems where file.Invalidated select file;
                    foreach (Item file in filesToSort) {
                        TreeHelper.createSortedTree(file, frequentSets, settings);
                    }
                    Load.FilesCache.SaveToDisk(Path.Combine(Application.UserAppDataPath, "FilesCache"));
                    Application.Exit();
                    return;
                }
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TwilightSortle());
        }
    }
}
