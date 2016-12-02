using Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatabaseManager
{
    /// <summary>
    /// Manages the files in the database
    /// </summary>
    public class FileManager : BaseDatabaseManager {
        /// <summary>
        /// Gets all the files in the database asynchronously
        /// </summary>
        /// <returns>List of all files</returns>
        public async Task<List<File>> GetAllFilesAsync() {
            using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM FILES", connection);

                using(MySqlDataReader reader = (await selectCommand.ExecuteReaderAsync() as MySqlDataReader)) {
                    return GetFiles(reader);
                }
            }
        }

        /// <summary>
        /// Gets all the files on a device
        /// </summary>
        /// <param name="_deviceId">The device ID</param>
        /// <returns>All the files on a device with the matching device ID</returns>
        public async Task<List<File>> GetAllFilesWithDeviceAsync(string _deviceId) {
            using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM FILES WHERE DEVICE_ID = @deviceId", connection);
                selectCommand.Parameters.AddWithValue("@deviceId", _deviceId);

                using(MySqlDataReader reader = (await selectCommand.ExecuteReaderAsync() as MySqlDataReader)) {
                    return GetFiles(reader);
                }
            }
        }
        
        /// <summary>
        /// Gets all the files owned by a user
        /// </summary>
        /// <param name="_userId">The user ID</param>
        /// <returns>All the fiels owned by the user with the matching user ID</returns>
        public async Task<List<File>> GetAllFilesWithUserAsync(string _userId) {
            using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
                connection.Open();

                string[] fields = { "*" };

                Dictionary<string, KeyValuePair<string, string>> paramsAndValues = new Dictionary<string, KeyValuePair<string, string>>();
                paramsAndValues.Add("USER_ID", new KeyValuePair<string, string>("@userId", _userId));

                MySqlCommand selectCommand = SelectCommand(connection, "FILES", fields, paramsAndValues);

                using(MySqlDataReader reader = (await selectCommand.ExecuteReaderAsync() as MySqlDataReader)) {
                    return GetFiles(reader);
                }
            }
        }

        /// <summary>
        /// Gets a file by its ID
        /// </summary>
        /// <param name="_fileId">The file ID</param>
        /// <returns>File with the matching file ID</returns>
        public async Task<File> GetFileByIdAsync(string _fileId) {
            using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM FILES WHERE FILE_ID = @fileId", connection);
                selectCommand.Parameters.AddWithValue("@fileId", _fileId);

                using(MySqlDataReader reader = (await selectCommand.ExecuteReaderAsync() as MySqlDataReader)) {
                    return GetFile(reader);
                }
            }
        }
        
        /// <summary>
        /// Gets a file by its name
        /// </summary>
        /// <param name="_fileName">The file's name</param>
        /// <returns>File with the matching name</returns>
        public async Task<File> GetFileByNameAsync(string _fileName) {
            using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM FILES WHERE FILE_NAME = @fileName", connection);
                selectCommand.Parameters.AddWithValue("@fileName", _fileName);

                using(MySqlDataReader reader = (await selectCommand.ExecuteReaderAsync() as MySqlDataReader)) {
                    return GetFile(reader);
                }
            }
        }

        /// <summary>
        /// Gets a file by its torrent's hash
        /// </summary>
        /// <param name="_fileHash">The file's torrent's hash</param>
        /// <returns>File with the matching hash</returns>
        public async Task<File> GetFileByHashAsync(string _fileHash) {
            using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM FILES WHERE FILE_HASH = @fileHash", connection);
                selectCommand.Parameters.AddWithValue("@fileHash", _fileHash);

                using(MySqlDataReader reader = (await selectCommand.ExecuteReaderAsync() as MySqlDataReader)) {
                    return GetFile(reader);
                }
            }
        }

        /// <summary>
        /// Gets the name of a file by its ID
        /// </summary>
        /// <param name="_fileId">The file ID</param>
        /// <returns>Name of the file with the matching ID</returns>
        public async Task<string> GetFileNameAsync(string _fileId) {
            using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM FILES WHERE FILE_ID = @fileId", connection);
                selectCommand.Parameters.AddWithValue("@fileId", _fileId);

                using(MySqlDataReader reader = (await selectCommand.ExecuteReaderAsync() as MySqlDataReader)) {
                    return GetFile(reader)?.FileName;
                }
            }
        }

        /// <summary>
        /// Adds a file to the database
        /// </summary>
        /// <param name="fileId">File ID</param>
        /// <param name="fileName">File name</param>
        /// <param name="fileHash">Hash of the file's torrent</param>
        /// <param name="fileMD5">MD5 hash of the file</param>
        /// <param name="fileSize">Size of the file (in bytes)</param>
        /// <param name="added">Date when the file was added (created)</param>
        /// <param name="lastModified">Date when the file was last modified</param>
        /// <param name="userId">User ID of the owner of the file</param>
        /// <param name="deviceId">Device ID of the device the file was added from</param>
        /// <param name="fileContents">Contents of the file's torrent file</param>
        /// <param name="fileVersion">Version of the file</param>
        /// <returns>Boolean value indicating whether the file was added or not</returns>
        public async Task<bool> AddFileAsync(string fileId, string fileName, string fileHash, string fileMD5, long fileSize, DateTime added, DateTime lastModified, string userId, string deviceId, byte[] fileContents, int fileVersion) {
            using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand insertCommand = new MySqlCommand("INSERT INTO FILES (FILE_ID, FILE_NAME, FILE_HASH, FILE_MD5, FILE_SIZE, ADDED, LAST_MODIFIED, USER_ID, DEVICE_ID, FILE_CONTENTS, FILE_VERSION) VALUES (@fileId, @fileName, @fileHash, @fileMD5, @fileSize, @added, @lastModified, @userId, @deviceId, @fileContents, @fileVersion)", connection);
                insertCommand.Parameters.AddWithValue("@fileId", fileId);
                insertCommand.Parameters.AddWithValue("@fileName", fileName);
                insertCommand.Parameters.AddWithValue("@fileHash", fileHash);
                insertCommand.Parameters.AddWithValue("@fileMD5", fileMD5);
                insertCommand.Parameters.AddWithValue("@fileSize", fileSize);
                insertCommand.Parameters.AddWithValue("@added", added);
                insertCommand.Parameters.AddWithValue("@lastModified", lastModified);
                insertCommand.Parameters.AddWithValue("@userId", userId);
                insertCommand.Parameters.AddWithValue("@deviceId", deviceId);
                insertCommand.Parameters.AddWithValue("@fileContents", fileContents);
                insertCommand.Parameters.AddWithValue("@fileVersion", fileVersion);

                int result = await insertCommand.ExecuteNonQueryAsync();

                return result > 0 ? true : false;
            }
        }

        /// <summary>
        /// Renames a file using its ID
        /// </summary>
        /// <param name="fileId">File ID</param>
        /// <param name="userId">User ID of the owner of the file</param>
        /// <param name="newFileName">The new name of the file</param>
        /// <returns>True if rename was successful; false if it wasn't</returns>
        public async Task<bool> RenameFileByIdAsync(string fileId, string userId, string newFileName) {
            using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand updateCommand = new MySqlCommand("UPDATE FILES SET FILE_NAME = @newFileName WHERE FILE_ID = @fileId AND USER_ID = @userId", connection);
                updateCommand.Parameters.AddWithValue("@fileId", fileId);
                updateCommand.Parameters.AddWithValue("@userId", userId);
                updateCommand.Parameters.AddWithValue("@newFileName", newFileName);

                int result = await updateCommand.ExecuteNonQueryAsync();

                return result > 0 ? true : false;
            }
        }

        /// <summary>
        /// Renames a file using its current name
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="userId">User ID of the owner of the file</param>
        /// <param name="newFileName">The new name of the file</param>
        /// <returns>True if successful; false otherwise</returns>
        public async Task<bool> RenameFileByNameAsync(string fileName, string userId, string newFileName) {
            using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand updateCommand = new MySqlCommand("UPDATE FILES SET FILE_NAME = @newFileName WHERE FILE_NAME = @fileName AND USER_ID = @userId", connection);
                updateCommand.Parameters.AddWithValue("@fileName", fileName);
                updateCommand.Parameters.AddWithValue("@userId", userId);
                updateCommand.Parameters.AddWithValue("@newFileName", newFileName);

                int result = await updateCommand.ExecuteNonQueryAsync();

                return result > 0 ? true : false;
            }
        }

        /// <summary>
        /// Removes a file using its ID
        /// </summary>
        /// <param name="fileId">File ID</param>
        /// <param name="userId">User ID of the owner</param>
        /// <returns>True if successful; false otherwise</returns>
        public async Task<bool> RemoveFileByIdAsync(string fileId, string userId) {
            if(await FileIdExistsAsync(fileId, userId)) {
                using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
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

        /// <summary>
        /// Removes a file using its name
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="userId">User ID of the owner</param>
        /// <returns>True if successful; false otherwise</returns>
        public async Task<bool> RemoveFileByNameAsync(string fileName, string userId) {
            if(await FileNameExistsAsync(fileName, userId)) {
                using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
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

        /// <summary>
        /// Remove a file using its hash
        /// </summary>
        /// <param name="fileHash">Hash of the torrent</param>
        /// <param name="userId">User ID of the owner</param>
        /// <returns>True if successful; false otherwise</returns>
        public async Task<bool> RemoveFileByHashAsync(string fileHash, string userId) {
            if(await FileHashExistsAsync(fileHash)) {
                using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
                    connection.Open();

                    MySqlCommand deleteCommand = new MySqlCommand("DELETE FROM FILES WHERE FILE_HASH = @fileHash", connection);
                    deleteCommand.Parameters.AddWithValue("@fileHash", fileHash);

                    File file = await GetFileByHashAsync(fileHash);

                    if((await GetFilesToRemoveAsync(userId)).Where(f => f.FileHash == fileHash).Count() < 1) {
                        await AddFileToRemoveQueueAsync(file);
                    }

                    int result = await deleteCommand.ExecuteNonQueryAsync();

                    return result > 0 ? true : false;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if file exists using its ID
        /// </summary>
        /// <param name="fileId">File ID</param>
        /// <param name="userId">User ID of the owner</param>
        /// <returns>True if it exists; false if it doesn't</returns>
        public async Task<bool> FileIdExistsAsync(string fileId, string userId) {
            return await GetUserFileByIdAsync(fileId, userId) == null ? false : true;
        }

        /// <summary>
        /// Checks if file exists using its name
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="userId">User ID of the owner</param>
        /// <returns>True if it exists; false if it does not</returns>
        public async Task<bool> FileNameExistsAsync(string fileName, string userId) {
            return await GetUserFileByNameAsync(fileName, userId) == null ? false : true;
        }

        /// <summary>
        /// Checks if file exists using its hash
        /// </summary>
        /// <param name="fileHash">Hash of the file's torrent</param>
        /// <returns>True if it exists; false if it doesn't</returns>
        public async Task<bool> FileHashExistsAsync(string fileHash) {
            return await GetFileByHashAsync(fileHash) == null ? false : true;
        }

        /// <summary>
        /// Get a user's file by its name
        /// </summary>
        /// <param name="_fileName">Name of the file</param>
        /// <param name="_userId">User ID of the user</param>
        /// <returns>Matching file</returns>
        public async Task<File> GetUserFileByNameAsync(string _fileName, string _userId) {
            using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM FILES WHERE FILE_NAME = @fileName AND USER_ID = @userId", connection);
                selectCommand.Parameters.AddWithValue("@fileName", _fileName);
                selectCommand.Parameters.AddWithValue("@userId", _userId);

                using(MySqlDataReader reader = (await selectCommand.ExecuteReaderAsync() as MySqlDataReader)) {
                    return GetFile(reader);
                }
            }
        }

        /// <summary>
        /// Get a user's file by its ID
        /// </summary>
        /// <param name="_fileId">File ID</param>
        /// <param name="_userId">User ID</param>
        /// <returns>Matching file</returns>
        public async Task<File> GetUserFileByIdAsync(string _fileId, string _userId) {
            using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM FILES WHERE FILE_ID = @fileId AND USER_ID = @userId", connection);
                selectCommand.Parameters.AddWithValue("@fileId", _fileId);
                selectCommand.Parameters.AddWithValue("@userId", _userId);

                using(MySqlDataReader reader = (await selectCommand.ExecuteReaderAsync() as MySqlDataReader)) {
                    return GetFile(reader);
                }
            }
        }

        /// <summary>
        /// Get a user's file by its hash
        /// </summary>
        /// <param name="_fileHash">Hash of the file's torrent</param>
        /// <param name="_userId">User ID</param>
        /// <returns>Matching file</returns>
        public async Task<File> GetUserFileByHashAsync(string _fileHash, string _userId) {
            using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM FILES WHERE FILE_HASH = @fileHash AND USER_ID = @userId", connection);
                selectCommand.Parameters.AddWithValue("@fileHash", _fileHash);
                selectCommand.Parameters.AddWithValue("@userId", _userId);

                using(MySqlDataReader reader = (await selectCommand.ExecuteReaderAsync() as MySqlDataReader)) {
                    return GetFile(reader);
                }
            }
        }

        /// <summary>
        /// Adds a file to the remove queue
        /// </summary>
        /// <param name="file">BitSynk's custom file model</param>
        /// <returns>True if file is added successfully; false if not</returns>
        public async Task<bool> AddFileToRemoveQueueAsync(File file) {
            string fileId = file.FileId;
            string fileName = file.FileName;
            string fileHash = file.FileHash;
            int fileVersion = file.FileVersion;
            string userId = file.UserId;

            using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand insertCommand = new MySqlCommand("INSERT INTO FILES_TO_REMOVE (FILE_ID, FILE_NAME, FILE_HASH, FILE_VERSION, USER_ID) VALUES (@fileId, @fileName, @fileHash, @fileVersion, @userId)", connection);
                insertCommand.Parameters.AddWithValue("@fileId", fileId);
                insertCommand.Parameters.AddWithValue("@fileName", fileName);
                insertCommand.Parameters.AddWithValue("@fileHash", fileHash);
                insertCommand.Parameters.AddWithValue("@fileVersion", fileVersion);
                insertCommand.Parameters.AddWithValue("@userId", userId);

                int result = await insertCommand.ExecuteNonQueryAsync();

                return result > 0 ? true : false;
            }
        }

        /// <summary>
        /// Gets a list of files to remove from the remove queue, owned by a user
        /// </summary>
        /// <param name="_userId">User ID</param>
        /// <returns>List of all files owned by a user that have to be deleted</returns>
        public async Task<List<File>> GetFilesToRemoveAsync(string _userId) {
            using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM FILES_TO_REMOVE WHERE USER_ID = @userId", connection);
                selectCommand.Parameters.AddWithValue("@userId", _userId);

                using(MySqlDataReader reader = (await selectCommand.ExecuteReaderAsync() as MySqlDataReader)) {
                    return GetFiles(reader);
                }
            }
        }

        /// <summary>
        /// Changes the owner of a file
        /// </summary>
        /// <param name="newUserId">New user's ID</param>
        /// <param name="oldUserId">Previous user's ID</param>
        /// <returns>True if the change is successful, false if not</returns>
        public async Task<bool> ChangeFileUser(string newUserId, string oldUserId) {
            using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand updateCommand = new MySqlCommand("UPDATE FILES SET USER_ID = @newUserId WHERE USER_ID = @oldUserId", connection);
                updateCommand.Parameters.AddWithValue("@newUserId", newUserId);
                updateCommand.Parameters.AddWithValue("@oldUserId", oldUserId);

                int result = await updateCommand.ExecuteNonQueryAsync();

                return result > 0 ? true : false;
            }
        }

        /// <summary>
        /// Checks if a file is removed
        /// </summary>
        /// <param name="_userId">User ID of the owner</param>
        /// <param name="_fileName">Name of the file</param>
        /// <returns>True if the file is removed; false if not</returns>
        public async Task<bool> IsRemovedFile(string _userId, string _fileName) {
            using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM FILES_TO_REMOVE WHERE USER_ID = @userId AND FILE_NAME = @fileName", connection);
                selectCommand.Parameters.AddWithValue("@userId", _userId);
                selectCommand.Parameters.AddWithValue("@fileName", _fileName);

                using(MySqlDataReader reader = (await selectCommand.ExecuteReaderAsync() as MySqlDataReader)) {
                    return GetFiles(reader).Count > 0;
                }
            }
        }

        /// <summary>
        /// Removes a file from the file removal queue using its name
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="userId">User ID of the file's owner</param>
        /// <returns>True if successful; false if not</returns>
        public async Task<bool> RemoveFileFromRemoveQueueByNameAsync(string fileName, string userId) {
            if(await FileNameExistsInRemoveQueueAsync(fileName, userId)) {
                using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
                    connection.Open();

                    MySqlCommand deleteCommand = new MySqlCommand("DELETE FROM FILES_TO_REMOVE WHERE FILE_NAME = @fileName AND USER_ID = @userId", connection);
                    deleteCommand.Parameters.AddWithValue("@fileName", fileName);
                    deleteCommand.Parameters.AddWithValue("@userId", userId);

                    int result = await deleteCommand.ExecuteNonQueryAsync();

                    return result > 0 ? true : false;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a file with a matching name, owned by a user, exists in the removal queue
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="userId">User ID</param>
        /// <returns>True if file exists; false otherwise</returns>
        private async Task<bool> FileNameExistsInRemoveQueueAsync(string fileName, string userId) {
            return await GetUserFileFromRemoveQueueByNameAsync(fileName, userId) == null ? false : true;
        }

        /// <summary>
        /// Gets the file owned by a user from the remove queue by its name
        /// </summary>
        /// <param name="_fileName">Name of the file</param>
        /// <param name="_userId">User id of the file's owner</param>
        /// <returns>File from the remove queue with the matching file name and user ID</returns>
        public async Task<File> GetUserFileFromRemoveQueueByNameAsync(string _fileName, string _userId) {
            using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM FILES_TO_REMOVE WHERE FILE_NAME = @fileName AND USER_ID = @userId", connection);
                selectCommand.Parameters.AddWithValue("@fileName", _fileName);
                selectCommand.Parameters.AddWithValue("@userId", _userId);

                using(MySqlDataReader reader = (await selectCommand.ExecuteReaderAsync() as MySqlDataReader)) {
                    return GetFile(reader);
                }
            }
        }

        /// <summary>
        /// Updates the file using its file ID; adds the file if it doesn't exist
        /// </summary>
        /// <param name="fileId">The file ID</param>
        /// <param name="fileName">Name of the file</param>
        /// <param name="fileHash">Hash of the file's torrent</param>
        /// <param name="fileMD5">MD5 hash of the file</param>
        /// <param name="fileSize">Size of the file (in bytes)</param>
        /// <param name="lastModified">Last modified date/time of the file</param>
        /// <param name="userId">User ID of the user</param>
        /// <param name="deviceId">Device ID</param>
        /// <param name="fileContents">Contents of the torrent file</param>
        /// <param name="fileVersion">Version of the file</param>
        /// <param name="newFileHash">The new hash of the torrent file</param>
        /// <param name="newFileMD5">The new MD5 hash of the file</param>
        /// <returns>True if successful; false otherwise</returns>
        public async Task<bool> UpdateFile(string fileId, string fileName, string fileHash, string fileMD5, long fileSize, DateTime lastModified, string userId, string deviceId, byte[] fileContents, int fileVersion, string newFileHash, string newFileMD5) {
            using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING)) {
                connection.Open();

                MySqlCommand updateCommand = new MySqlCommand("INSERT INTO FILES (FILE_ID, FILE_HASH, FILE_MD5, FILE_SIZE, LAST_MODIFIED, FILE_CONTENTS, FILE_VERSION) VALUES (@fileId, @newFileHash, @fileMD5, @fileSize, @lastModified, @fileContents, @fileVersion) ON DUPLICATE KEY UPDATE FILE_HASH = @newFileHash, FILE_MD5 = @newFileMD5, FILE_SIZE = @fileSize, LAST_MODIFIED = @lastModified, FILE_CONTENTS = @fileContents, FILE_VERSION = @fileVersion", connection);
                updateCommand.Parameters.AddWithValue("@fileId", fileId);
                updateCommand.Parameters.AddWithValue("@fileHash", fileHash);
                updateCommand.Parameters.AddWithValue("@fileMD5", fileMD5);
                updateCommand.Parameters.AddWithValue("@userId", userId);
                updateCommand.Parameters.AddWithValue("@newFileHash", newFileHash);
                updateCommand.Parameters.AddWithValue("@newFileMD5", newFileMD5);
                updateCommand.Parameters.AddWithValue("@fileSize", fileSize);
                updateCommand.Parameters.AddWithValue("@fileContents", fileContents);
                updateCommand.Parameters.AddWithValue("@fileVersion", fileVersion);
                updateCommand.Parameters.AddWithValue("@lastModified", lastModified);

                int result = await updateCommand.ExecuteNonQueryAsync();

                return result > 0 ? true : false;
            }
        }

        /// <summary>
        /// Gets a the file from the query's result
        /// </summary>
        /// <param name="reader">The reader that contains the result of the query</param>
        /// <returns>The file that matches the given conditions</returns>
        private File GetFile(MySqlDataReader reader) {
            File file = null;

            while(reader.Read()) {
                file = new File();

                string fileId = reader["FILE_ID"].ToString();
                string fileName = reader["FILE_NAME"].ToString();
                string fileHash = reader["FILE_HASH"].ToString();
                string userId = reader["USER_ID"].ToString();
                string deviceId = "";
                string fileMD5 = "";

                int fileVersion = int.Parse(reader["FILE_VERSION"].ToString());

                byte[] fileContents = null;

                long fileSize = 0;

                DateTime added = DateTime.MinValue;
                DateTime lastModified = DateTime.MinValue;
                try {
                    fileSize = long.Parse(reader["FILE_SIZE"].ToString());
                    added = DateTime.Parse(reader["ADDED"].ToString());
                    lastModified = DateTime.Parse(reader["LAST_MODIFIED"].ToString());
                    deviceId = reader["DEVICE_ID"].ToString();
                    fileContents = (byte[])reader["FILE_CONTENTS"];
                    fileMD5 = reader["FILE_MD5"].ToString();
                } catch(Exception ex) {

                }

                file.FileId = fileId;
                file.FileName = fileName;
                file.FileHash = fileHash;
                file.FileMD5 = fileMD5;
                file.FileSize = fileSize;
                file.Added = added;
                file.LastModified = lastModified;
                file.UserId = userId;
                file.DeviceId = deviceId;
                file.FileContents = fileContents;
                file.FileVersion = fileVersion;
                //user.Devices = new DeviceManager().GetAllDevicesByUser(userId);
            }

            return file;
        }

        /// <summary>
        /// Gets a list of files from the query's result
        /// </summary>
        /// <param name="reader">The reader that contains the result of the query</param>
        /// <returns>List of files matching the given conditions</returns>
        private List<File> GetFiles(MySqlDataReader reader) {
            List<File> files = new List<File>();

            while(reader.Read()) {
                string fileId = reader["FILE_ID"].ToString();
                string fileName = reader["FILE_NAME"].ToString();
                string fileHash = reader["FILE_HASH"].ToString();
                string userId = reader["USER_ID"].ToString();
                string deviceId = "";
                string fileMD5 = "";

                int fileVersion = int.Parse(reader["FILE_VERSION"].ToString());

                byte[] fileContents = null;

                long fileSize = 0;

                DateTime added = DateTime.MinValue;
                DateTime lastModified = DateTime.MinValue;
                try {
                    fileSize = long.Parse(reader["FILE_SIZE"].ToString());
                    added = DateTime.Parse(reader["ADDED"].ToString());
                    lastModified = DateTime.Parse(reader["LAST_MODIFIED"].ToString());
                    deviceId = reader["DEVICE_ID"].ToString();
                    fileContents = (byte[])reader["FILE_CONTENTS"];
                    fileMD5 = reader["FILE_MD5"].ToString();
                } catch(Exception ex) {

                }

                File file = new File();
                file.FileId = fileId;
                file.FileName = fileName;
                file.FileHash = fileHash;
                file.FileMD5 = fileMD5;
                file.FileSize = fileSize;
                file.Added = added;
                file.LastModified = lastModified;
                file.UserId = userId;
                file.DeviceId = deviceId;
                file.FileContents = fileContents;
                file.FileVersion = fileVersion;
                //user.Devices = new DeviceManager().GetAllDevicesByUser(userId);

                files.Add(file);
            }

            return files;
        }
    }
}
