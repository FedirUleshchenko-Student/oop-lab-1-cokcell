using oop_lab_1_cokcell.Classes.Cells;
using oop_lab_1_cokcell.Classes.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace oop_lab_1_cokcell.Pages
{
    public partial class UIManager : ContentPage
    {
        #region Data
        public enum ViewModes { EXPRESSION_MODE, VALUE_MODE };
        private ViewModes viewMode = ViewModes.VALUE_MODE;

        private Table currentTable;
        private CellId selectedCell;
        private Entry? _selectedEntry;
        private Entry selectedEntry
        {
            get => _selectedEntry;
            set
            {
                if (_selectedEntry == value) return;
                if (_selectedEntry != null)
                {
                    _selectedEntry.FontSize = 12;
                    _selectedEntry.TextColor = Colors.White;
                    UpdateGridCellBackgroundColor(_selectedEntry, true);
                }

                _selectedEntry = value;

                _selectedEntry.FontSize = 15;
                _selectedEntry.TextColor = Color.FromArgb("#9CDCFE");
                UpdateGridCellBackgroundColor(_selectedEntry);

                // shit code
                // for default text, not "" when selecting empty cell
                ValueOrErrorDisplay.Text = "";
            }
        }
        private bool calculatingCurrentCell = false;

        private class SavedFileState
        {
            public enum StorageMode { MACHINE, GOOGLE }
            public StorageMode CurrentMode { get; private set; }

            public bool PossibleSave
            {
                get =>
                CurrentMode == StorageMode.MACHINE ?
                !string.IsNullOrEmpty(machinePath) :
                !string.IsNullOrEmpty(googleFileId);
            }

            private string machinePath = "";
            public string MachinePath => machinePath;

            private string googleFileId = "";
            public string GoogleFileId => googleFileId;



            private bool changes = false;
            public bool Changes { get => changes; }
            public void FlagChanges() { changes = true; }

            // for first opener
            public SavedFileState() { }

            public SavedFileState(string pathOrName, Table data, StorageMode mode)
            {
                CurrentMode = mode;
                if (mode == StorageMode.MACHINE)
                    machinePath = pathOrName;
                else
                    googleFileId = pathOrName;

                Save(data);
            }

            public SavedFileState(string pathOrName, out Table data, StorageMode mode)
            {
                CurrentMode = mode;
                if (mode == StorageMode.MACHINE)
                {
                    machinePath = pathOrName;
                    data = FileManager.LoadFromFile(machinePath);
                }
                else
                {
                    googleFileId = pathOrName;
                    data = GoogleDriveManager.LoadTable(googleFileId);
                }
                changes = false;
            }

            public void Save(Table data)
            {
                if (CurrentMode == StorageMode.MACHINE)
                {
                    FileManager.SaveToFile(data, machinePath);
                }
                else if (CurrentMode == StorageMode.GOOGLE)
                {
                    GoogleDriveManager.SaveTable(data, googleFileId);
                }
                changes = false;
            }
        }
        private SavedFileState? fileState;

        private Dictionary<Entry, CellId> uiCells = new Dictionary<Entry, CellId>();
        private Dictionary<CellId, Entry> entrys = new Dictionary<CellId, Entry>();
        #endregion

        public UIManager()
        {
            InitializeComponent();
            OpenNewTable();
        }

        private void ForceCalculation()
        {
            var it = currentTable.GetEnummeratorCells;
            while (it.MoveNext())
                it.Current.Value.Recalculate();
        }

        #region UpdateUI
        #region StartUI
        private void BuildGridUI()
        {
            DataGrid.RowDefinitions.Clear();
            DataGrid.ColumnDefinitions.Clear();
            uiCells.Clear();
            entrys.Clear();

            for (int r = 0; r <= currentTable.Rows; r++)
                DataGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            for (int c = 0; c <= currentTable.Columns; c++)
                DataGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = c == 0 ? 40 : 100 });

            for (uint r = 0; r <= currentTable.Rows; r++)
            {
                for (uint c = 0; c <= currentTable.Columns; c++)
                {
                    View cellView;

                    if (r == 0 && c == 0)
                    {
                        cellView = new BoxView { Color = Color.FromArgb("#2D2D30") };
                    }
                    else if (r == 0)
                    {
                        string colName = new CellId(1, c).Column;
                        cellView = new Label
                        {
                            Text = colName,
                            HorizontalTextAlignment = TextAlignment.Center,
                            VerticalTextAlignment = TextAlignment.Center,
                            TextColor = Colors.White,
                            BackgroundColor = Color.FromArgb("#2D2D30")
                        };
                    }
                    else if (c == 0)
                    {
                        cellView = new Label
                        {
                            Text = r.ToString(),
                            HorizontalTextAlignment = TextAlignment.Center,
                            VerticalTextAlignment = TextAlignment.Center,
                            TextColor = Colors.White,
                            BackgroundColor = Color.FromArgb("#2D2D30")
                        };
                    }
                    else
                    {
                        CellId entryCellId = new CellId(r, c);
                        string entryCellIdStr = entryCellId.ToString();
                        var entry = new Entry
                        {
                            Text = "",
                            FontSize = 12,
                            TextColor = Colors.White,
                            BackgroundColor = Color.FromArgb("#1E1E1E"),
                            HorizontalTextAlignment = TextAlignment.Center,
                            ClearButtonVisibility = ClearButtonVisibility.Never
                        };

                        entry.Focused += (s, e) =>
                        {
                            selectedCell = entryCellId;
                            selectedEntry = entry;

                            ActiveCellLabel.Text = entryCellIdStr;

                            var cell = currentTable.FindCell(entryCellId);
                            FormulaBar.Text = cell != null ? cell.Expression : "";
                            ValueOrErrorDisplay.Text = cell != null ?
                            cell.GetAbsoluteErrorSupportedValue() : "";

                            // Temp
                            entry.Text = cell != null ? cell.Expression : "";
                        };

                        entry.Unfocused += (s, e) =>
                        {
                            if (viewMode == ViewModes.VALUE_MODE)
                                PushInput(FormulaBar.Text, entryCellId);
                        };

                        entry.TextChanged += GridCell_TextChanged;

                        entry.Completed += GridCell_Completed;
                        uiCells[entry] = entryCellId;
                        entrys[entryCellId] = entry;
                        cellView = entry;
                    }

                    DataGrid.Add(cellView, (int)c, (int)r);
                }
            }
        }
        #endregion

        private void UpdateGridCellBackgroundColor(Entry? entry, bool isForcedUnfocus = false)
        {
            if (entry == null) return;

            if (viewMode == ViewModes.VALUE_MODE)
            {
                uiCells.TryGetValue(entry, out CellId? id);
                var cell = id == null ? null : currentTable.FindCell(id);

                if (!isForcedUnfocus && selectedEntry == entry)
                    entry.BackgroundColor = Color.FromArgb("#3E3E42");
                else if (cell?.ErrorMessage != null)
                    entry.BackgroundColor = Color.FromArgb("#8A0000");
                else
                    entry.BackgroundColor = Color.FromArgb("#1E1E1E");
            }
            else
            {
                if (!isForcedUnfocus && selectedEntry == entry)
                    entry.BackgroundColor = Color.FromArgb("#3E3E42");
                else
                    entry.BackgroundColor = Color.FromArgb("#1E1E1E");
            }
        }

        private void UpdateGridCell(CellId id)
        {
            var entry = entrys[id];
            var cell = currentTable.FindCell(id);

            if (cell == null || cell.IsEmpty)
            {
                entry.Text = "";
                return;
            }

            if (viewMode == ViewModes.VALUE_MODE)
            {
                try
                {
                    entry.Text = cell.GetValue().ToString();
                    if (cell.ErrorMessage?.Message.Length > 0)
                        throw new Exception();
                }
                catch (Exception)
                {
                    entry.Text = "#ERROR";
                }
            }
            else
            {
                entry.Text = cell.Expression;
            }

            UpdateGridCellBackgroundColor(entry);
        }

        private void UpdateGridAll()
        {
            if (viewMode == ViewModes.VALUE_MODE)
                ForceCalculation();
            var it = currentTable.GetEnummeratorCells;
            while (it.MoveNext())
                // Same as : it.Current.Value.Id
                UpdateGridCell(it.Current.Key);
        }

        private void UpdateGridCellSmart(CellId id)
        {
            if (viewMode == ViewModes.VALUE_MODE)
                UpdateGridAll();
            else
                UpdateGridCell(id);
        }
        #endregion

        #region OnFieldsTextChanged
        private void PushInput(string expression, CellId? position = null)
        {
            // spam click fix
            if (string.IsNullOrEmpty(expression))
            {
                if (position == null) currentTable.ClearCell(selectedCell);
                else currentTable.ClearCell(position);
                return;
            }

            calculatingCurrentCell = true;
            var cell = currentTable.PullCell(
                (position == null) ? selectedCell : position
                );

            // NEED ??
            //FormulaBar.Text = cell != null ? cell.Expression : "";
            cell.SetExpression(expression, currentTable);
            ValueOrErrorDisplay.Text = cell != null ?
                cell.GetAbsoluteErrorSupportedValue() : "";

            UpdateGridCellSmart(cell.Id);
            fileState?.FlagChanges();
            calculatingCurrentCell = false;
        }

        private void GridCell_TextChanged(object? sender, TextChangedEventArgs e)
        {
            if (sender == null || calculatingCurrentCell) return;
            if ((Entry)sender == selectedEntry && FormulaBar.Text != e.NewTextValue)
                FormulaBar.Text = e.NewTextValue;
        }

        private void FormulaBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (selectedEntry.Text != e.NewTextValue)
                selectedEntry.Text = e.NewTextValue;
        }

        private void GridCell_Completed(object? sender, EventArgs e)
        {
            PushInput(((Entry)sender).Text);
        }

        private void FormulaBar_Completed(object sender, EventArgs e)
        {
            PushInput(((Entry)sender).Text);
        }
        #endregion

        #region Full table management
        bool isAsyncUIbusy = false;
        private void NewTable_Clicked(object sender, EventArgs e)
        {
            OpenNewTable();
        }

        private async void OpenNewTable()
        {
            if (fileState != null && fileState.Changes)
            {
                if (!fileState.PossibleSave)
                {
                    bool call = await DisplayAlert("Незбережені дані",
                        "Ваша таблиця не має файлу.\nСтворити файл?", "Створити", "Ні");
                    if (call)
                        await SaveTableAs();
                        Task.Run(() => SaveTableAs()).Wait();
                }
                else
                {
                    bool call = await DisplayAlert("Незбережені дані",
                        "Ваша таблиця має незбережені зміни.\nЗберегти їх?", "Зберегти", "Ні");
                    if (call)
                        fileState.Save(currentTable);
                }
            }

            fileState = new SavedFileState();
            currentTable = new Table(10, 10);
            selectedCell = new CellId(1, 1);
            BuildGridUI();
            selectedEntry = entrys[selectedCell];
        }

        private void SaveTable_Clicked(object sender, EventArgs e)
        {
            if (fileState != null && fileState.PossibleSave)
                fileState.Save(currentTable);
            else
                SaveTableAs_Clicked(sender, e);
        }

        private async Task SaveTableAs()
        {
            try
            {
                bool call = await DisplayAlert("Зберегти у...", "Оберіть місце для збереження", "Локально", "Google");

                if (call)
                {
                    string appFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CokCell");
                    if(!System.IO.Directory.Exists(appFolder))
                        System.IO.Directory.CreateDirectory(appFolder);
                    string filePath = System.IO.Path.Combine(appFolder, "CokCell_Table.json");

                    fileState = new SavedFileState(filePath, currentTable, SavedFileState.StorageMode.MACHINE);

                    await DisplayAlert("Успіх", $"Успішно збережено у " +
                    $"{System.IO.Path.Combine(FileSystem.CacheDirectory, "CokCell_Table.json")}", "ОК");
                }
                else
                {
                    fileState = new SavedFileState(
                        "CokCell_Table.json",
                        currentTable, SavedFileState.StorageMode.GOOGLE);

                    await DisplayAlert("Успіх", "Успішно збережено на Google Drive", "ОК");
                }

            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка збереження", ex.Message, "ОК");
            }
        }
        private async Task LoadTable()
        {
            try
            {
                bool call = await DisplayAlert("Завантажити з...", "Оберіть місце для завантаження", "Локально", "Google");

                if (call)
                {
                    string appFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CokCell");
                    if (!System.IO.Directory.Exists(appFolder))
                        System.IO.Directory.CreateDirectory(appFolder);
                    string filePath = System.IO.Path.Combine(appFolder, "CokCell_Table.json");

                    fileState = new SavedFileState(filePath, out currentTable, SavedFileState.StorageMode.MACHINE);

                    await DisplayAlert("Успіх", $"Успішно завантажено з " +
                        $"{System.IO.Path.Combine(FileSystem.CacheDirectory, "CokCell_Table.json")}", "ОК");
                }
                else
                {
                    fileState = new SavedFileState(
                        "CokCell_Table.json",
                        out currentTable, SavedFileState.StorageMode.GOOGLE);

                    await DisplayAlert("Успіх", "Успішно завантажено з Google Drive", "ОК");
                }
                BuildGridUI();
                selectedCell = new CellId(1, 1);
                selectedEntry = entrys[selectedCell];
                UpdateGridAll();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка завантаження", ex.Message, "ОК");
            }
        }

        // should be with ?? :
        // try {} finally {}

        private async void SaveTableAs_Clicked(object sender, EventArgs e)
        {
            if (isAsyncUIbusy) return;
            try
            {
                isAsyncUIbusy = true;
                await SaveTableAs();
            }
            finally { isAsyncUIbusy = false; }
        }
        private async void LoadTable_Clicked(object sender, EventArgs e)
        {
            if (isAsyncUIbusy) return;
            try
            {
                isAsyncUIbusy = true;
                await LoadTable();
            }
            finally { isAsyncUIbusy = false; }
        }
        #endregion

        #region TableSizeChange
        // remove cells bypass
        private void NewTableOverrideBypass()
        {
            BuildGridUI();
            selectedEntry = entrys[selectedCell];
            UpdateGridAll();
        }

        private async void DeleteRow_Clicked(object sender, EventArgs e)
        {
            try
            {
                string inputText = RowTargetEntry.Text;
                uint targetRow;
                if (string.IsNullOrWhiteSpace(inputText))
                    targetRow = currentTable.Rows;
                else if (!uint.TryParse(inputText, out targetRow))
                    throw new Exception("Не всі знаки є числами");

                currentTable.RemoveRow(targetRow);

                if (selectedCell.Row > currentTable.Rows)
                {
                    selectedCell = new CellId(1, 1);
                }

                NewTableOverrideBypass();
                //RowTargetEntry.Text = string.Empty;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка", $"{ex.Message}", "ОК");
            }
        }
        private async void AddRow_Clicked(object sender, EventArgs e)
        {
            try
            {
                string inputText = RowTargetEntry.Text;
                uint targetRow;
                if (string.IsNullOrWhiteSpace(inputText))
                    targetRow = currentTable.Rows;
                else if (!uint.TryParse(inputText, out targetRow))
                    throw new Exception("Не всі знаки є числами");

                currentTable.AddRow(targetRow);

                NewTableOverrideBypass();
                //RowTargetEntry.Text = string.Empty;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка", $"{ex.Message}", "ОК");
            }
        }

        private async void DeleteCol_Clicked(object sender, EventArgs e)
        {
            try
            {
                string inputText = ColTargetEntry.Text;
                uint targetCol;
                if (string.IsNullOrWhiteSpace(inputText))
                    targetCol = currentTable.Columns;
                else if (inputText.All(x => char.IsUpper(x)))
                    targetCol = (new CellId(inputText + "1")).ColumnInt;
                else
                    throw new Exception("Не всі знаки є великими буквами англійського алфавіту");

                currentTable.RemoveColumn(targetCol);

                if (selectedCell.ColumnInt > currentTable.Columns)
                {
                    selectedCell = new CellId(1, 1);
                }

                NewTableOverrideBypass();
                //ColTargetEntry.Text = string.Empty;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка", $"{ex.Message}", "ОК");
            }
        }
        private async void AddCol_Clicked(object sender, EventArgs e)
        {
            try
            {
                string inputText = ColTargetEntry.Text;
                uint targetCol;
                if (string.IsNullOrWhiteSpace(inputText))
                    targetCol = currentTable.Columns;
                else if (inputText.All(x => char.IsUpper(x)))
                    targetCol = (new CellId(inputText + "1")).ColumnInt;
                else
                    throw new Exception("Не всі знаки є великими буквами англійського алфавіту");

                currentTable.AddColumn(targetCol);

                NewTableOverrideBypass();
                //ColTargetEntry.Text = string.Empty;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка", $"{ex.Message}", "ОК");
            }
        }
        #endregion

        private void ModeSwitch_Toggled(object sender, EventArgs e)
        {
            if (viewMode == ViewModes.EXPRESSION_MODE)
            {
                ForceCalculation();
                viewMode = ViewModes.VALUE_MODE;
                ModeLabel.Text = "Режим: ЗНАЧЕННЯ";
            }
            else
            {
                viewMode = ViewModes.EXPRESSION_MODE;
                ModeLabel.Text = "Режим: ФОРМУЛА";
            }
            UpdateGridAll();
        }

        private async void Docs_Clicked(object sender, EventArgs e)
        {
            string docsText =
                "[*] РОБОТА З КЛІТИНКАМИ\n" +
                "Натисніть на будь-яку клітинку, щоб виділити її. Вводити числа або формули (наприклад, A1 + B2) можна як у саму клітинку, так і у верхній рядок формул. Натисніть Enter для підтвердження.\n\n" +
                "[*] РЕЖИМИ ВІДОБРАЖЕННЯ\n" +
                "* ЗНАЧЕННЯ: у таблиці виводяться результати обчислень. Якщо у формулі помилка — клітинка стане червоною.\n" +
                "* ФОРМУЛА: у таблиці відображаються сирі вирази, які ви вводили.\n\n" +
                "[*] ЗМІНА РОЗМІРУ ТАБЛИЦІ\n" +
                "Щоб додати або видалити рядок чи стовпчик, введіть його номер у відповідне поле зверху та натисніть кнопку. Якщо залишити поле порожнім, дія застосується до крайнього (останнього) рядка або стовпця.\n\n" +
                "[*] ЗБЕРЕЖЕННЯ ФАЙЛІВ\n" +
                "Натисніть «Зберегти таблицю як...», щоб обрати місце збереження: локально на вашому пристрої або у хмарі Google Drive.";

            await DisplayAlert("Документація CokCell", docsText, "Зрозуміло");
        }
    }
}
