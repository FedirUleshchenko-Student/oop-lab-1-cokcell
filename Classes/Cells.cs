using oop_lab_1_cokcell.Classes.ExpressionNodes;
using oop_lab_1_cokcell.Classes.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


namespace oop_lab_1_cokcell.Classes.Cells
{
    internal delegate BigInteger TableAction(CellId id);

    internal class CellId
    {
        //private const string logname = "Cell Id";
        public const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private uint _row = 0;
        private uint _column = 0;

        public uint Row
        {
            get => _row;
            set
            {
                if (value == 0)
                    //throw new Exception("Cannot set row to 0");
                    throw new Exception("Неможливо присвоїти значення 0 рядку");
                _row = value;
            }
        }

        public uint ColumnInt
        {
            get => _column;
            set
            {
                if (value == 0)
                    //throw new Exception("Cannot set column to 0");
                    throw new Exception("Неможливо присвоїти значення 0 колонці");
                _column = value;
            }
        }

        public string Column
        {
            get
            {
                if (ColumnInt == 0) return "0";

                string ans = string.Empty;
                uint tC = ColumnInt;

                while (tC > 0)
                {
                    tC--;
                    // adding to the end (first - bigger powers)
                    ans = (char)Alphabet[(int)tC % Alphabet.Length] + ans;
                    tC /= (uint)Alphabet.Length;
                }

                return ans;
            }
            set
            {
                if (string.IsNullOrEmpty(value)) {
                    ColumnInt = 0; return;
                }

                uint ans = 0;
                foreach (char c in value)
                    ans = ans * 26 + (uint)Alphabet.IndexOf(c) + 1;

                ColumnInt = ans;
            }
        }

        public CellId() { }
        public CellId(uint row, uint column) : this()
        {
            Row = row;
            ColumnInt = column;
        }
        public CellId(string Id) : this()
        {
            int numberIndex = 0;

            while (numberIndex < Id.Length
                && char.IsLetter(Id[numberIndex])) {
                numberIndex++; }

            if(numberIndex == 0)
                //throw new Exception($"There is no column position (chars) in refNode {Id}");
                throw new Exception($"Не існує позиції колонки (знаки) у ноді посилання {Id}");
            Column = Id.Substring(0, numberIndex);

            if (numberIndex < Id.Length)
                Row = uint.Parse(Id.Substring(numberIndex));
            else
                //throw new Exception($"There is no row position (number) in refNode {Id}");
                throw new Exception($"Не існує позиції рядку (цифри) у ноді посилання {Id}");
        }

        public static string getStringSafe(CellId id)
        {
            string keyStr;
            try
            {
                keyStr = id.ToString();
            }
            catch
            {
                keyStr = "it's a NULL cell [0,0]";
            }
            return keyStr;
        }

        public override string ToString()
        {
            if (ColumnInt == 0 || Row == 0)
                //throw new Exception($"Cannot get a location Id of a position [{ColumnInt}, {Row}]");
                throw new Exception($"Неможливо отримати CellId позиції [{ColumnInt}, {Row}]");
            return Column + Row.ToString();
        }

        // for table's dictionary's keys
        #region Comparament
        //https://learn.microsoft.com/uk-ua/dotnet/api/system.object.equals?view=net-10.0
        //https://learn.microsoft.com/uk-ua/dotnet/api/system.object.gethashcode?view=net-10.0#system-object-gethashcode
        //https://learn.microsoft.com/uk-ua/dotnet/api/system.icomparable-1?view=net-10.0&viewFallbackFrom=netframework-1.1
        
        // ??
        // we need it because, Dict use : (obj is CellId comp) { comp.Equals(); // our } 
        // ??
        public override bool Equals(object? obj)
        {
            if (obj is CellId other)
                return this.Row == other.Row && this.ColumnInt == other.ColumnInt;
            return false;
        }

        public override int GetHashCode()
        {
            // this is THE STUFF
            // Real key
            return HashCode.Combine(Row, ColumnInt);
        }
        #endregion
    }

    internal class TableCell
    {
        public CellId Id = new CellId();
        private string rawExpression = string.Empty;
        public string Expression
        {
            get => rawExpression;
        }

        public bool IsEmpty
        {
            // Null Expression is empty (?)
            get => (parsedExpression == null
                || parsedExpression.GetType() == typeof(Expression))
                // DANGER
                // COULD BREAK PARSER LOGIC ???
                && ErrorMessage == null;
        }

        public bool recalculationNeed = true;
        private BigInteger CalculatedValue;
        private Expression? parsedExpression;

        public Exception? ErrorMessage;

        public event Action<CellId>? onErrorThrown;
        public event Action? onRecalculation;
        private List<CellId> foundation = new List<CellId>();
        public List<CellId> Foundation { get => foundation; }

