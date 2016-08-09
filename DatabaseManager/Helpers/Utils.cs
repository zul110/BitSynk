using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseManager.Helpers {
    public static class Utils {
        public static async Task<string> ReadFileAsync(string filePath) {
            return File.ReadAllText(filePath);
        }
    }
}
