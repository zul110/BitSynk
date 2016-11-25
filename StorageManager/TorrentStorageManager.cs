using DatabaseManager;
using DatabaseManager.Helpers;
using MonoTorrent;
using MonoTorrent.BEncoding;
using MonoTorrent.Client;
using MonoTorrent.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageManager {
    public class TorrentStorageManager {
        private static string USER_ID = "";
        private static string DEVICE_ID = "";

        public TorrentStorageManager() {

        }

        public TorrentStorageManager(string userId, string deviceId) {
            USER_ID = userId;
            DEVICE_ID = deviceId;
        }

        public bool CreateFile(string path, string savePath) {
            try {
                CreateTorrentFile(path, savePath, "");

                return true;
            } catch(Exception ex) {
                return false;
            }
        }

        public bool CreateFile(string path, string savePath, string fileHash) {
            try {
                CreateTorrentFile(path, savePath, fileHash);

                return true;
            } catch(Exception ex) {
                return false;
            }
        }

        public bool RemoveFile(string path) {
            try {
                if(!path.Contains(".torrent")) {
                    path += ".torrent";
                }

                File.Delete(path);

                return true;
            } catch(Exception ex) {
                return false;
            }
        }

        public bool CreateAndLoadTorrent(string path, string savePath, ClientEngine engine) {
            try {
                // Instantiate a torrent creator
                TorrentCreator creator = new TorrentCreator();
                // Create a TorrentFileSource which is used to populate the .torrent
                ITorrentFileSource files = new TorrentFileSource(path);
                // Create the Torrent metadata blob
                BEncodedDictionary torrentDict = creator.Create(files);

                var tier = new RawTrackerTier();
                tier.Add("http://127.0.0.1:10000/announce");//http://localhost/announce");

                creator.Announces.Add(tier);

                // Instantiate a torrent
                Torrent torrent = Torrent.Load(torrentDict);

                // Create a fake fast resume data with all the pieces set to true so we
                // don't have to hash the torrent when adding it directly to the engine.
                BitField bitfield = new BitField(torrent.Pieces.Count).Not();
                FastResume fastResumeData = new FastResume(torrent.InfoHash, bitfield);

                // Create a TorrentManager so the torrent can be downloaded and load
                // the FastResume data so we don't have to hash it again.
                TorrentManager manager = new TorrentManager(torrent, "C:\\bitsynk", new TorrentSettings());
                manager.LoadFastResume(fastResumeData);

                // Register it with the engine and start the download
                //engine.Register(manager);
                //engine.StartAll();

                //DownloadTorrent(path, engine);

                return true;
            } catch(Exception ex) {
                return false;
            }
        }

        private async void CreateTorrentFile(string path, string savePath, string fileHash) {
            // The class used for creating the torrent
            TorrentCreator c = new TorrentCreator();

            // Add one tier which contains two trackers
            RawTrackerTier tier = new RawTrackerTier();
            tier.Add("udp://tracker.openbittorrent.com:80");//"udp://tracker.opentrackr.org:1337/announce");//"http://www.torrent-downloads.to:2710/announce");//http://bttrack.9you.com/");//http://opensharing.org:2710/announce");
            //tier.Add("http://" + Utils.GetPublicIPAddress() + ":10000/announce/");//Utils.GetLocalIPAddress() + ":10000");// "http://localhost/announce");

            c.Announces.Add(tier);
            c.Comment = "This is the comment";
            c.CreatedBy = "Doug using " + VersionInfo.ClientVersion;
            c.Publisher = "www.aaronsen.com";

            // Set the torrent as private so it will not use DHT or peer exchange
            // Generally you will not want to set this.
            c.Private = false;

            // Every time a piece has been hashed, this event will fire. It is an
            // asynchronous event, so you have to handle threading yourself.
            c.Hashed += delegate (object o, TorrentCreatorEventArgs e) {
                Console.WriteLine("Current File is {0}% hashed", e.FileCompletion);
                Console.WriteLine("Overall {0}% hashed", e.OverallCompletion);
                Console.WriteLine("Total data to hash: {0}", e.OverallSize);
            };

            // ITorrentFileSource can be implemented to provide the TorrentCreator
            // with a list of files which will be added to the torrent metadata.
            // The default implementation takes a path to a single file or a path
            // to a directory. If the path is a directory, all files will be
            // recursively added
            ITorrentFileSource fileSource = new TorrentFileSource(path);

            string torrentFilePath = GetTorrentFilePath(savePath, path);
            // Create the torrent file and save it directly to the specified path
            // Different overloads of 'Create' can be used to save the data to a Stream
            // or just return it as a BEncodedDictionary (its native format) so it can be
            // processed in memory
            c.Create(fileSource, torrentFilePath);// savePath + Path.GetFileNameWithoutExtension(path) + ".torrent");

            AnnounceFileAddition(Path.GetFileName(path), fileHash, await Utils.ReadFileAsync(torrentFilePath));
        }

        public string GetTorrentFilePath(string savePath, string path) {
            return savePath + GetTorrentFileName(path);
        }

        public string GetTorrentFileName(string path) {
            return Path.GetFileNameWithoutExtension(path) + ".torrent";
        }

        public async void AnnounceFileAddition(string fileName, string fileHash, byte[] fileContents, int fileVersion = 0) {
            FileManager fileService = new FileManager();

            FileInfo fileInfo = new FileInfo(fileName);
            long fileSize = fileInfo.Length;
            DateTime added = fileInfo.CreationTimeUtc;
            DateTime lastModified = fileInfo.LastWriteTimeUtc;

            await fileService.AddFileAsync(Guid.NewGuid().ToString(), fileName, fileHash, fileSize, added, lastModified, USER_ID, DEVICE_ID, fileContents, fileVersion);
        }

        public void DownloadTorrent(string path, ClientEngine engine) {
            // Open the .torrent file
            Torrent torrent = Torrent.Load(path);

            // Create the manager which will download the torrent to savePath
            // using the default settings.
            TorrentManager manager = new TorrentManager(torrent, "c:\\bitsynk", new TorrentSettings());

            // Register the manager with the engine
            engine.Register(manager);

            // Begin the download. It is not necessary to call HashCheck on the manager
            // before starting the download. If a hash check has not been performed, the
            // manager will enter the Hashing state and perform a hash check before it
            // begins downloading.

            // If the torrent is fully downloaded already, calling 'Start' will place
            // the manager in the Seeding state.
            manager.Start();
        }

        public bool ChangeFileName(string path, string newName) {
            try {
                string directory = Path.GetDirectoryName(path);

                if(directory != null && directory != "") {
                    File.Move(path, directory + newName);

                    return true;
                } else {
                    return false;
                }
            } catch(Exception ex) {
                return false;
            }
        }

        public bool ChangeFilePath(string path, string newPath) {
            string newDirectory = "";
            string newFileName = "";

            try {
                if(path != "" && newPath != "") {
                    newPath = newPath.Length == 3 && newPath.Last() == '\\' ? newPath.Remove(newPath.Length - 1, 1) : newPath;

                    newDirectory = Path.GetDirectoryName(newPath);
                    newFileName = Path.GetFileName(newPath);
                    newFileName = newFileName == "" ? Path.GetFileName(path) : newFileName;

                    File.Move(path, newDirectory + "\\" + newFileName);

                    return true;
                } else {
                    return false;
                }
            } catch(Exception ex) {
                return false;
            }
        }
    }
}
