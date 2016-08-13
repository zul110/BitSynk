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
                    List<File> files = new List<File>();

                    while(reader.Read()) {
                        string fileId = reader["FILE_ID"].ToString();
                        string fileName = reader["FILE_NAME"].ToString();
                        string fileHash = reader["FILE_HASH"].ToString();
                        string userId = reader["USER_ID"].ToString();
                        string deviceId = reader["DEVICE_ID"].ToString();
                        byte[] fileContents = (byte[])reader["FILE_CONTENTS"];

                        File file = new File();
                        file.FileId = fileId;
                        file.FileName = fileName;
                        file.FileHash = fileHash;
                        file.UserId = userId;
                        file.DeviceId = deviceId;
                        file.FileContents = fileContents;
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
                    List<File> files = new List<File>();

                    while(reader.Read()) {
                        string fileId = reader["FILE_ID"].ToString();
                        string fileName = reader["FILE_NAME"].ToString();
                        string fileHash = reader["FILE_HASH"].ToString();
                        string userId = reader["USER_ID"].ToString();
                        string deviceId = reader["DEVICE_ID"].ToString();
                        byte[] fileContents = (byte[])reader["FILE_CONTENTS"];

                        File file = new File();
                        file.FileId = fileId;
                        file.FileName = fileName;
                        file.FileHash = fileHash;
                        file.UserId = userId;
                        file.DeviceId = deviceId;
                        file.FileContents = fileContents;
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
                    List<File> files = new List<File>();

                    while(reader.Read()) {
                        string fileId = reader["FILE_ID"].ToString();
                        string fileName = reader["FILE_NAME"].ToString();
                        string fileHash = reader["FILE_HASH"].ToString();
                        string userId = reader["USER_ID"].ToString();
                        string deviceId = reader["DEVICE_ID"].ToString();
                        byte[] fileContents = (byte[])reader["FILE_CONTENTS"];

                        File file = new File();
                        file.FileId = fileId;
                        file.FileName = fileName;
                        file.FileHash = fileHash;
                        file.UserId = userId;
                        file.DeviceId = deviceId;
                        file.FileContents = fileContents;
                        //user.Devices = new DeviceManager().GetAllDevicesByUser(userId);

                        files.Add(file);
                    }

                    return files;
                }
            }
        }

        //public async Task<Device> GetDeviceWithFileAsync(string _fileId) {
        //    using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
        //        connection.Open();

        //        MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM DEVICES WHERE FILE_ID = @fileId", connection);
        //        selectCommand.Parameters.AddWithValue("@fileId", _fileId);

        //        using(MySqlDataReader reader = selectCommand.ExecuteReader()) {
        //            Device device = null;

        //            while(reader.Read()) {
        //                device = new Device();

        //                string fileId = reader["FILE_ID"].ToString();
        //                string fileName = reader["FILE_NAME"].ToString();
        //                string fileHash = reader["FILE_HASH"].ToString();
        //                string userId = reader["USER_ID"].ToString();
        //                string deviceId = reader["DEVICE_ID"].ToString();
        //                byte[] fileContents = (byte[])reader["FILE_CONTENTS"];

        //                file.FileId = fileId;
        //                file.FileName = fileName;
        //                file.FileHash = fileHash;
        //                file.UserId = userId;
        //                file.DeviceId = deviceId;
        //                file.FileContents = fileContents;
        //                //user.Devices = new DeviceManager().GetAllDevicesByUser(userId);
        //            }

        //            return await new DeviceManager().GetDeviceAsync(file?.DeviceId);
        //        }
        //    }
        //}

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
                        byte[] fileContents = (byte[])reader["FILE_CONTENTS"];

                        file.FileId = fileId;
                        file.FileName = fileName;
                        file.FileHash = fileHash;
                        file.UserId = userId;
                        file.DeviceId = deviceId;
                        file.FileContents = fileContents;
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
                        byte[] fileContents = (byte[])reader["FILE_CONTENTS"];

                        file.FileId = fileId;
                        file.FileName = fileName;
                        file.FileHash = fileHash;
                        file.UserId = userId;
                        file.DeviceId = deviceId;
                        file.FileContents = fileContents;
                        //user.Devices = new DeviceManager().GetAllDevicesByUser(userId);
                    }

                    return file;
                }
            }
        }

        public async Task<File> GetFileByHashAsync(string _fileHash) {
            using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM FILES WHERE FILE_HASH = @fileHash", connection);
                selectCommand.Parameters.AddWithValue("@fileHash", _fileHash);

                using(MySqlDataReader reader = (await selectCommand.ExecuteReaderAsync() as MySqlDataReader)) {
                    File file = null;

                    while(reader.Read()) {
                        file = new File();

                        string fileId = reader["FILE_ID"].ToString();
                        string fileName = reader["FILE_NAME"].ToString();
                        string fileHash = reader["FILE_HASH"].ToString();
                        string userId = reader["USER_ID"].ToString();
                        string deviceId = reader["DEVICE_ID"].ToString();
                        byte[] fileContents = (byte[])reader["FILE_CONTENTS"];

                        file.FileId = fileId;
                        file.FileName = fileName;
                        file.FileHash = fileHash;
                        file.UserId = userId;
                        file.DeviceId = deviceId;
                        file.FileContents = fileContents;
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
                        byte[] fileContents = (byte[])reader["FILE_CONTENTS"];

                        file.FileId = fileId;
                        file.FileName = fileName;
                        file.FileHash = fileHash;
                        file.UserId = userId;
                        file.DeviceId = deviceId;
                        file.FileContents = fileContents;
                        //user.Devices = new DeviceManager().GetAllDevicesByUser(userId);
                    }

                    return file?.FileName;
                }
            }
        }

        //public async Task<User> GetUserWithFileAsync(string _fileId) {
        //    using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
        //        connection.Open();

        //        MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM FILES WHERE FILE_ID = @fileId", connection);
        //        selectCommand.Parameters.AddWithValue("@fileId", _fileId);

        //        using(MySqlDataReader reader = (await selectCommand.ExecuteReaderAsync() as MySqlDataReader)) {
        //            File file = null;

        //            while(reader.Read()) {
        //                file = new File();

        //                string fileId = reader["FILE_ID"].ToString();
        //                string fileName = reader["FILE_NAME"].ToString();
        //                string fileHash = reader["FILE_HASH"].ToString();
        //                string userId = reader["USER_ID"].ToString();
        //                string deviceId = reader["DEVICE_ID"].ToString();
        //                byte[] fileContents = (byte[])reader["FILE_CONTENTS"];

        //                file.FileId = fileId;
        //                file.FileName = fileName;
        //                file.FileHash = fileHash;
        //                file.UserId = userId;
        //                file.DeviceId = deviceId;
        //                file.FileContents = fileContents;
        //                //user.Devices = new DeviceManager().GetAllDevicesByUser(userId);
        //            }

        //            return await new UserManager().GetUserAsync(file?.UserId);
        //        }
        //    }
        //}

        public async Task<bool> AddFileAsync(string fileId, string fileName, string fileHash, string userId, string deviceId, byte[] fileContents) {
            using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand insertCommand = new MySqlCommand("INSERT INTO FILES (FILE_ID, FILE_NAME, FILE_HASH, USER_ID, DEVICE_ID, FILE_CONTENTS) VALUES (@fileId, @fileName, @fileHash, @userId, @deviceId, @fileContents)", connection);
                insertCommand.Parameters.AddWithValue("@fileId", fileId);
                insertCommand.Parameters.AddWithValue("@fileName", fileName);
                insertCommand.Parameters.AddWithValue("@fileHash", fileHash);
                insertCommand.Parameters.AddWithValue("@userId", userId);
                insertCommand.Parameters.AddWithValue("@deviceId", deviceId);
                insertCommand.Parameters.AddWithValue("@fileContents", fileContents);

                int result = await insertCommand.ExecuteNonQueryAsync();

                return result > 0 ? true : false;
            }
        }

        public async Task<bool> RenameFileByIdAsync(string fileId, string userId, string newFileName) {
            using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand updateCommand = new MySqlCommand("UPDATE FILES SET FILE_NAME = @newFileName WHERE FILE_ID = @fileId AND USER_ID = @userId", connection);
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

                MySqlCommand updateCommand = new MySqlCommand("UPDATE FILES SET FILE_NAME = @newFileName WHERE FILE_NAME = @fileName AND USER_ID = @userId", connection);
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

        public async Task<bool> RemoveFileByHashAsync(string fileHash, string userId) {
            if(await FileHashExistsAsync(fileHash)) {
                using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
                    connection.Open();

                    MySqlCommand deleteCommand = new MySqlCommand("DELETE FROM FILES WHERE FILE_HASH = @fileHash", connection);
                    deleteCommand.Parameters.AddWithValue("@fileHash", fileHash);

                    //if((await GetFilesToRemoveAsync(userId)).Count < 1) {
                        await AddFileToRemoveQueueAsync(await GetFileByHashAsync(fileHash));
                    //}

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

        public async Task<bool> FileHashExistsAsync(string fileHash) {
            return await GetFileByHashAsync(fileHash) == null ? false : true;
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
                        byte[] fileContents = (byte[])reader["FILE_CONTENTS"];

                        file.FileId = fileId;
                        file.FileName = fileName;
                        file.FileHash = fileHash;
                        file.UserId = userId;
                        file.DeviceId = deviceId;
                        file.FileContents = fileContents;
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
                        byte[] fileContents = (byte[])reader["FILE_CONTENTS"];

                        file.FileId = fileId;
                        file.FileName = fileName;
                        file.FileHash = fileHash;
                        file.UserId = userId;
                        file.DeviceId = deviceId;
                        file.FileContents = fileContents;
                        //user.Devices = new DeviceManager().GetAllDevicesByUser(userId);
                    }

                    return file;
                }
            }
        }

        public async Task<File> GetUserFileByHashAsync(string _fileHash, string _userId) {
            using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM FILES WHERE FILE_HASH = @fileHash AND USER_ID = @userId", connection);
                selectCommand.Parameters.AddWithValue("@fileHash", _fileHash);
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
                        byte[] fileContents = (byte[])reader["FILE_CONTENTS"];

                        file.FileId = fileId;
                        file.FileName = fileName;
                        file.FileHash = fileHash;
                        file.UserId = userId;
                        file.DeviceId = deviceId;
                        file.FileContents = fileContents;
                    }

                    return file;
                }
            }
        }

        public async Task<bool> AddFileToRemoveQueueAsync(File file) {
            string fileId = file.FileId;
            string fileName = file.FileName;
            string fileHash = file.FileHash;
            string userId = file.UserId;

            using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand insertCommand = new MySqlCommand("INSERT INTO FILES_TO_REMOVE (FILE_ID, FILE_NAME, FILE_HASH, USER_ID) VALUES (@fileId, @fileName, @fileHash, @userId)", connection);
                insertCommand.Parameters.AddWithValue("@fileId", fileId);
                insertCommand.Parameters.AddWithValue("@fileName", fileName);
                insertCommand.Parameters.AddWithValue("@fileHash", fileHash);
                insertCommand.Parameters.AddWithValue("@userId", userId);

                int result = await insertCommand.ExecuteNonQueryAsync();

                return result > 0 ? true : false;
            }
        }

        public async Task<List<File>> GetFilesToRemoveAsync(string _userId) {
            using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM FILES_TO_REMOVE WHERE USER_ID = @userId", connection);
                selectCommand.Parameters.AddWithValue("@userId", _userId);

                using(MySqlDataReader reader = (await selectCommand.ExecuteReaderAsync() as MySqlDataReader)) {
                    List<File> files = new List<File>();

                    while(reader.Read()) {
                        string fileId = reader["FILE_ID"].ToString();
                        string fileName = reader["FILE_NAME"].ToString();
                        string fileHash = reader["FILE_HASH"].ToString();
                        string userId = reader["USER_ID"].ToString();

                        File file = new File();
                        file.FileId = fileId;
                        file.FileName = fileName;
                        file.FileHash = fileHash;
                        file.UserId = userId;
                        //user.Devices = new DeviceManager().GetAllDevicesByUser(userId);

                        files.Add(file);
                    }

                    return files;
                }
            }
        }

        public async Task<bool> ChangeFileUser(string newUserId, string oldUserId) {
            using(MySqlConnection connection = new MySqlConnection(Constants.CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand updateCommand = new MySqlCommand("UPDATE FILES SET USER_ID = @newUserId WHERE USER_ID = @oldUserId", connection);
                updateCommand.Parameters.AddWithValue("@newUserId", newUserId);
                updateCommand.Parameters.AddWithValue("@oldUserId", oldUserId);

                int result = await updateCommand.ExecuteNonQueryAsync();

                return result > 0 ? true : false;
            }
        }
    }
}
