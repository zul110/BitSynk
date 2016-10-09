using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseManager {
    public class BaseDatabaseManager {
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

        private string GetFields(string[] fields) {
            string fieldsString = fields[0];
            for(int i = 1; i < fields.Length; i++) {
                fieldsString += ", " + fields[i];
            }

            return fieldsString;
        }

        private string GetConditions(Dictionary<string, KeyValuePair<string, string>> paramsAndValues) {
            string conditionsString = paramsAndValues != null ? paramsAndValues.ElementAt(0).Key + " = " + paramsAndValues.ElementAt(0).Value.Key : "";
            for(int i = 1; i < paramsAndValues.Count; i++) {
                conditionsString += " AND " + paramsAndValues.ElementAt(i).Key + " = " + paramsAndValues.ElementAt(i).Value.Key;
            }

            return conditionsString;
        }

        private void GetCommandParameters(ref MySqlCommand selectCommand, Dictionary<string, KeyValuePair<string, string>> paramsAndValues) {
            for(int i = 0; i < paramsAndValues.Count; i++) {
                selectCommand.Parameters.AddWithValue(paramsAndValues.ElementAt(i).Value.Key, paramsAndValues.ElementAt(i).Value.Value);
            }
        }
    }
}
