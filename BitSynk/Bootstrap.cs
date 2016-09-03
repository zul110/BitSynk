using BitSynk.Helpers;
using BitSynk.ViewModels;
using DatabaseManager;
using DatabaseManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitSynk {
    public class Bootstrap {
        private static readonly Bootstrap instance = new Bootstrap();
        public static Bootstrap Instance {
            get {
                return instance;
            }
        }

        private UserManager userManager;
        private DeviceManager deviceManager;
        private FileManager fileManager;

        private Bootstrap() {
            
        }

        public async void InitBootstrap() {
            Settings.Bootstrap();

            FileTrackerViewModel fileTrackerVM = new FileTrackerViewModel();

            await InitUserInfoAsync();

            await InitDeviceInfoAsync();

            await InitFileInfoAsync();
        }

        private async Task InitUserInfoAsync() {
            userManager = new UserManager();

            if(await userManager.GetUserAsync(Settings.USER_ID) == null) {
                await userManager.AddUserIdAsync(Settings.USER_ID);
            }
        }

        private async Task InitDeviceInfoAsync() {
            deviceManager = new DeviceManager();

            if(await deviceManager.GetDeviceAsync(Settings.DEVICE_ID) == null) {
                await deviceManager.AddDeviceAsync(Settings.DEVICE_ID, Settings.DEVICE_NAME, Utils.GetPublicIPAddress(), Settings.USER_ID, DeviceStatus.Online);
            } else {
                await deviceManager.UpdateDeviceAsync(Settings.DEVICE_ID, Settings.DEVICE_NAME, Utils.GetPublicIPAddress(), Settings.USER_ID, DeviceStatus.Online);
            }
        }

        private async Task InitFileInfoAsync() {
            fileManager = new FileManager();

            List<File> files = (await fileManager.GetAllFilesWithUserAsync(Settings.USER_ID))?.ToList();

            if(files != null) {
                string s = "";

                foreach(File file in files) {
                    s += String.Format("\n\nFileId: {0}\nFileName: {1}\nUserId: {2}\nDeviceId: {3}\nDeviceAddress: {4}\n\n", file.FileId, file.FileName, file.UserId, file.DeviceId, (await deviceManager.GetDeviceAsync(file.DeviceId)).DeviceAddress);
                }

                Console.WriteLine(s);
            }
        }
    }
}
