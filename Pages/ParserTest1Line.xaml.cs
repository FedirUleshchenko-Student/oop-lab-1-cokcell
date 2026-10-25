using System.IO;
using System.Numerics;
using oop_lab_1_cokcell.Classes.ExpressionNodes;
using oop_lab_1_cokcell.Classes.Parser;
using oop_lab_1_cokcell.Classes.Cells;

namespace oop_lab_1_cokcell.Pages
{
	public partial class ParserTest1 : ContentPage
	{
		public ParserTest1()
		{
			InitializeComponent();
		}

        private void OnEntryCompleted(object sender, EventArgs e)
        {
            // GEMINI - how to simplify this c++ type of try / catch structure ? 
            try
            {
                // GEMINI - why not get value from e (EventArgs) ?

                //Expression? exp = null;
                //exp = ExpressionParser.GetExpression(((Entry)sender).Text);
                //outputView.Text = (exp?.calculate()).ToString();
                var temp = new List<CellId>();
                var testReader = ExpressionParser.Parse(((Entry)sender).Text, (id) => 0, out temp);
                errorView.Text = "";
                answerView.Text = testReader.ToString();
                calculatedValueView.Text = testReader.Calculate().ToString();
                //try
                //{
                //}
                //catch (Exception ex)
                //{
                //    calculatedValueView.Text = "no ans - stpd NGR\n"+"Message:\n"+ex.Message;
                //}
            }
            catch (Exception ex)
            {
                errorView.Text = "ERROR : \n" + ex.Message;
                answerView.Text = "";
                calculatedValueView.Text = "";

            }

        }
    }
}