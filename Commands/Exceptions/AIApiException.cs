using AIProgrammingAssistant.Classification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIProgrammingAssistant.Commands.Exceptions
{
    internal class AIApiException : Exception
    {
        public AIApiException(string message)
            : base(message)
        {
        }

        public AIApiException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
