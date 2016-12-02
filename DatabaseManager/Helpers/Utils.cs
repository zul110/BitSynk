using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseManager.Helpers {
    /// <summary>
    /// Utility methods for database management
    /// </summary>
    public static class Utils {
        /// <summary>
        /// Reads the contents of a file asynchronously
        /// </summary>
        /// <param name="filePath">The file path</param>
        /// <returns>Contents of the file in bytes</returns>
        public static async Task<byte[]> ReadFileAsync(string filePath) {
            return File.ReadAllBytes(filePath);
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
    }
}
