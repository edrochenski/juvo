// <copyright file="StorageHandler.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.IO
{
    using System;
    using System.IO;

    /// <summary>
    /// Default (BCL) storage handler.
    /// </summary>
    public class StorageHandler : IStorageHandler
    {
        /// <inheritdoc/>
        public DirectoryInfo DirectoryCreate(string path)
        {
            return Directory.CreateDirectory(Environment.ExpandEnvironmentVariables(path));
        }

        /// <inheritdoc/>
        public bool DirectoryExists(string path)
        {
            return Directory.Exists(Environment.ExpandEnvironmentVariables(path));
        }

        /// <inheritdoc/>
        public string[] DirectoryGetFiles(string path)
        {
            return Directory.GetFiles(Environment.ExpandEnvironmentVariables(path));
        }

        /// <inheritdoc/>
        public void FileDelete(string path)
        {
            File.Delete(Environment.ExpandEnvironmentVariables(path));
        }

        /// <inheritdoc/>
        public bool FileExists(string path)
        {
            return File.Exists(Environment.ExpandEnvironmentVariables(path));
        }

        /// <inheritdoc/>
        public string FileReadAllText(string path)
        {
            return File.ReadAllText(Environment.ExpandEnvironmentVariables(path));
        }

        /// <inheritdoc/>
        public void FileWriteAllText(string path, string contents)
        {
            File.WriteAllText(Environment.ExpandEnvironmentVariables(path), contents);
        }
    }
}
