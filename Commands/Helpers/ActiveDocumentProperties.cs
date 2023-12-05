using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIProgrammingAssistant.Commands.Helpers
{

    /// <summary>
    /// A class that holds the relevant properties of the active document.
    /// </summary>
    internal class ActiveDocumentProperties
    {

        public string WholeCode { get; set; }
        public string SelectedCode { get; set; }
        public int NumberOfStartingSpaces { get; set; }
        
        public int OriginalStartPosition { get; set; }
        public int OriginalEndPosition { get; set; }
        public int SuggestionEndPosition { get; set; }
    }
}
