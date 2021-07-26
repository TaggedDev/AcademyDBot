using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace Template.Services
{
    /// <summary>
    /// DatabaseHandler is a class to operate with database. 
    /// </summary>
    [Summary("DatabaseHandler is a class to operate with database.")]
    public static class DatabaseHandler
    {
        // Declaring fields, containing name of the server and the database,
        // authorization data.
        // <enter here authorization data>
        private static readonly string _serverLink = "";
        private static readonly string _dbName = "";
        private static readonly string _dbUserName = "";
        private static readonly string _dbUserPassword = "";

        // Database connection variable.
        private static SqlConnection connection = null;


        /// <summary>
        /// Static constructor. Getting DBMS connection data.
        /// </summary>
        static DatabaseHandler()
        {
            // Trying to get DBMS connection data.
            try
            {
                using (StreamReader r = new StreamReader("db_credentials.json"))
                {
                    // Deserialize json into dynamic variable.
                    string json = r.ReadToEnd();
                    dynamic jsonList = JsonConvert.DeserializeObject(json);

                    // Getting DBMS connection data.
                    _serverLink = jsonList._serverLink;
                    _dbName = jsonList._dbName;
                    _dbUserName = jsonList._dbUserName;
                    _dbUserPassword = jsonList._dbUserPassword;
                }
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("Database credentials' file not found");
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// DBMS connection method. By default, it is called from "main.cs".
        /// Returns true, if connection is successful.
        /// </summary>
        [Summary("DBMS connection method.")]
        public static bool ConnectDatabase()
        {
            // Create a connection string to Azure database server.
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
            {
                DataSource = _serverLink,  // link to server
                InitialCatalog = _dbName,  // database name
                UserID = _dbUserName,      // user name
                Password = _dbUserPassword // user password
            };

            // Trying to open connection with database.
            try
            {
                connection = new SqlConnection(builder.ConnectionString);
                connection.Open();
                return true;
            }
            catch (SqlException e)
            {
                Console.WriteLine("DB connection failed");
                Console.WriteLine(e.ToString());
                return false;
            }
        }
    }
}