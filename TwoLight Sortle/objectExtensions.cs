using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Extensions {
    static class objectExtensions {
        /// <summary>
        /// Saves the object to disk under the filename
        /// </summary>
        /// <param name="toSerialise"></param>
        /// <param name="file"></param>
        public static void SaveToDisk(this object toSerialise, string file) {
            string savePath = Path.GetDirectoryName(Path.GetFullPath(file));
            
            if (!Directory.Exists(savePath)) {
                Directory.CreateDirectory(savePath);
            }
            Stream writeStream = File.Open(Path.Combine(savePath, file), FileMode.Create);
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(writeStream, toSerialise);
            writeStream.Close();
        }

        /// <summary>
        /// Loads the object and returns it
        /// </summary>
        /// <typeparam name="T">The Type</typeparam>
        /// <param name="toDeserialise">The Object (Will be returned if path cannot be deserialised as Type</param>
        /// <param name="path">The path to load from</param>
        /// <returns>The loaded object</returns>
        public static T LoadFromDisk<T>(this object toDeserialise, string path) {
            try {
                object newThing = toDeserialise;
                if (File.Exists(path)) {
                    Stream readStream = File.Open(path, FileMode.Open);
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    newThing = binaryFormatter.Deserialize(readStream);
                    readStream.Close();
                }
                return (T)newThing;
            }
            catch (Exception) {
                return (T)toDeserialise;
            }
        }

        public static string GetName<T>(this T item) where T : class {
            return typeof(T).GetProperties()[0].Name;
        }
    }
}
