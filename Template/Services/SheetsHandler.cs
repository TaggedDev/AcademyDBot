using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Template.Models;

namespace Template.Services
{
    /// <summary>
    /// All google sheets logic in this class only
    /// </summary>
    class SheetsHandler
    {
        static readonly string ApplicationName = "Students Interview Time";
        static readonly string SpreadsheetId = "1PkKRYj1E7H2I2GEYBVry-gAJqCXu_xXnltpcfip30bc";
        static readonly string sheet = "interview-sheet";

        static readonly UserCredential _credential;
        static SheetsService service;
        static string[] Scopes = { SheetsService.Scope.Spreadsheets };

        static SheetsHandler()
        {
            // Read the credentials file from google API
            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                _credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = _credential,
                ApplicationName = ApplicationName,
            });
        }

        /// <summary>
        /// Write user information method
        /// </summary>
        /// <param name="discord_id">Discord user ID</param>
        /// <param name="startTime">Interview start time</param>
        /// <param name="endTime">Interview end time</param>
        public static void AddRow(ulong discord_id, DateTime startTime, DateTime endTime, string flow)
        {
            var range = $"{sheet}!A:F";
            var valueRange = new ValueRange();

            var objects = new List<object>() { "Name Surname", $"{discord_id}", $"{startTime}", $"{endTime}", $"{flow}" };
            valueRange.Values = new List<IList<object>> { objects };

            var appendRequest = service.Spreadsheets.Values.Append(valueRange, SpreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var appendResponse = appendRequest.Execute();
        }

        /// <summary>
        /// Read user information and generate list of all possible users
        /// </summary>
        /// <returns>List of all correct students</returns>
        public static List<Student> ReadRow()
        {
            var range = $"{sheet}!A:D";
            SpreadsheetsResource.ValuesResource.GetRequest request = service.Spreadsheets.Values.Get(SpreadsheetId, range);
            var response = request.Execute();
            IList<IList<object>> values = response.Values;

            List<Student> students = new List<Student>();

            if (values != null && values.Count > 0)
                foreach (var row in values)
                    try
                    {
                        string name = Convert.ToString(row[0]);
                        string firstName = name.Split(' ')[0];
                        string secondName = name.Split(' ')[1];
                        ulong discordId = Convert.ToUInt64(row[1]);
                        DateTime startTime = Convert.ToDateTime(row[2]);
                        DateTime endTime = Convert.ToDateTime(row[3]);
                        Student student = new Student(firstName, secondName, discordId, startTime, endTime);
                        students.Add(student);
                    }
                    catch { }
            else
                Console.WriteLine("No data found.");
            return students;
        }

        /// <summary>
        /// Checks the last interview end time (if exists) 
        /// </summary>
        /// <returns>Returns [2021.09.10 in 16:00] if no data yet, else returns new InterviewStart time</returns>
        public static DateTime GetInterviewStart(TimeSpan breakTime)
        {
            DateTime lastRecord = new DateTime();
            var range = $"{sheet}!A:D";
            SpreadsheetsResource.ValuesResource.GetRequest request = service.Spreadsheets.Values.Get(SpreadsheetId, range);
            var response = request.Execute();
            IList<IList<object>> values = response.Values;

            if (values.Count <= 1)
            {
                return new DateTime(year: 2021, month: 9, day: 10, hour: 16, minute: 0, second: 0);
            }
            else
            {
                var lastRow = values[values.Count - 1];
                lastRecord = DateTime.ParseExact(lastRow[3].ToString(), "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture) + breakTime;
            }
            return lastRecord;
        }
    }
}
