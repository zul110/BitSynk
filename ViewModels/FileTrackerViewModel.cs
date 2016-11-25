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
    public class FileTrackerViewModel : BaseViewModel {
        public static List<string> knownFiles;

        public FileTrackerViewModel() {
            InitViewModel();
        }

        protected override void InitViewModel() {
            base.InitViewModel();

            knownFiles = new List<string>();

            if(!Directory.Exists(Settings.FILES_DIRECTORY)) {
                Directory.CreateDirectory(Settings.FILES_DIRECTORY);
            }
        }
        
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
                    await fileManager.AddFileAsync(Guid.NewGuid().ToString(), Path.GetFileName(file), hash, fileSize, added, lastModified, Settings.USER_ID, Settings.DEVICE_ID, await Utils.ReadFileAsync(torrentPath), fileVersion);

                    knownFiles.Add(hash);
                }
            } catch(Exception ex) {
                throw ex;
            }
        }

        public async Task UpdateFileInDatabase(string fileId, string file, string hash, string torrentPath, string newFileHash, int fileVersion) {
            try {
                FileManager fileManager = new FileManager();

                FileInfo fileInfo = new FileInfo(file);
                long fileSize = fileInfo.Length;
                DateTime lastModified = fileInfo.LastWriteTimeUtc;

                if((await fileManager.GetFileByIdAsync(fileId) != null)) {
                    await fileManager.UpdateFile(fileId, Path.GetFileName(file), hash, fileSize, lastModified, Settings.USER_ID, Settings.DEVICE_ID, await Utils.ReadFileAsync(torrentPath), fileVersion, newFileHash);

                    if(knownFiles != null && knownFiles.Count > 0) {
                        knownFiles[knownFiles.IndexOf(knownFiles.Where(f => f == hash).FirstOrDefault())] = newFileHash;
                    }
                }
            } catch(Exception ex) {
                throw ex;
            }
        }

        public async Task RemoveFileAsync(Models.BitSynkTorrentModel bitSynkTorrentModel) {
            await DeleteFileLocally(bitSynkTorrentModel.Name);
            await DeleteTorrent(bitSynkTorrentModel.Name);
            //await AddFileToRemoveQueue(bitSynkTorrentModel);
            await DeleteFileFromDatabase(bitSynkTorrentModel);
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

        public async Task DeleteFileFromDatabase(Models.BitSynkTorrentModel bitSynkTorrentModel) {
            await new FileManager().RemoveFileByHashAsync(bitSynkTorrentModel.Hash, Settings.USER_ID);
        }

        public async Task DeleteTorrent(string fileName) {
            File.Delete(Settings.FILES_DIRECTORY + "//" + Path.GetFileNameWithoutExtension(fileName) + ".torrent");
        }

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

        private async Task SetAttributesNormal(DirectoryInfo dir) {
            foreach(var subDir in dir.GetDirectories()) {
                await SetAttributesNormal(subDir);
                subDir.Attributes = FileAttributes.Normal;
            }
            foreach(var file in dir.GetFiles()) {
                file.Attributes = FileAttributes.Normal;
            }
        }

        public async Task<List<Models.File>> GetFilesToRemove(string userId) {
            return await new FileManager().GetFilesToRemoveAsync(userId);
        }

        public async Task<List<string>> DeleteFilesInQueue() {
            List<string> filesToDelete = new List<string>();
            foreach(Models.File file in await GetFilesToRemove(Settings.USER_ID)) {
                await DeleteFileLocally(file.FileName);
                await DeleteTorrent(file.FileName);

                filesToDelete.Add(file.FileHash);
            }

            return filesToDelete;
        }
    }
}
