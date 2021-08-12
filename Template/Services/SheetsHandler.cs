using Discord.Commands;
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
    [Summary("All google sheets logic in this class only")]
    class SheetsHandler
    {
        static readonly string ApplicationName = "Students Interview Time";
        static readonly string SpreadsheetId = "1PkKRYj1E7H2I2GEYBVry-gAJqCXu_xXnltpcfip30bc";
        static readonly string sheet = "interview-sheet";

        static readonly UserCredential _credential;
        static readonly SheetsService service;
        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };

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
        [Summary("Write user information method")]
        public static void AddRow(string userNickname, ulong discord_id, DateTime startTime, DateTime endTime, string flow)
        {
            var range = $"{sheet}!A:F";
            var valueRange = new ValueRange();

            var objects = new List<object>() { "0", $"{userNickname}", $"{discord_id}", $"{startTime}", $"{endTime}", $"{flow}" };
            valueRange.Values = new List<IList<object>> { objects };

            var appendRequest = service.Spreadsheets.Values.Append(valueRange, SpreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            appendRequest.Execute();
        }

        /// <summary>
        /// Read user information and generate list of all possible users
        /// </summary>
        /// <returns>List of all correct students</returns>
        [Summary("Read user information and generate list of all possible users")]
        public static List<Student> ReadRow()
        {
            var range = $"{sheet}!A:I";
            SpreadsheetsResource.ValuesResource.GetRequest request = service.Spreadsheets.Values.Get(SpreadsheetId, range);
            var response = request.Execute();
            IList<IList<object>> values = response.Values;

            List<Student> students = new List<Student>();

            if (values != null && values.Count > 0)
            {
                string status, name, firstName, secondName, channel, teacher1Name, teacher2Name ;
                ulong discordId;
                DateTime startTime, endTime;

                foreach (var row in values)
                {

                    try
                    {
                        status = Convert.ToString(row[0]);
                        if (status.Equals("1"))
                            continue;

                        name = Convert.ToString(row[1]);
                        firstName = name.Split(' ')[0];
                        secondName = name.Split(' ')[1];
                        discordId = Convert.ToUInt64(row[2]);
                        startTime = Convert.ToDateTime(row[3]);
                        endTime = Convert.ToDateTime(row[4]);
                        channel = Convert.ToString(row[6]);
                        teacher1Name = Convert.ToString(row[7]);
                        teacher2Name = Convert.ToString(row[8]);
                        Student student = new Student(firstName, secondName, discordId, startTime, endTime, channel, teacher1Name, teacher2Name);
                        students.Add(student);
                    }
                    catch { }
                }
            }
            else
            {
                Console.WriteLine("No data found.");
            }
            return students;
        }

        /// <summary>
        /// Checks the last interview end time (if exists) 
        /// </summary>
        /// <returns>Returns [2021.09.10 in 16:00] if no data yet, else returns new InterviewStart time</returns>
        [Summary("Checks the last interview end time (if exists)")]
        public static DateTime GetInterviewStart(TimeSpan breakTime)
        {
            
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
                return DateTime.ParseExact(lastRow[3].ToString(), "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture) + breakTime;
            }
        }

        /// <summary>
        /// Marks sent interviews as sent prevent sending them invite second time
        /// </summary>
        public static void MarkSentInterviews(List<Student> students)
        {
            for (int i = 2; i < students.Count + 2; i++)
            {
                string range = $"{sheet}!A{i}";
                var valueRange = new ValueRange();
                var oblist = new List<object>() { "1" };
                valueRange.Values = new List<IList<object>> { oblist };
                SpreadsheetsResource.ValuesResource.UpdateRequest update = service.Spreadsheets.Values.Update(valueRange, SpreadsheetId, range);
                update.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
                update.Execute();
            }
            
        }
    }
}