        private bool RelaysOn(CellId targetId, Table currTable, HashSet<CellId>? visited = null)
        {
            visited ??= new HashSet<CellId>();
            if (!visited.Add(Id)) return false;

            if (Foundation.Contains(targetId))
                return true;

            foreach (var depId in Foundation)
            {
                var cell = currTable.FindCell(depId);
                if (cell != null && cell.RelaysOn(targetId, currTable, visited))
                    return true;
            }
            return false;
        }


        private void OnOccuredError(CellId sourceId)
        {
            recalculationNeed = true;
            //ErrorMessage = new Exception($"Broken calculation by related referenced cell [{sourceId}]");
            ErrorMessage = new Exception($"Знаманий підрахунок через залежну клітинку [{sourceId}]");
            onErrorThrown?.Invoke(Id);
        }

        private void RecalculationForced()
        {

            // Expr or Error - would be
            // Answer does not change without SetExpression
            try
            {
                CalculatedValue = parsedExpression ?? (BigInteger)0;
                recalculationNeed = false;
                ErrorMessage = null;
                onRecalculation?.Invoke();
            }
            catch (Exception e)
            {
                ErrorMessage = e;
            }
        }

        public void SetExpression(string expression, Table currTable)
        {
            try
            {
                // for EXPRESSION VIEW
                // Should be at the end, but ^^^
                rawExpression = expression;

                // Removing old linkables
                foreach (var cellId in Foundation)
                {
                    var cell = currTable.FindCell(cellId);
                    if (cell != null)
                    {
                        cell.onErrorThrown -= OnOccuredError;
                        cell.onRecalculation -= RecalculationForced;
                    }
                }

                parsedExpression = ExpressionParser.Parse(expression, (id) =>
                {
                    var c = currTable.FindCell(id);
                    if (c == null)
                        //throw new Exception($"Cannot locate cell: {CellId.getStringSafe(id)}");
                        throw new Exception($"Неможливо знайти клітинку: {CellId.getStringSafe(id)}");
                    return c.GetValue(true);
                }, out foundation);

                if (parsedExpression.GetType() == typeof(Expression))
                    parsedExpression = null;

                foreach (var cellId in Foundation)
                {
                    if(cellId.Equals(Id))
                        //throw new Exception($"Self-recursive reference occured");
                        throw new Exception($"Знайдене посилання сам-у-себе");
                }

                // Test move, for 1 -> 2 -> 3 -> 1 situation
                // Setting up new linkables for recalc
                foreach (var cellId in Foundation)
                {
                    var cell = currTable.PullCell(cellId);
                    cell.onRecalculation += RecalculationForced;
                }

                foreach (var cellId in Foundation)
                {
                    var cell = currTable.FindCell(cellId);
                    if (cell != null && cell.RelaysOn(Id, currTable))
                        //throw new Exception($"New recursive reference occured, relays on [{cellId}]");
                        throw new Exception($"Знайдене нове рекурсивне посилання, залежить від [{cellId}]");
                }

                // Test move, for 1 -> 2 -> 3 -> 1 situation
                // Double-linking new linkables
                foreach (var cellId in Foundation)
                {
                    var cell = currTable.PullCell(cellId);
                    cell.onErrorThrown += OnOccuredError;
                }

                recalculationNeed = true;
                ErrorMessage = null;
            }
            catch (Exception e)
            {
                // Should be safe because of 2 ways of getting here:
                // 1. Wrong expression (parser)
                // 2. Recursion (same, wrong expr)
                
                // No ?
                // 1 -> 2 -> 3 -> 1
                // destroying link 3->1 does not activates recalc
                // Foundation.Clear();

                ErrorMessage = e;
                onErrorThrown?.Invoke(Id);
            }
        }

        public TableCell(CellId position)
        {
            Id = position;
        }

        public BigInteger GetValue(bool? relationErrorCellId = false)
        {
            if(recalculationNeed)
                Recalculate();
            if (ErrorMessage != null)
            {
                onErrorThrown?.Invoke(Id);

                if (relationErrorCellId == true)
                    //throw new Exception($"Broken calculation by related referenced cell [{Id}]");
                    throw new Exception($"Знаманий підрахунок через залежну клітинку [{Id}]");

                throw ErrorMessage;
            }

            return CalculatedValue;
        }

        public string? GetAbsoluteErrorSupportedValue()
        {
            string ans;
            try
            {
                ans = GetValue().ToString();
            }
            catch(Exception e)
            {
                ans = e.Message;
            }
            //return (ans == "" || (parsedExpression?.GetType() == typeof(Exception))) ? null : ans;
            return ans == "" ? null : ans;
        }

        public void Recalculate()
        {
            if (ErrorMessage != null) return;
            RecalculationForced();
        }
    }
}
