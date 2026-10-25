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

        public void CreateCell(CellId id)
        {
            if (id.Row > maxrows || id.ColumnInt > maxcolumns)
                throw new Exception($"Targeted cell [{CellId.getStringSafe(id)}] goes beyond the defined table area");
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
            if (id.Row > maxrows || id.ColumnInt > maxcolumns)
                throw new Exception($"Targeted cell [{CellId.getStringSafe(id)}] goes beyond the defined table area");
            cells.TryGetValue(id, out TableCell? ans);
            return ans;
        }

        public const uint maxcolumns = 255;
        public const uint maxrows = 255;

        private uint columns = 0;
        private uint rows = 0;

        public uint Columns
        {
            get => columns;
            set
            {
                if (value > maxcolumns)
                    throw new Exception($"Cannot create table of size {Rows}x{value} (too many columns)");

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
            set
            {
                if (value > maxrows)
                    throw new Exception($"Cannot create table of size {value}x{Columns} (too many rows)");

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
}
