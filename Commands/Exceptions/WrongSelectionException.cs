using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIProgrammingAssistant.Commands.Exceptions
{
    internal class WrongSelectionException : Exception
    {
        public WrongSelectionException() : base("No code selected!")
        {
        }

        public WrongSelectionException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
