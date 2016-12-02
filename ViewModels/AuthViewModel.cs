using Helpers;
using DatabaseManager;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels {
    /// <summary>
    /// User Authentication view model. Inherits PropertyChanged notifier from the BaseViewModel class.
    /// Has the following functions:
    /// 1. Register a new user
    /// 2. Log a user in
    /// 3. Log a user out of the app
    /// 4. Link a user's devices
    /// </summary>
    public class AuthViewModel : BaseViewModel {
        /// <summary>
        /// Stores user's information.
        /// Has a public getter, and a private setter
        /// </summary>
        private User user;
        public User User {
            get { return user; }
            private set {
                user = value;

                // Notify observers that the property has been changed
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Privately sets, or publicly gets whether the user is logged in or not
        /// </summary>
        private static bool isLoggedIn;
        public static bool IsLoggedIn
        {
            get { return isLoggedIn; }
            private set {
                isLoggedIn = value;
            }
        }
        
        /// <summary>
        /// Empty constructor. No initiation required in this ViewModel
        /// </summary>
        public AuthViewModel() {

        }

        /// <summary>
        /// Add a new user asynchronously (if it does not already exist)
        /// </summary>
        /// <param name="userEmail">New user's email address</param>
        /// <param name="userPassword">New user's chosen password</param>
        /// <returns>If the user is added successfully, returns true; else false.</returns>
        public async Task<bool> Register(string userEmail, string userPassword) {
            try {
                // Initialize the UserManager
                UserManager userManager = new UserManager();

                // Get a user with the matching ID (to check if the user already exists)
                User user = await userManager.GetUserAsync(Settings.USER_ID);           
                
                // If the user does exist,
                if(user != null) {
                    // Check if its records are empty, and update its email and password
                    // This is done to future-proof BitSynk.
                    // Currently, email and password are not required;
                    // a user id is generated automatically, and the user's record is created.
                    // However, in the future updates, BitSynk might introduce a (set of) feature(s)
                    // to allow the users to make their accounts more private, by providing their emails,
                    // as well as their passwords to BitSynk. In this case, this method will
                    // seamlessly add the new information to the database, and simply update the user's record.
                    // Additional information, if required, can be added, too, by adding more fields (eg: UserName)
                    if((user.UserEmail == null || user.UserEmail == "") && (user.UserPassword == null || user.UserPassword == "")) {
                        // Update the user record with their email and password
                        await userManager.UpdateUser(Settings.USER_ID, Guid.NewGuid().ToString(), userPassword, userEmail);

                        // Flag the user as logged in
                        IsLoggedIn = true;

                        // Return true to the calling method
                        return true;
                    } else {
                        // Flag the user as not logged in, and throw an exception
                        IsLoggedIn = false;

                        // The user exists, and has its email and password saved in the records. The user is adviced to log in instead.
                        throw new Exception("User exists. Please log in with your email and password to continue.");
                    }
                } else {
                    // The user does not exist, and is added to the database.
                    await userManager.AddUserAsync(Settings.USER_ID, Guid.NewGuid().ToString(), userPassword, userEmail);

                    // The new user is flagged as logged in
                    IsLoggedIn = true;

                    // Return true to the calling method
                    return true;
                }
            } catch(Exception ex) {
                // In case something goes wrong,
                // The new user is flagged as not logged in
                IsLoggedIn = false;

                // The exception is thrown to the calling method
                throw ex;
            }
        }

        /// <summary>
        /// Log the user in asynchronously if it exists; link the user's devices if the user logs in using a new device
        /// </summary>
        /// <param name="userEmail">The user's email address</param>
        /// <param name="userPassword">The user's password</param>
        /// <returns>Return true if the user is logged in successfull; false if the user could not be logged in</returns>
        public async Task<bool> Login(string userEmail, string userPassword) {
            try {
                //Initiate the UserManager
                UserManager userManager = new UserManager();

                // Get the user that matches the given credentials
                User user = await userManager.GetUserAsync(userEmail, userPassword);

                // If log in is successful (the user exists)
                if(user != null) {
                    // Link devices, if logged in on a new device
                    await LinkDevices(user);

                    // Flag the user as logged in
                    IsLoggedIn = true;
                    
                    // Return true to the calling function
                    return true;
                } else {
                    // Flag the user as not logged in, if the log in fails
                    IsLoggedIn = false;

                    // Exception is raised, notifying the user to try again with valid credentials
                    throw new Exception("Invalid credentials. Please try again, or create a new account to continue.");
                }
            } catch(Exception ex) {
                // In case of an error,
                // Flag the user as not logged in
                IsLoggedIn = false;

                // Throw the exception to the calling function
                throw ex;
            }
        }

        /// <summary>
        /// Log the user out by resetting the local credentials
        /// </summary>
        /// <returns>Notify the calling function that the user has successfully been logged out</returns>
        public bool Logout() {
            Settings.USER_ID = "";
            Settings.USER_EMAIL = "";

            IsLoggedIn = false;

            return true;
        }

        /// <summary>
        /// Link devices asynchronously using the user's code
        /// </summary>
        /// <param name="userCode">5 character long user code (acquired from the device link page in the application)</param>
        /// <returns>Returns true if the devices have been linked successfully; false if they are not</returns>
        public async Task<bool> LinkDevices(string userCode) {
            try {
                // Initialize the UserManager
                UserManager userManager = new UserManager();

                // Get the user with matching code
                User user = await userManager.GetUserWithMatchingCodeAsync(userCode);

                // If the user exists, link devices, and return true to the calling function
                if(user != null) {
                    await LinkDevices(user);

                    return true;
                }

                // If the user does not exist, return false to the calling function
                return false;
            } catch(Exception ex) {
                // In case something goes wrong,

                // Throw the exception to the calling function
                throw ex;
            }
        }

        /// <summary>
        /// Link devices asynchronously using the user's data
        /// 
        /// LOGIC:
        /// Download the user's data, and add the current device to the database under the user's data
        /// Remove the initial user data from the database and locally
        /// Add the user's new data to the local storage
        /// </summary>
        /// <param name="user">The complete user data, including its credentials</param>
        /// <returns>
        /// Returns nothing; this task is completed asynchronously, and thus can be awaited.
        /// The local persistent storage is updated instead.
        /// </returns>
        private async Task LinkDevices(User user) {
            try {
                // If the device update is successful (i.e. if the device already exists, and its user id has changed)
                if(await new DeviceManager().UpdateDeviceAsync(Settings.DEVICE_ID, Settings.DEVICE_NAME, Utils.GetPublicIPAddress(), user.UserId, DateTime.UtcNow)) {
                    // If there were any files being shared by the user on the current device, WITHOUT linking the device,
                    // change their users to the updated user
                    await new FileManager().ChangeFileUser(user.UserId, Settings.USER_ID);

                    // Remove the previous user's records
                    await new UserManager().RemoveUserAsync(Settings.USER_ID);

                    // Save the credentials locally
                    Settings.USER_ID = user.UserId;
                    Settings.USER_EMAIL = user.UserEmail;

                    // Save the credentials in the persistent storage
                    Settings.SetValue(Constants.USER_ID, user.UserId);
                } else {
                    // Add a new device under the current user, if the device update fails
                    await new DeviceManager().AddDeviceAsync(Settings.DEVICE_ID, Settings.DEVICE_NAME, Utils.GetPublicIPAddress(), user.UserId, DateTime.UtcNow);
                }
            } catch(Exception ex) {
                // In case something goes wrong,

                // Throw the exception to the calling function
                throw ex;
            }
        }
    }
}
