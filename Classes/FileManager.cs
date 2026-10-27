using oop_lab_1_cokcell.Classes.Cells;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace oop_lab_1_cokcell.Classes.Files
{
    internal static class FileManager
    {
        public static void SaveToFile(Table table, string filePath)
        {
            var data = new TableSaveData
            {
                Columns = table.Columns,
                Rows = table.Rows
            };

            var it = table.GetEnummeratorCells;
            while (it.MoveNext()) {
                var cell = it.Current;
                if (!string.IsNullOrEmpty(cell.Value.Expression))
                    data.Cells[cell.Key.ToString()] = cell.Value.Expression;
            }

            //https://learn.microsoft.com/en-us/dotnet/api/system.text.json.jsonserializeroptions?view=net-11.0-pp
            //https://learn.microsoft.com/en-us/dotnet/api/system.text.json.jsonserializeroptions.writeindented?view=net-11.0-pp#system-text-json-jsonserializeroptions-writeindented
            var options = new JsonSerializerOptions { WriteIndented = true };
            //https://learn.microsoft.com/en-us/dotnet/api/system.text.json.jsonserializer?view=net-11.0-pp
            string jsonString = JsonSerializer.Serialize(data, options);

            File.WriteAllText(filePath, jsonString);
        }

        public static Table LoadFromFile(string filePath)
        {
            string jsonString = File.ReadAllText(filePath);
            var data = JsonSerializer.Deserialize<TableSaveData>(jsonString);

            if (data == null)
                //throw new Exception("Cannot open file to load from");
                throw new Exception("Не вдалося прочитати файл збереження");

            Table newTable = new Table(data.Columns, data.Rows);

            foreach (var i in data.Cells)
            {
                CellId id = new CellId(i.Key);
                TableCell cell = newTable.PullCell(id)!;
                cell.SetExpression(i.Value, newTable);
            }

            return newTable;
        }
    }
}
