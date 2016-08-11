using BitSynk.Helpers;
using BitSynk.Models;
using BitSynk.ViewModels;
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
    public class Client : INotifyPropertyChanged {
        private int port = 10000;

        private static string dhtNodeFile;
        private static string basePath;
        private static string downloadsPath;
        private static string fastResumeFile;
        private static string torrentsPath;
        private static ClientEngine engine;				// The engine used for downloading
        public static ObservableCollection<TorrentManager> torrents;	// The list where all the torrentManagers will be stored that the engine gives us
        private static Top10Listener listener;			// This is a subclass of TraceListener which remembers the last 20 statements sent to it

        private EngineSettings engineSettings;
        private TorrentSettings torrentDefaults;

        private List<RawTrackerTier> trackers;

        public ObservableCollection<BitSynkTorrentModel> BitSynkTorrents = new ObservableCollection<BitSynkTorrentModel>(); 

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

        private object parameter;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string property = "") {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public Client(object parameter) {
            this.parameter = parameter;

            /* Generate the paths to the folder we will save .torrent files to and where we download files to */
            basePath = Environment.CurrentDirectory;						// This is the directory we are currently in
            torrentsPath = Path.Combine(basePath, Settings.FILES_DIRECTORY_NAME);				// This is the directory we will save .torrents to
            downloadsPath = Path.Combine(basePath, Settings.FILES_DIRECTORY_NAME);			// This is the directory we will save downloads to
            fastResumeFile = Path.Combine(torrentsPath, "fastresume.data");
            dhtNodeFile = Path.Combine(basePath, "DhtNodes");
            Torrents = new ObservableCollection<TorrentManager>();							// This is where we will store the torrentmanagers
            listener = new Top10Listener(10);

            // We need to cleanup correctly when the user closes the window by using ctrl-c
            // or an unhandled exception happens
            Console.CancelKeyPress += delegate { shutdown(); };
            AppDomain.CurrentDomain.ProcessExit += delegate { shutdown(); };
            AppDomain.CurrentDomain.UnhandledException += delegate (object sender, UnhandledExceptionEventArgs e) { Console.WriteLine(e.ExceptionObject); shutdown(); };
            Thread.GetDomain().UnhandledException += delegate (object sender, UnhandledExceptionEventArgs e) { Console.WriteLine(e.ExceptionObject); shutdown(); };

            // If the SavePath does not exist, we want to create it.
            //if(!Directory.Exists(engine.Settings.SavePath))
            //    Directory.CreateDirectory(engine.Settings.SavePath);

            // If the torrentsPath does not exist, we want to create it
            if(!Directory.Exists(torrentsPath))
                Directory.CreateDirectory(torrentsPath);

            InitTrackers();

            InitEngine();
        }

        private void InitTrackers() {
            List<string> trackers = new Trackers().trackers;
            this.trackers = new List<RawTrackerTier>();

            RawTrackerTier rawTrackerTier = new RawTrackerTier();

            foreach(string tracker in trackers) {
                rawTrackerTier.Add(tracker.Trim());
            }

            this.trackers.Add(rawTrackerTier);
        }

        private BEncodedDictionary GetFastResumeFile() {
            BEncodedDictionary fastResume;

            try {
                fastResume = BEncodedValue.Decode<BEncodedDictionary>(File.ReadAllBytes(fastResumeFile));
            } catch {
                fastResume = new BEncodedDictionary();
            }

            return fastResume;
        }

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

            InitDHT();
        }

        private void InitDHT() {
            byte[] nodes = null;

            try {
                nodes = File.ReadAllBytes(dhtNodeFile);
            } catch {
                Console.WriteLine("No existing dht nodes could be loaded");
            }

            DhtListener dhtListner = new DhtListener(new IPEndPoint(IPAddress.Any, port));
            DhtEngine dht = new DhtEngine(dhtListner);
            Engine.RegisterDht(dht);
            dhtListner.Start();
            Engine.DhtEngine.Start(nodes);
        }

        public async void StartEngineUsingTorrents() {
            Torrent torrent = null;

            FileManager fileManager = new FileManager();
            FileTrackerViewModel fileTrackerVM = new FileTrackerViewModel();

            List<DatabaseManager.Models.File> filesToDownload = await fileManager.GetAllFilesWithUserAsync(Settings.USER_ID);

            BEncodedDictionary fastResume = GetFastResumeFile();

            // For each file in the torrents path that is a .torrent file, load it into the engine.
            foreach(string file in Directory.GetFiles(torrentsPath)) {
                if(file.EndsWith(".torrent")) {
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

                    fileTrackerVM.AddFileToDatabase(file, Utils.GetTorrentInfoHash(file), file);// torrent.InfoHash.ToString());

                    // When any preprocessing has been completed, you create a TorrentManager
                    // which you then register with the engine.
                    TorrentManager manager = new TorrentManager(torrent, downloadsPath, torrentDefaults);
                    torrent = manager.Torrent;
                    if(fastResume.ContainsKey(torrent.InfoHash.ToHex()))
                        manager.LoadFastResume(new FastResume((BEncodedDictionary)fastResume[torrent.infoHash.ToHex()]));
                    Engine.Register(manager);

                    // Store the torrent manager in our list so we can access it later
                    Torrents.Add(manager);
                    manager.PeersFound += new EventHandler<PeersAddedEventArgs>(manager_PeersFound);
                }
            }

            if(filesToDownload != null && filesToDownload.Count > 0) {
                foreach(var file in filesToDownload) {
                    //hashes.Add(file.FileHash);
                    string torrentFilePath = await Utils.CreateFile(file);

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

                    //fileTrackerVM.AddFileToDatabase(file, Utils.GetTorrentInfoHash(file), file);// torrent.InfoHash.ToString());

                    // When any preprocessing has been completed, you create a TorrentManager
                    // which you then register with the engine.
                    TorrentManager manager = new TorrentManager(torrent, downloadsPath, torrentDefaults);
                    if(!Torrents.Contains(manager)) {
                        torrent = manager.Torrent;

                        if(fastResume.ContainsKey(torrent.InfoHash.ToHex())) {
                            manager.LoadFastResume(new FastResume((BEncodedDictionary)fastResume[torrent.infoHash.ToHex()]));
                            Engine.Register(manager);
                        }

                        Torrents.Add(manager);

                        manager.PeersFound += new EventHandler<PeersAddedEventArgs>(manager_PeersFound);
                    }
                }
            }

            //if(torrents == null || torrents.Count < 1) {
            //    Console.WriteLine("No files to download.");
            //} else {
            //    foreach(string hash in hashes) {
            //        TorrentManager manager1 = new TorrentManager(InfoHash.FromHex(hash), downloadsPath, torrentDefaults, torrentsPath, trackers); //new List<RawTrackerTier>()); //new TorrentManager(torrent, downloadsPath, torrentDefaults);
            //                                                                                                                                                        //    manager1.LoadFastResume(new FastResume ((BEncodedDictionary)fastResume[torrent.infoHash.ToHex ()]));
            //        engine.Register(manager1);

            //        torrents.Add(manager1);
            //        manager1.PeersFound += new EventHandler<PeersAddedEventArgs>(manager_PeersFound);
            //    }
            //}

            // If we loaded no torrents, just exist. The user can put files in the torrents directory and start
            // the client again
            if(Torrents.Count == 0) {
                Console.WriteLine("No torrents found in the Torrents directory");
                Console.WriteLine("Exiting...");
                Engine.Dispose();
                return;
            } else {
                TorrentsAdded();
            }

            // For each torrent manager we loaded and stored in our list, hook into the events
            // in the torrent manager and start the engine.
            foreach(TorrentManager manager in Torrents) {
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
                // Start the torrentmanager. The file will then hash (if required) and begin downloading/seeding
                manager.Start();
            }

            // While the torrents are still running, print out some stats to the screen.
            // Details for all the loaded torrent managers are shown.
            int i = 0;
            bool running = true;
            StringBuilder sb = new StringBuilder(1024);
            while(running) {
                if((i++) % 10 == 0) {
                    sb.Remove(0, sb.Length);
                    running = Torrents.ToList().Exists(delegate (TorrentManager m) { return m.State != TorrentState.Stopped; });

                    AppendFormat(sb, "Total Download Rate: {0:0.00}kB/sec", Engine.TotalDownloadSpeed / 1024.0);
                    AppendFormat(sb, "Total Upload Rate:   {0:0.00}kB/sec", Engine.TotalUploadSpeed / 1024.0);
                    AppendFormat(sb, "Disk Read Rate:      {0:0.00} kB/s", Engine.DiskManager.ReadRate / 1024.0);
                    AppendFormat(sb, "Disk Write Rate:     {0:0.00} kB/s", Engine.DiskManager.WriteRate / 1024.0);
                    AppendFormat(sb, "Total Read:         {0:0.00} kB", Engine.DiskManager.TotalRead / 1024.0);
                    AppendFormat(sb, "Total Written:      {0:0.00} kB", Engine.DiskManager.TotalWritten / 1024.0);
                    AppendFormat(sb, "Open Connections:    {0}", Engine.ConnectionManager.OpenConnections);

                    foreach(TorrentManager manager in Torrents) {
                        BitSynkTorrentModel bitSynkTorrent = BitSynkTorrents?.Where(t => t.Name == manager.Torrent.Name)?.FirstOrDefault();
                        if(bitSynkTorrent == null) {
                            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                                BitSynkTorrents.Add(new Models.BitSynkTorrentModel() {
                                    Name = manager.Torrent.Name,
                                    Progress = manager.Progress,
                                    State = manager.State.ToString(),
                                    DownloadSpeed = manager.Monitor.DownloadSpeed / 1024.0,
                                    UploadSpeed = manager.Monitor.DownloadSpeed / 1024.0
                                });
                            }));
                        } else {
                            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                                bitSynkTorrent.Progress = manager.Progress;
                                bitSynkTorrent.State = manager.State.ToString();
                                bitSynkTorrent.DownloadSpeed = manager.Monitor.DownloadSpeed / 1024.0;
                                bitSynkTorrent.UploadSpeed = manager.Monitor.DownloadSpeed / 1024.0;
                            }));
                        }

                        AppendSeperator(sb);
                        AppendFormat(sb, "State:           {0}", manager.State);
                        AppendFormat(sb, "Name:            {0}", manager.Torrent == null ? "MetaDataMode" : manager.Torrent.Name);
                        AppendFormat(sb, "Progress:           {0:0.00}", manager.Progress);
                        AppendFormat(sb, "Download Speed:     {0:0.00} kB/s", manager.Monitor.DownloadSpeed / 1024.0);
                        AppendFormat(sb, "Upload Speed:       {0:0.00} kB/s", manager.Monitor.UploadSpeed / 1024.0);
                        AppendFormat(sb, "Total Downloaded:   {0:0.00} MB", manager.Monitor.DataBytesDownloaded / (1024.0 * 1024.0));
                        AppendFormat(sb, "Total Uploaded:     {0:0.00} MB", manager.Monitor.DataBytesUploaded / (1024.0 * 1024.0));
                        MonoTorrent.Client.Tracker.Tracker tracker = manager.TrackerManager.CurrentTracker;
                        //AppendFormat(sb, "Tracker Status:     {0}", tracker == null ? "<no tracker>" : tracker.State.ToString());
                        AppendFormat(sb, "Warning Message:    {0}", tracker == null ? "<no tracker>" : tracker.WarningMessage);
                        AppendFormat(sb, "Failure Message:    {0}", tracker == null ? "<no tracker>" : tracker.FailureMessage);
                        if(manager.PieceManager != null)
                            AppendFormat(sb, "Current Requests:   {0}", manager.PieceManager.CurrentRequestCount());

                        foreach(PeerId p in manager.GetPeers())
                            AppendFormat(sb, "\t{2} - {1:0.00}/{3:0.00}kB/sec - {0}", p.Peer.ConnectionUri,
                                                                                      p.Monitor.DownloadSpeed / 1024.0,
                                                                                      p.AmRequestingPiecesCount,
                                                                                      p.Monitor.UploadSpeed / 1024.0);

                        AppendFormat(sb, "", null);
                        if(manager.Torrent != null)
                            foreach(TorrentFile file in manager.Torrent.Files)
                                AppendFormat(sb, "{1:0.00}% - {0}", file.Path, file.BitField.PercentComplete);
                    }
                    //Console.Clear();
                    Console.WriteLine(sb.ToString());
                    listener.ExportTo(Console.Out);
                }

                System.Threading.Thread.Sleep(500);
            }
        }

        static void manager_PeersFound(object sender, PeersAddedEventArgs e) {
            lock(listener)
                listener.WriteLine(string.Format("Found {0} new peers and {1} existing peers", e.NewPeers, e.ExistingPeers));//throw new Exception("The method or operation is not implemented.");
        }

        private static void AppendSeperator(StringBuilder sb) {
            AppendFormat(sb, "", null);
            AppendFormat(sb, "- - - - - - - - - - - - - - - - - - - - - - - - - - - - - -", null);
            AppendFormat(sb, "", null);
        }

        private static void AppendFormat(StringBuilder sb, string str, params object[] formatting) {
            if(formatting != null)
                sb.AppendFormat(str, formatting);
            else
                sb.Append(str);
            sb.AppendLine();
        }

        private static void shutdown() {
            BEncodedDictionary fastResume = new BEncodedDictionary();
            for(int i = 0; i < torrents.Count; i++) {
                torrents[i].Stop();
                
                while(torrents[i].State != TorrentState.Stopped) {
                    Console.WriteLine("{0} is {1}", torrents[i].Torrent.Name, torrents[i].State);
                    Thread.Sleep(250);
                }

                fastResume.Add(torrents[i].Torrent.InfoHash.ToHex(), torrents[i].SaveFastResume().Encode());
            }

#if !DISABLE_DHT
            File.WriteAllBytes(dhtNodeFile, engine.DhtEngine.SaveNodes());
#endif
            File.WriteAllBytes(fastResumeFile, fastResume.Encode());
            engine.Dispose();

            foreach(TraceListener lst in Debug.Listeners) {
                lst.Flush();
                lst.Close();
            }

            System.Threading.Thread.Sleep(2000);
        }

        public event EventHandler OnTorrentsAdded;
        private void TorrentsAdded() {
            if(OnTorrentsAdded != null) {
                OnTorrentsAdded(this, new EventArgs());
            }
        }
    }
}
