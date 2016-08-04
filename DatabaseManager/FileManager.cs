using DatabaseManager.Helpers;
using DatabaseManager.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseManager
{
    public class FileManager {
        public async Task<List<File>> GetAllFilesAsync() {
            using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM FILES", connection);

                using(MySqlDataReader reader = (await selectCommand.ExecuteReaderAsync() as MySqlDataReader)) {
                    List<File> files = null;

                    while(reader.Read()) {
                        files = new List<File>();

                        string fileId = reader["FILE_ID"].ToString();
                        string fileName = reader["FILE_NAME"].ToString();
                        string fileHash = reader["FILE_HASH"].ToString();
                        string userId = reader["USER_ID"].ToString();
                        string deviceId = reader["DEVICE_ID"].ToString();

                        File file = new File();
                        file.FileId = fileId;
                        file.FileName = fileName;
                        file.FileHash = fileHash;
                        file.UserId = userId;
                        file.DeviceId = deviceId;
                        //user.Devices = new DeviceManager().GetAllDevicesByUser(userId);

                        files.Add(file);
                    }

                    return files;
                }
            }
        }

        public async Task<List<File>> GetAllFilesWithDeviceAsync(string _deviceId) {
            using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM FILES WHERE DEVICE_ID = @deviceId", connection);
                selectCommand.Parameters.AddWithValue("@deviceId", _deviceId);

                using(MySqlDataReader reader = (await selectCommand.ExecuteReaderAsync() as MySqlDataReader)) {
                    List<File> files = null;

                    while(reader.Read()) {
                        files = new List<File>();

                        string fileId = reader["FILE_ID"].ToString();
                        string fileName = reader["FILE_NAME"].ToString();
                        string fileHash = reader["FILE_HASH"].ToString();
                        string userId = reader["USER_ID"].ToString();
                        string deviceId = reader["DEVICE_ID"].ToString();

                        File file = new File();
                        file.FileId = fileId;
                        file.FileName = fileName;
                        file.FileHash = fileHash;
                        file.UserId = userId;
                        file.DeviceId = deviceId;
                        //user.Devices = new DeviceManager().GetAllDevicesByUser(userId);

                        files.Add(file);
                    }

                    return files;
                }
            }
        }

        public async Task<List<File>> GetAllFilesWithUserAsync(string _userId) {
            using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM FILES WHERE USER_ID = @userId", connection);
                selectCommand.Parameters.AddWithValue("@userId", _userId);

                using(MySqlDataReader reader = (await selectCommand.ExecuteReaderAsync() as MySqlDataReader)) {
                    List<File> files = null;

                    while(reader.Read()) {
                        files = new List<File>();

                        string fileId = reader["FILE_ID"].ToString();
                        string fileName = reader["FILE_NAME"].ToString();
                        string fileHash = reader["FILE_HASH"].ToString();
                        string userId = reader["USER_ID"].ToString();
                        string deviceId = reader["DEVICE_ID"].ToString();

                        File file = new File();
                        file.FileId = fileId;
                        file.FileName = fileName;
                        file.FileHash = fileHash;
                        file.UserId = userId;
                        file.DeviceId = deviceId;
                        //user.Devices = new DeviceManager().GetAllDevicesByUser(userId);

                        files.Add(file);
                    }

                    return files;
                }
            }
        }

        public async Task<Device> GetDeviceWithFileAsync(string _fileId) {
            using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM FILES WHERE FILE_ID = @fileId", connection);
                selectCommand.Parameters.AddWithValue("@fileId", _fileId);

                using(MySqlDataReader reader = selectCommand.ExecuteReader()) {
                    File file = null;

                    while(reader.Read()) {
                        file = new File();

                        string fileId = reader["FILE_ID"].ToString();
                        string fileName = reader["FILE_NAME"].ToString();
                        string fileHash = reader["FILE_HASH"].ToString();
                        string userId = reader["USER_ID"].ToString();
                        string deviceId = reader["DEVICE_ID"].ToString();


                        file.FileId = fileId;
                        file.FileName = fileName;
                        file.FileHash = fileHash;
                        file.UserId = userId;
                        file.DeviceId = deviceId;
                        //user.Devices = new DeviceManager().GetAllDevicesByUser(userId);
                    }

                    return await new DeviceManager().GetDeviceAsync(file?.DeviceId);
                }
            }
        }

        public async Task<File> GetFileByIdAsync(string _fileId) {
            using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM FILES WHERE FILE_ID = @fileId", connection);
                selectCommand.Parameters.AddWithValue("@fileId", _fileId);

                using(MySqlDataReader reader = (await selectCommand.ExecuteReaderAsync() as MySqlDataReader)) {
                    File file = null;

                    while(reader.Read()) {
                        file = new File();

                        string fileId = reader["FILE_ID"].ToString();
                        string fileName = reader["FILE_NAME"].ToString();
                        string fileHash = reader["FILE_HASH"].ToString();
                        string userId = reader["USER_ID"].ToString();
                        string deviceId = reader["DEVICE_ID"].ToString();


                        file.FileId = fileId;
                        file.FileName = fileName;
                        file.FileHash = fileHash;
                        file.UserId = userId;
                        file.DeviceId = deviceId;
                        //user.Devices = new DeviceManager().GetAllDevicesByUser(userId);
                    }

                    return file;
                }
            }
        }

        public async Task<File> GetFileByNameAsync(string _fileName) {
            using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM FILES WHERE FILE_NAME = @fileName", connection);
                selectCommand.Parameters.AddWithValue("@fileName", _fileName);

                using(MySqlDataReader reader = (await selectCommand.ExecuteReaderAsync() as MySqlDataReader)) {
                    File file = null;

                    while(reader.Read()) {
                        file = new File();

                        string fileId = reader["FILE_ID"].ToString();
                        string fileName = reader["FILE_NAME"].ToString();
                        string fileHash = reader["FILE_HASH"].ToString();
                        string userId = reader["USER_ID"].ToString();
                        string deviceId = reader["DEVICE_ID"].ToString();


                        file.FileId = fileId;
                        file.FileName = fileName;
                        file.FileHash = fileHash;
                        file.UserId = userId;
                        file.DeviceId = deviceId;
                        //user.Devices = new DeviceManager().GetAllDevicesByUser(userId);
                    }

                    return file;
                }
            }
        }

        public async Task<string> GetFileNameAsync(string _fileId) {
            using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM FILES WHERE FILE_ID = @fileId", connection);
                selectCommand.Parameters.AddWithValue("@fileId", _fileId);

                using(MySqlDataReader reader = (await selectCommand.ExecuteReaderAsync() as MySqlDataReader)) {
                    File file = null;

                    while(reader.Read()) {
                        file = new File();

                        string fileId = reader["FILE_ID"].ToString();
                        string fileName = reader["FILE_NAME"].ToString();
                        string fileHash = reader["FILE_HASH"].ToString();
                        string userId = reader["USER_ID"].ToString();
                        string deviceId = reader["DEVICE_ID"].ToString();


                        file.FileId = fileId;
                        file.FileName = fileName;
                        file.FileHash = fileHash;
                        file.UserId = userId;
                        file.DeviceId = deviceId;
                        //user.Devices = new DeviceManager().GetAllDevicesByUser(userId);
                    }

                    return file?.FileName;
                }
            }
        }

        public async Task<User> GetUserWithFileAsync(string _fileId) {
            using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM FILES WHERE FILE_ID = @fileId", connection);
                selectCommand.Parameters.AddWithValue("@fileId", _fileId);

                using(MySqlDataReader reader = (await selectCommand.ExecuteReaderAsync() as MySqlDataReader)) {
                    File file = null;

                    while(reader.Read()) {
                        file = new File();

                        string fileId = reader["FILE_ID"].ToString();
                        string fileName = reader["FILE_NAME"].ToString();
                        string fileHash = reader["FILE_HASH"].ToString();
                        string userId = reader["USER_ID"].ToString();
                        string deviceId = reader["DEVICE_ID"].ToString();


                        file.FileId = fileId;
                        file.FileName = fileName;
                        file.FileHash = fileHash;
                        file.UserId = userId;
                        file.DeviceId = deviceId;
                        //user.Devices = new DeviceManager().GetAllDevicesByUser(userId);
                    }

                    return await new UserManager().GetUserAsync(file?.UserId);
                }
            }
        }

        public async Task<bool> AddFileAsync(string fileId, string fileName, string fileHash, string userId, string deviceId) {
            using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand insertCommand = new MySqlCommand("INSERT INTO FILES (FILE_ID, FILE_NAME, FILE_HASH, USER_ID, DEVICE_ID) VALUES (@fileId, @fileName, @fileHash, @userId, @deviceId)", connection);
                insertCommand.Parameters.AddWithValue("@fileId", fileId);
                insertCommand.Parameters.AddWithValue("@fileName", fileName);
                insertCommand.Parameters.AddWithValue("@fileHash", fileHash);
                insertCommand.Parameters.AddWithValue("@userId", userId);
                insertCommand.Parameters.AddWithValue("@deviceId", deviceId);

                int result = await insertCommand.ExecuteNonQueryAsync();

                return result > 0 ? true : false;
            }
        }

        public async Task<bool> RenameFileByIdAsync(string fileId, string userId, string newFileName) {
            using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand updateCommand = new MySqlCommand("UPDATE FILES FILE_NAME = @newFileName WHERE FILE_ID = @fileId AND USER_ID = @userId", connection);
                updateCommand.Parameters.AddWithValue("@fileId", fileId);
                updateCommand.Parameters.AddWithValue("@userId", userId);
                updateCommand.Parameters.AddWithValue("@newFileName", newFileName);

                int result = await updateCommand.ExecuteNonQueryAsync();

                return result > 0 ? true : false;
            }
        }

        public async Task<bool> RenameFileByNameAsync(string fileName, string userId, string newFileName) {
            using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand updateCommand = new MySqlCommand("UPDATE FILES FILE_NAME = @newFileName WHERE FILE_NAME = @fileName AND USER_ID = @userId", connection);
                updateCommand.Parameters.AddWithValue("@fileName", fileName);
                updateCommand.Parameters.AddWithValue("@userId", userId);
                updateCommand.Parameters.AddWithValue("@newFileName", newFileName);

                int result = await updateCommand.ExecuteNonQueryAsync();

                return result > 0 ? true : false;
            }
        }

        public async Task<bool> RemoveFileByIdAsync(string fileId, string userId) {
            if(await FileIdExistsAsync(fileId, userId)) {
                using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
                    connection.Open();

                    MySqlCommand deleteCommand = new MySqlCommand("DELETE FROM FILES WHERE FILE_ID = @fileId AND USER_ID = @userId", connection);
                    deleteCommand.Parameters.AddWithValue("@fileId", fileId);
                    deleteCommand.Parameters.AddWithValue("@userId", userId);

                    int result = await deleteCommand.ExecuteNonQueryAsync();

                    return result > 0 ? true : false;
                }
            }

            return false;
        }

        public async Task<bool> RemoveFileByNameAsync(string fileName, string userId) {
            if(await FileNameExistsAsync(fileName, userId)) {
                using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
                    connection.Open();

                    MySqlCommand deleteCommand = new MySqlCommand("DELETE FROM FILES WHERE FILE_NAME = @fileName AND USER_ID = @userId", connection);
                    deleteCommand.Parameters.AddWithValue("@fileName", fileName);
                    deleteCommand.Parameters.AddWithValue("@userId", userId);

                    int result = await deleteCommand.ExecuteNonQueryAsync();

                    return result > 0 ? true : false;
                }
            }

            return false;
        }

        public async Task<bool> FileIdExistsAsync(string fileId, string userId) {
            return await GetUserFileByIdAsync(fileId, userId) == null ? false : true;
        }

        public async Task<bool> FileNameExistsAsync(string fileName, string userId) {
            return await GetUserFileByNameAsync(fileName, userId) == null ? false : true;
        }

        public async Task<File> GetUserFileByNameAsync(string _fileName, string _userId) {
            using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM FILES WHERE FILE_NAME = @fileName AND USER_ID = @userId", connection);
                selectCommand.Parameters.AddWithValue("@fileName", _fileName);
                selectCommand.Parameters.AddWithValue("@userId", _userId);

                using(MySqlDataReader reader = (await selectCommand.ExecuteReaderAsync() as MySqlDataReader)) {
                    File file = null;

                    while(reader.Read()) {
                        file = new File();

                        string fileId = reader["FILE_ID"].ToString();
                        string fileName = reader["FILE_NAME"].ToString();
                        string fileHash = reader["FILE_HASH"].ToString();
                        string userId = reader["USER_ID"].ToString();
                        string deviceId = reader["DEVICE_ID"].ToString();


                        file.FileId = fileId;
                        file.FileName = fileName;
                        file.FileHash = fileHash;
                        file.UserId = userId;
                        file.DeviceId = deviceId;
                        //user.Devices = new DeviceManager().GetAllDevicesByUser(userId);
                    }

                    return file;
                }
            }
        }

        public async Task<File> GetUserFileByIdAsync(string _fileId, string _userId) {
            using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM FILES WHERE FILE_ID = @fileId AND USER_ID = @userId", connection);
                selectCommand.Parameters.AddWithValue("@fileId", _fileId);
                selectCommand.Parameters.AddWithValue("@userId", _userId);

                using(MySqlDataReader reader = (await selectCommand.ExecuteReaderAsync() as MySqlDataReader)) {
                    File file = null;

                    while(reader.Read()) {
                        file = new File();

                        string fileId = reader["FILE_ID"].ToString();
                        string fileName = reader["FILE_NAME"].ToString();
                        string fileHash = reader["FILE_HASH"].ToString();
                        string userId = reader["USER_ID"].ToString();
                        string deviceId = reader["DEVICE_ID"].ToString();


                        file.FileId = fileId;
                        file.FileName = fileName;
                        file.FileHash = fileHash;
                        file.UserId = userId;
                        file.DeviceId = deviceId;
                        //user.Devices = new DeviceManager().GetAllDevicesByUser(userId);
                    }

                    return file;
                }
            }
        }
    }
}
