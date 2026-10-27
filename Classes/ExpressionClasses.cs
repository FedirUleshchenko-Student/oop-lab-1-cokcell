using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace oop_lab_1_cokcell.Classes.ExpressionNodes
{
    internal class Expression
    {
        //public virtual bool isLogicUI { get => false; }

        public virtual BigInteger? Calculate()
        {
            return null;
        }

        public static implicit operator BigInteger(Expression expr)
        {
            return expr.Calculate() ?? 0;
        }
    }

    internal class NumberNode : Expression
    {
        public BigInteger value=0;

        public NumberNode() { }
        public NumberNode(BigInteger value)
        {
            this.value = value;
        }

        public override BigInteger? Calculate()
        {
            return value;
        }
    }

    internal abstract class OperationNode : Expression
    {
        public abstract string NameLog { get; }
        /// <summary>
        /// Lower - first to calculate
        /// </summary>
        public virtual int Priority => 0;
        /// <summary>
        /// Always needs at least 1
        /// Set null if there is no limit at argument count (possible - [1, inf])
        /// </summary>
        public virtual int? NeededArgs { get; }
        public List<Expression> Arguments = new List<Expression>();


        public virtual bool Ready() { return (!NeededArgs.HasValue) || Arguments.Count == NeededArgs; }

        protected abstract BigInteger calculationLogic();
        protected void CalculationCheck()
        {
            if (!Ready())
            {
                //throw new Exception(
                //    $"Cannot use operation {NameLog} " +
                //    $"on {Arguments.Count} arguments (Must be {NeededArgs})");
                throw new Exception(
                    $"Неможливо використати операцію {NameLog} " +
                    $"на {Arguments.Count} аргументах (Має бути {NeededArgs})");
            }
        }

        public override BigInteger? Calculate()
        {
            CalculationCheck();
            return calculationLogic();
        }
    }

    internal class AddNode : OperationNode
    {
        public override string NameLog => "+ (binary)";

        public override int Priority => 4;
        public override int? NeededArgs => 2;

        protected override BigInteger calculationLogic()
        {
            return (BigInteger)Arguments[0] + Arguments[1];
        }
    }

    internal class SubtractNode : OperationNode
    {
        public override string NameLog => "- (binary)";

        public override int Priority => 4;
        public override int? NeededArgs => 2;

        protected override BigInteger calculationLogic()
        {
            return (BigInteger)Arguments[0] - Arguments[1];
        }
    }

    internal class MultiplyNode : OperationNode
    {
        public override string NameLog => "*";

        public override int Priority => 3;
        public override int? NeededArgs => 2;

        protected override BigInteger calculationLogic()
        {
            return (BigInteger)Arguments[0] * Arguments[1];
        }
    }

    internal class DivideNode : OperationNode
    {
        public override string NameLog => "/";

        public override int Priority => 3;
        public override int? NeededArgs => 2;

        public DivideNode() {}
        public DivideNode(List<Expression> _args)
        {
            Arguments = _args;
            CalculationCheck();
        }

        protected override BigInteger calculationLogic()
        {
            if ((BigInteger)Arguments[1] == 0)
                //throw new Exception("Cannot divide by 0");
                throw new Exception("Неможливо поділити на 0");
            return (BigInteger)Arguments[0] / Arguments[1];
        }
    }

    internal class UnaryPlusNode : OperationNode
    {
        public override string NameLog => "+ (unary)";

        public override int Priority => 1;
        public override int? NeededArgs => 1;
        protected override BigInteger calculationLogic()
        {
            return Arguments[0];
        }
    }

    internal class UnaryMinusNode : OperationNode
    {
        public override string NameLog => "- (unary)";

        public override int Priority => 1;
        public override int? NeededArgs => 1;
        protected override BigInteger calculationLogic()
        {
            return (BigInteger)(-1) * Arguments[0];
        }
    }

    internal class AbsoluteNode : OperationNode
    {
        public override string NameLog => "abs";

        public override int? NeededArgs => 1;
        public AbsoluteNode(List<Expression> _args)
        {
            Arguments = _args;
            CalculationCheck();
        }

        protected override BigInteger calculationLogic()
        {
            return BigInteger.Abs(Arguments[0]);
        }
    }

    internal class ModuleNode : OperationNode
    {
        public override string NameLog => "%";

        public override int? NeededArgs => 2;
        public ModuleNode(List<Expression> _args)
        {
            Arguments = _args;
            CalculationCheck();
        }

        protected override BigInteger calculationLogic()
        {
            return (BigInteger)Arguments[0] % Arguments[1];
        }
    }

    internal class DivNode : OperationNode
    {
        public override string NameLog => "div";

        public override int? NeededArgs => 2;
        public DivNode(List<Expression> _args)
        {
            Arguments = _args;
            CalculationCheck();
        }

        protected override BigInteger calculationLogic()
        {
            if ((BigInteger)Arguments[1] == 0)
                //throw new Exception("Cannot divide by 0");
                throw new Exception("Неможливо провести цілочисельне ділення на 0");
            return (BigInteger)Arguments[0] / Arguments[1];
        }
    }

    internal class NmaxNode : OperationNode
    {
        public override string NameLog => "nmax";

        public NmaxNode(List<Expression> _args)
        {
            Arguments = _args;
            CalculationCheck();
        }

        public override bool Ready() { return Arguments.Count > 0; }

        protected override BigInteger calculationLogic()
        {
            BigInteger result = Arguments[0];
            foreach (var arg in Arguments)
                result = BigInteger.Max(result, arg);

            return result;
        }
    }

    internal class NminNode : OperationNode
    {
        public override string NameLog => "nmin";

        public NminNode(List<Expression> _args)
        {
            Arguments = _args;
            CalculationCheck();
        }

        public override bool Ready() { return Arguments.Count > 0; }

        protected override BigInteger calculationLogic()
        {
            BigInteger result = Arguments[0];
            foreach (var arg in Arguments)
                result = BigInteger.Max(result, arg);

            return result;
        }
    }

    internal class PowerNode : OperationNode
    {
        public override string NameLog => "^";

        public override int Priority => 3;
        public override int? NeededArgs => 2;
        public override bool Ready() { return Arguments.Count > 0; }

        protected override BigInteger calculationLogic()
        {
            return BigInteger.Pow(Arguments[0], (int)(BigInteger)Arguments[1]);
        }
    }


    //internal class EqualNode : OperationNode
    //{
    //    public override string NameLog => "=";
    //    public override bool isLogicUI => true;

    //    public override int Priority => 3;
    //    public override int? NeededArgs => 2;

    //    protected override BigInteger calculationLogic()
    //    {
    //        return (BigInteger)Arguments[0] == Arguments[1]
    //            ? BigInteger.One : BigInteger.Zero;
    //    }
    //}
}
