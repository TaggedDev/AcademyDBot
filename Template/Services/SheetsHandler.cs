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

        static public void ReadEntries()
        {
            var range = $"{sheet}!A:D";
            SpreadsheetsResource.ValuesResource.GetRequest request = service.Spreadsheets.Values.Get(SpreadsheetId, range);

            var response = request.Execute();
            IList<IList<object>> values = response.Values;
            if (values != null && values.Count > 0)
            {
                foreach (var row in values)
                {
                    // Print columns A to F, which correspond to indices 0 and 4.
                    Console.WriteLine("{0} | {1} | {2} | {3} | {4} | {5}", row[0], row[1], row[2], row[3], row[4], row[5]);
                }
            }
            else
            {
                Console.WriteLine("No data found.");
            }
        }

        public static void AddRow(ulong discord_id, DateTime startTime, DateTime endTime)
        {
            var range = $"{sheet}!A:F";
            var valueRange = new ValueRange();

            var objects = new List<object>() { "Name Surname", $"{discord_id}", $"{startTime}", $"{endTime}" };
            valueRange.Values = new List<IList<object>> { objects };

            var appendRequest = service.Spreadsheets.Values.Append(valueRange, SpreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var appendResponse = appendRequest.Execute();
        }

        public static List<Student> ReadRow()
        {
            var range = $"{sheet}!A:D";
            SpreadsheetsResource.ValuesResource.GetRequest request = service.Spreadsheets.Values.Get(SpreadsheetId, range);

            List<Student> students = new List<Student>();
            
            var response = request.Execute();
            IList<IList<object>> values = response.Values;
            
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
                    catch { Console.WriteLine("Error!"); }
            else
                Console.WriteLine("No data found.");
            
            return students;
        }
    }
}
