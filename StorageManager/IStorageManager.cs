using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageManager
{
    public interface IStorageManager {
        bool CreateFile(string path, string savePath);

        bool RemoveFile(string path);

        bool ChangeFileName(string path, string newName);

        bool ChangeFilePath(string path, string newPath);
    }
}
