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
    /// Manages the device records in the database
    /// </summary>
    public class DeviceManager : BaseDatabaseManager {
        /// <summary>
        /// Adds a device to the database
        /// </summary>
        /// <param name="deviceId">Unique device ID</param>
        /// <param name="deviceName">Device's name</param>
        /// <param name="deviceAddress">The IP address of the device</param>
        /// <param name="userId">The user ID of the user that owns the device</param>
        /// <param name="lastSeen">The last date/time the user was seen online (when the user was added)</param>
        /// <returns>A boolean value indicating whether the user was added successfully or not</returns>
        public async Task<bool> AddDeviceAsync(string deviceId, string deviceName, string deviceAddress, string userId, DateTime lastSeen) {
            using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand insertCommand = new MySqlCommand("INSERT INTO DEVICES (DEVICE_ID, DEVICE_NAME, DEVICE_ADDRESS, USER_ID, LAST_SEEN) VALUES (@deviceId, @deviceName, @deviceAddress, @userId, @lastSeen)", connection);
                insertCommand.Parameters.AddWithValue("@deviceId", deviceId);
                insertCommand.Parameters.AddWithValue("@deviceName", deviceName);
                insertCommand.Parameters.AddWithValue("@deviceAddress", deviceAddress);
                insertCommand.Parameters.AddWithValue("@userId", userId);
                insertCommand.Parameters.AddWithValue("@lastSeen", lastSeen);

                int result = await insertCommand.ExecuteNonQueryAsync();

                return result > 0 ? true : false;
            }
        }

        /// <summary>
        /// Gets all the devices from all users in the database
        /// </summary>
        /// <returns>List of all devices</returns>
        public async Task<List<Device>> GetAllDevicesAsync() {
            using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM DEVICES", connection);

                using(MySqlDataReader reader = (await selectCommand.ExecuteReaderAsync() as MySqlDataReader)) {
                    List<Device> devices = null;

                    while(reader.Read()) {
                        devices = new List<Device>();

                        string deviceId = reader["DEVICE_ID"].ToString();
                        string deviceName = reader["DEVICE_NAME"].ToString();
                        string deviceAddress = reader["DEVICE_ADDRESS"].ToString();
                        string userId = reader["USER_ID"].ToString();
                        DateTime lastSeen = DateTime.Parse(reader["LAST_SEEN"].ToString());

                        Device device = new Device();
                        device.DeviceId = deviceId;
                        device.DeviceName = deviceName;
                        device.DeviceAddress = deviceAddress;
                        device.UserId = userId;
                        device.LastSeen = lastSeen;

                        devices.Add(device);
                    }

                    return devices;
                }
            }
        }

        /// <summary>
        /// Gets all the devices owned by a user
        /// </summary>
        /// <param name="_userId">User ID</param>
        /// <returns>List of devices owned by the user</returns>
        public async Task<List<Device>> GetAllDevicesByUserAsync(string _userId) {
            using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM DEVICES WHERE USER_ID = @userId", connection);
                selectCommand.Parameters.AddWithValue("userId", _userId);

                using(MySqlDataReader reader = (await selectCommand.ExecuteReaderAsync() as MySqlDataReader)) {
                    List<Device> devices = new List<Device>();

                    while(reader.Read()) {
                        string deviceId = reader["DEVICE_ID"].ToString();
                        string deviceName = reader["DEVICE_NAME"].ToString();
                        string deviceAddress = reader["DEVICE_ADDRESS"].ToString();
                        string userId = reader["USER_ID"].ToString();
                        DateTime lastSeen = DateTime.Parse(reader["LAST_SEEN"].ToString());

                        Device device = new Device();
                        device.DeviceId = deviceId;
                        device.DeviceName = deviceName;
                        device.DeviceAddress = deviceAddress;
                        device.UserId = userId;
                        device.LastSeen = lastSeen;

                        devices.Add(device);
                    }

                    return devices;
                }
            }
        }

        /// <summary>
        /// Gets a single device by its device ID
        /// </summary>
        /// <param name="_deviceId">The device ID</param>
        /// <returns>Device that matches the given device ID</returns>
        public async Task<Device> GetDeviceAsync(string _deviceId) {
            using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM DEVICES WHERE DEVICE_ID = @deviceId", connection);
                selectCommand.Parameters.AddWithValue("deviceId", _deviceId);

                using(MySqlDataReader reader = (await selectCommand.ExecuteReaderAsync() as MySqlDataReader)) {
                    Device device = null;

                    while(reader.Read()) {
                        device = new Device();

                        string deviceId = reader["DEVICE_ID"].ToString();
                        string deviceName = reader["DEVICE_NAME"].ToString();
                        string deviceAddress = reader["DEVICE_ADDRESS"].ToString();
                        string userId = reader["USER_ID"].ToString();
                        DateTime lastSeen = DateTime.Parse(reader["LAST_SEEN"].ToString());

                        device.DeviceId = deviceId;
                        device.DeviceName = deviceName;
                        device.DeviceAddress = deviceAddress;
                        device.UserId = userId;
                        device.LastSeen = lastSeen;
                    }

                    return device;
                }
            }
        }

        /// <summary>
        /// Gets the last seen date/time of the device by its device ID
        /// </summary>
        /// <param name="deviceId">The device ID</param>
        /// <returns>The date/time when the device was last seen online</returns>
        public async Task<DateTime> GetLastSeenByIdAsync(string deviceId) {
            using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM DEVICES WHERE DEVICE_ID = @deviceId", connection);
                selectCommand.Parameters.AddWithValue("deviceId", deviceId);

                using(MySqlDataReader reader = (await selectCommand.ExecuteReaderAsync() as MySqlDataReader)) {
                    DateTime lastSeen = DateTime.UtcNow;

                    while(reader.Read()) {
                        lastSeen = DateTime.Parse(reader["LAST_SEEN"].ToString());
                    }

                    return lastSeen;
                }
            }
        }

        /// <summary>
        /// Gets the last seen date/time of the device by its name
        /// </summary>
        /// <param name="deviceName">The name of the device</param>
        /// <returns>The last seen date/time</returns>
        public async Task<DateTime> GetLastSeenByNameAsync(string deviceName) {
            using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM DEVICES WHERE DEVICE_NAME = @deviceName", connection);
                selectCommand.Parameters.AddWithValue("deviceName", deviceName);

                using(MySqlDataReader reader = (await selectCommand.ExecuteReaderAsync() as MySqlDataReader)) {
                    DateTime lastSeen = DateTime.UtcNow;

                    while(reader.Read()) {
                        lastSeen = DateTime.Parse(reader["LAST_SEEN"].ToString());
                    }

                    return lastSeen;
                }
            }
        }

        /// <summary>
        /// Updates the device with matching device ID
        /// </summary>
        /// <param name="deviceId">The device ID</param>
        /// <param name="deviceName">The name of the device</param>
        /// <param name="deviceAddress">The IP address of the device</param>
        /// <param name="userId">The user ID of the user that owns the device</param>
        /// <param name="lastSeen">The date/time when the device was seen online</param>
        /// <returns></returns>
        public async Task<bool> UpdateDeviceAsync(string deviceId, string deviceName, string deviceAddress, string userId, DateTime lastSeen) {
            using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand updateCommand = new MySqlCommand("UPDATE DEVICES SET DEVICE_NAME = @deviceName, DEVICE_ADDRESS = @deviceAddress, USER_ID = @userId, LAST_SEEN = @lastSeen WHERE DEVICE_ID = @deviceId", connection);
                updateCommand.Parameters.AddWithValue("@deviceId", deviceId);
                updateCommand.Parameters.AddWithValue("@deviceName", deviceName);
                updateCommand.Parameters.AddWithValue("@deviceAddress", deviceAddress);
                updateCommand.Parameters.AddWithValue("@userId", userId);
                updateCommand.Parameters.AddWithValue("@lastSeen", lastSeen);

                int result = await updateCommand.ExecuteNonQueryAsync();

                return result > 0 ? true : false;
            }
        }

        /// <summary>
        /// Checks whether the device is currently online or not
        /// </summary>
        /// <param name="deviceId">The device ID</param>
        /// <returns>
        /// Boolean value denoting whether the user is online or not
        /// (> 6 minutes = offline, as the device heartbeat runs every 5 minutes
        /// </returns>
        public async Task<bool> IsOnline(string deviceId) {
            return (DateTime.UtcNow - (await GetLastSeenByIdAsync(deviceId))) < TimeSpan.FromMinutes(6);
        }
    }
}
