using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Linq;
using Extensions;

namespace TwoLight_Sortle {
    /// <summary>
    /// Represents an individual image
    /// </summary>
    [Serializable]
    class Item : ISerializable{
        #region Private Instance Variables
        private List<Tag> _tags;
        private long _filesize;
        private uint _hash;
        private Size _dimensions;
        private HashSet<string> _links;
        private bool _invalidated;
        private string _externalUrl;
        private bool _uploaded;
        private bool _tagRenameStateChanged;
        #endregion

        #region WinAPI Declarations
        [DllImport("kernel32.dll", EntryPoint = "CreateSymbolicLinkW", CharSet = CharSet.Unicode)]
        public static extern bool CreateSymbolicLink([In] string lpSymlinkFileName, [In] string lpTargetFileName,
                                                     int dwFlags);
        #endregion

        #region Public Read-Only interfaces
        /// <summary>
        /// The absolute path of the image
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The filename of the image, without path or extension
        /// Setter renames the file and removes all orphaned symbolic links.
        /// </summary>
        public string Filename {
            get { return System.IO.Path.GetFileNameWithoutExtension(Path); }
            set { Rename(value+Extension); }
        }

        /// <summary>
        /// The extension of the image, with the dot.
        /// </summary>
        public string Extension {
            get { return System.IO.Path.GetExtension(Path); }
        }

        /// <summary>
        /// The Directory this item resides in
        /// </summary>
        public string Directory {
            get { return System.IO.Path.GetDirectoryName(Path); }
        }

        /// <summary>
        /// The full size image 
        /// </summary>
        public Image Image {
            get { return getImage(); }
        }

        /// <summary>
        /// An image resized to be appropriate for a thumbnail
        /// </summary>
        public Image Thumb {
            get { return getImage(new Size(100, 100)); }
        }

        /// <summary>
        /// The list of Tags associated with this image
        /// </summary>
        public IEnumerable<Tag> Tags {
            get {
                return (from tag in _tags orderby tag.Name select tag).ToList();
            }
        }

        /// <summary>
        /// The image dimensions
        /// </summary>
        public Size Dimensions {
            get {
                if (_dimensions.IsEmpty) {
                    getDimensions();
                }
                return _dimensions;
            }
        }

        /// <summary>
        /// The image's filesize string, in the most appropriate format.
        /// </summary>
        public string Filesize {
            get {
                if (_filesize == 0) {
                    getFilesize();
                }
                string[] symbols = new[] {"B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"}; //Future proofing!
                long returnSize = _filesize;
                int symbol = 0;
                while (returnSize > 1024) {
                    symbol++;
                    returnSize /= 1024;
                }
                return "{0} {1}".With(returnSize, symbols[symbol]);
            }
        }

        /// <summary>
        /// The length of the image in bytes.
        /// </summary>
        public long RawFilesize {
            get {
                if (_filesize == 0) {
                    getFilesize();
                }
                return _filesize;
            }
        }

        /// <summary>
        /// The Hash of the image. This will be unique within the system, even in the unlikely event two images collide.
        /// </summary>
        public UInt32 Hash {
            get {
                if (_hash == 0) {
                    _hash = Load.Hash(Path);
                }
                return _hash;
            }
        }

        /// <summary>
        /// Returns whether the image has any associated tags
        /// </summary>
        public bool HasTags {
            get { return Tags.Count() > 0; }
        }

        /// <summary>
        /// Returns whether the image linking state has been invalidated - by renaming or moving the file or such.
        /// </summary>
        public bool Invalidated {
            get { return _invalidated; }
            set { _invalidated = value; }
        }

        public bool Exists {
            get { return System.IO.File.Exists(Path); }
        }

        public string ExternalUrl {
            get {
                if (_externalUrl == null) {
                    return "";
                }
                else {
                    return _externalUrl;
                }
            }
            set {
                if (!_uploaded) {
                    _externalUrl = value;
                }
            }
        }

        public string UploadedUrl {
            get {
                if (!_uploaded) {
                    _externalUrl = Upload();
                }
                return _externalUrl;
            }
        }

        #endregion

#region Constructors
        public Item(string path) {
            Path = path;
            _tags = new List<Tag>();
            //getDimensions();
            _dimensions = new Size();
            //getFilesize();
            _filesize = 0;
            _hash = 0;
            _invalidated = true;
            _links = new HashSet<string>();
            _externalUrl = null;
            _tagRenameStateChanged = true;
        }

        public Item(SerializationInfo info, StreamingContext context) {
            Path = info.GetString("Path");
            _tags = (List<Tag>) info.GetValue("_tags", typeof (List<Tag>));
            _filesize = (long) info.GetValue("_filesize", typeof (long));
            _hash = info.GetUInt32("_hash");
            _dimensions = (Size) info.GetValue("_dimensions", typeof (Size));
            _links = (HashSet<string>) info.GetValue("_links", typeof (HashSet<string>));
            _invalidated = info.GetBoolean("_invalidated");
            _externalUrl = info.GetString("_externalUrl");
            _uploaded = info.GetBoolean("_uploaded");
            _tagRenameStateChanged = info.GetBoolean("_tagRenameStateChanged");
        }

