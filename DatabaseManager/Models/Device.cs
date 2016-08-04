using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseManager.Models {
    public class Device {
        string deviceId;
        string deviceName;
        string deviceAddress;
        string userId;
        DeviceStatus deviceStatus;
        
        public string DeviceId
        {
            get { return deviceId; }
            set { deviceId = value; }
        }
        
        public string DeviceName
        {
            get { return deviceName; }
            set { deviceName = value; }
        }
        
        public string DeviceAddress
        {
            get { return deviceAddress; }
            set { deviceAddress = value; }
        }
        
        public string UserId
        {
            get { return userId; }
            set { userId = value; }
        }
        
        public DeviceStatus DeviceStatus
        {
            get { return deviceStatus; }
            set { deviceStatus = value; }
        }
    }
}
