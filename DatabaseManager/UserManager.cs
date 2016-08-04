using DatabaseManager.Helpers;
using DatabaseManager.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseManager {
    public class UserManager {
        public async Task<bool> AddUserIdAsync(string userId) {
            using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand insertCommand = new MySqlCommand("INSERT INTO USERS (USER_ID) VALUES (@userId)", connection);
                insertCommand.Parameters.AddWithValue("@userId", userId);
                //insertCommand.Parameters.AddWithValue("@userName", userName);
                //insertCommand.Parameters.AddWithValue("@userPassword", userPassword);
                //insertCommand.Parameters.AddWithValue("@userEmail", userEmail);

                int result = await insertCommand.ExecuteNonQueryAsync();

                return result > 0 ? true : false;
            }
        }

        public async Task<bool> AddUserAsync(string userId, string userName, string userPassword, string userEmail) {
            using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
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

        public async Task<bool> UpdateUser(string userId, string userName, string userPassword, string userEmail) {
            using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
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

        public async Task<List<User>> GetAllUsers() {
            using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
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

        public async Task<User> GetUserAsync(string _userId) {
            using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
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

        public async Task<User> GetUserWithMatchingCodeAsync(string _userCode) {
            using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM USERS WHERE USER_ID LIKE @userCode%", connection);
                selectCommand.Parameters.AddWithValue("@userCode", _userCode);

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

        public async Task<User> GetUserAsync(string _userEmail, string _userPassword) {
            using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM USERS WHERE USER_EMAIL = @userEmail AND USER_PASSWORD = @userPassword", connection);
                selectCommand.Parameters.AddWithValue("@userEmail", _userEmail);
                selectCommand.Parameters.AddWithValue("@userPassword", _userPassword);

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

        public async Task<bool> RemoveUserAsync(string userId) {
            if(await GetUserAsync(userId) != null) {
                using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
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
