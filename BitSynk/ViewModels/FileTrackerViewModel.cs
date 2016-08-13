using BitSynk.Helpers;
using DatabaseManager;
using MonoTorrent.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using BitSynk.Models;

namespace BitSynk.ViewModels {
    public class FileTrackerViewModel : BaseViewModel {
        private DispatcherTimer timer;
        public static List<string> knownFiles;

        public FileTrackerViewModel() {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += Timer_Tick;

            InitViewModel();
        }

        protected override void InitViewModel() {
            base.InitViewModel();

            knownFiles = new List<string>();

            if(!Directory.Exists(Settings.FILES_DIRECTORY)) {
                Directory.CreateDirectory(Settings.FILES_DIRECTORY);
            }
            
            //timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e) {
            CheckForNewFiles();
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
                                AddFileToDatabase(file, hash, torrentPath);
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
                            AddFileToDatabase(directory, hash, torrentPath);
                        }
                    }
                }
            }

            return knownFiles;
        }

        public async void AddFileToDatabase(string file, string hash, string torrentPath) {
            try {
                FileManager fileManager = new FileManager();

                if(!(await fileManager.GetFileByHashAsync(hash) != null)) {
                    await fileManager.AddFileAsync(Guid.NewGuid().ToString(), Path.GetFileName(file), hash, Settings.USER_ID, Settings.DEVICE_ID, await Utils.ReadFileAsync(torrentPath));

                    knownFiles.Add(hash);
                }
            } catch(Exception ex) {
                throw ex;
            }
        }

        public void RemoveFile(BitSynkTorrentModel bitSynkTorrentModel) {
            DeleteFileLocally(bitSynkTorrentModel.Name);
            DeleteTorrent(bitSynkTorrentModel.Name);
            //AddFileToRemoveQueue(bitSynkTorrentModel);
            DeleteFileFromDatabase(bitSynkTorrentModel);
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

        public async void DeleteFileFromDatabase(BitSynkTorrentModel bitSynkTorrentModel) {
            await new FileManager().RemoveFileByHashAsync(bitSynkTorrentModel.Hash, Settings.USER_ID);
        }

        public void DeleteTorrent(string fileName) {
            File.Delete(Settings.FILES_DIRECTORY + "//" + Path.GetFileNameWithoutExtension(fileName) + ".torrent");
        }

        public void DeleteFileLocally(string fileName) {
            File.Delete(Settings.FILES_DIRECTORY + "//" + fileName);
        }

        public async Task<List<DatabaseManager.Models.File>> GetFilesToRemove(string userId) {
            return await new FileManager().GetFilesToRemoveAsync(userId);
        }

        public async Task<List<string>> DeleteFilesInQueue() {
            List<string> filesToDelete = new List<string>();
            foreach(DatabaseManager.Models.File file in await GetFilesToRemove(Settings.USER_ID)) {
                DeleteFileLocally(file.FileName);
                DeleteTorrent(file.FileName);

                filesToDelete.Add(file.FileHash);
            }

            return filesToDelete;
        }
    }
}
