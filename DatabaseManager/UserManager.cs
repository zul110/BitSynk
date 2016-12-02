using DatabaseManager.Helpers;
using Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseManager {
    /// <summary>
    /// Manages the user records in the database
    /// </summary>
    public class UserManager : BaseDatabaseManager {
        /// <summary>
        /// Adds a user asynchronously to the database
        /// Generates a new GUID as the user's name, as the current implementation of BitSynk does not require a user name
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>Returns true if the user is added successfully, false otherwise</returns>
        public async Task<bool> AddUserIdAsync(string userId) {
            using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand insertCommand = new MySqlCommand("INSERT INTO USERS (USER_ID, USER_NAME) VALUES (@userId, @userName)", connection);
                insertCommand.Parameters.AddWithValue("@userId", userId);
                insertCommand.Parameters.AddWithValue("@userName", Guid.NewGuid().ToString());
                //insertCommand.Parameters.AddWithValue("@userPassword", userPassword);
                //insertCommand.Parameters.AddWithValue("@userEmail", userEmail);

                int result = await insertCommand.ExecuteNonQueryAsync();

                return result > 0 ? true : false;
            }
        }

        /// <summary>
        /// Add user asynchronously with all the fields
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="userName">User's name</param>
        /// <param name="userPassword">User's password</param>
        /// <param name="userEmail">User's email</param>
        /// <returns>Returns true if the user is added successfully, false otherwise</returns>
        public async Task<bool> AddUserAsync(string userId, string userName, string userPassword, string userEmail) {
            using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand insertCommand = new MySqlCommand("INSERT INTO USERS (USER_ID, USER_NAME, USER_PASSWORD, USER_EMAIL) VALUES (@userId, @userName, @userPassword, @userEmail)", connection);
                insertCommand.Parameters.AddWithValue("@userId", userId);
                insertCommand.Parameters.AddWithValue("@userName", userName);
                insertCommand.Parameters.AddWithValue("@userPassword", userPassword);
                insertCommand.Parameters.AddWithValue("@userEmail", userEmail);

                int result = await insertCommand.ExecuteNonQueryAsync();

                return result > 0 ? true : false;
            }
        }

        /// <summary>
        /// Update the user's record
        /// </summary>
        /// <param name="userId">The user ID to match</param>
        /// <param name="userName">User name</param>
        /// <param name="userPassword">User password</param>
        /// <param name="userEmail">User email</param>
        /// <returns>Returns true if the user record is updated successfully, false otherwise</returns>
        public async Task<bool> UpdateUser(string userId, string userName, string userPassword, string userEmail) {
            using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand updateCommand = new MySqlCommand("UPDATE USERS USER_NAME = @userName, USER_PASSWORD = @userPassword, USER_EMAIL = @userEmail WHERE USER_ID = @userId", connection);
                updateCommand.Parameters.AddWithValue("@userId", userId);
                updateCommand.Parameters.AddWithValue("@userName", userName);
                updateCommand.Parameters.AddWithValue("@userPassword", userPassword);
                updateCommand.Parameters.AddWithValue("@userEmail", userEmail);

                int result = await updateCommand.ExecuteNonQueryAsync();

                return result > 0 ? true : false;
            }
        }

        /// <summary>
        /// Gets all the users in the database
        /// </summary>
        /// <returns>List of all users</returns>
        public async Task<List<User>> GetAllUsers() {
            using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM USERS", connection);

                using(MySqlDataReader reader = (await selectCommand.ExecuteReaderAsync() as MySqlDataReader)) {
                    List<User> users = null;

                    while(reader.Read()) {
                        users = new List<User>();

                        string userId = reader["USER_ID"].ToString();
                        string userName = reader["USER_NAME"].ToString();
                        //string userPassword = reader["USER_PASSWORD"].ToString();
                        string userEmail = reader["USER_EMAIL"].ToString();

                        User user = new User();
                        user.UserId = userId;
                        user.UserName = userName;
                        //user.UserPassword = userPassword;
                        user.UserEmail = userEmail;
                        user.Devices = await new DeviceManager().GetAllDevicesByUserAsync(userId);

                        users.Add(user);
                    }

                    return users;
                }
            }
        }

        /// <summary>
        /// Get the user with the matching user ID
        /// </summary>
        /// <param name="_userId">The user ID</param>
        /// <returns>The user with the matching user ID</returns>
        public async Task<User> GetUserAsync(string _userId) {
            using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM USERS WHERE USER_ID = @userId", connection);
                selectCommand.Parameters.AddWithValue("@userId", _userId);

                using(MySqlDataReader reader = (await selectCommand.ExecuteReaderAsync() as MySqlDataReader)) {
                    User user = null;

                    while(reader.Read()) {
                        user = new User();

                        string userId = reader["USER_ID"].ToString();
                        string userName = reader["USER_NAME"].ToString();
                        //string userPassword = reader["USER_PASSWORD"].ToString();
                        string userEmail = reader["USER_EMAIL"].ToString();

                        user.UserId = userId;
                        user.UserName = userName;
                        user.UserPassword = ""; //userPassword;
                        user.UserEmail = userEmail;
                        user.Devices = await new DeviceManager().GetAllDevicesByUserAsync(userId);
                    }

                    return user;
                }
            }
        }

        /// <summary>
        /// Gets the user with the matching user code (for device linking)
        /// </summary>
        /// <param name="_userCode">The user code to match</param>
        /// <returns>The matched user</returns>
        public async Task<User> GetUserWithMatchingCodeAsync(string _userCode) {
            using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM USERS WHERE USER_ID LIKE @userCode", connection);
                selectCommand.Parameters.AddWithValue("@userCode", _userCode + "%");

                using(MySqlDataReader reader = (await selectCommand.ExecuteReaderAsync() as MySqlDataReader)) {
                    User user = null;

                    while(reader.Read()) {
                        user = new User();

                        string userId = reader["USER_ID"].ToString();
                        string userName = reader["USER_NAME"].ToString();
                        //string userPassword = reader["USER_PASSWORD"].ToString();
                        string userEmail = reader["USER_EMAIL"].ToString();

                        user.UserId = userId;
                        user.UserName = userName;
                        user.UserPassword = ""; //userPassword;
                        user.UserEmail = userEmail;
                        user.Devices = await new DeviceManager().GetAllDevicesByUserAsync(userId);
                    }

                    return user;
                }
            }
        }

        /// <summary>
        /// Gets the user with matching credentials
        /// </summary>
        /// <param name="_userEmail">User email</param>
        /// <param name="_userPassword">User password</param>
        /// <returns>The user with the matching email and password</returns>
        public async Task<User> GetUserAsync(string _userEmail, string _userPassword) {
            using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
                connection.Open();

                string[] fields = { "*" };

                Dictionary<string, KeyValuePair<string, string>> paramsAndValues = new Dictionary<string, KeyValuePair<string, string>>();
                paramsAndValues.Add("USER_EMAIL", new KeyValuePair<string, string>("@userEmail", _userEmail));
                paramsAndValues.Add("USER_PASSWORD", new KeyValuePair<string, string>("@userPassword", _userPassword));

                MySqlCommand selectCommand = SelectCommand(connection, "USERS", fields, paramsAndValues);

                //MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM USERS WHERE USER_EMAIL = @userEmail AND USER_PASSWORD = @userPassword", connection);
                //selectCommand.Parameters.AddWithValue("@userEmail", _userEmail);
                //selectCommand.Parameters.AddWithValue("@userPassword", _userPassword);

                using(MySqlDataReader reader = (await selectCommand.ExecuteReaderAsync() as MySqlDataReader)) {
                    User user = null;

                    while(reader.Read()) {
                        user = new User();

                        string userId = reader["USER_ID"].ToString();
                        string userName = reader["USER_NAME"].ToString();
                        string userPassword = reader["USER_PASSWORD"].ToString();
                        string userEmail = reader["USER_EMAIL"].ToString();

                        user.UserId = userId;
                        user.UserName = userName;
                        user.UserPassword = userPassword;
                        user.UserEmail = userEmail;
                        user.Devices = await new DeviceManager().GetAllDevicesByUserAsync(userId);
                    }

                    return user;
                }
            }
        }

        /// <summary>
        /// Removes a user with the matching user ID asynchronously
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Returns true if the user is successfully removed, else false</returns>
        public async Task<bool> RemoveUserAsync(string userId) {
            if(await GetUserAsync(userId) != null) {
                using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
                    connection.Open();

                    MySqlCommand deleteCommand = new MySqlCommand("DELETE FROM USERS WHERE USER_ID = @userId", connection);
                    deleteCommand.Parameters.AddWithValue("@userId", userId);

                    int result = await deleteCommand.ExecuteNonQueryAsync();

                    return result > 0 ? true : false;
                }
            }

            return false;
        }
    }
}
