using Helpers;
using DatabaseManager;
using MonoTorrent.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using ViewModels;

namespace ViewModels {
    /// <summary>
    /// ViewModel to track the files locally or in the database
    /// </summary>
    public class FileTrackerViewModel : BaseViewModel {
        // A static list of all known files for the current user, accessible throughout the app
        public static List<string> knownFiles;

        /// <summary>
        /// Constructor; initializes the knownFiles list,
        /// and creates a central directory for files and torrents, if it does not already exist
        /// </summary>
        public FileTrackerViewModel() {
            knownFiles = new List<string>();

            if(!Directory.Exists(Settings.FILES_DIRECTORY)) {
                Directory.CreateDirectory(Settings.FILES_DIRECTORY);
            }
        }
        
        /// <summary>
        /// Checks for, adds to the database, and returs a list of new local files asynchronously
        /// </summary>
        /// <returns>A list of new files</returns>
        public async Task<List<string>> CheckForNewFiles() {
            string hash = "";

            List<string> files = Directory.GetFiles(Settings.FILES_DIRECTORY).ToList();
            List<string> directories = Directory.GetDirectories(Settings.FILES_DIRECTORY).ToList();

            if(files != null && files.Count > 0) {
                foreach(string file in files) {
                    if(!file.Contains("fastresume.data") && !file.EndsWith(".torrent")) {
                        string torrentPath = Utils.CreateTorrent(file, Settings.FILES_DIRECTORY);
                        hash = Utils.GetTorrentInfoHash(torrentPath);

                        if(!knownFiles.Contains(hash)) {
                            if(!(await (new FileManager().FileHashExistsAsync(hash)))) {
                                await AddFileToDatabase(file, hash, torrentPath); // SOMETIMES THE APP SEES A MODIFIED FILE AS A NEW FILE. FIX IT.
                            }
                        }
                    }
                }
            }

        if(directories != null && directories.Count > 0) {
            foreach(string directory in directories) {
                    string torrentPath = Utils.CreateTorrent(directory, Settings.FILES_DIRECTORY);
                    hash = Utils.GetTorrentInfoHash(torrentPath);

                    if(!knownFiles.Contains(hash)) {
                        if(!(await (new FileManager().FileHashExistsAsync(hash)))) {
                            await AddFileToDatabase(directory, hash, torrentPath);
                        }
                    }
                }
            }

            return knownFiles;
        }

        /// <summary>
        /// Add a file to the database asynchronously
        /// </summary>
        /// <param name="file">The file path of the file</param>
        /// <param name="hash">The hash of the torrent of the file</param>
        /// <param name="torrentPath">The path of the torrent of the file</param>
        /// <param name="fileVersion">OPTIONAL: the file version (defaults to 0, as it is a new file)</param>
        /// <returns>Nothing, as it updates the file in the database, as well as the list of known files</returns>
        public async Task AddFileToDatabase(string file, string hash, string torrentPath, int fileVersion = 0) {
            try {
                if(!file.Contains(Settings.FILES_DIRECTORY)) {
                    file = Settings.FILES_DIRECTORY + "\\" + file;
                }

                FileManager fileManager = new FileManager();

                FileInfo fileInfo = new FileInfo(file);
                long fileSize = fileInfo.Length;
                DateTime added = fileInfo.CreationTimeUtc;
                DateTime lastModified = fileInfo.LastWriteTimeUtc;

                if(!(await fileManager.GetFileByHashAsync(hash) != null)) {
                    await fileManager.AddFileAsync(Guid.NewGuid().ToString(), Path.GetFileName(file), hash, Utils.GetFileMD5Hash(file), fileSize, added, lastModified, Settings.USER_ID, Settings.DEVICE_ID, await Utils.ReadFileAsync(torrentPath), fileVersion);

                    knownFiles.Add(hash);
                }
            } catch(Exception ex) {
                throw ex;
            }
        }

        /// <summary>
        /// Updates the file (using the file id) in the database asynchronously
        /// </summary>
        /// <param name="fileId">The file's ID</param>
        /// <param name="file">File's path</param>
        /// <param name="hash">Hash of the file's torrent</param>
        /// <param name="fileMD5">The MD5 hash of the file</param>
        /// <param name="torrentPath">The path of the torrent file</param>
        /// <param name="newFileHash">New hash of the updated file's torrent</param>
        /// <param name="fileVersion">File's new version</param>
        /// <param name="newFileMD5">The new MD5 hash of the updated file</param>
        /// <returns></returns>
        public async Task UpdateFileInDatabase(string fileId, string file, string hash, string fileMD5, string torrentPath, string newFileHash, int fileVersion, string newFileMD5) {
            try {
                FileManager fileManager = new FileManager();

                FileInfo fileInfo = new FileInfo(file);
                long fileSize = fileInfo.Length;
                DateTime lastModified = fileInfo.LastWriteTimeUtc;

                if((await fileManager.GetFileByIdAsync(fileId) != null)) {
                    await fileManager.UpdateFile(fileId, Path.GetFileName(file), hash, fileMD5, fileSize, lastModified, Settings.USER_ID, Settings.DEVICE_ID, await Utils.ReadFileAsync(torrentPath), fileVersion, newFileHash, newFileMD5);

                    if(knownFiles != null && knownFiles.Count > 0) {
                        knownFiles[knownFiles.IndexOf(knownFiles.Where(f => f == hash).FirstOrDefault())] = newFileHash;
                    }
                }
            } catch(Exception ex) {
                throw ex;
            }
        }

