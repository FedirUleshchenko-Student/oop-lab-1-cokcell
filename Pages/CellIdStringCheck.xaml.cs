using oop_lab_1_cokcell.Classes.Cells;

namespace oop_lab_1_cokcell.Pages;

public partial class CellIdStringCheck : ContentPage
{
	public CellIdStringCheck()
	{
		InitializeComponent();
	}

    CellId c = new CellId();

    private void Test1(object sender, EventArgs e)
    {
        try
        {
            c.Row++;
            c.ColumnInt++;
            answerView.Text = c.ToString() + ' ';
        }
        catch (Exception ex)
        {
            answerView.Text = "ERROR : \n" + ex.Message + '\n';
            c = new CellId();
        }

    }

    private void TestAll(object sender, EventArgs e)
    {
        try
        {
            EmptyCell(sender, e);
            while (c.Row < 1000)
            {
                c.Row++;
                c.ColumnInt++;
                if(c.Row % 5 == 0)
                    answerView.Text += c.ToString() + "    ";
            }
        }
        catch (Exception ex)
        {
            answerView.Text = "ERROR : \n" + ex.Message + '\n';
            c = new CellId();
        }

    }

    private void EmptyCell(object sender, EventArgs e)
    {
        c = new CellId();
        answerView.Text = "";
    }
}