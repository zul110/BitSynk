using BencodeNET;
using BencodeNET.Objects;
using BitSynk.ViewModels;
using MonoTorrent;
using MonoTorrent.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BitSynk.Helpers {
    public static class Utils {
        public static string GetLocalIPAddress() {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach(var ip in host.AddressList) {
                if(ip.AddressFamily == AddressFamily.InterNetwork) {
                    return ip.ToString();
                }
            }

            throw new Exception("Local IP Address Not Found!");
        }

        public static IPAddress GetLocalIPAddressRaw() {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach(var ip in host.AddressList) {
                if(ip.AddressFamily == AddressFamily.InterNetwork) {
                    return ip;
                }
            }

            throw new Exception("Local IP Address Not Found!");
        }

        public static string GetMagnetLink(string torrentPath) {
            return String.Format("magnet:?xt=urn:btih:{0}&db={1}", GetTorrentInfoHash(torrentPath), Path.GetFileNameWithoutExtension(torrentPath));
        }

        public static string GetTorrentInfoHash(string torrentPath) {
            BencodeNET.Objects.TorrentFile torrent = Bencode.DecodeTorrentFile(torrentPath);

            return torrent.CalculateInfoHash();
        }

        public static string CreateTorrent(string path, string savePath) {
            // The class used for creating the torrent
            TorrentCreator c = new TorrentCreator();

            // Add one tier which contains two trackers
            RawTrackerTier tier = new RawTrackerTier();
            foreach(string tracker in new Trackers().trackers) {
                tier.Add(tracker.Trim());
            }
            //tier.Add("udp://tracker.openbittorrent.com:80/announce");
            //tier.Add("udp://tracker.publicbt.com:80/announce");
            //tier.Add("udp://tracker.ccc.de:80/announce");// "udp://tracker.openbittorrent.com:80");//"udp://tracker.opentrackr.org:1337/announce");//"http://www.torrent-downloads.to:2710/announce");//http://bttrack.9you.com/");//http://opensharing.org:2710/announce");
            //tier.Add("http://" + Utils.GetPublicIPAddress() + ":10000/announce/");//Utils.GetLocalIPAddress() + ":10000");// "http://localhost/announce");

            c.Announces.Add(tier);
            c.Comment = "BitSynk";
            c.CreatedBy = "Zul";
            c.Publisher = "zul";

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

            // Create the torrent file and save it directly to the specified path
            // Different overloads of 'Create' can be used to save the data to a Stream
            // or just return it as a BEncodedDictionary (its native format) so it can be
            // processed in memory
            string torrentPath = savePath + "\\" + Path.GetFileNameWithoutExtension(path) + ".torrent";

            if(!File.Exists(torrentPath)) {
                c.Create(fileSource, torrentPath); // GetTorrentFilePath(savePath, path));
                //FileTrackerViewModel.knownFiles.Add(savePath + "\\" + Path.GetFileName(path));
            }

            return torrentPath; // savePath + "\\" + Path.GetFileNameWithoutExtension(path) + ".torrent";

            //AnnounceFileAddition(Path.GetFileName(path), fileHash);
        }

        public static string GetTorrentFilePath(string savePath, string path) {
            return savePath + GetTorrentFileName(path);
        }

        public static string GetTorrentFileName(string path) {
            return Path.GetFileNameWithoutExtension(path) + ".torrent";
        }
    }
}
