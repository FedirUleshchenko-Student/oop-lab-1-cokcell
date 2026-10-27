using oop_lab_1_cokcell.Classes.Cells;
using oop_lab_1_cokcell.Classes.Files;
using oop_lab_1_cokcell.Classes.Parser;
using System.Runtime.CompilerServices;

namespace oop_lab_1_cokcell.Pages {
    public partial class TableTester : ContentPage
    {
        Func<Table> tableCreation = () =>
        {
            return new Table(3, 3);
        };
        Table table;
        //Table table = new Table(3, 3);

        public TableTester()
        {
            InitializeComponent();
            table = tableCreation();
        }

        private void UpdateGUI()
        {
            globalErrorView.Text = "";

            TableCell? c = table.PullCell(new CellId("A1"));
            c.Recalculate();
            inputField1.Text = c.Expression;
            answerView1.Text = (c.IsEmpty)
                ? "empty" : (c.ErrorMessage != null)
            ? c.ErrorMessage.Message : c.GetValue().ToString();

            c = table.PullCell(new CellId("A2"));
            c.Recalculate();
            inputField2.Text = c.Expression;
            answerView2.Text = (c.IsEmpty)
                ? "empty" : (c.ErrorMessage != null)
            ? c.ErrorMessage.Message : c.GetValue().ToString();

            c = table.PullCell(new CellId("A3"));
            c.Recalculate();
            inputField3.Text = c.Expression;
            answerView3.Text = (c.IsEmpty)
                ? "empty" : (c.ErrorMessage != null)
            ? c.ErrorMessage.Message : c.GetValue().ToString();

        }

        private void OnEntryCompleted1(object sender, EventArgs e)
        {
            TableCell c = table.PullCell(new CellId("A1"));
            c.SetExpression(((Entry)sender).Text, table);
            c.Recalculate();


            UpdateGUI();
        }

        private void OnEntryCompleted2(object sender, EventArgs e)
        {
            TableCell c = table.PullCell(new CellId("A2"));
            c.SetExpression(((Entry)sender).Text, table);
            c.Recalculate();


            UpdateGUI();
        }

        private void OnEntryCompleted3(object sender, EventArgs e)
        {
            TableCell c = table.PullCell(new CellId("A3"));
            c.SetExpression(((Entry)sender).Text, table);
            c.Recalculate();


            UpdateGUI();
        }

        private void OnTableClearing(object sender, EventArgs e)
        {
            try
            {
                globalErrorView.Text = "";

                table = tableCreation();
                UpdateGUI();
            }
            catch (Exception ex)
            {
                globalErrorView.Text = "ERROR : \n" + ex.Message + '\n';
            }
        }

        private void OnTableSaving(object sender, EventArgs e)
        {
            try
            {
                globalErrorView.Text = "";
                FileManager.SaveToFile(table, "testSave.json");
                UpdateGUI();
            }
            catch (Exception ex)
            {
                globalErrorView.Text = "ERROR : \n" + ex.Message + '\n';
            }
        }

        private void OnTableLoading(object sender, EventArgs e)
        {
            try
            {
                globalErrorView.Text = "";
                table = FileManager.LoadFromFile("testSave.json");
                UpdateGUI();
            }
            catch (Exception ex)
            {
                globalErrorView.Text = "ERROR : \n" + ex.Message + '\n';
            }
        }

    }
}