using BitSynk.Helpers;
using DatabaseManager;
using DatabaseManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitSynk.ViewModels {
    public class AuthViewModel : BaseViewModel {
        private User user;
        public User User {
            get { return user; }
            set {
                user = value;
                NotifyPropertyChanged();
            }
        }

        private static bool isLoggedIn;
        public static bool IsLoggedIn
        {
            get { return isLoggedIn; }
            set {
                isLoggedIn = value;
            }
        }


        public AuthViewModel() {

        }

        // Add a new user (if it does not already exist)
        public async Task<bool> Register(string userEmail, string userPassword) {
            try {
                UserManager userManager = new UserManager();
                User user = await userManager.GetUserAsync(Settings.USER_ID);
                
                if(user != null) {
                    if((user.UserEmail == null || user.UserEmail == "") && (user.UserPassword == null || user.UserPassword == "")) {
                        await userManager.UpdateUser(Settings.USER_ID, Guid.NewGuid().ToString(), userPassword, userEmail);

                        IsLoggedIn = true;

                        return true;
                    } else {
                        IsLoggedIn = false;

                        throw new Exception("User exists. Please log in with your email and password to continue.");
                    }
                } else {
                    await userManager.AddUserAsync(Settings.USER_ID, Guid.NewGuid().ToString(), userPassword, userEmail);

                    IsLoggedIn = true;

                    return true;
                }
            } catch(Exception ex) {
                IsLoggedIn = false;

                throw ex;
            }
        }

        // Log the user in if it exists
        // Also used to link devices
        public async Task<bool> Login(string userEmail, string userPassword) {
            try {
                UserManager userManager = new UserManager();
                User user = await userManager.GetUserAsync(userEmail, userPassword);

                // If log in is successful (the user exists)
                if(user != null) {
                    await LinkDevices(user);

                    IsLoggedIn = true;
                    
                    return true;
                } else {
                    IsLoggedIn = false;

                    throw new Exception("Invalid credentials. Please try again, or create a new account to continue.");
                }
            } catch(Exception ex) {
                IsLoggedIn = false;

                throw ex;
            }
        }

        public bool Logout() {
            Settings.USER_ID = "";
            Settings.USER_EMAIL = "";

            IsLoggedIn = false;

            return true;
        }

        public async Task<bool> LinkDevices(string userCode) {
            try {
                UserManager userManager = new UserManager();
                User user = await userManager.GetUserWithMatchingCodeAsync(userCode);

                if(user != null) {
                    await LinkDevices(user);

                    return true;
                }

                return false;
            } catch(Exception ex) {
                throw ex;
            }
        }

        private async Task LinkDevices(User user) {
            try {
                // Download the user's data, and add this device to the database under the user's data
                // Remove the temporary user data from the database and locally
                // Add the user's data to the local storage
                if(await new DeviceManager().UpdateDeviceAsync(Settings.DEVICE_ID, Settings.DEVICE_NAME, Utils.GetLocalIPAddress(), user.UserId, DeviceStatus.Online)) {
                    await new FileManager().ChangeFileUser(user.UserId, Settings.USER_ID);
                    await new UserManager().RemoveUserAsync(Settings.USER_ID);

                    Settings.USER_ID = user.UserId;
                    Settings.USER_EMAIL = user.UserEmail;
                } else {
                    await new DeviceManager().AddDeviceAsync(Settings.DEVICE_ID, Settings.DEVICE_NAME, Utils.GetLocalIPAddress(), user.UserId, DeviceStatus.Online);
                }
            } catch(Exception ex) {
                throw ex;
            }
        }
    }
}
