using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace oop_lab_1_cokcell.Classes.Table
{

    internal class CellId
    {

    // to rewrite ???
    public static readonly string Alphabet =
        (new Func<string>(() => {
            string a=string.Empty;
            for (char i = 'A'; i <= 'Z'; i++) a += i;
            return a;
        }))();

        private uint _row = 0;
        private uint _column = 0;

        public uint Row
        {
            get => _row;
            set => _row = value;
        }

        // temp
        public uint ColumnInt
        {
            get => _column;
            set => _column = value;
        }

        public string Column
        {
            get
            {
                string ans = string.Empty;
                int siz = 1;

                uint tC = ColumnInt - 1;

                int pow = 1;
                uint stepic = (uint)Math.Pow(Alphabet.Length, pow);
                uint maxCh = (uint)Alphabet.Length;
                while(stepic <= tC)
                {
                    pow++;
                    stepic += (uint)Math.Pow(Alphabet.Length, pow);
                    maxCh *= (uint)Alphabet.Length;
                    siz++;
                }

                stepic -= (uint)Math.Pow(Alphabet.Length, pow);
                pow--;
                maxCh /= (uint)Alphabet.Length;


                // for 1 sized
                if (siz == 1)
                {
                    ans += Alphabet[(int)(tC / maxCh)];
                    return ans;
                }

                siz--;
                ans += Alphabet[(int)(tC / maxCh) - 1];
                tC -= stepic * ((tC / maxCh));
                stepic -= (uint)Math.Pow(Alphabet.Length, pow);
                maxCh /= (uint)Alphabet.Length;
                pow--;

                while (siz > 0)
                {
                    siz--;
                    ans += Alphabet[(int)(tC / maxCh)];
                    tC -= stepic * ((tC / maxCh));
                    stepic -= (uint)Math.Pow(Alphabet.Length, pow);
                    maxCh /= (uint)Alphabet.Length;
                    pow--;
                }

                return ans;
            }
            set
            {

            }
        }

        public CellId()
        {

        }
        public CellId(uint row, uint column) : this()
        {
            Row = row;
            ColumnInt = column;
        }
        public CellId(string Id) : this()
        {
            Column = Id;
        }

        public override string ToString()
        {
            return Column + Row.ToString();
        }
    }

    //internal class Cell
    //{
    //    CellId id = new CellId();
    //}
}
