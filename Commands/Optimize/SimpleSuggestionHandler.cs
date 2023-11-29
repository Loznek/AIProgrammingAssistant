using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using EnvDTE;
using AIProgrammingAssistant.Commands.Helpers;

namespace AIProgrammingAssistant.Commands.Optimize
{
    internal class SimpleSuggestionHandler : IOleCommandTarget
    {
        private readonly IVsTextView textView;
        private ActiveDocumentProperties activeDocumentProperties;
        private readonly string insertedText;
        private IOleCommandTarget nextCommandTarget;

        public SimpleSuggestionHandler(IVsTextView textView, ActiveDocumentProperties activeDocumentProperties, string insertedText)
        {
            this.textView = textView ?? throw new ArgumentNullException(nameof(textView));
            this.insertedText = insertedText ?? throw new ArgumentNullException(nameof(insertedText));
            this.activeDocumentProperties = activeDocumentProperties ?? throw new ArgumentNullException(nameof(activeDocumentProperties));
            textView.AddCommandFilter(this, out nextCommandTarget);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if ( pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint)VSConstants.VSStd2KCmdID.BACKSPACE)
            {
                DocumentHelper.deleteSuggestion(activeDocumentProperties.ActiveDocument, activeDocumentProperties.OriginalEndPosition, activeDocumentProperties.OptimizedEndPosition);
                textView.RemoveCommandFilter(this);
                return VSConstants.S_OK;
            }
            else if (pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint)VSConstants.VSStd2KCmdID.RETURN)
            {
                DocumentHelper.enforceSuggestion(activeDocumentProperties.ActiveDocument, activeDocumentProperties.OriginalStartPosition, activeDocumentProperties.OptimizedEndPosition, insertedText);
                textView.RemoveCommandFilter(this);
                return VSConstants.S_OK;

            }
            // Allow the key event to be processed by other filters
            return nextCommandTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            // For other commands, delegate to the next command target in the chain
            return nextCommandTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText); ;
        }
    }
}
