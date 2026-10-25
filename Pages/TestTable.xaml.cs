using oop_lab_1_cokcell.Classes.Cells;
using oop_lab_1_cokcell.Classes.Parser;
using System.Runtime.CompilerServices;

namespace oop_lab_1_cokcell.Pages {
    public partial class TableTester : ContentPage
    {
        Table table = new Table(3, 3);

        public TableTester()
        {
            InitializeComponent();
        }

        private void UpdateGUI()
        {
            TableCell? c = table.PullCell(new CellId("A1"));
            c.Recalculate();
            answerView1.Text = (c.IsEmpty)
                ? "empty" : (c.ErrorMessage != null)
            ? c.ErrorMessage.ToString() : c.GetValue().ToString();

            c = table.PullCell(new CellId("A2"));
            c.Recalculate();
            answerView2.Text = (c.IsEmpty)
                ? "empty" : (c.ErrorMessage != null)
            ? c.ErrorMessage.ToString() : c.GetValue().ToString();

            c = table.PullCell(new CellId("A3"));
            c.Recalculate();
            answerView3.Text = (c.IsEmpty)
                ? "empty" : (c.ErrorMessage != null)
            ? c.ErrorMessage.ToString() : c.GetValue().ToString();

        }

        private void OnEntryCompleted1(object sender, EventArgs e)
        {
            TableCell? c = table.PullCell(new CellId("A1"));
            c?.SetExpression(((Entry)sender).Text, table);
            c.Recalculate();


            UpdateGUI();
            //c.Calculate();
            //answerView1.Text = (c.ErrorMessage != null)
            //    ? c.ErrorMessage.ToString() : c.GetValue().ToString();
        }

        private void OnEntryCompleted2(object sender, EventArgs e)
        {
            TableCell? c = table.PullCell(new CellId("A2"));
            c?.SetExpression(((Entry)sender).Text, table);
            c.Recalculate();


            UpdateGUI();
            //c.Calculate();
            //answerView2.Text = (c.ErrorMessage != null)
            //    ? c.ErrorMessage.ToString() : c.GetValue().ToString();
        }

        private void OnEntryCompleted3(object sender, EventArgs e)
        {
            TableCell? c = table.PullCell(new CellId("A3"));
            c?.SetExpression(((Entry)sender).Text, table);
            c.Recalculate();


            UpdateGUI();
            //c.Calculate();
            //answerView3.Text = (c.ErrorMessage != null)
            //    ? c.ErrorMessage.ToString() : c.GetValue().ToString();
        }

    }
}