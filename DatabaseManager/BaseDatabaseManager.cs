using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseManager {
    /// <summary>
    /// Base Database manager class
    /// Includes common database functionality that will be used by the derived classes
    /// </summary>
    public class BaseDatabaseManager {
        // Constant connection string
        protected const string CONNECTION_STRING = "Server=sql6.freesqldatabase.com; Port=3306 ; Uid=sql6129394; Pwd=XZvazWNKyq; Database=sql6129394; Convert Zero Datetime=True";

        /// <summary>
        /// Select command
        /// </summary>
        /// <param name="connection">MySqlConnection</param>
        /// <param name="table">The table name to query</param>
        /// <param name="fields">Array of fields to get</param>
        /// <param name="paramsAndValues">Key-value pair for the WHERE condition</param>
        /// <returns>Constructed select command</returns>
        protected MySqlCommand SelectCommand(MySqlConnection connection, string table, string[] fields, Dictionary<string, KeyValuePair<string, string>> paramsAndValues = null) {
            StringBuilder command = new StringBuilder("SELECT ");

            string fieldsString = GetFields(fields);
            string conditionsString = GetConditions(paramsAndValues);

            command.Append(fieldsString);
            command.Append(" FROM " + table);
            command.Append(" WHERE ");
            command.Append(conditionsString);

            MySqlCommand selectCommand = new MySqlCommand(command.ToString(), connection);
            GetCommandParameters(ref selectCommand, paramsAndValues);

            return selectCommand;
        }

        /// <summary>
        /// Constructs the fields in the proper format for the query
        /// </summary>
        /// <param name="fields">Array of fields</param>
        /// <returns>Formatted string of fields</returns>
        private string GetFields(string[] fields) {
            string fieldsString = fields[0];
            for(int i = 1; i < fields.Length; i++) {
                fieldsString += ", " + fields[i];
            }

            return fieldsString;
        }

        /// <summary>
        /// Gets formatted conditions for the query
        /// </summary>
        /// <param name="paramsAndValues">Key-value pairs of conditions</param>
        /// <returns>Formatted string of conditions</returns>
        private string GetConditions(Dictionary<string, KeyValuePair<string, string>> paramsAndValues) {
            string conditionsString = paramsAndValues != null ? paramsAndValues.ElementAt(0).Key + " = " + paramsAndValues.ElementAt(0).Value.Key : "";
            for(int i = 1; i < paramsAndValues.Count; i++) {
                conditionsString += " AND " + paramsAndValues.ElementAt(i).Key + " = " + paramsAndValues.ElementAt(i).Value.Key;
            }

            return conditionsString;
        }

        /// <summary>
        /// Appends the SELECT command with the conditions
        /// </summary>
        /// <param name="selectCommand">The initial SELECT command</param>
        /// <param name="paramsAndValues">Formatted conditions</param>
        private void GetCommandParameters(ref MySqlCommand selectCommand, Dictionary<string, KeyValuePair<string, string>> paramsAndValues) {
            for(int i = 0; i < paramsAndValues.Count; i++) {
                selectCommand.Parameters.AddWithValue(paramsAndValues.ElementAt(i).Value.Key, paramsAndValues.ElementAt(i).Value.Value);
            }
        }
    }
}
