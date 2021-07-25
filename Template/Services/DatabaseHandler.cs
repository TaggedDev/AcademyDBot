using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("DB connection failed");
                Console.ResetColor();
                Console.WriteLine(e.ToString());
                return false;
            }
        }

    }
}