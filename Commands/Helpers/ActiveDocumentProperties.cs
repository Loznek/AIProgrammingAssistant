using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIProgrammingAssistant.Commands.Helpers
{
    internal class ActiveDocumentProperties
    {
        public DocumentView ActiveDocument { get; set; }

        public string WholeCode { get; set; }
        public string SelectedCode { get; set; }
        public int NumberOfStartingSpaces { get; set; }
        
        public int OriginalStartPosition { get; set; }
        public int OriginalEndPosition { get; set; }
        public int OptimizedEndPosition { get; set; }
    }
}
