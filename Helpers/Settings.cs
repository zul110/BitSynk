using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers {
    /// <summary>
    /// Settings accessible to the whole app
    /// </summary>
    public static class Settings {
        // The base path, where the application is installed
        private static string BASE_PATH = Environment.CurrentDirectory;

        public static string USER_ID;
        public static string USER_EMAIL;

        public static string DEVICE_ID;
        public static string DEVICE_NAME;

        public static string FILES_DIRECTORY_NAME;
        public static string FILES_DIRECTORY;

        public static string DOWNLOADS_DIRECTORY_NAME;
        public static string DOWNLOADS_DIRECTORY;

        public static bool AUTO_ADD = true;

        /// <summary>
        /// Checks if the app is run for the first time ever on the current device
        /// </summary>
        public static bool FIRST_RUN {
            get {
                string firstRun = GetValue(Constants.FIRST_RUN);
                return (firstRun == null || firstRun == "") ? true : bool.Parse(firstRun);
            }

            set {
                SetValue(Constants.FIRST_RUN, value.ToString());
            }
        }

        /// <summary>
        /// Bootstrap function: Initializes the app on launch
        /// </summary>
        public static void Bootstrap() {
            SetupDirectories();

            InitUserId();

            InitDeviceId();

            InitDeviceName();
        }

        /// <summary>
        /// Creates directories if required
        /// </summary>
        private static void SetupDirectories() {
            FILES_DIRECTORY_NAME = GetValue(Constants.FILES_DIRECTORY_NAME);
            DOWNLOADS_DIRECTORY_NAME = GetValue(Constants.DOWNLOADS_DIRECTORY_NAME);

            if(FILES_DIRECTORY_NAME == null || FILES_DIRECTORY_NAME == "") {
                FILES_DIRECTORY_NAME = Constants.DEFAULT_FILES_DIRECTORY_NAME;

                SetValue(Constants.FILES_DIRECTORY_NAME, FILES_DIRECTORY_NAME);
            }

            if(DOWNLOADS_DIRECTORY_NAME == null || DOWNLOADS_DIRECTORY_NAME == "") {
                DOWNLOADS_DIRECTORY_NAME = Constants.DEFAULT_DOWNLOADS_DIRECTORY_NAME;

                SetValue(Constants.DOWNLOADS_DIRECTORY_NAME, DOWNLOADS_DIRECTORY_NAME);
            }

            FILES_DIRECTORY = Path.Combine(BASE_PATH, FILES_DIRECTORY_NAME);
            DOWNLOADS_DIRECTORY = Path.Combine(BASE_PATH, DOWNLOADS_DIRECTORY_NAME);
        }

        /// <summary>
        /// Sets the device name for the session
        /// If the app is launched for the first time, creates a new name (a GUID for randomness), and makes it persistent
        /// </summary>
        private static void InitDeviceName() {
            DEVICE_NAME = GetValue(Constants.DEVICE_NAME);

            if(DEVICE_NAME == null || DEVICE_NAME == "") {
                DEVICE_NAME = Guid.NewGuid().ToString();

                SetValue(Constants.DEVICE_NAME, DEVICE_NAME);
            }
        }

        /// <summary>
        /// Sets the device id for the session
        /// If the app is launched for the first time, creates a new id, and makes it persistent
        /// </summary>
        private static void InitDeviceId() {
            DEVICE_ID = GetValue(Constants.DEVICE_ID);

            if(DEVICE_ID == null || DEVICE_ID == "") {
                DEVICE_ID = Guid.NewGuid().ToString();

                SetValue(Constants.DEVICE_ID, DEVICE_ID);
            }
        }

        /// <summary>
        /// Sets the user id for the session
        /// If the app is launched for the first time, creates a new id, and makes it persistent
        /// </summary>
        private static void InitUserId() {
            USER_ID = GetValue(Constants.USER_ID);

            if(USER_ID == null || USER_ID == "") {
                USER_ID = Guid.NewGuid().ToString();

                SetValue(Constants.USER_ID, USER_ID);
            }
        }

        /// <summary>
        /// Makes the value persistent
        /// </summary>
        /// <param name="key">Key to store the value in</param>
        /// <param name="value">The value to be stored</param>
        public static void SetValue(string key, string value) {
            Properties.Settings.Default[key] = value;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Gets a stored value
        /// </summary>
        /// <param name="key">The key in which the value is stored</param>
        /// <returns>The value as string, or null if the key doesn't exist</returns>
        public static string GetValue(string key) {
            return Properties.Settings.Default[key]?.ToString();
        }

        /// <summary>
        /// Resets a stored value with a system specified default value
        /// </summary>
        /// <param name="key">The key that has to be reset</param>
        public static void ResetValue(string key) {
            Properties.Settings.Default[key] = Properties.Settings.Default.Properties[key].DefaultValue;
        }
    }
}
