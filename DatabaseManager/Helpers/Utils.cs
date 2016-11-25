using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseManager.Helpers {
    public static class Utils {
        public static async Task<byte[]> ReadFileAsync(string filePath) {
            return File.ReadAllBytes(filePath);
        }

        public static string GetFileMD5Hash(string filePath) {
            using(var md5 = MD5.Create()) {
                using(var stream = File.OpenRead(filePath)) {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToUpper();
                }
            }
        }
    }
}
