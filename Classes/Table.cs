using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace oop_lab_1_cokcell.Classes.Cells
{
    internal class Table
    {
        private Dictionary<CellId, TableCell> cells = new Dictionary<CellId, TableCell>();

        public Dictionary<CellId, TableCell>.Enumerator GetEnummeratorCells
        {
            get => cells.GetEnumerator();
        }

        public bool IsCellCouldExist(CellId id)
        {
            return (1 <= id.Row && id.Row <= Rows
            && 1 <= id.ColumnInt && id.ColumnInt <= Columns);
        }

        public bool IsCellCouldPossiblyExist(CellId id)
        {
            return (1 <= id.Row && id.Row <= maxrows
            && 1 <= id.ColumnInt && id.ColumnInt <= maxcolumns);
        }

        private void CheckCellExistence(CellId id)
        {
            if (!IsCellCouldExist(id))
                //throw new Exception($"Targeted cell [{CellId.getStringSafe(id)}] goes beyond the defined table area");
                throw new Exception($"Клітинка [{CellId.getStringSafe(id)}] не лежить у теперішніх розмірах таблиці");
        }

        private void CheckCellDefinedArea(CellId id)
        {
            if (!IsCellCouldPossiblyExist(id))
                //throw new Exception($"Targeted cell [{CellId.getStringSafe(id)}] could not possibly exist - goes beyond max table area");
                throw new Exception($"Клітинка [{CellId.getStringSafe(id)}] неможилива - вона вилазить за обмеження");
        }

        public void CreateCell(CellId id)
        {
            CheckCellExistence(id);
            cells.Add(id, new TableCell(id));
        }

        public TableCell PullCell(CellId id)
        {
            if (!cells.ContainsKey(id))
                CreateCell(id);
            return FindCell(id);
        }

        public TableCell? FindCell(CellId id)
        {
            CheckCellDefinedArea(id);
            cells.TryGetValue(id, out TableCell? ans);
            return ans;
        }

        public void ClearCell(CellId id)
        {
            CheckCellExistence(id);
            if (cells.ContainsKey(id))
                cells.Remove(id);
        }

        private Dictionary<CellId, TableCell> GetNewDictionary(Action<CellId> positionMove)
        {
            Dictionary<CellId, TableCell> newCells = new Dictionary<CellId, TableCell>();

            foreach (var x in cells)
            {
                var key = x.Key;
                var cell = x.Value;
                positionMove(key);
                newCells.Add(key, cell);
            }
            return newCells;
        }

        public void RemoveColumn(uint id)
        {
            if (Columns == 1)
                //throw new Exception("Cannot remove last column");
                throw new Exception("Неможливо видалити останню колонку");
            if (!IsCellCouldExist(new CellId(1, id)))
                //throw new Exception($"Cannot remove column {id} : it does not exist at the time");
                throw new Exception($"Неможливо видалити колонку {id} : на цей час вона не існує");

            List<CellId> toDelete = new List<CellId>();
            foreach (var x in cells)
                if (x.Value.Id.ColumnInt == id)
                    toDelete.Add(x.Key);

            foreach (var x in toDelete)
                cells.Remove(x);

            cells = GetNewDictionary(
                (pos) => {
                    if(pos.ColumnInt > id)
                        pos.ColumnInt--;
                });

            Columns--;
        }

        public void AddColumn(uint id)
        {
            if (id > Columns + 1 || id == 0)
                //throw new Exception($"Cannot add column {id} : it could not exist at the time");
                throw new Exception($"Неможливо додати колонку {id} : на цей час вона не може існувати");
            if (Columns == maxcolumns)
                //throw new Exception("Cannot add any more columns");
                throw new Exception("Неможиво додати більше колонок - переповнення");

            cells = GetNewDictionary(
                (pos) => {
                    if (pos.ColumnInt >= id)
                        pos.ColumnInt++;
                });

            Columns++;
        }

        public void RemoveRow(uint id)
        {
            if (Rows == 1)
                //throw new Exception("Cannot remove last row");
                throw new Exception("Неможливо видалити останній рядок");
            if (!IsCellCouldExist(new CellId(id, 1)))
                //throw new Exception($"Cannot remove row {id} : it does not exist at the time");
                throw new Exception($"Неможливо видалити рядок {id} : на цей час він не існує");

            List<CellId> toDelete = new List<CellId>();
            foreach (var x in cells)
                if (x.Value.Id.Row == id)
                    toDelete.Add(x.Key);

            foreach (var x in toDelete)
                cells.Remove(x);

            cells = GetNewDictionary(
                (pos) => {
                    if (pos.Row > id)
                        pos.Row--;
                });

            Rows--;
        }

        public void AddRow(uint id)
        {
            if (id > Rows + 1 || id == 0)
                //throw new Exception($"Cannot add row {id} : it could not exist at the time");
                throw new Exception($"Неможливо додати рядок {id} : на цей час він не може існувати");
            if (Rows == maxrows)
                //throw new Exception("Cannot add any more rows");
                throw new Exception("Неможиво додати більше рядків - переповнення");

            cells = GetNewDictionary(
                (pos) => {
                    if (pos.Row >= id)
                        pos.Row++;
                });

            Rows++;
        }

        public const uint maxcolumns = 255;
        public const uint maxrows = 255;

        private uint columns = 0;
        private uint rows = 0;

        public uint Columns
        {
            get => columns;
            private set
            {
                if (value == 0)
                    //throw new Exception("Cannot create empty Table (columns == 0)");
                    throw new Exception("Неможливо створити пусту таблицю (колонки == 0)");
                if (value > maxcolumns)
                    //throw new Exception($"Cannot create table of size {Rows}x{value} (too many columns)");
                    throw new Exception($"Неможливо створити таблицю {Rows}x{value} (забагато колонок)");

                if (value < columns)
                    foreach (var cell in cells)
                    {
                        var keysToRemove = cells.Keys.Where(k => k.ColumnInt > value).ToList();
                        foreach (var key in keysToRemove)
                        {
                            cells.Remove(key);
                        }
                    }

                columns = value;
            }
        }

        public uint Rows
        {
            get => rows;
            private set
            {
                if (value == 0)
                    //throw new Exception("Cannot create empty Table (rows == 0)");
                    throw new Exception("Неможливо створити пусту таблицю (рядки == 0)");
                if (value > maxrows)
                    //throw new Exception($"Cannot create table of size {value}x{Columns} (too many rows)");
                    throw new Exception($"Неможливо створити таблицю {value}x{Columns} (забагато рядків)");

                if (value < rows)
                    foreach (var cell in cells)
                    {
                        var keysToRemove = cells.Keys.Where(k => k.Row > value).ToList();
                        foreach (var key in keysToRemove)
                        {
                            cells.Remove(key);
                        }
                    }

                rows = value;
            }
        }

        public Table() { }
        public Table(uint columnSize, uint rowSize)
        {
            Columns = columnSize;
            Rows = rowSize;
        }

    }

    internal class TableSaveData
    {
        public uint Columns { get; set; }
        public uint Rows { get; set; }

        public Dictionary<string, string> Cells { get; set; } = new Dictionary<string, string>();
    }
}
