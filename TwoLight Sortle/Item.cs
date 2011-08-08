using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Extensions;

namespace TwoLight_Sortle {
    /// <summary>
    /// Represents an individual image
    /// </summary>
    class Item {
#region Private Instance Variables
        private HashSet<Tag> _tags;
        private Directory _directory;
        private long _filesize;
        private uint _hash;
        private Size _dimensions;
        private HashSet<string> _links;
#endregion

#region Public Read-Only interfaces
        /// <summary>
        /// The absolute path of the image
        /// </summary>
        public string Path { get; internal set; }

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
            get { return _tags; }
        }

        /// <summary>
        /// The image dimensions
        /// </summary>
        public Size Dimensions {
            get {
                if (_dimensions.IsEmpty) {
                    System.Drawing.Image tempImage = System.Drawing.Image.FromFile(Path);
                    _dimensions = tempImage.Size;
                    tempImage.Dispose();
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
                    FileInfo info = new FileInfo(Path);
                    _filesize = info.Length;
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
                    FileInfo info = new FileInfo(Path);
                    _filesize = info.Length;
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
#endregion

#region Constructors
        public Item(string path) {
            Path = path;
            _tags = new HashSet<Tag>();
            _dimensions = new Size();
            _filesize = 0;
            _hash = 0;
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
            imageFile.Close();
            if (resize!=new Size()) {
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
            return newImage;
            
        }
#endregion

#region Public Methods
        /// <summary>
        /// Add a Tag to this Image (No duplicate tags will be added)
        /// </summary>
        /// <param name="tag">The tag to add</param>
        public void Add(Tag tag) {
            _tags.Add(tag);
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
            _tags.Remove(tag);
        }

        /// <summary>
        /// Creates a symbolic link to this image.
        /// </summary>
        /// <param name="target">The path to link to</param>
        /// <returns>Whether we were successfull or not (If not it's probably due to access restrictions)</returns>
        public bool Link(string target) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Renames this file.
        /// </summary>
        /// <param name="newFileName">The new filename, including extension</param>
        public void Rename(string newFileName) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Uploads this file and returns the uploaded URL
        /// </summary>
        /// <returns></returns>
        public string Upload() {
            throw new NotImplementedException();
        }

        #endregion

        public override string ToString() {
            if (_tags.Count > 0) {
                return String.Join(", ", _tags);
            }
            else {
                return Path;
            }
        }
    }
}
