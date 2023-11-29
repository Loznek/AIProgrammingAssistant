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
        private int queryIndex;

        public CreateQueryHandler(IVsTextView textView, ActiveDocumentProperties activeDocumentProperties, List<string> queries)
        {
            this.textView = textView ?? throw new ArgumentNullException(nameof(textView));
            this.queries = queries ?? throw new ArgumentNullException(nameof(queries));
            this.activeDocumentProperties = activeDocumentProperties ?? throw new ArgumentNullException(nameof(activeDocumentProperties));
            queryIndex = 0;
            textView.AddCommandFilter(this, out nextCommandTarget);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string query = queries[queryIndex];
            if (pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint)VSConstants.VSStd2KCmdID.BACKSPACE)
            {
                DocumentHelper.deleteSuggestion(activeDocumentProperties.ActiveDocument, activeDocumentProperties.OriginalEndPosition, activeDocumentProperties.OptimizedEndPosition);
                textView.RemoveCommandFilter(this);
                return VSConstants.S_OK;
            }
            else if (pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint)VSConstants.VSStd2KCmdID.RETURN)
            {
                DocumentHelper.enforceSuggestion(activeDocumentProperties.ActiveDocument, activeDocumentProperties.OriginalStartPosition, activeDocumentProperties.OptimizedEndPosition, query.Replace(SuggestionLineSign.linq, "    "));
                textView.RemoveCommandFilter(this);
                return VSConstants.S_OK;

            }
            else if (pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint)VSConstants.VSStd2KCmdID.RIGHT)
            {
                queryIndex++;
                if (queryIndex >= queries.Count) queryIndex = 0;
                DocumentHelper.enforceSuggestion(activeDocumentProperties.ActiveDocument, activeDocumentProperties.OriginalStartPosition, activeDocumentProperties.OptimizedEndPosition, query); 
                activeDocumentProperties.OptimizedEndPosition = activeDocumentProperties.OriginalEndPosition + query.Count();
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