        /// <summary>
        /// The base file removal method that calls other methods (local, database, torrent)
        /// </summary>
        /// <param name="bitSynkTorrentModel">The custom Torrent model of BitSynk</param>
        /// <returns>Nothing, as the method runs asynchronously</returns>
        public async Task RemoveFileAsync(Models.BitSynkTorrentModel bitSynkTorrentModel) {
            await DeleteFileLocally(bitSynkTorrentModel.Name);
            await DeleteTorrent(bitSynkTorrentModel.Name);
            //await AddFileToRemoveQueue(bitSynkTorrentModel);
            await DeleteFileFromDatabase(bitSynkTorrentModel);
        }

        /// <summary>
        /// Deletes a file from the database asynchronously
        /// </summary>
        /// <param name="bitSynkTorrentModel">The custom Torrent model of BitSynk</param>
        /// <returns>Nothing, as the file is deleted from the database asynchronously</returns>
        public async Task DeleteFileFromDatabase(Models.BitSynkTorrentModel bitSynkTorrentModel) {
            await new FileManager().RemoveFileByHashAsync(bitSynkTorrentModel.Hash, Settings.USER_ID);
        }

        /// <summary>
        /// Deletes a torrent file
        /// </summary>
        /// <param name="fileName">The name of the torrent file</param>
        /// <returns>Nothing, as the file is deleted asynchronously</returns>
        public async Task DeleteTorrent(string fileName) {
            File.Delete(Settings.FILES_DIRECTORY + "//" + Path.GetFileNameWithoutExtension(fileName) + ".torrent");
        }

        /// <summary>
        /// Deletes a file (or a folder) locally
        /// </summary>
        /// <param name="fileName">The name of the file</param>
        /// <returns>Nothing, as the files (or folders) get removed asynchronously</returns>
        public async Task DeleteFileLocally(string fileName) {
            string fileOrFolder = Settings.FILES_DIRECTORY + "\\" + fileName;

            if(File.Exists(fileOrFolder)) {
                File.Delete(fileOrFolder);
            } else if(Directory.Exists(fileOrFolder)) {
                var dir = new DirectoryInfo(fileOrFolder);

                await SetAttributesNormal(dir);
                
                Directory.Delete(dir.FullName, true);
            }
        }

        /// <summary>
        /// Sets attributes of files and folders in a directory to normal. Ensures that the files are accessible.
        /// </summary>
        /// <param name="dir">The directory information of the parent directory</param>
        /// <returns>Nothing, as the attributes are set asynchronously</returns>
        private async Task SetAttributesNormal(DirectoryInfo dir) {
            foreach(var subDir in dir.GetDirectories()) {
                await SetAttributesNormal(subDir);
                subDir.Attributes = FileAttributes.Normal;
            }
            foreach(var file in dir.GetFiles()) {
                file.Attributes = FileAttributes.Normal;
            }
        }
        
        /// <summary>
        /// Gets the files owned by to remove from the database asynchronously
        /// </summary>
        /// <param name="userId">The user's ID</param>
        /// <returns>List of files to remove</returns>
        public async Task<List<Models.File>> GetFilesToRemove(string userId) {
            return await new FileManager().GetFilesToRemoveAsync(userId);
        }

        /// <summary>
        /// Deletes local files and their torrents found in the 'FILES_TO_REMOVE' table in the database asynchronously
        /// </summary>
        /// <returns>List of files (file paths) removed</returns>
        public async Task<List<string>> DeleteFilesInQueue() {
            List<string> filesToDelete = new List<string>();
            foreach(Models.File file in await GetFilesToRemove(Settings.USER_ID)) {
                await DeleteFileLocally(file.FileName);
                await DeleteTorrent(file.FileName);

                filesToDelete.Add(file.FileHash);
            }

            return filesToDelete;
        }

        /// <summary>
        /// Renames a file locally asynchronously
        /// </summary>
        /// <param name="oldFileName">Old name of the file</param>
        /// <param name="newFileName">New name of the file</param>
        /// <returns></returns>
        public async Task RenameFileAsync(string oldFileName, string newFileName) {
            File.Move(Settings.FILES_DIRECTORY + "\\" + oldFileName, Settings.FILES_DIRECTORY + "\\" + newFileName);
        }

        //private async void AddFileToRemoveQueue(BitSynkTorrentModel bitSynkTorrentModel) {
        //    FileManager fileManager = new FileManager();

        //    await fileManager.AddFileToRemoveQueueAsync(new DatabaseManager.Models.File() {
        //        FileName = bitSynkTorrentModel.Name,
        //        FileHash = bitSynkTorrentModel.Hash,
        //        FileId = (await fileManager.GetFileByHashAsync(bitSynkTorrentModel.Hash)).FileId,
        //        UserId = Settings.USER_ID
        //    });
        //}
    }
}