        #endregion

#region Private Methods
        /// <summary>
        /// Returns the image, at a specified Size if given
        /// </summary>
        /// <param name="resize">Optional size to fit the image into.</param>
        /// <returns></returns>
        private Image getImage(Size resize = new Size()) {
            Stream imageFile = File.Open(Path, FileMode.Open, FileAccess.Read);
            Image newImage = Image.FromStream(imageFile);
            FrameDimension frameDimension = new FrameDimension(newImage.FrameDimensionsList[0]);
            if (newImage.GetFrameCount(frameDimension) > 1) {
                resize = newImage.Size;
            }
            if (!resize.IsEmpty) {
                Bitmap resizedImage = new Bitmap(resize.Width, resize.Height);
                using (Graphics gr = Graphics.FromImage(resizedImage)) {
                    int newWidth, newHeight;
                    if (newImage.Width > newImage.Height) {
                        newWidth = resize.Width;
                        newHeight = (newImage.Height * newWidth) / newImage.Width;
                    }
                    else {
                        newHeight = resize.Height;
                        newWidth = (newImage.Width * newHeight) / newImage.Height;
                    }
                    gr.DrawImage(newImage, 0, 0, newWidth, newHeight);
                }
                newImage.Dispose();
                newImage = resizedImage;
            }
            imageFile.Close();
            return newImage;
        }

        private void getDimensions() {
            System.Drawing.Image tempImage = Image;
            _dimensions = tempImage.Size;
            tempImage.Dispose();
        }

        private void getFilesize() {
            FileInfo info = new FileInfo(Path);
            _filesize = info.Length;
            info = null;
            GC.Collect();
        }

        private void removeLinks() {
            foreach (string link in _links) {
                if (File.Exists(link)) {
                    File.Delete(link);
                }
            }
        }
#endregion

#region Public Methods
        /// <summary>
        /// Add a Tag to this Image (No duplicate tags will be added)
        /// </summary>
        /// <param name="tag">The tag to add</param>
        public void Add(Tag tag) {
            if (!_tags.Contains(tag)) {
                tag.AddedImage(this);
                _tags.Add(tag);
                _tagRenameStateChanged = true;
            }
        }

        /// <summary>
        /// Add a tag to this Image
        /// </summary>
        /// <param name="tag">The string value of the tag</param>
        public void Add(string tag) {
            Add(TwoLight_Sortle.Tags.GetTag(tag));
        }

        /// <summary>
        /// Removes a Tag from this image, if it exists
        /// </summary>
        /// <param name="tag">The tag to remove</param>
        public void Remove(Tag tag) {
            if (_tags.Contains(tag)) {
                tag.RemovedImage(this);
                _tags.Remove(tag);
                _tagRenameStateChanged = true;
            }
        }

        /// <summary>
        /// Remove a tag from this image, if it exists
        /// </summary>
        /// <param name="tag">the name of the tag</param>
        public void Remove(string tag) {
            Remove(TwoLight_Sortle.Tags.GetTag(tag));
        }

        /// <summary>
        /// Creates a symbolic link to this image.
        /// </summary>
        /// <param name="target">The path to link to</param>
        /// <returns>Whether we were successfull or not (If not it's probably due to access restrictions)</returns>
        public void Link(string target) {
            if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(target))) {
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(target));
            }
            CreateSymbolicLink(target, Path, 0);
            _links.Add(target);
            _invalidated = false;
        }

        /// <summary>
        /// Renames this file.
        /// </summary>
        /// <param name="newFileName">The new filename, including extension</param>
        public void Rename(string newFileName) {
            if ((Filename + Extension) == newFileName || !_tagRenameStateChanged) {
                return;
            }
            string pathRoot = System.IO.Path.GetDirectoryName(Path);
            string newPath = System.IO.Path.Combine(pathRoot, newFileName);
            int numFiles = 0;
            while (File.Exists(newPath)) {
                string filenameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(newFileName);
                newPath = System.IO.Path.Combine(pathRoot,
                                                 "{0} ({1}){2}".With(filenameWithoutExtension, ++numFiles, Extension));
            }
            File.Move(Path, newPath);
            Path = newPath;
            _invalidated = true;
            _tagRenameStateChanged = false;
            removeLinks();
        }

        /// <summary>
        /// Uploads this file and returns the uploaded URL
        /// </summary>
        /// <returns></returns>
        public string Upload() {
            string imageData = Convert.ToBase64String(File.ReadAllBytes(Path));
            NameValueCollection data = new NameValueCollection {
                                                                       {"key", "fb316777154d4a81fe16064fd73ce264"},
                                                                       {"image", imageData}
                                                                   };
            WebClient newClient = new WebClient();
            byte[] uploadValues = newClient.UploadValues(@"http://imgur.com/api/upload.xml", data);
            XDocument page = XDocument.Load(new MemoryStream(uploadValues));
            List<XElement> origImages = page.Descendants("original_image").ToList();
            string originalImage = (string)origImages[0].Value;
            _uploaded = true;
            return originalImage;
        }


        public void RenameToTags() {
            if (HasTags) {
                string newName = String.Join(" ", from tag in Tags orderby tag.Name descending select tag.Name);
                Filename = newName;
            }
        }


        public void RenameToHash() {
            Filename = Hash.ToString();
        }

        public void Delete() {
            if (Exists) {
                uint hash = Hash;
                File.Delete(Path);
                Load.FilesCache.Remove(hash);
            }
            removeLinks();
            foreach (Tag tag in Tags) {
                Remove(tag);
            }
        }
        #endregion

        public override string ToString() {
            if (HasTags) {
                return String.Join(", ", _tags);
            }
            else {
                return "None";
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("Path", Path);
            info.AddValue("_tags", _tags, _tags.GetType());
            info.AddValue("_filesize", _filesize);
            info.AddValue("_hash", _hash);
            info.AddValue("_dimensions", _dimensions);
            info.AddValue("_links", _links);
            info.AddValue("_invalidated", _invalidated);
            info.AddValue("_externalUrl", _externalUrl);
            info.AddValue("_uploaded", _uploaded);
            info.AddValue("_tagRenameStateChanged", _tagRenameStateChanged);
        }

    }
}
