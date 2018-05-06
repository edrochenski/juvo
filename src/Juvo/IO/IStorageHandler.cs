// <copyright file="IStorageHandler.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.IO
{
    using System.IO;

    /// <summary>
    /// Provides a mechanism for working with storage IO (disk, file, directory, ...)
    /// </summary>
    public interface IStorageHandler
    {
        /// <summary>
        /// Creates all directories and subdirectories in the specified path unless they already exist.
        /// </summary>
        /// <param name="path">Directory to create.</param>
        /// <returns>
        /// An object that represents the directory at the specified path. This object is
        /// returned regardless of whether a directory at the specified path already exists.
        /// </returns>
        DirectoryInfo DirectoryCreate(string path);

        /// <summary>
        /// Determines whether the given path refers to an existing directory on disk.
        /// </summary>
        /// <param name="path">The path to test.</param>
        /// <returns>
        /// true if path refers to an existing directory; false if the directory does not
        /// exist or an error occurs when trying to determine if the specified directory exists.
        /// </returns>
        bool DirectoryExists(string path);

        /// <summary>
        /// Returns the names of files (including their paths) in the specified directory.
        /// </summary>
        /// <param name="path">
        /// The relative or absolute path to the directory to search. This string is not
        /// case-sensitive.
        /// </param>
        /// <returns>
        /// An array of the full names (including paths) for the files in the specified directory,
        /// or an empty array if no files are found.
        /// </returns>
        string[] DirectoryGetFiles(string path);

        /// <summary>
        /// Deletes the specified file.
        /// </summary>
        /// <param name="path">The name of the file to be deleted. Wildcard characters are not supported.</param>
        void FileDelete(string path);

        /// <summary>
        /// Determines whether the specified file exists.
        /// </summary>
        /// <param name="path">The file to check.</param>
        /// <returns>
        /// true if the caller has the required permissions and path contains the name of
        /// an existing file; otherwise, false. This method also returns false if path is
        /// null, an invalid path, or a zero-length string. If the caller does not have sufficient
        /// permissions to read the specified file, no exception is thrown and the method
        /// returns false regardless of the existence of path.
        /// </returns>
        bool FileExists(string path);

        /// <summary>
        /// Opens a text file, reads all lines of the file, and then closes the file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        /// <returns>A string containing all lines of the file.</returns>
        string FileReadAllText(string path);
    }
}
