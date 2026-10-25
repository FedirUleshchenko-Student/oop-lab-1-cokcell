using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace oop_lab_1_cokcell.Classes.Exceptions
{
    internal class SyntaxError : Exception
    {
        int? pos = null;
        int? pos2 = null;
        string? org = null;
        string mes;

        public SyntaxError(string message)
        {
            this.mes = message;
        }

        public SyntaxError(string message, string original, int errorPositon)
            : this(message)
        {
            this.pos = errorPositon;
            this.org = original;
        }

        public SyntaxError(string message, string original, int position1, int position2)
            : this(message, original, position1)
        {
            this.pos2 = position2;
        }

        public override string Message
        {
            get
            {
                if (pos.HasValue && pos2.HasValue && pos.Value < pos2.Value)
                    return $"{org}\n{new string(' ', pos.Value)}^" +
                            $"{new string('~', pos2.Value - pos.Value - 1)}^\n" +
                            $"{mes}\n";
                else if (pos.HasValue)
                    return $"{org}\n{new string(' ', pos.Value)}^\n{mes}\n";
                else
                    return $"{mes}\n";
            }
        }
    }
    //internal class TreeRecursionError : Exception
    //{

    //}
    //internal class InvalidArgument : Exception
    //{
    //    string? objname;
    //    string mes;

    //    public InvalidArgument(string nameObj, string message)
    //    {
    //        objname = nameObj;
    //    }

    //    public override string Message
    //    {
    //        get
    //        {
    //            return $"{objname} : {mes}";
    //        }
    //    }
    //}
}
