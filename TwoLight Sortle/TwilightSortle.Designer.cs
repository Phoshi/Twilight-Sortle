namespace TwoLight_Sortle {
    partial class TwilightSortle {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.splitListDetails = new System.Windows.Forms.SplitContainer();
            this.mainList = new System.Windows.Forms.ListView();
            this.searchBox = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.mainListHeaderTags = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.mainListHeaderFilename = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.mainListHeaderDirectory = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.mainListHeaderFilesize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.mainListHeaderDimensions = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.menuStrip2 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.preferencesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.searchOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.taggedFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.untaggedFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filepathsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tagsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.regularExpressionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.caseSensitiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.foldersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sortToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.byTagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.byHashToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.byLastModifiedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.organiseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buildToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addTagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeTagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buildTagDatabaseFilenamesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setAsWallpaperToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameTagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.downloadToToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uploadToToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imagurToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.splitListDetails)).BeginInit();
            this.splitListDetails.Panel1.SuspendLayout();
            this.splitListDetails.SuspendLayout();
            this.menuStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitListDetails
            // 
            this.splitListDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitListDetails.Location = new System.Drawing.Point(0, 24);
            this.splitListDetails.Name = "splitListDetails";
            // 
            // splitListDetails.Panel1
            // 
            this.splitListDetails.Panel1.Controls.Add(this.button1);
            this.splitListDetails.Panel1.Controls.Add(this.searchBox);
            this.splitListDetails.Panel1.Controls.Add(this.mainList);
            this.splitListDetails.Panel1.Controls.Add(this.menuStrip1);
            this.splitListDetails.Size = new System.Drawing.Size(812, 490);
            this.splitListDetails.SplitterDistance = 561;
            this.splitListDetails.TabIndex = 0;
            // 
            // mainList
            // 
            this.mainList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mainList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.mainListHeaderTags,
            this.mainListHeaderFilename,
            this.mainListHeaderDirectory,
            this.mainListHeaderFilesize,
            this.mainListHeaderDimensions});
            this.mainList.FullRowSelect = true;
            this.mainList.HideSelection = false;
            this.mainList.Location = new System.Drawing.Point(13, 26);
            this.mainList.Name = "mainList";
            this.mainList.Size = new System.Drawing.Size(545, 452);
            this.mainList.TabIndex = 0;
            this.mainList.UseCompatibleStateImageBehavior = false;
            this.mainList.View = System.Windows.Forms.View.Details;
            // 
            // searchBox
            // 
            this.searchBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.searchBox.Location = new System.Drawing.Point(13, 0);
            this.searchBox.Name = "searchBox";
            this.searchBox.Size = new System.Drawing.Size(514, 20);
            this.searchBox.TabIndex = 1;
            this.searchBox.TextChanged += new System.EventHandler(this.searchBox_TextChanged);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(533, 0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(25, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "X";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // mainListHeaderTags
            // 
            this.mainListHeaderTags.Text = "Tags";
            // 
            // mainListHeaderFilename
            // 
            this.mainListHeaderFilename.Text = "File Name";
            // 
            // mainListHeaderDirectory
            // 
            this.mainListHeaderDirectory.Text = "Directory";
            // 
            // mainListHeaderFilesize
            // 
            this.mainListHeaderFilesize.Text = "File Size";
            // 
            // mainListHeaderDimensions
            // 
            this.mainListHeaderDimensions.Text = "Dimensions";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(561, 24);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // menuStrip2
            // 
            this.menuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.searchOptionsToolStripMenuItem,
            this.sortToolStripMenuItem,
            this.manageToolStripMenuItem,
            this.downloadToToolStripMenuItem,
            this.uploadToToolStripMenuItem});
            this.menuStrip2.Location = new System.Drawing.Point(0, 0);
            this.menuStrip2.Name = "menuStrip2";
            this.menuStrip2.Size = new System.Drawing.Size(812, 24);
            this.menuStrip2.TabIndex = 1;
            this.menuStrip2.Text = "menuStrip2";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.preferencesToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // preferencesToolStripMenuItem
            // 
            this.preferencesToolStripMenuItem.Name = "preferencesToolStripMenuItem";
            this.preferencesToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.preferencesToolStripMenuItem.Text = "Preferences";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // searchOptionsToolStripMenuItem
            // 
            this.searchOptionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.taggedFilesToolStripMenuItem,
            this.untaggedFilesToolStripMenuItem,
            this.filepathsToolStripMenuItem,
            this.tagsToolStripMenuItem,
            this.regularExpressionsToolStripMenuItem,
            this.caseSensitiveToolStripMenuItem,
            this.foldersToolStripMenuItem});
            this.searchOptionsToolStripMenuItem.Name = "searchOptionsToolStripMenuItem";
            this.searchOptionsToolStripMenuItem.Size = new System.Drawing.Size(99, 20);
            this.searchOptionsToolStripMenuItem.Text = "Search Options";
            // 
            // taggedFilesToolStripMenuItem
            // 
            this.taggedFilesToolStripMenuItem.Name = "taggedFilesToolStripMenuItem";
            this.taggedFilesToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.taggedFilesToolStripMenuItem.Text = "Tagged Files";
            // 
            // untaggedFilesToolStripMenuItem
            // 
            this.untaggedFilesToolStripMenuItem.Name = "untaggedFilesToolStripMenuItem";
            this.untaggedFilesToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.untaggedFilesToolStripMenuItem.Text = "Untagged Files";
            // 
            // filepathsToolStripMenuItem
            // 
            this.filepathsToolStripMenuItem.Name = "filepathsToolStripMenuItem";
            this.filepathsToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.filepathsToolStripMenuItem.Text = "Filepaths";
            // 
            // tagsToolStripMenuItem
            // 
            this.tagsToolStripMenuItem.Name = "tagsToolStripMenuItem";
            this.tagsToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.tagsToolStripMenuItem.Text = "Tags";
            // 
            // regularExpressionsToolStripMenuItem
            // 
            this.regularExpressionsToolStripMenuItem.Name = "regularExpressionsToolStripMenuItem";
            this.regularExpressionsToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.regularExpressionsToolStripMenuItem.Text = "Regular Expressions";
            // 
            // caseSensitiveToolStripMenuItem
            // 
            this.caseSensitiveToolStripMenuItem.Name = "caseSensitiveToolStripMenuItem";
            this.caseSensitiveToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.caseSensitiveToolStripMenuItem.Text = "Case Sensitive";
            // 
            // foldersToolStripMenuItem
            // 
            this.foldersToolStripMenuItem.Name = "foldersToolStripMenuItem";
            this.foldersToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.foldersToolStripMenuItem.Text = "Folders";
            // 
            // sortToolStripMenuItem
            // 
            this.sortToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.renameToolStripMenuItem,
            this.organiseToolStripMenuItem});
            this.sortToolStripMenuItem.Name = "sortToolStripMenuItem";
            this.sortToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.sortToolStripMenuItem.Text = "Sort";
            // 
            // renameToolStripMenuItem
            // 
            this.renameToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.byTagToolStripMenuItem,
            this.byHashToolStripMenuItem,
            this.byLastModifiedToolStripMenuItem});
            this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
            this.renameToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.renameToolStripMenuItem.Text = "Rename";
            // 
            // byTagToolStripMenuItem
            // 
            this.byTagToolStripMenuItem.Name = "byTagToolStripMenuItem";
            this.byTagToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.byTagToolStripMenuItem.Text = "By Tag";
            // 
            // byHashToolStripMenuItem
            // 
            this.byHashToolStripMenuItem.Name = "byHashToolStripMenuItem";
            this.byHashToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.byHashToolStripMenuItem.Text = "By Hash";
            // 
            // byLastModifiedToolStripMenuItem
            // 
            this.byLastModifiedToolStripMenuItem.Name = "byLastModifiedToolStripMenuItem";
            this.byLastModifiedToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.byLastModifiedToolStripMenuItem.Text = "By Last Modified";
            // 
            // organiseToolStripMenuItem
            // 
            this.organiseToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buildToolStripMenuItem,
            this.removeToolStripMenuItem});
            this.organiseToolStripMenuItem.Name = "organiseToolStripMenuItem";
            this.organiseToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.organiseToolStripMenuItem.Text = "Organise";
            // 
            // buildToolStripMenuItem
            // 
            this.buildToolStripMenuItem.Name = "buildToolStripMenuItem";
            this.buildToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.buildToolStripMenuItem.Text = "Build";
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.removeToolStripMenuItem.Text = "Remove";
            // 
            // manageToolStripMenuItem
            // 
            this.manageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addTagToolStripMenuItem,
            this.removeTagToolStripMenuItem,
            this.deleteFileToolStripMenuItem,
            this.toolStripSeparator1,
            this.buildTagDatabaseFilenamesToolStripMenuItem,
            this.renameTagToolStripMenuItem,
            this.setAsWallpaperToolStripMenuItem});
            this.manageToolStripMenuItem.Name = "manageToolStripMenuItem";
            this.manageToolStripMenuItem.Size = new System.Drawing.Size(62, 20);
            this.manageToolStripMenuItem.Text = "Manage";
            // 
            // addTagToolStripMenuItem
            // 
            this.addTagToolStripMenuItem.Name = "addTagToolStripMenuItem";
            this.addTagToolStripMenuItem.Size = new System.Drawing.Size(237, 22);
            this.addTagToolStripMenuItem.Text = "Add Tag";
            // 
            // removeTagToolStripMenuItem
            // 
            this.removeTagToolStripMenuItem.Name = "removeTagToolStripMenuItem";
            this.removeTagToolStripMenuItem.Size = new System.Drawing.Size(237, 22);
            this.removeTagToolStripMenuItem.Text = "Remove Tag";
            // 
            // deleteFileToolStripMenuItem
            // 
            this.deleteFileToolStripMenuItem.Name = "deleteFileToolStripMenuItem";
            this.deleteFileToolStripMenuItem.Size = new System.Drawing.Size(237, 22);
            this.deleteFileToolStripMenuItem.Text = "Delete File";
            // 
            // buildTagDatabaseFilenamesToolStripMenuItem
            // 
            this.buildTagDatabaseFilenamesToolStripMenuItem.Name = "buildTagDatabaseFilenamesToolStripMenuItem";
            this.buildTagDatabaseFilenamesToolStripMenuItem.Size = new System.Drawing.Size(237, 22);
            this.buildTagDatabaseFilenamesToolStripMenuItem.Text = "Build Tag Database (Filenames)";
            // 
            // setAsWallpaperToolStripMenuItem
            // 
            this.setAsWallpaperToolStripMenuItem.Name = "setAsWallpaperToolStripMenuItem";
            this.setAsWallpaperToolStripMenuItem.Size = new System.Drawing.Size(237, 22);
            this.setAsWallpaperToolStripMenuItem.Text = "Set as Wallpaper";
            // 
            // renameTagToolStripMenuItem
            // 
            this.renameTagToolStripMenuItem.Name = "renameTagToolStripMenuItem";
            this.renameTagToolStripMenuItem.Size = new System.Drawing.Size(237, 22);
            this.renameTagToolStripMenuItem.Text = "Rename Tag";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(234, 6);
            // 
            // downloadToToolStripMenuItem
            // 
            this.downloadToToolStripMenuItem.Name = "downloadToToolStripMenuItem";
            this.downloadToToolStripMenuItem.Size = new System.Drawing.Size(89, 20);
            this.downloadToToolStripMenuItem.Text = "Download To";
            // 
            // uploadToToolStripMenuItem
            // 
            this.uploadToToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.imagurToolStripMenuItem});
            this.uploadToToolStripMenuItem.Name = "uploadToToolStripMenuItem";
            this.uploadToToolStripMenuItem.Size = new System.Drawing.Size(73, 20);
            this.uploadToToolStripMenuItem.Text = "Upload To";
            // 
            // imagurToolStripMenuItem
            // 
            this.imagurToolStripMenuItem.Name = "imagurToolStripMenuItem";
            this.imagurToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.imagurToolStripMenuItem.Text = "Imgur";
            // 
            // TwilightSortle
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(812, 514);
            this.Controls.Add(this.splitListDetails);
            this.Controls.Add(this.menuStrip2);
            this.DoubleBuffered = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "TwilightSortle";
            this.Text = "Twilight Sortle";
            this.Load += new System.EventHandler(this.TwilightSortle_Load);
            this.splitListDetails.Panel1.ResumeLayout(false);
            this.splitListDetails.Panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitListDetails)).EndInit();
            this.splitListDetails.ResumeLayout(false);
            this.menuStrip2.ResumeLayout(false);
            this.menuStrip2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitListDetails;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox searchBox;
        private System.Windows.Forms.ListView mainList;
        private System.Windows.Forms.ColumnHeader mainListHeaderTags;
        private System.Windows.Forms.ColumnHeader mainListHeaderFilename;
        private System.Windows.Forms.ColumnHeader mainListHeaderDirectory;
        private System.Windows.Forms.ColumnHeader mainListHeaderFilesize;
        private System.Windows.Forms.ColumnHeader mainListHeaderDimensions;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.MenuStrip menuStrip2;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem preferencesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem searchOptionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem taggedFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem untaggedFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem filepathsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tagsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem regularExpressionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem caseSensitiveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem foldersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sortToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem byTagToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem byHashToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem byLastModifiedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem organiseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem buildToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem manageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addTagToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeTagToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem buildTagDatabaseFilenamesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renameTagToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setAsWallpaperToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem downloadToToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uploadToToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem imagurToolStripMenuItem;

    }
}

