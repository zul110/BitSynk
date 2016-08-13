using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitSynk.Helpers {
    public static class Settings {
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

        public static bool FIRST_RUN
        {
            get {
                string firstRun = GetValue(Constants.FIRST_RUN);
                return (firstRun == null || firstRun == "") ? true : bool.Parse(firstRun);
            }

            set {
                SetValue(Constants.FIRST_RUN, value.ToString());
            }
        }

        public static void Bootstrap() {
            SetupDirectories();

            InitUserId();

            InitDeviceId();

            InitDeviceName();
        }

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

        private static void InitDeviceName() {
            DEVICE_NAME = GetValue(Constants.DEVICE_NAME);

            if(DEVICE_NAME == null || DEVICE_NAME == "") {
                DEVICE_NAME = Guid.NewGuid().ToString();

                SetValue(Constants.DEVICE_NAME, DEVICE_NAME);
            }
        }

        private static void InitDeviceId() {
            DEVICE_ID = GetValue(Constants.DEVICE_ID);

            if(DEVICE_ID == null || DEVICE_ID == "") {
                DEVICE_ID = Guid.NewGuid().ToString();

                SetValue(Constants.DEVICE_ID, DEVICE_ID);
            }
        }

        private static void InitUserId() {
            USER_ID = GetValue(Constants.USER_ID);

            if(USER_ID == null || USER_ID == "") {
                USER_ID = Guid.NewGuid().ToString();

                SetValue(Constants.USER_ID, USER_ID);
            }
        }

        public static void SetValue(string key, string value) {
            Properties.Settings.Default[key] = value;
            Properties.Settings.Default.Save();
        }

        public static string GetValue(string key) {
            return Properties.Settings.Default[key]?.ToString();
        }

        public static void ResetValue(string key) {
            Properties.Settings.Default[key] = Properties.Settings.Default.Properties[key].DefaultValue;
        }
    }
}
