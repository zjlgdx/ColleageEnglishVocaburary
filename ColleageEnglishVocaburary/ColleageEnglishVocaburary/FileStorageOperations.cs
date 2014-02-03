using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace CaptainsLog
{
    class FileStorageOperations
    {
        /// <summary>
        /// Saves a string to a file using the WP7.1 style APIs - also supported in WP8
        /// </summary>
        /// <param name="logData">string data to save</param>
        public static void SaveToIsolatedStorage(string logData)
        {
            // Get a reference to the Local Folder
            IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();

            using (IsolatedStorageFileStream fs = new IsolatedStorageFileStream(
                        "CaptainsLog.store", FileMode.Create, isf))
            {
                StreamWriter writer = new StreamWriter(fs);
                writer.Write(logData);
                writer.Close();
            };
        }

        /// <summary>
        /// Saves a string to a file using the WP8 APIs - not supported in WP7.1
        /// </summary>
        /// <param name="logData">string data to save</param>
        public async static Task SaveToLocalFolderAsync(string desiredName, string logData)
        {
            // Get a reference to the Local Folder
            Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            // Create the file in the local folder, or if it already exists, just open it
            Windows.Storage.StorageFile storageFile =
                await localFolder.CreateFileAsync(desiredName, CreationCollisionOption.ReplaceExisting);

            Stream writeStream = await storageFile.OpenStreamForWriteAsync();
            using (StreamWriter writer = new StreamWriter(writeStream))
            {
                await writer.WriteAsync(logData);
            }

            // Other winRT APIs exist to achieve the same thing, although less concisely. For example:
            //
            //var inputStream = await storageFile.OpenAsync(FileAccessMode.ReadWrite);
            //var writeStream = inputStream.GetOutputStreamAt(0);
            //Windows.Storage.Streams.DataWriter writer = new Windows.Storage.Streams.DataWriter(writeStream);
            //writer.WriteString(logData);
            //await writer.StoreAsync();
            //await writeStream.FlushAsync();
        }

        public static async Task SaveToLocalFolderAsync(string desiredName,Stream stream)
        {
            // Get a reference to the Local Folder
            Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            // Create the file in the local folder, or if it already exists, just open it
            Windows.Storage.StorageFile storageFile =
                await localFolder.CreateFileAsync(desiredName, CreationCollisionOption.ReplaceExisting);

            Stream writeStream = await storageFile.OpenStreamForWriteAsync();
            //using (BinaryWriter writer = new BinaryWriter(writeStream))
            //{
            //    byte[] bytes = new byte[stream.Length];
            //    await stream.ReadAsync(bytes, 0, bytes.Length);
            //    // 设置当前流的位置为流的开始
            //    stream.Seek(0, SeekOrigin.Begin);

            //    await writer.WriteAsync(bytes, 0, bytes.Length);
            //}

            
            using (BinaryWriter BW = new BinaryWriter(writeStream))
                {
                    Stream S = stream;
                    long lg = S.Length;
                    byte[] bff = new byte[32];
                    int Count = 0;
                    using (BinaryReader BR = new BinaryReader(stream))
                    {
                        // Now we have to read file in chunks in order to reduce memory consumption and increase performance
                        while (Count < lg)
                        {
                            int actual = BR.Read(bff, 0, bff.Length);
                            Count += actual;
                            BW.Write(bff, 0, actual);

                        }
                       
                    }
                }
        }

        /// <summary>
        /// Loads a string from a file using the WP 7.1 style APIs
        /// </summary>
        /// <returns></returns>
        public static string LoadFromIsolatedStorage()
        {
            string theData = string.Empty;

            var isf = IsolatedStorageFile.GetUserStoreForApplication();

            if (!isf.FileExists("CaptainsLog.store"))
            {
                // Initialise the return data
                theData = string.Empty;
            }
            else
            {
                using (IsolatedStorageFileStream fs = new IsolatedStorageFileStream(
                            "CaptainsLog.store", FileMode.Open, isf))
                {
                    StreamReader reader = new StreamReader(fs);
                    theData = reader.ReadToEnd();
                    reader.Close();
                };
            }

            return theData;
        }

        public async static Task<string> LoadFromLocalFolderAsync()
        {
            string theData = string.Empty;

            // There's no FileExists method in WinRT, so have to try to get a reference to it
            // and catch the exception instead
            StorageFile storageFile = null;
            bool fileExists = false;
            try
            {
                // See if file exists
                storageFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(
                    new Uri("ms-appdata:///local/CaptainsLog.store "));
                fileExists = true;
            }
            catch (FileNotFoundException)
            {
                // File doesn't exist
                fileExists = false;
            }

            if (!fileExists)
            {
                // Initialise the return data
                theData = string.Empty;
            }
            else
            {
                // File does exists, so open it and read the contents
                Stream readStream = await storageFile.OpenStreamForReadAsync();
                using (StreamReader reader = new StreamReader(readStream))
                {
                    theData = await reader.ReadToEndAsync();
                }            
            }

            return theData;
        }
    }
}
