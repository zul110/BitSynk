using Helpers;
using ViewModels;
using DatabaseManager;
using MonoTorrent;
using MonoTorrent.BEncoding;
using MonoTorrent.Client;
using MonoTorrent.Client.Encryption;
using MonoTorrent.Client.Tracker;
using MonoTorrent.Common;
using MonoTorrent.Dht;
using MonoTorrent.Dht.Listeners;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace BitSynk {
    /// <summary>
    /// The core BitSynk Client
    /// Includes every functionality related to the synchronization process
    /// </summary>
    public class Client : INotifyPropertyChanged {
        // Arbitrary port: chosen at random; unused by other common services, to the best of our knowledge
        private int port = 52111;

        private static string dhtNodeFile;                              // File containing DHT node data
        private static string basePath;                                 // Base path for local storage operations
        private static string downloadsPath;                            // Path where the files are downloaded
        private static string fastResumeFile;                           // Fast resume file
        private static string torrentsPath;                             // Path where the torrents are stored
        private static ClientEngine engine;				                // The MonoTorrent engine used for downloading
        public static ObservableCollection<TorrentManager> torrents;	// The list where all the torrentManagers will be stored that the engine gives us
        private static Top10Listener listener;			                // This is a subclass of TraceListener which remembers the last 10 statements sent to it

        private EngineSettings engineSettings;                          // Settings for the engine
        private TorrentSettings torrentDefaults;                        // Default settings

        private List<RawTrackerTier> trackers;                          // List of trackers (defined, but unused in BitSynk)

        private BackgroundWorker refreshBw;

        List<string> files = new List<string>();                        // List of files added to the engine
        List<string> folders = new List<string>();                      // List of folders added to the engine
        List<Models.File> filesToDownload;                              // List of files to be downloaded

        // Custom BitSynk model of torrents: provides enough information to enable syncing
        public ObservableCollection<Models.BitSynkTorrentModel> bitSynkTorrents = new ObservableCollection<Models.BitSynkTorrentModel>();
        List<IPEndPoint> initialNodes = new List<IPEndPoint>();         // Initial DHT nodes

        /// <summary>
        /// The MonoTorrent Client Engine
        /// </summary>
        public ClientEngine Engine {
            get {
                return engine;
            }

            set {
                engine = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<TorrentManager> Torrents {
            get {
                return torrents;
            }

            set {
                torrents = value;
                NotifyPropertyChanged();
            }
        }
        
        // Timer that manages when to refresh the sync information
        private DispatcherTimer refreshTimer;

        /// <summary>
        /// Property Changed Event Handler
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string property = "") {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        /// <summary>
        /// Constructor:
        /// Initializes the BitSynk client with an initial set of DHT nodes
        /// Also creates a watcher for the files directory to watch for changes (modifications, additions, removals, and renames)
        /// </summary>
        /// <param name="initialNodes">Initial set of known DHT nodes</param>
        public Client(List<IPEndPoint> initialNodes) {
            this.initialNodes = initialNodes;

            CreateFileWatcher();

            refreshTimer = new DispatcherTimer();
            refreshTimer.Interval = TimeSpan.FromSeconds(5);
            refreshTimer.Tick += refreshTimer_Tick;

            /* Generate the paths to the folder we will save .torrent files to and where we download files to */
            basePath = Environment.CurrentDirectory;						            // This is the directory we are currently in
            torrentsPath = Path.Combine(basePath, Settings.FILES_DIRECTORY_NAME);		// This is the directory we will save .torrents to
            downloadsPath = Path.Combine(basePath, Settings.FILES_DIRECTORY_NAME);		// This is the directory we will save downloads to
            fastResumeFile = Path.Combine(torrentsPath, "fastresume.data");
            dhtNodeFile = Path.Combine(basePath, "DhtNodes");
            Torrents = new ObservableCollection<TorrentManager>();						// This is where we will store the torrentmanagers
            listener = new Top10Listener(10);

            // We need to cleanup correctly when the user closes the window by using ctrl-c
            // or an unhandled exception happens
            Console.CancelKeyPress += delegate { shutdown(); };
            AppDomain.CurrentDomain.ProcessExit += delegate { shutdown(); };
            AppDomain.CurrentDomain.UnhandledException += delegate (object sender, UnhandledExceptionEventArgs e) { Console.WriteLine(e.ExceptionObject); shutdown(); };
            Thread.GetDomain().UnhandledException += delegate (object sender, UnhandledExceptionEventArgs e) { Console.WriteLine(e.ExceptionObject); shutdown(); };
            
            // If the torrentsPath does not exist, we want to create it
            if(!Directory.Exists(torrentsPath))
                Directory.CreateDirectory(torrentsPath);

            InitTrackers();

            InitEngine();
        }

        /// <summary>
        /// Initializes a list of trackers from the Trackers class
        /// Defined, but not used in BitSynk
        /// </summary>
        private void InitTrackers() {
            List<string> trackers = new Trackers().trackers;
            this.trackers = new List<RawTrackerTier>();

            RawTrackerTier rawTrackerTier = new RawTrackerTier();

            foreach(string tracker in trackers) {
                rawTrackerTier.Add(tracker.Trim());
            }

            this.trackers.Add(rawTrackerTier);
        }

        /// <summary>
        /// Initializes the engine with default settings
        /// </summary>
        private void InitEngine() {
            engineSettings = new EngineSettings(downloadsPath, port);
            engineSettings.PreferEncryption = true;
            engineSettings.AllowedEncryption = EncryptionTypes.All;
            //engineSettings.GlobalMaxUploadSpeed = 30 * 1024;
            //engineSettings.GlobalMaxDownloadSpeed = 100 * 1024;
            //engineSettings.MaxReadRate = 1 * 1024 * 1024;


            // Create the default settings which a torrent will have.
            // 4 Upload slots - a good ratio is one slot per 5kB of upload speed
            // 50 open connections - should never really need to be changed
            // Unlimited download speed - valid range from 0 -> int.Max
            // Unlimited upload speed - valid range from 0 -> int.Max
            torrentDefaults = new TorrentSettings(4, 150, 0, 0);

            Engine = new ClientEngine(engineSettings);
            Engine.ChangeListenEndpoint(new IPEndPoint(IPAddress.Any, port));
            Engine.TorrentRegistered += (sender, e) => {
                if(!Engine.Torrents.Contains(e.TorrentManager)) {
                    Engine.Torrents.Add(e.TorrentManager);
                }
            };

            InitDHT();
        }

        /// <summary>
        /// Initializees the DHT, and registers it with the client engine; adds nodes to the engine
        /// </summary>
        private void InitDHT() {
            DhtListener dhtListner = new DhtListener(new IPEndPoint(IPAddress.Any, port));
            DhtEngine dht = new DhtEngine(dhtListner);
            Engine.RegisterDht(dht);
            dhtListner.Start();

            try {
                BEncodedList details = new BEncodedList();

                initialNodes.Add(new IPEndPoint(IPAddress.Parse(Utils.GetIPAddress()), port));
            } catch {
                Console.WriteLine("No existing dht nodes could be loaded");
            }

            StartEngine();
        }

        /// <summary>
        /// Starts the client engine, and starts the sync process (starts the refresh timer)
        /// </summary>
        public void StartEngine() {
            Engine.DhtEngine.Start(initialNodes);

            refreshTimer.Start();
        }

        /// <summary>
        /// Enables the file system to watch for and notify the changes in the files directory
        /// </summary>
        private void CreateFileWatcher() {
            FileSystemWatcher watcher = new FileSystemWatcher(Settings.FILES_DIRECTORY);
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            watcher.Changed += Watcher_Changed;                     // When a change occurs in any of the files or directories
            watcher.Created += Watcher_Created;                     // When a new file or folder is created
            watcher.Deleted += Watcher_Deleted;                     // When a file or folder is removed
            watcher.Renamed += Watcher_Renamed;                     // When a file or folder is renamed

            watcher.EnableRaisingEvents = true;                     // Starts the watcher
        }

        /// <summary>
        /// When a file has been modified, its records are updated, and other devices start receiving the latest version of the file
        /// </summary>
        /// <param name="sender">FileSystemWatcher</param>
        /// <param name="e">File system event arguments</param>
        private async void Watcher_Changed(object sender, FileSystemEventArgs e) {
            // Disable file system watcher events, as it raises multiple events at once (issue in the .NET implementation)
            (sender as FileSystemWatcher).EnableRaisingEvents = false;

            // As long as it is not a torrent file, or the fast resume file that gets modified, update the records
            if(!e.Name.EndsWith(".torrent") && !e.Name.Contains("fastresume.data")) {
                // If there are files to be downloaded (basically, files in the database)
                if(filesToDownload != null && filesToDownload.Count > 0) {
                    // Get the file with the matching name
                    Models.File fileToDownload = filesToDownload.Where(file => file.FileName == e.Name).FirstOrDefault();

                    // If there are torrents stored
                    if(Torrents != null && Torrents.Count > 0) {
                        // Get the matching torrent
                        TorrentManager torrent = Torrents.Where(t => t.Torrent.Name == e.Name).FirstOrDefault();

                        // If the torrent does exist
                        if(torrent != null) {
                            try {
                                // Get the stored torrent's hash
                                string hash = torrent.Torrent.InfoHash.ToString().Replace("-", "").ToString();

                                // Calculate the new modified file's new torrent file's hash (by first creating the torrent file)
                                string newHash = Utils.GetTorrentInfoHash(Utils.CreateTorrent(e.FullPath, Settings.FILES_DIRECTORY));

                                // Update the file in the detabase
                                await new FileTrackerViewModel().UpdateFileInDatabase(fileToDownload.FileId, e.FullPath, hash, fileToDownload.FileMD5, torrent.Torrent.TorrentPath, newHash, fileToDownload.FileVersion + 1, Utils.GetFileMD5Hash(e.FullPath));

                                // Remove the record of the previous torrent
                                Torrents.Remove(Torrents.Where(t => t.Torrent.InfoHash.ToString().Replace("-", "").ToString() == hash).FirstOrDefault());
                            } catch(Exception ex) {
                                // Do nothing...
                            }
                        }
                    }
                }
            }

            // Enable events again
            (sender as FileSystemWatcher).EnableRaisingEvents = true;
        }

        /// <summary>
        /// When a new file or directory is created, this handler gets triggered
        /// </summary>
        /// <param name="sender">FileSystemWatcher</param>
        /// <param name="e">File system event arguments</param>
        private void Watcher_Created(object sender, FileSystemEventArgs e) {
            // Does nothing, as this scenario is taken care of in the refresh method
        }

        /// <summary>
        /// When a file or directory is removed, this handler gets triggered
        /// </summary>
        /// <param name="sender">FileSystemWatcher</param>
        /// <param name="e">File system event arguments</param>
        private void Watcher_Deleted(object sender, FileSystemEventArgs e) {
            // Does nothing, as this scenario is taken care of in the refresh method
        }

        /// <summary>
        /// When a new file or directory is renamed, this handler updates the records in the database
        /// </summary>
        /// <param name="sender">FileSystemWatcher</param>
        /// <param name="e">File system event arguments</param>
        private async void Watcher_Renamed(object sender, RenamedEventArgs e) {
            await new FileManager().RenameFileByNameAsync(e.OldName, Settings.USER_ID, e.Name);
        }

        /// <summary>
        /// When the refresh timer's interval hits, call the Refresh method
        /// </summary>
        /// <param name="sender">Refresh timer</param>
        /// <param name="e">Empty event arguments</param>
        private void refreshTimer_Tick(object sender, EventArgs e) {
            Refresh();
        }

        /// <summary>
        /// 1.Re-initializes the file manager, file tracker, and the files and folders lists.
        /// 2. Triggers the following functions in the background:
        /// ** a. Removes files to be deleted
        /// ** b. Gets files to be downloaded from the database
        /// ** c. Updates modified files
        /// ** d. Adds new files to the downloads/sync queue
        /// ** e. Removes files that no longer exist locally, and notifies other devices
        /// 3. Verifies whether torrents have been added or not; notifies observers of torrents being added
        /// 4. (Re-)starts the syncing process with the newly formed information
        /// 5. Updates the status of each torrent
        /// 6. In case of an error, restarts the timer
        /// </summary>
        public void Refresh() {
            //if(refreshBw == null) {
                refreshBw = new BackgroundWorker();

                refreshBw.DoWork += async (s, ev) => {
                    try {
                        refreshTimer.Stop();

                        FileManager fileManager = new FileManager();
                        FileTrackerViewModel fileTrackerVM = new FileTrackerViewModel();
                        BEncodedDictionary fastResume = GetFastResumeFile();

                        files = new List<string>();
                        folders = new List<string>();

                        await RemoveFilesFromQueue(fileManager, fileTrackerVM);

                        VerifyTorrents();
                        StartSyncing();

                        UpdateStats();
                    } catch(Exception ex) {
                        refreshTimer.Start();
                    }
                };
            //}

            refreshBw.RunWorkerCompleted += (s, ev) => {
                if(ev.Error != null) {

                }
            };

            refreshBw.RunWorkerAsync();
        }

        /// <summary>
        /// Adds a new file to BitSynk for synching in the background
        /// </summary>
        /// <param name="filePath">Path of the new file</param>
        /// <param name="isFolder">Whether it is a folder or not</param>
        public void AddNewTorrent(string filePath, bool isFolder) {
            BackgroundWorker bw = new BackgroundWorker();

            bw.DoWork += async (s, ev) => {
                // Initializes a new file manager for the new file
                FileManager fileManager = new FileManager();

                // If it is a removed file, remove it from the remove queue, and add it as a new file
                // If this step is not done, the file will never be added
                if(await fileManager.IsRemovedFile(Settings.USER_ID, Path.GetFileName(filePath))) {
                    await fileManager.RemoveFileFromRemoveQueueByNameAsync(Path.GetFileName(filePath), Settings.USER_ID);
                }

                // Verify whether the new file or folder exists in the files directory
                string folder = Settings.FILES_DIRECTORY + "\\" + Path.GetFileNameWithoutExtension(filePath);
                string file = Settings.FILES_DIRECTORY + "\\" + Path.GetFileName(filePath);
                bool fileOrFolderExists = isFolder ? Directory.Exists(folder) : File.Exists(file);

                // If it doesn't (it exists elsewhere on the device),
                // Create it
                // Else, notify the user that the task already exists
                if(!fileOrFolderExists) {
                    BEncodedDictionary fastResume = GetFastResumeFile();
                    await CopyFileOrFolderToFilesDirectory(isFolder, filePath);
                } else {
                    MessageBox.Show("Task already exists!");
                }
            };

            bw.RunWorkerCompleted += (s, ev) => {
                if(ev.Error != null) {

                }
            };

            bw.RunWorkerAsync();
        }

        /// <summary>
        /// Copies the file or folder to the files directory
        /// This is done so that the original file does not get affected
        /// by modifications and/or deletions (especially at this stage of BitSynk)
        /// Also allows for easier management of the file
        /// </summary>
        /// <param name="isFolder">Whether it's a folder</param>
        /// <param name="filePath">Path of the file or folder</param>
        /// <returns>The path of the copied file</returns>
        private async Task<string> CopyFileOrFolderToFilesDirectory(bool isFolder, string filePath) {
            string fileCopy = isFolder ? await Utils.CopyFolder(filePath) : await Utils.CopyFile(filePath);
            string torrentFilePath = Utils.CreateTorrent(fileCopy, Settings.FILES_DIRECTORY);

            return torrentFilePath;
        }

        /// <summary>
        /// Checks for files that are renamed offline
        /// </summary>
        /// <param name="fileTrackerVM">The file tracker that is currently being used by the app</param>
        /// <returns>Nothing, as it is an asynchronous task</returns>
        private async System.Threading.Tasks.Task CheckForRenamedFiles(FileTrackerViewModel fileTrackerVM) {
            // Get each file in the files directory
            foreach(string localFile in Directory.GetFiles(Settings.FILES_DIRECTORY)) {
                // Get each file in the database (to be downloaded)
                foreach(Models.File fileToDownload in filesToDownload) {
                    try {
                        // Get their names
                        string localFileName = Path.GetFileName(localFile);
                        string fileToDownloadName = fileToDownload.FileName;

                        // Check if their names match
                        bool nameMatch = (localFileName == fileToDownloadName);

                        // Check if their MD5 hashes match
                        bool MD5Match = Utils.GetFileMD5Hash(localFile) == fileToDownload.FileMD5;

                        // If a file's MD5 matches, but its name doesn't, it is renamed
                        // Update its record in the database
                        if(MD5Match && !nameMatch) {
                            await fileTrackerVM.RenameFileAsync(localFileName, fileToDownloadName);
                        }
                    } catch(Exception ex) {
                        // Do nothing...
                    }
                }
            }
        }

        /// <summary>
        /// Check for files that are modified offline
        /// For each file in the folder that exists in the Torrents list:
        /// If the NAMEs are same, but the HASHes are different, it's a modified file
        /// Update the database with the modified file's info
        /// </summary>
        /// <param name="fileManager">The file manager currently used by the app</param>
        /// <param name="fileTrackerVM">The file tracker currently being used by the app</param>
        /// <returns>Nothing, as it is an asynchronous task</returns>
        private async System.Threading.Tasks.Task CheckForModifiedFiles(FileManager fileManager, FileTrackerViewModel fileTrackerVM) {
            List<TorrentManager> modifiedFiles = new List<TorrentManager>();

            foreach(string localFile in Directory.GetFiles(Settings.FILES_DIRECTORY)) {
                if(!localFile.Contains(".torrent") && !localFile.Contains("fastresume")) {
                    foreach(Models.File fileToDownload in filesToDownload) {
                        FileInfo localFileInfo = new FileInfo(localFile);

                        string fileToDownloadName = fileToDownload.FileName;
                        string localFileName = Path.GetFileName(localFile);

                        bool fileModified = IsFileModified(localFile, fileToDownload);

                        if((fileToDownloadName == localFileName) && fileModified) {
                            TorrentManager torrent = Torrents.Where(t => t.Torrent.Name == fileToDownloadName).FirstOrDefault();

                            if(torrent != null) {
                                try {
                                    string hash = torrent.Torrent.InfoHash.ToString().Replace("-", "").ToString();
                                    string newHash = Utils.GetTorrentInfoHash(Utils.CreateTorrent(localFile, Settings.FILES_DIRECTORY));

                                    await new FileTrackerViewModel().UpdateFileInDatabase(fileToDownload.FileId, localFile, hash, fileToDownload.FileMD5, torrent.Torrent.TorrentPath, newHash, fileToDownload.FileVersion + 1, Utils.GetFileMD5Hash(localFile));

                                    Torrents.Remove(Torrents.Where(t => t.Torrent.InfoHash.ToString().Replace("-", "").ToString() == hash).FirstOrDefault());
                                } catch(Exception ex) {
                                    // Do nothing...
                                }
                            }
                        }
                    }
                }
            }

            await CheckForNewFiles(fileManager, fileTrackerVM);
        }

        /// <summary>
        /// Check if a file has been modified
        /// </summary>
        /// <param name="localFile">Path of the modified file</param>
        /// <param name="fileToDownload">File to match with to check if it is modified</param>
        /// <returns></returns>
        private bool IsFileModified(string localFile, Models.File fileToDownload) {
            string localFileInfoHash = Utils.GetTorrentInfoHashOfFile(localFile);
            string torrentFileInfoHash = Utils.GetTorrentInfoHash(Settings.FILES_DIRECTORY + "\\" + Path.GetFileNameWithoutExtension(localFile) + ".torrent");

            #region OTHER METHODS NOT BEING USED CURRENTLY
            /*
             * NOT USED ANYMORE, BUT THIS WAS THE FIRST ALGORITHM DEVELOPED TO DETECT FILE MODIFICATION
             * STILL EXISTS BECAUSE IT _MIGHT_ BE NEEDED LATER
             * ALSO, IT TOOK ME A WHILE TO DEVELOP IT, SO I DON'T WANT TO WASTE ALL THAT EFFORT!
             * 
             * FileInfo localFileInfo = new FileInfo(localFile);

             * string fileToDownloadName = fileToDownload.FileName;
             * string localFileName = Path.GetFileName(localFile);

             * DateTime fileToDownloadLastModified = fileToDownload.LastModified;
             * DateTime localLastModified = localFileInfo.LastWriteTimeUtc;

             * DateTime fileToDownloadCreated = fileToDownload.Added;
             * DateTime localCreated = localFileInfo.CreationTimeUtc;

             * string localFileCreatedDate = localCreated.ToShortDateString();
             * string fileToDownloadCreatedDate = localLastModified.ToShortDateString();

             * string localFileCreatedTime = localCreated.ToString("HH:mm:ss");
             * string fileToDownloadCreatedTime = localLastModified.ToString("HH:mm:ss");

             * string localFileModifiedDate = localLastModified.ToShortDateString();
             * string fileToDownloadModifiedDate = fileToDownloadLastModified.ToShortDateString();

             * string localFileModifiedTime = localLastModified.ToString("HH:mm:ss");
             * string fileToDownloadModifiedTime = fileToDownloadLastModified.ToString("HH:mm:ss");

             * string localFileMD5 = Utils.GetFileMD5Hash(localFile);
             * string fileToDownloadMD5 = fileToDownload.FileMD5;
             * 
             * 
            // If the file's local CREATED date and time DOES NOT equal its MODIFIED date and time,
            // AND
            // If the local file's MODIFIED date and time is GREATED (newer) than the file's MODIFIED date and time in the database
            // ** OR
            // ** ** If the local file's MODIFIED date is GREATER than, or EQUAL to the file's MODIFIED date in the records
            // ** ** in the database (same day, or a later day)
            // ** ** AND
            // ** ** The local file's MODIFIED time is NOT equal to the file's MODIFIED time in the database,
            // THEN the file IS MODIFIED (according to its date/time)
            bool lastModifiedChanged =
                    (
                        localFileCreatedDate + "_" + localFileCreatedTime
                        !=
                        localFileModifiedDate + "_" + localFileModifiedTime
                    )
                    &&
                    (
                        DateTime.Parse(localFileModifiedDate)
                        >
                        DateTime.Parse(fileToDownloadModifiedDate)
                        ||
                        (
                            (
                                DateTime.Parse(localFileModifiedDate)
                                >=
                                DateTime.Parse(fileToDownloadModifiedDate)
                            )
                            &&
                            (
                                DateTime.Parse(fileToDownloadModifiedTime)
                                !=
                                DateTime.Parse(localFileModifiedTime)
                            )
                        )
                    );
                    */
            #endregion

            // If the file hashes do not match, the file has been modified
            return localFileInfoHash != torrentFileInfoHash;
        }

        /// <summary>
        /// Downloads updated files
        /// </summary>
        /// <returns>Nothing, as it stores the information in the files to download list asynchronously</returns>
        private async System.Threading.Tasks.Task DownloadUpdatedFiles() {
            List<TorrentManager> modifiedFiles = new List<TorrentManager>();
            
            foreach(string localFile in Directory.GetFiles(Settings.FILES_DIRECTORY)) {
                if(!localFile.Contains(".torrent") && !localFile.Contains("fastresume")) {
                    foreach(Models.File fileToDownload in filesToDownload) {
                        try {
                            string fileToDownloadName = fileToDownload.FileName;
                            string localFileName = Path.GetFileName(localFile);

                            DateTime fileToDownloadLastModified = fileToDownload.LastModified;
                            DateTime localLastModified = new FileInfo(localFile).LastWriteTimeUtc;

                            string localFileMD5 = Utils.GetFileMD5Hash(localFile);
                            string fileToDownloadMD5 = fileToDownload.FileMD5;
                            bool fileModified = localFileMD5 != fileToDownloadMD5;

                            if((fileToDownloadName == localFileName) && fileModified) {
                                var fileTrackerVM = new FileTrackerViewModel();
                                await fileTrackerVM.DeleteFileLocally(fileToDownloadName);
                                await fileTrackerVM.DeleteTorrent(fileToDownloadName);

                                Torrents.RemoveAt(Torrents.IndexOf(Torrents.Where(t => t.Torrent.Name == fileToDownloadName).FirstOrDefault()));

                                filesToDownload.Add(fileToDownload);
                            }
                        } catch(Exception ex) {
                            // Do nothing...
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removes files from the file removal queue
        /// </summary>
        /// <param name="fileManager">The file manager currently being used by the app</param>
        /// <param name="fileTrackerVM">The file tracker currently being used by the app</param>
        /// <returns>Nothing, as the files are removed asynchronously</returns>
        private async System.Threading.Tasks.Task RemoveFilesFromQueue(FileManager fileManager, FileTrackerViewModel fileTrackerVM) {
            List<string> filesToDelete = await fileTrackerVM.DeleteFilesInQueue();

            if(filesToDelete.Count > 0) {
                foreach(string fileToDelete in filesToDelete) {
                    Torrents.Remove(Torrents.Where(t => t.InfoHash.ToString().Replace("-", "") == fileToDelete).FirstOrDefault());
                }
            }

            await DownloadFiles(fileManager, fileTrackerVM);
        }

        /// <summary>
        /// Downloads files from the database (adds them to the files list)
        /// </summary>
        /// <param name="fileManager">The file manager currently being used by the app</param>
        /// <param name="fileTrackerVM">The file tracker currently being used by the app</param>
        /// <returns>Nothing, as the files are added to the list asynchronously</returns>
        private async System.Threading.Tasks.Task DownloadFiles(FileManager fileManager, FileTrackerViewModel fileTrackerVM) {
            Torrent torrent = null;
            BEncodedDictionary fastResume = GetFastResumeFile();

            string torrentFilePath = "";

            filesToDownload = await fileManager.GetAllFilesWithUserAsync(Settings.USER_ID);

            await DownloadUpdatedFiles();

            if(filesToDownload != null && filesToDownload.Count > 0) {
                foreach(var file in filesToDownload) {
                    if(Torrents.Where(t => t.InfoHash.Hash.ToString().Replace("-", "") == file.FileHash).Count() < 1) {
                        torrentFilePath = await Utils.CreateFile(file);
                        
                        try {
                            // Load the .torrent from the file into a Torrent instance
                            // You can use this to do preprocessing should you need to
                            torrent = Torrent.Load(torrentFilePath);
                            Console.WriteLine(torrent.InfoHash.ToString());
                        } catch(Exception e) {
                            Console.Write("Couldn't decode {0}: ", file);
                            Console.WriteLine(e.Message);
                            continue;
                        }

                        // When any preprocessing has been completed, you create a TorrentManager
                        // which you then register with the engine.
                        TorrentManager manager = new TorrentManager(torrent, downloadsPath, torrentDefaults);
                        if(!Torrents.Contains(manager)) {
                            torrent = manager.Torrent;

                            if(fastResume.ContainsKey(torrent.InfoHash.ToHex()))
                                manager.LoadFastResume(new FastResume((BEncodedDictionary)fastResume[torrent.infoHash.ToHex()]));

                            Torrents.Add(manager);

                            manager.PeersFound += new EventHandler<PeersAddedEventArgs>(manager_PeersFound);
                        }
                    }
                }
            }

            await CheckForNewFiles(fileManager, fileTrackerVM);
        }

        /// <summary>
        /// Checks for new files in the database, and adds them to the files list
        /// </summary>
        /// <param name="fileManager">The file manager currently being used by the app</param>
        /// <param name="fileTrackerVM">The file tracker currently being used by the app</param>
        /// <returns>Nothing, as the files are checked for and added asynchronously</returns>
        private async System.Threading.Tasks.Task CheckForNewFiles(FileManager fileManager, FileTrackerViewModel fileTrackerVM) {
            Torrent torrent = null;
            BEncodedDictionary fastResume = GetFastResumeFile();

            await CheckForRenamedFiles(fileTrackerVM);

            await fileTrackerVM.CheckForNewFiles();

            // For each file in the torrents path that is a .torrent file, load it into the engine.
            foreach(string file in Directory.GetFiles(torrentsPath)) {
                if(Torrents.Where(t => t.Torrent.TorrentPath == file).Count() < 1) {
                    if(file.EndsWith(".torrent") && !file.Contains("fastresume")) {
                        try {
                            // Load the .torrent from the file into a Torrent instance
                            // You can use this to do preprocessing should you need to
                            torrent = Torrent.Load(file);
                            Console.WriteLine(torrent.InfoHash.ToString());
                        } catch(Exception e) {
                            Console.Write("Couldn't decode {0}: ", file);
                            Console.WriteLine(e.Message);
                            continue;
                        }

                        await fileTrackerVM.AddFileToDatabase(Directory.GetFiles(Settings.FILES_DIRECTORY, Path.GetFileNameWithoutExtension(file) + ".*").ToList().Where(f => !f.EndsWith(".torrent")).FirstOrDefault(), Utils.GetTorrentInfoHash(file), file);// torrent.InfoHash.ToString());

                        // When any preprocessing has been completed, you create a TorrentManager
                        // which you then register with the engine.
                        TorrentManager manager = new TorrentManager(torrent, downloadsPath, torrentDefaults);
                        if(!Torrents.Contains(manager)) {
                            torrent = manager.Torrent;
                            if(fastResume.ContainsKey(torrent.InfoHash.ToHex()))
                                manager.LoadFastResume(new FastResume((BEncodedDictionary)fastResume[torrent.infoHash.ToHex()]));

                            // Store the torrent manager in our list so we can access it later
                            Torrents.Add(manager);
                            manager.PeersFound += new EventHandler<PeersAddedEventArgs>(manager_PeersFound);
                        }
                    } else {
                        if(!files.Contains(file)) {
                            files.Add(file);
                        }
                    }
                } else {
                    if(!file.EndsWith(".torrent") && !files.Contains(file)) {
                        files.Add(file);
                    }
                }
            }
        }

        /// <summary>
        /// Checks for new folders
        /// </summary>
        /// <returns>Nothing, it's an asynchronous task</returns>
        private async System.Threading.Tasks.Task CheckForNewFolders() {
            foreach(string folder in Directory.GetDirectories(torrentsPath)) {
                if(!folders.Contains(folder)) {
                    folders.Add(folder);
                }
            }
        }

        /// <summary>
        /// Removes local files that no longer exist on the device: adds them to the file removal queue
        /// </summary>
        /// <param name="fileTrackerVM">The file tracker currently being used by the app</param>
        /// <returns>Nothing, as the files are removed asynchronously</returns>
        private async System.Threading.Tasks.Task RemoveNonExistantFiles(FileTrackerViewModel fileTrackerVM) {
            // For each torrent, check if it still exists; if it doesn't remove it, and add it to the file removal queue
            for(int i = 0; i < Torrents.Count; i++) {
                bool hasFile = files.Where(f => Torrents[i].SavePath + "\\" + Torrents[i].Torrent.Name == f).Count() > 0;
                bool hasFolder = folders.Where(f => Torrents[i].SavePath + "\\" + Torrents[i].Torrent.Name == f).Count() > 0;
                if(!hasFile && !hasFolder) {
                    if(bitSynkTorrents.Count > i) {
                        await fileTrackerVM.RemoveFileAsync(bitSynkTorrents[i]);

                        await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                            bitSynkTorrents.Remove(bitSynkTorrents.Where(t => t.Hash == bitSynkTorrents[i].Hash).FirstOrDefault());
                        }));

                        if(bitSynkTorrents.Count > i) {
                            Torrents.Remove(Torrents.Where(t => t.InfoHash.ToString().Replace("-", "") == bitSynkTorrents[i].Hash).FirstOrDefault());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Starts the synchronization process
        /// </summary>
        private void StartSyncing() {
            // For each torrent manager we loaded and stored in our list, hook into the events
            // in the torrent manager and start the engine.
            foreach(TorrentManager manager in Torrents) {
                if(Engine.Torrents.Where(t => t.InfoHash.Hash.ToString() == manager.InfoHash.Hash.ToString()).Count() < 1) {
                    // Every time a piece is hashed, this is fired.
                    manager.PieceHashed += delegate (object o, PieceHashedEventArgs e) {
                        lock(listener)
                            listener.WriteLine(string.Format("Piece Hashed: {0} - {1}", e.PieceIndex, e.HashPassed ? "Pass" : "Fail"));
                    };

                    // Every time the state changes (Stopped -> Seeding -> Downloading -> Hashing) this is fired
                    manager.TorrentStateChanged += delegate (object o, TorrentStateChangedEventArgs e) {
                        lock(listener)
                            listener.WriteLine("OldState: " + e.OldState.ToString() + " NewState: " + e.NewState.ToString());
                    };

                    // Every time the tracker's state changes, this is fired
                    foreach(TrackerTier tier in manager.TrackerManager) {
                        foreach(Tracker t in tier.Trackers) {
                            t.AnnounceComplete += delegate (object sender, AnnounceResponseEventArgs e) {
                                listener.WriteLine(string.Format("{0}: {1}", e.Successful, e.Tracker.ToString()));
                            };
                        }
                    }

                    Engine.Register(manager);

                    if(manager.State != TorrentState.Stopped) {
                        // Start the torrentmanager. The file will then hash (if required) and begin downloading/seeding
                        manager.Start();
                    }
                } else {
                    if(!Engine.Disposed) {
                        try {
                            if(manager.State == TorrentState.Stopped) {
                                if(!Engine.Torrents.Contains(manager)) {
                                    Engine.Register(manager);
                                }
                                
                                // Start the torrentmanager. The file will then hash (if required) and begin downloading/seeding
                                manager.Start();
                            }
                        } catch(Exception ex) {
                            // Do nothing... The method will be hit later on anyways...
                        }
                    }
                }
            }

            Engine.StartAll();
            Engine.DhtEngine.Start();
        }

        /// <summary>
        /// Check if torrents exist; if they do, raise the event
        /// </summary>
        private void VerifyTorrents() {
            if(Torrents.Count == 0) {
                Console.WriteLine("No new torrents found...");
            } else {
                TorrentsAdded();
            }
        }

        /// <summary>
        /// Gets the fast resume file; creates it if it doesn't already exist
        /// </summary>
        /// <returns>The fast resume file</returns>
        private BEncodedDictionary GetFastResumeFile() {
            BEncodedDictionary fastResume;

            try {
                fastResume = BEncodedValue.Decode<BEncodedDictionary>(File.ReadAllBytes(fastResumeFile));
            } catch {
                fastResume = new BEncodedDictionary();
            }

            return fastResume;
        }

        /// <summary>
        /// Updates the status of the synchronization process in the output window, as well as raises events to update the UI
        /// </summary>
        private void UpdateStats() {
            // While the torrents are still running, print out some stats to the screen.
            // Details for all the loaded torrent managers are shown.
            bool running = true;

            StringBuilder sb = new StringBuilder(1024);
            sb.Remove(0, sb.Length);
            running = Torrents.ToList().Exists(delegate (TorrentManager m) { return m.State != TorrentState.Stopped; });

            AppendFormat(sb, "Total Download Rate: {0:0.00}kB/sec", Engine.TotalDownloadSpeed / 1024.0);
            AppendFormat(sb, "Total Upload Rate:   {0:0.00}kB/sec", Engine.TotalUploadSpeed / 1024.0);
            AppendFormat(sb, "Disk Read Rate:      {0:0.00} kB/s", Engine.DiskManager.ReadRate / 1024.0);
            AppendFormat(sb, "Disk Write Rate:     {0:0.00} kB/s", Engine.DiskManager.WriteRate / 1024.0);
            AppendFormat(sb, "Total Read:         {0:0.00} kB", Engine.DiskManager.TotalRead / 1024.0);
            AppendFormat(sb, "Total Written:      {0:0.00} kB", Engine.DiskManager.TotalWritten / 1024.0);
            AppendFormat(sb, "Open Connections:    {0}", Engine.ConnectionManager.OpenConnections);

            // Convert each MonoTorrent's torrent information to the BitSynk's torrent model
            // This will be used to udpate the BitSynk UI
            foreach(TorrentManager manager in Torrents) {
                Models.BitSynkTorrentModel bitSynkTorrent = bitSynkTorrents?.Where(t => t.Name == manager.Torrent.Name)?.FirstOrDefault();
                if(bitSynkTorrent == null) {
                    if(Application.Current != null) {
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                            bitSynkTorrents.Add(new Models.BitSynkTorrentModel() {
                                Name = manager.Torrent.Name,
                                Hash = manager.Torrent.InfoHash.ToString().Replace("-", ""),
                                Progress = manager.Progress,
                                State = manager.State.ToString(),
                                DownloadSpeed = manager.Monitor.DownloadSpeed / 1024.0,
                                UploadSpeed = manager.Monitor.DownloadSpeed / 1024.0
                            });

                            bitSynkTorrent = bitSynkTorrents.Last();
                        }));
                    }
                } else {
                    if(Application.Current != null) {
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                            bitSynkTorrent.Progress = manager.Progress;
                            bitSynkTorrent.State = manager.State.ToString();
                            bitSynkTorrent.DownloadSpeed = manager.Monitor.DownloadSpeed / 1024.0;
                            bitSynkTorrent.UploadSpeed = manager.Monitor.DownloadSpeed / 1024.0;
                        }));
                    }
                }

                PeerChanged();

                AppendSeperator(sb);
                AppendFormat(sb, "State:           {0}", manager.State);
                AppendFormat(sb, "Name:            {0}", manager.Torrent == null ? "MetaDataMode" : manager.Torrent.Name);
                AppendFormat(sb, "Progress:           {0:0.00}", manager.Progress);
                AppendFormat(sb, "Download Speed:     {0:0.00} kB/s", manager.Monitor.DownloadSpeed / 1024.0);
                AppendFormat(sb, "Upload Speed:       {0:0.00} kB/s", manager.Monitor.UploadSpeed / 1024.0);
                AppendFormat(sb, "Total Downloaded:   {0:0.00} MB", manager.Monitor.DataBytesDownloaded / (1024.0 * 1024.0));
                AppendFormat(sb, "Total Uploaded:     {0:0.00} MB", manager.Monitor.DataBytesUploaded / (1024.0 * 1024.0));
                MonoTorrent.Client.Tracker.Tracker tracker = manager.TrackerManager.CurrentTracker;
                AppendFormat(sb, "Warning Message:    {0}", tracker == null ? "<no tracker>" : tracker.WarningMessage);
                AppendFormat(sb, "Failure Message:    {0}", tracker == null ? "<no tracker>" : tracker.FailureMessage);
                if(manager.PieceManager != null)
                    AppendFormat(sb, "Current Requests:   {0}", manager.PieceManager.CurrentRequestCount());

                foreach(PeerId p in manager.GetPeers()) {
                    Models.BitSynkPeerModel bitSynkPeer = bitSynkTorrent?.BitSynkPeers?.Where(peer => peer.ConnectionUri == p.Peer.ConnectionUri)?.FirstOrDefault();
                    if(bitSynkPeer == null) {
                        if(Application.Current != null) {
                            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                                bitSynkTorrent.BitSynkPeers.Add(new Models.BitSynkPeerModel() {
                                    ConnectionUri = p.Peer.ConnectionUri,
                                    DownloadSpeed = p.Monitor.DownloadSpeed / 1024.0,
                                    UploadSpeed = p.Monitor.UploadSpeed / 1024.0,
                                    PiecesCount = p.AmRequestingPiecesCount
                                });
                            }));
                        }
                    } else {
                        if(Application.Current != null) {
                            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                                bitSynkPeer.DownloadSpeed = p.Monitor.DownloadSpeed / 1024.0;
                                bitSynkPeer.UploadSpeed = p.Monitor.UploadSpeed / 1024.0;
                                bitSynkPeer.PiecesCount = p.AmRequestingPiecesCount;
                            }));
                        }
                    }

                    AppendFormat(sb, "\t{2} - {1:0.00}/{3:0.00}kB/sec - {0}", p.Peer.ConnectionUri,
                                                                                p.Monitor.DownloadSpeed / 1024.0,
                                                                                p.AmRequestingPiecesCount,
                                                                                p.Monitor.UploadSpeed / 1024.0);
                }

                AppendFormat(sb, "", null);
                if(manager.Torrent != null)
                    foreach(TorrentFile file in manager.Torrent.Files)
                        AppendFormat(sb, "{1:0.00}% - {0}", file.Path, file.BitField.PercentComplete);
            }
                
            Console.WriteLine(sb.ToString());
            listener.ExportTo(Console.Out);
            
            if(!refreshTimer.IsEnabled) {
                refreshTimer.Start();
            }
        }

        /// <summary>
        /// Event handler when peers have been found in a torrent
        /// </summary>
        /// <param name="sender">Torrent manager</param>
        /// <param name="e">Peers added event arguments</param>
        static void manager_PeersFound(object sender, PeersAddedEventArgs e) {
            lock(listener)
                // Update the peers' information on the output window
                listener.WriteLine(string.Format("Found {0} new peers and {1} existing peers", e.NewPeers, e.ExistingPeers));//throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Adds a separator to the output window
        /// </summary>
        /// <param name="sb">String builder</param>
        private static void AppendSeperator(StringBuilder sb) {
            AppendFormat(sb, "", null);
            AppendFormat(sb, "- - - - - - - - - - - - - - - - - - - - - - - - - - - - - -", null);
            AppendFormat(sb, "", null);
        }

        /// <summary>
        /// Formats strings to show in the debug output
        /// </summary>
        /// <param name="sb">String builder</param>
        /// <param name="str">String to append</param>
        /// <param name="formatting">The format</param>
        private static void AppendFormat(StringBuilder sb, string str, params object[] formatting) {
            if(formatting != null)
                sb.AppendFormat(str, formatting);
            else
                sb.Append(str);
            sb.AppendLine();
        }

        /// <summary>
        /// Tasks to perform when the app is shutting down
        /// </summary>
        private static async void shutdown() {
            // Await the device's status, especially the last seen date/time
            await new DeviceManager().UpdateDeviceAsync(Settings.DEVICE_ID, Settings.DEVICE_NAME, Utils.GetIPAddress(), Settings.USER_ID, DateTime.UtcNow);

            // Get the fast resume file, and update it with the torrents list
            BEncodedDictionary fastResume = new BEncodedDictionary();
            for(int i = 0; i < torrents.Count; i++) {
                torrents[i].Stop();
                
                while(torrents[i].State != TorrentState.Stopped) {
                    Console.WriteLine("{0} is {1}", torrents[i].Torrent.Name, torrents[i].State);
                    Thread.Sleep(250);
                }

                fastResume.Add(torrents[i].Torrent.InfoHash.ToHex(), torrents[i].SaveFastResume().Encode());
            }

            // Save the DHT nodes
            var bnodes = engine.DhtEngine.SaveNodes();
            var nodes = Node.FromCompactNode(bnodes);
            
            string s = "";
            foreach(var node in nodes) {
                s += node.EndPoint.Address + "\n";
            }
            
            // Write the saved information in the fast resume file
            File.WriteAllBytes(fastResumeFile, fastResume.Encode());

            // Dispose the engine, and close all listeners
            engine.Dispose();

            foreach(TraceListener lst in Debug.Listeners) {
                lst.Flush();
                lst.Close();
            }

            // Wait for 2 seconds before finally closing the app; enough time to save everything
            System.Threading.Thread.Sleep(2000);
        }

        /// <summary>
        /// Raise an event that torrents have been added
        /// </summary>
        public event EventHandler OnTorrentsAdded;
        private void TorrentsAdded() {
            if(OnTorrentsAdded != null) {
                OnTorrentsAdded(this, new EventArgs());
            }
        }

        /// <summary>
        /// Raise an event that a peer has changed
        /// </summary>
        public event EventHandler OnPeerChanged;
        private void PeerChanged() {
            if(OnPeerChanged != null) {
                OnPeerChanged(this, new EventArgs());
            }
        }
    }
}