using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIProgrammingAssistant.Classification;
using AIProgrammingAssistant.Commands.Helpers;

namespace AIProgrammingAssistant.Commands.CreateQuery
{
    internal class CreateQueryHandler : IOleCommandTarget
    {

        private readonly IVsTextView textView;
        private  ActiveDocumentProperties activeDocumentProperties;
        private readonly List<string> queries;
        private IOleCommandTarget nextCommandTarget;
        private DocumentView activeDocument;
        private string activeQuery;
        private int queryIndex;

        public CreateQueryHandler(IVsTextView textView, DocumentView activeDocument, ActiveDocumentProperties activeDocumentProperties, List<string> queries)
        {
            this.textView = textView ?? throw new ArgumentNullException(nameof(textView));
            this.queries = queries ?? throw new ArgumentNullException(nameof(queries));
            this.activeDocumentProperties = activeDocumentProperties ?? throw new ArgumentNullException(nameof(activeDocumentProperties));
            this.activeDocument = activeDocument ?? throw new ArgumentNullException(nameof(activeDocument));
            queryIndex = 0;
            activeQuery = queries[queryIndex];
            textView.AddCommandFilter(this, out nextCommandTarget);
        }

        /// <summary>
        /// Handles the key events that are passed to the command filter by the text view.
        /// At backspace or enter, the suggestion is deleted or inserted and then the command filter is removed.
        /// At right arrow, the next query is inserted.
        /// </summary>
        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            
            if (pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint)VSConstants.VSStd2KCmdID.BACKSPACE)
            {
                activeDocument.DeleteSuggestion( activeDocumentProperties.OriginalEndPosition, activeDocumentProperties.SuggestionEndPosition);
                textView.RemoveCommandFilter(this);
                return VSConstants.S_OK;
            }
            else if (pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint)VSConstants.VSStd2KCmdID.RETURN)
            {
                activeDocument.EnforceSuggestion(activeDocumentProperties.OriginalStartPosition, activeDocumentProperties.SuggestionEndPosition, activeQuery.Replace(SuggestionLineSign.linq, "    "));
                textView.RemoveCommandFilter(this);
                return VSConstants.S_OK;

            }
            else if (pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint)VSConstants.VSStd2KCmdID.RIGHT)
            {
                queryIndex++;
                if (queryIndex >= queries.Count) queryIndex = 0;
                activeQuery = queries[queryIndex];
                activeDocument.TextBuffer.Replace(new Span(activeDocumentProperties.OriginalEndPosition, activeDocumentProperties.SuggestionEndPosition - activeDocumentProperties.OriginalEndPosition), activeQuery);
                activeDocumentProperties.SuggestionEndPosition = activeDocumentProperties.OriginalEndPosition + activeQuery.Count();
            }

            return nextCommandTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return nextCommandTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText); ;
        }
    }
}
