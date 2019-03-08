// <copyright file="StorageHandler.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.IO
{
    using System.IO;

    /// <summary>
    /// Default (BCL) storage handler.
    /// </summary>
    public class StorageHandler : IStorageHandler
    {
        /// <inheritdoc/>
        public DirectoryInfo DirectoryCreate(string path)
        {
            return Directory.CreateDirectory(path);
        }

        /// <inheritdoc/>
        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        /// <inheritdoc/>
        public string[] DirectoryGetFiles(string path)
        {
            return Directory.GetFiles(path);
        }

        /// <inheritdoc/>
        public void FileDelete(string path)
        {
            File.Delete(path);
        }

        /// <inheritdoc/>
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        /// <inheritdoc/>
        public string FileReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        /// <inheritdoc/>
        public void FileWriteAllText(string path, string contents)
        {
            File.WriteAllText(path, contents);
        }
    }
}
