using BencodeNET;
using MonoTorrent;
using MonoTorrent.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Helpers {
    /// <summary>
    /// Utility methods available to the whole application
    /// </summary>
    public static class Utils {
        /// <summary>
        /// Gets the local IP address of the device as a string
        /// </summary>
        /// <returns>Local IP address string</returns>
        public static string GetLocalIPAddress() {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach(var ip in host.AddressList) {
                if(ip.AddressFamily == AddressFamily.InterNetwork) {
                    return ip.ToString();
                }
            }

            throw new Exception("Local IP Address Not Found!");
        }

        /// <summary>
        /// Gets the local IP address of the device as returned by .Net
        /// </summary>
        /// <returns>Local IP address</returns>
        public static IPAddress GetLocalIPAddressRaw() {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach(var ip in host.AddressList) {
                if(ip.AddressFamily == AddressFamily.InterNetwork) {
                    return ip;
                }
            }

            throw new Exception("Local IP Address Not Found!");
        }

        /// <summary>
        /// Gets the public IP address of the device as a string
        /// </summary>
        /// <returns>Public IP address</returns>
        public static string GetPublicIPAddress() {
            return new WebClient().DownloadString(new Uri("http://www.canihazip.com/s", UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        /// Returns the magnet link of a torrent
        /// </summary>
        /// <param name="torrentPath">Path of the torrent file</param>
        /// <returns>Magnet link</returns>
        public static string GetMagnetLink(string torrentPath) {
            return String.Format("magnet:?xt=urn:btih:{0}&db={1}", GetTorrentInfoHash(torrentPath), Path.GetFileNameWithoutExtension(torrentPath));
        }

        /// <summary>
        /// Returns the info hash of a torrent file
        /// </summary>
        /// <param name="torrentPath">Path of the torrent file</param>
        /// <returns>Info hash of the torrent file as a string</returns>
        public static string GetTorrentInfoHash(string torrentPath) {
            BencodeNET.Objects.TorrentFile torrent = Bencode.DecodeTorrentFile(torrentPath);
            
            return torrent.CalculateInfoHash();
        }

        /// <summary>
        /// Returns the MD5 hash of a file
        /// </summary>
        /// <param name="filePath">Path of a file</param>
        /// <returns>MD5 hash of the file as an upper case string</returns>
        public static string GetFileMD5Hash(string filePath) {
            using(var md5 = MD5.Create()) {
                using(var stream = File.OpenRead(filePath)) {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToUpper();
                }
            }
        }

        /// <summary>
        /// Creates a torrent file in the path specified
        /// </summary>
        /// <param name="path">Path of the file whose torrent file is to be created</param>
        /// <param name="savePath">Path where the torrent file will be saved</param>
        /// <returns>Path to the torrent file</returns>
        public static string CreateTorrent(string path, string savePath) {
            // Initialize the TorrentCreator
            TorrentCreator creator = new TorrentCreator();

            // Adds trackers to the torrent file
            // Since the current implementation of BitSynk does not use trackers,
            // the trackers codes are commented out
            /* TRACKER CODES
             * 
             * RawTrackerTier tier = new RawTrackerTier();
             * 
             * foreach(string tracker in new Trackers().trackers) {
             *     tier.Add(tracker.Trim());
             * }

             * tier.Add("udp://tracker.openbittorrent.com:80/announce");
             * tier.Add("udp://tracker.publicbt.com:80/announce");
             * tier.Add("udp://tracker.ccc.de:80/announce");// "udp://tracker.openbittorrent.com:80");//"udp://tracker.opentrackr.org:1337/announce");//"http://www.torrent-downloads.to:2710/announce");//http://bttrack.9you.com/");//http://opensharing.org:2710/announce");
             * tier.Add("http://" + Utils.GetPublicIPAddress() + ":10000/announce/");//Utils.GetLocalIPAddress() + ":10000");// "http://localhost/announce");
             * 
             * creator.Announces.Add(tier);
            */

            // Additional metadata
            creator.Comment = "BitSynk";
            creator.CreatedBy = "Zul";
            creator.Publisher = "zul";

            // Set the torrent as private so it will not use DHT or peer exchange
            // Generally you will not want to set this.
            creator.Private = false;

            // Every time a piece has been hashed, this event will fire. It is an
            // asynchronous event, so you have to handle threading yourself.
            creator.Hashed += delegate (object o, TorrentCreatorEventArgs e) {
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
            
            // Create the torrent file and save it directly to the specified path
            // Different overloads of 'Create' can be used to save the data to a Stream
            // or just return it as a BEncodedDictionary (its native format) so it can be
            // processed in memory
            string torrentPath = savePath + "\\" + Path.GetFileNameWithoutExtension(path) + ".torrent";
            creator.Create(fileSource, torrentPath);
            
            return torrentPath;
        }

        /// <summary>
        /// Gets the torrent info hash of a file without creating a torrent file
        /// </summary>
        /// <param name="filePath">Path of the file whose torrent hash is to be calculated</param>
        /// <returns>The torrent info hash</returns>
        public static string GetTorrentInfoHashOfFile(string filePath) {
            TorrentCreator creator = new TorrentCreator();
            
            creator.Comment = "BitSynk";
            creator.CreatedBy = "Zul";
            creator.Publisher = "zul";
            creator.Private = false;
            
            creator.Hashed += delegate (object o, TorrentCreatorEventArgs e) {
                Console.WriteLine("Current File is {0}% hashed", e.FileCompletion);
                Console.WriteLine("Overall {0}% hashed", e.OverallCompletion);
                Console.WriteLine("Total data to hash: {0}", e.OverallSize);
            };

            ITorrentFileSource fileSource = new TorrentFileSource(filePath);

            Torrent torrent = Torrent.Load(creator.Create(fileSource));

            return torrent.InfoHash.ToString().Replace("-", "");
        }

        /// <summary>
        /// Get the path of a torrent of a file
        /// </summary>
        /// <param name="savePath">The location where the file is saved</param>
        /// <param name="path">The path of the file whose torrent is to be found</param>
        /// <returns>The complete path of the torrent file</returns>
        public static string GetTorrentFilePath(string savePath, string path) {
            return savePath + GetTorrentFileName(path);
        }

        /// <summary>
        /// Returns the name of the torrent file
        /// </summary>
        /// <param name="path">The original file path</param>
        /// <returns>The name of the torrent file</returns>
        public static string GetTorrentFileName(string path) {
            return Path.GetFileNameWithoutExtension(path) + ".torrent";
        }

        /// <summary>
        /// Reads the contents of a file asynchronously
        /// </summary>
        /// <param name="filePath">The file path</param>
        /// <returns>Contents of the file in bytes</returns>
        public static async Task<byte[]> ReadFileAsync(string filePath) {
            return File.ReadAllBytes(filePath);
        }

        /// <summary>
        /// Creates a file, according to the parameters in the BitSynk file model
        /// </summary>
        /// <param name="file">BitSynk's custom file model</param>
        /// <returns>Path of the file created</returns>
        public static async Task<string> CreateFile(Models.File file) {
            string filePath = Settings.FILES_DIRECTORY + "\\" + GetTorrentFileName(file.FileName);

            if(!File.Exists(filePath)) {
                File.WriteAllBytes(filePath, file.FileContents);
            }

            return filePath;
        }

        /// <summary>
        /// Copies a file to the files directory of BitSynk
        /// </summary>
        /// <param name="file">The path or name of the file to be copied</param>
        /// <returns>Path of the copied file</returns>
        public static async Task<string> CopyFile(string file) {
            string filePath = Settings.FILES_DIRECTORY + "\\" + Path.GetFileName(file);

            File.Copy(file, filePath);

            return filePath;
        }

        /// <summary>
        /// Copies a folder and its contents to the files directory of BitSynk
        /// </summary>
        /// <param name="folder">The path of the folder</param>
        /// <returns>The path of the copied folder</returns>
        public static async Task<string> CopyFolder(string folder) {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(folder);

            string destinationFolder = Settings.FILES_DIRECTORY + "\\" + dir.Name; // Settings.FILES_DIRECTORY + "\\" + Path.GetFileNameWithoutExtension(folder);

            if(!dir.Exists) {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + folder);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if(!Directory.Exists(destinationFolder)) {
                Directory.CreateDirectory(destinationFolder);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach(FileInfo file in files) {
                string temppath = Path.Combine(destinationFolder, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            foreach(DirectoryInfo subdir in dirs) {
                string temppath = Path.Combine(destinationFolder, subdir.Name);
                await CopyFolder(subdir.FullName);
            }

            return Settings.FILES_DIRECTORY + "\\" + Path.GetFileNameWithoutExtension(folder);
        }
    }
}
