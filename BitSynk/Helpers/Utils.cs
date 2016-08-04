using BencodeNET;
using BencodeNET.Objects;
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
            TorrentFile torrent = Bencode.DecodeTorrentFile(torrentPath);

            return torrent.CalculateInfoHash();
        }
    }
}
