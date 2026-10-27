using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using oop_lab_1_cokcell.Classes.Cells;

namespace oop_lab_1_cokcell.Classes.Files
{
    internal static class GoogleDriveManager
    {
        static string[] Scopes = { DriveService.Scope.DriveFile };
        static string ApplicationName = "CokCell Spreadsheet";

        private static DriveService GetService()
        {
            UserCredential credential;

            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".credentials/cokcell-drive-dotnet.json");

                credential = Task.Run(() => GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true))).Result;
            }

            return new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }

        internal static void SaveTable(Table table, string targetFileName)
        {
            var service = GetService();
            string fileId = null;

            // Searching for real file ID by name
            var listRequest = service.Files.List();
            listRequest.Q = $"name = '{targetFileName}' and trashed = false";
            listRequest.Spaces = "drive";
            var files = listRequest.Execute().Files;

            if (files != null && files.Count > 0)
            {
                fileId = files[0].Id; // Found existing file
            }

            // Packaging table
            var data = new TableSaveData { Columns = table.Columns, Rows = table.Rows };
            var it = table.GetEnummeratorCells;
            while (it.MoveNext())
            {
                if (!string.IsNullOrEmpty(it.Current.Value.Expression))
                    data.Cells[it.Current.Key.ToString()] = it.Current.Value.Expression;
            }

            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            byte[] byteArray = Encoding.UTF8.GetBytes(json);

            // Sending to Google Drive
            using (var stream = new MemoryStream(byteArray))
            {
                if (string.IsNullOrEmpty(fileId))
                {
                    // Creating new
                    var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                    {
                        Name = targetFileName,
                        MimeType = "application/json"
                    };

                    var request = service.Files.Create(fileMetadata, stream, "application/json");
                    request.Fields = "id";
                    request.Upload();
                }
                else
                {
                    // Update existing
                    var updateMetadata = new Google.Apis.Drive.v3.Data.File();
                    var request = service.Files.Update(updateMetadata, fileId, stream, "application/json");
                    request.Upload();
                }
            }
        }

        internal static Table LoadTable(string targetFileName)
        {
            var service = GetService();
            string fileId = null;

            // Searching for file by name
            var listRequest = service.Files.List();
            listRequest.Q = $"name = '{targetFileName}' and trashed = false";
            listRequest.Spaces = "drive";
            var files = listRequest.Execute().Files;

            if (files != null && files.Count > 0)
            {
                fileId = files[0].Id;
            }
            else
            {
                throw new Exception($"Файл з іменем '{targetFileName}' не знайдено на Google Drive");
            }

            // Downloading the file by it's read ID
            using (var stream = new MemoryStream())
            {
                var request = service.Files.Get(fileId);
                request.Download(stream);

                string json = Encoding.UTF8.GetString(stream.ToArray());

                // Empty file check
                if (string.IsNullOrWhiteSpace(json))
                    throw new Exception("Файл на Google Drive порожній або пошкоджений");

                var data = JsonSerializer.Deserialize<TableSaveData>(json);

                if (data == null) throw new Exception("Помилка десеріалізації з Google Drive");

                Table newTable = new Table(data.Columns, data.Rows);

                foreach (var kvp in data.Cells)
                    newTable.CreateCell(new CellId(kvp.Key));

                foreach (var kvp in data.Cells)
                {
                    var cell = newTable.FindCell(new CellId(kvp.Key));
                    cell.SetExpression(kvp.Value, newTable);
                }

                return newTable;
            }
        }
    }
}