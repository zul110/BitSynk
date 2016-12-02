using Helpers;
using ViewModels;
using DatabaseManager;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BitSynk {
    /// <summary>
    /// Bootstrap singleton class: initializes the application on launch
    /// </summary>
    public class Bootstrap {
        /// <summary>
        /// Readonly instance property of the Bootstrap class
        /// </summary>
        private static readonly Bootstrap instance = new Bootstrap();
        public static Bootstrap Instance {
            get {
                return instance;
            }
        }

        // Declare the 3 managers to be initialized
        private UserManager userManager;
        private DeviceManager deviceManager;
        private FileManager fileManager;

        /// <summary>
        /// Empty private constructor, to ensure the class cannot be initialized outside
        /// </summary>
        private Bootstrap() {
            
        }

        /// <summary>
        /// The bootstrap initializer
        /// </summary>
        public async void InitBootstrap() {
            // The bootstrap from the Settings, which initializes the ids and names
            Settings.Bootstrap();

            // Initialize the file tracker view model to create the files directory if it doesn't exist
            FileTrackerViewModel fileTrackerVM = new FileTrackerViewModel();

            // Registers the new user if it doesn't exist
            await InitUserInfoAsync();

            // Registers or updates the current device
            await InitDeviceInfoAsync();

            // Gets all files owned by the user from the database
            await InitFileInfoAsync();
        }

        /// <summary>
        /// If the current user does not exist (new user, first launch), register it with the database
        /// </summary>
        /// <returns>Nothing as it is an asynchronous task</returns>
        private async Task InitUserInfoAsync() {
            userManager = new UserManager();

            if(await userManager.GetUserAsync(Settings.USER_ID) == null) {
                await userManager.AddUserIdAsync(Settings.USER_ID);
            }
        }

        /// <summary>
        /// Registers the current device under the user, in case it is a new device,
        /// or updates the device's information (IP address and the date/time are the main pieces of information)
        /// </summary>
        /// <returns>Nothing, as it is an asynchronous task</returns>
        private async Task InitDeviceInfoAsync() {
            deviceManager = new DeviceManager();

            if(await deviceManager.GetDeviceAsync(Settings.DEVICE_ID) == null) {
                await deviceManager.AddDeviceAsync(Settings.DEVICE_ID, Settings.DEVICE_NAME, Utils.GetLocalIPAddress(), Settings.USER_ID, DateTime.UtcNow);
            } else {
                await deviceManager.UpdateDeviceAsync(Settings.DEVICE_ID, Settings.DEVICE_NAME, Utils.GetLocalIPAddress(), Settings.USER_ID, DateTime.UtcNow);
            }
        }

        /// <summary>
        /// Gets the files owned by the user
        /// </summary>
        /// <returns>Nothing, as it is an asynchronous task</returns>
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
