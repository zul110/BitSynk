using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseManager.Models {
    public class File {
        string fileId;
        string fileName;
        string fileHash;
        string userId;
        string deviceId;
        byte[] fileContents;
        
        public string FileId
        {
            get { return fileId; }
            set { fileId = value; }
        }
        
        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }
        
        public string FileHash
        {
            get { return fileHash; }
            set { fileHash = value; }
        }
        
        public string UserId
        {
            get { return userId; }
            set { userId = value; }
        }
        
        public string DeviceId
        {
            get { return deviceId; }
            set { deviceId = value; }
        }

        public byte[] FileContents
        {
            get { return fileContents; }
            set { fileContents = value; }
        }
    }
}
