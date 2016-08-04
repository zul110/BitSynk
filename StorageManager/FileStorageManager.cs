using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageManager {
    public class FileStorageManager : IStorageManager {
        public FileStorageManager() {

        }

        public bool CreateFile(string path, string savePath) {
            throw new NotImplementedException();
        }

        public bool RemoveFile(string path) {
            try {
                File.Delete(path);

                return true;
            } catch(Exception ex) {
                return false;
            }
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
