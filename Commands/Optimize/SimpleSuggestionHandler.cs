using AIProgrammingAssistant.Commands.Helpers;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace AIProgrammingAssistant.Commands.Optimize
{
    internal class SimpleSuggestionHandler : IOleCommandTarget
    {
        private readonly IVsTextView textView;
        private ActiveDocumentProperties activeDocumentProperties;
        private DocumentView activeDocument;
        private readonly string insertedText;
        private IOleCommandTarget nextCommandTarget;

        public SimpleSuggestionHandler(IVsTextView textView,DocumentView activeDocument, ActiveDocumentProperties activeDocumentProperties, string insertedText)
        {
            this.textView = textView ?? throw new ArgumentNullException(nameof(textView));
            this.insertedText = insertedText ?? throw new ArgumentNullException(nameof(insertedText));
            this.activeDocument = activeDocument ?? throw new ArgumentNullException(nameof(activeDocument));
            this.activeDocumentProperties = activeDocumentProperties ?? throw new ArgumentNullException(nameof(activeDocumentProperties));
            textView.AddCommandFilter(this, out nextCommandTarget);
        }

        /// <summary>
        /// Handles the key events that are passed to the command filter by the text view.
        /// At backspace or enter, the suggestion is deleted or inserted and then the command filter is removed.
        /// </summary>
        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if ( pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint)VSConstants.VSStd2KCmdID.BACKSPACE)
            {
                activeDocument.DeleteSuggestion(activeDocumentProperties.OriginalEndPosition, activeDocumentProperties.SuggestionEndPosition);
                textView.RemoveCommandFilter(this);
                return VSConstants.S_OK;
            }
            else if (pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint)VSConstants.VSStd2KCmdID.RETURN)
            {
                activeDocument.EnforceSuggestion(activeDocumentProperties.OriginalStartPosition, activeDocumentProperties.SuggestionEndPosition, insertedText);
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
