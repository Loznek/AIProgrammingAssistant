using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIProgrammingAssistant.Commands.SuggestVariableNames
{
    internal class SuggestVariableNamesHandler : IOleCommandTarget
    {

        private readonly IVsTextView textView;
        private readonly DocumentView activeDocumentView;
        private readonly SnapshotPoint originalStartPoint;
        private readonly SnapshotPoint originalEndPoint;
        private readonly SnapshotPoint optimizedEndPoint;
        private readonly string insertedText;
        private bool deleted = false;
        private IOleCommandTarget nextCommandTarget;

        public SuggestVariableNamesHandler(IVsTextView textView, DocumentView activeDocumentView, SnapshotPoint originalStartPoint, SnapshotPoint originalEndPoint, SnapshotPoint optimizedEndPoint, string insertedText)
        {
            this.textView = textView ?? throw new ArgumentNullException(nameof(textView));
            this.insertedText = insertedText ?? throw new ArgumentNullException(nameof(insertedText));
            this.activeDocumentView = activeDocumentView ?? throw new ArgumentNullException(nameof(activeDocumentView));
            this.originalStartPoint = originalStartPoint;
            this.originalEndPoint = originalEndPoint;
            this.optimizedEndPoint = optimizedEndPoint;
            textView.AddCommandFilter(this, out nextCommandTarget);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (!deleted && pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint)VSConstants.VSStd2KCmdID.BACKSPACE)
            {
                // User pressed Enter, so delete the inserted text
                var edit = activeDocumentView.TextBuffer.CreateEdit();
                edit.Delete(new Span(originalEndPoint.Position, optimizedEndPoint.Position - originalEndPoint.Position));
                edit.Apply();
                deleted = true;
                textView.RemoveCommandFilter(this);
                return VSConstants.S_OK;
            }
            else if (!deleted && pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint)VSConstants.VSStd2KCmdID.RETURN)
            {
                var edit = activeDocumentView.TextBuffer.CreateEdit();
                edit.Delete(new Span(originalStartPoint.Position, optimizedEndPoint.Position - originalStartPoint.Position));
                edit.Insert(originalStartPoint, insertedText);
                edit.Apply();
                deleted = true;
                textView.RemoveCommandFilter(this);
                return VSConstants.S_OK;

            }
            return nextCommandTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            // For other commands, delegate to the next command target in the chain
            return nextCommandTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText); ;
        }
    }
}
