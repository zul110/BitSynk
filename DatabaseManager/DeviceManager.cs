using DatabaseManager.Helpers;
using DatabaseManager.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseManager {
    public class DeviceManager {
        public async Task<bool> AddDeviceAsync(string deviceId, string deviceName, string deviceAddress, string userId, DeviceStatus deviceStatus) {
            using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand insertCommand = new MySqlCommand("INSERT INTO DEVICES (DEVICE_ID, DEVICE_NAME, DEVICE_ADDRESS, USER_ID, DEVICE_STATUS) VALUES (@deviceId, @deviceName, @deviceAddress, @userId, @deviceStatus)", connection);
                insertCommand.Parameters.AddWithValue("@deviceId", deviceId);
                insertCommand.Parameters.AddWithValue("@deviceName", deviceName);
                insertCommand.Parameters.AddWithValue("@deviceAddress", deviceAddress);
                insertCommand.Parameters.AddWithValue("@userId", userId);
                insertCommand.Parameters.AddWithValue("@deviceStatus", deviceStatus.ToString());

                int result = await insertCommand.ExecuteNonQueryAsync();

                return result > 0 ? true : false;
            }
        }

        public async Task<List<Device>> GetAllDevicesAsync() {
            using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
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
                        string deviceStatus = reader["DEVICE_STATUS"].ToString();

                        Device device = new Device();
                        device.DeviceId = deviceId;
                        device.DeviceName = deviceName;
                        device.DeviceAddress = deviceAddress;
                        device.UserId = userId;
                        device.DeviceStatus = (DeviceStatus)Enum.Parse(typeof(DeviceStatus), deviceStatus, true);

                        devices.Add(device);
                    }

                    return devices;
                }
            }
        }

        public async Task<List<Device>> GetAllDevicesByUserAsync(string _userId) {
            using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
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
                        string deviceStatus = reader["DEVICE_STATUS"].ToString();

                        Device device = new Device();
                        device.DeviceId = deviceId;
                        device.DeviceName = deviceName;
                        device.DeviceAddress = deviceAddress;
                        device.UserId = userId;
                        device.DeviceStatus = (DeviceStatus)Enum.Parse(typeof(DeviceStatus), deviceStatus, true);

                        devices.Add(device);
                    }

                    return devices;
                }
            }
        }

        public async Task<Device> GetDeviceAsync(string _deviceId) {
            using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
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
                        string deviceStatus = reader["DEVICE_STATUS"].ToString();

                        device.DeviceId = deviceId;
                        device.DeviceName = deviceName;
                        device.DeviceAddress = deviceAddress;
                        device.UserId = userId;
                        device.DeviceStatus = (DeviceStatus)Enum.Parse(typeof(DeviceStatus), deviceStatus, true);
                    }

                    return device;
                }
            }
        }

        public async Task<DeviceStatus> GetDeviceStatusByIdAsync(string deviceId) {
            using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM DEVICES WHERE DEVICE_ID = @deviceId", connection);
                selectCommand.Parameters.AddWithValue("deviceId", deviceId);

                using(MySqlDataReader reader = (await selectCommand.ExecuteReaderAsync() as MySqlDataReader)) {
                    DeviceStatus _deviceStatus = DeviceStatus.Unknown;

                    while(reader.Read()) {
                        //string deviceId = reader["DEVICE_ID"].ToString();
                        //string deviceName = reader["DEVICE_NAME"].ToString();
                        //string deviceAddress = reader["DEVICE_ADDRESS"].ToString();
                        //string userId = reader["USER_ID"].ToString();
                        string deviceStatus = reader["DEVICE_STATUS"].ToString();

                        //device.DeviceId = deviceId;
                        //device.DeviceName = deviceName;
                        //device.DeviceAddress = deviceAddress;
                        //device.UserId = userId;
                        _deviceStatus = (DeviceStatus)Enum.Parse(typeof(DeviceStatus), deviceStatus, true);
                    }

                    return _deviceStatus;
                }
            }
        }

        public async Task<DeviceStatus> GetDeviceStatusByNameAsync(string deviceName) {
            using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM DEVICES WHERE DEVICE_NAME = @deviceName", connection);
                selectCommand.Parameters.AddWithValue("deviceName", deviceName);

                using(MySqlDataReader reader = (await selectCommand.ExecuteReaderAsync() as MySqlDataReader)) {
                    DeviceStatus _deviceStatus = DeviceStatus.Unknown;

                    while(reader.Read()) {
                        //string deviceId = reader["DEVICE_ID"].ToString();
                        //string deviceName = reader["DEVICE_NAME"].ToString();
                        //string deviceAddress = reader["DEVICE_ADDRESS"].ToString();
                        //string userId = reader["USER_ID"].ToString();
                        string deviceStatus = reader["DEVICE_STATUS"].ToString();

                        //device.DeviceId = deviceId;
                        //device.DeviceName = deviceName;
                        //device.DeviceAddress = deviceAddress;
                        //device.UserId = userId;
                        _deviceStatus = (DeviceStatus)Enum.Parse(typeof(DeviceStatus), deviceStatus, true);
                    }

                    return _deviceStatus;
                }
            }
        }

        public async Task<bool> UpdateDeviceAsync(string deviceId, string deviceName, string deviceAddress, string userId, DeviceStatus deviceStatus) {
            using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand updateCommand = new MySqlCommand("UPDATE DEVICES SET DEVICE_NAME = @deviceName, DEVICE_ADDRESS = @deviceAddress, USER_ID = @userId, DEVICE_STATUS = @deviceStatus WHERE DEVICE_ID = @deviceId", connection);
                updateCommand.Parameters.AddWithValue("@deviceId", deviceId);
                updateCommand.Parameters.AddWithValue("@deviceName", deviceName);
                updateCommand.Parameters.AddWithValue("@deviceAddress", deviceAddress);
                updateCommand.Parameters.AddWithValue("@userId", userId);
                updateCommand.Parameters.AddWithValue("@deviceStatus", deviceStatus);

                int result = await updateCommand.ExecuteNonQueryAsync();

                return result > 0 ? true : false;
            }
        }
    }
}
