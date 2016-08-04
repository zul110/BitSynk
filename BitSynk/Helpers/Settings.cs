using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitSynk.Helpers {
    public static class Settings {
        public static string USER_ID;
        public static string USER_EMAIL;
        public static string DEVICE_ID;
        public static string DEVICE_NAME;

        public static void Bootstrap() {
            InitUserId();

            InitDeviceId();

            InitDeviceName();
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
    }
}
