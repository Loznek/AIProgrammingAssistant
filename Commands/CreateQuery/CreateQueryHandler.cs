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

namespace AIProgrammingAssistant.Commands.CreateQuery
{
    internal class CreateQueryHandler : IOleCommandTarget
    {

        private readonly IVsTextView textView;
        private readonly DocumentView activeDocumentView;
        private readonly int originalStartPosition;
        private readonly int originalEndPosition;
        private int optimizedEndPosition;
        private readonly List<string> queries;
        private bool deleted = false;
        private IOleCommandTarget nextCommandTarget;
        private int queryIndex;

        public CreateQueryHandler(IVsTextView textView, DocumentView activeDocumentView, SnapshotPoint originalStartPoint, SnapshotPoint originalEndPoint, SnapshotPoint optimizedEndPoint, List<string> queries)
        {
            this.textView = textView ?? throw new ArgumentNullException(nameof(textView));
            this.queries = queries ?? throw new ArgumentNullException(nameof(queries));
            this.activeDocumentView = activeDocumentView ?? throw new ArgumentNullException(nameof(activeDocumentView));
            this.originalStartPosition = originalStartPoint.Position;
            this.originalEndPosition = originalEndPoint.Position;
            this.optimizedEndPosition = optimizedEndPoint.Position;
            queryIndex = 0;
            textView.AddCommandFilter(this, out nextCommandTarget);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string query = queries[queryIndex];
            if (!deleted && pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint)VSConstants.VSStd2KCmdID.BACKSPACE)
            {
                // User pressed delete, so delete the inserted text
                var edit = activeDocumentView.TextBuffer.CreateEdit();
                edit.Delete(new Span(originalEndPosition, optimizedEndPosition - originalEndPosition));
                edit.Apply();
                deleted = true;
                textView.RemoveCommandFilter(this);
                return VSConstants.S_OK;
            }
            else if (!deleted && pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint)VSConstants.VSStd2KCmdID.RETURN)
            {
                var edit = activeDocumentView.TextBuffer.CreateEdit();
                edit.Delete(new Span(originalStartPosition, optimizedEndPosition - originalStartPosition));
                edit.Insert(originalStartPosition, query.Replace(SuggestionLineSign.linq, "    "));
                edit.Apply();
                deleted = true;
                textView.RemoveCommandFilter(this);
                return VSConstants.S_OK;

            }
            else if (pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint)VSConstants.VSStd2KCmdID.RIGHT)
            {
                queryIndex++;
                if (queryIndex >= queries.Count) queryIndex = 0;
                var edit = activeDocumentView.TextBuffer.CreateEdit();
                edit.Delete(new Span(originalEndPosition, optimizedEndPosition - originalEndPosition));    
                edit.Insert(originalEndPosition, query);         
                edit.Apply();
                optimizedEndPosition = originalEndPosition + query.Count();
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
