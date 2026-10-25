using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using oop_lab_1_cokcell.Classes.Cells;

namespace oop_lab_1_cokcell.Classes.ExpressionNodes
{
    internal class ReferenceNode : Expression
    {
        private CellId key;
        private TableAction valueProvider;

        public ReferenceNode(CellId position, TableAction provider)
        {
            key = position;
            valueProvider = provider;
        }

        public override BigInteger? Calculate()
        {
            return valueProvider(key);
        }

        public CellId GetKey() { return key; }
    }
}
