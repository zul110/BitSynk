using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseManager.Models {
    public class User {
        string userId;
        string userName;
        string userPassword;
        string userEmail;
        List<Device> devices;
        
        public string UserId
        {
            get { return userId; }
            set { userId = value; }
        }
        
        public string UserName
        {
            get { return userName; }
            set { userName = value; }
        }
        
        public string UserPassword
        {
            get { return userPassword; }
            set { userPassword = value; }
        }
        
        public string UserEmail
        {
            get { return userEmail; }
            set { userEmail = value; }
        }
        
        public List<Device> Devices
        {
            get { return devices; }
            set { devices = value; }
        }
    }
}
