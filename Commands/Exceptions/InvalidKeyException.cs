using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIProgrammingAssistant.Commands.Exceptions
{
    internal class InvalidKeyException : Exception
    {
        public InvalidKeyException() : base("Invalid Api Key was given")
        {
        }

        public InvalidKeyException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }   
}
