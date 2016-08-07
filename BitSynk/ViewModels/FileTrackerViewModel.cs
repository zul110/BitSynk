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
            
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e) {
            CheckForNewFiles();
        }

        private async void CheckForNewFiles() {
            string hash = "";

            List<string> files = Directory.GetFiles(Settings.FILES_DIRECTORY).ToList();

            if(files != null && files.Count > 0) {
                foreach(string file in files) {
                    if(!file.Contains("fastresume.data") && !knownFiles.Contains(file) && !file.EndsWith(".torrent")) {
                        hash = Utils.GetTorrentInfoHash(Utils.CreateTorrent(file, Settings.FILES_DIRECTORY));

                        if(!(await(new FileManager().FileHashExistsAsync(hash)))) {
                            AddFileToDatabase(file, hash);
                        }
                    }
                }
            }
        }

        public async void AddFileToDatabase(string file, string hash) {
            try {
                FileManager fileManager = new FileManager();

                if(!(await fileManager.GetUserFileByHashAsync(hash, Settings.USER_ID) != null)) {
                    await fileManager.AddFileAsync(Guid.NewGuid().ToString(), Path.GetFileName(file), hash, Settings.USER_ID, Settings.DEVICE_ID);

                    knownFiles.Add(file);
                }
            } catch(Exception ex) {
                throw ex;
            }
        }
    }
}
