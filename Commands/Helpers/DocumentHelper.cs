using AIProgrammingAssistant.Commands.Exceptions;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIProgrammingAssistant.Commands.Helpers
{
    internal static class DocumentHelper
    {
        public static async Task<ActiveDocumentProperties> GetActiveDocumentPropertiesAsync() 
        {
            ActiveDocumentProperties properties = new ActiveDocumentProperties();
            var activeDocument = await VS.Documents.GetActiveDocumentViewAsync();
            properties.ActiveDocument = activeDocument;
            var selectedCode = activeDocument?.TextView.Selection.SelectedSpans.FirstOrDefault();
            if (!selectedCode.HasValue) throw new WrongSelectionException();
            properties.SelectedCode = selectedCode?.GetText();
            properties.NumberOfStartingSpaces = (int)(selectedCode?.Start.GetContainingLine().GetText().TakeWhile(c => c == ' ').Count());
            properties.WholeCode = activeDocument?.TextView.TextBuffer.CurrentSnapshot.GetText();
            properties.OriginalStartPosition = (int)(activeDocument?.TextView.Selection.SelectedSpans.FirstOrDefault().Start.Position);
            properties.OriginalEndPosition = (int)(activeDocument?.TextView.Selection.SelectedSpans.LastOrDefault().End);
            return properties;
        }

        public static int insertSuggestion(DocumentView activeDocument, string suggestion) {
            var edit = activeDocument.TextBuffer.CreateEdit();
            var position = activeDocument?.TextView.Selection.AnchorPoint.Position;
            edit.Insert(position.Value, suggestion);
            edit.Apply();
            AIProgrammingAssistantPackage._dte.ExecuteCommand("Edit.FormatDocument");
            return (int)(activeDocument?.TextView.Selection.SelectedSpans.LastOrDefault().End.Position);
        }

        public static void deleteSuggestion(DocumentView activeDocument, int startPosition, int endPosition)
        {
            var edit = activeDocument.TextBuffer.CreateEdit();
            edit.Delete(new Span(startPosition, endPosition - startPosition));
            edit.Apply();
            AIProgrammingAssistantPackage._dte.ExecuteCommand("Edit.FormatDocument");
        }

        public static void enforceSuggestion(DocumentView activeDocument, int startPosition, int endPosition, string suggestion)
        {
            var edit = activeDocument.TextBuffer.CreateEdit();
            edit.Delete(new Span(startPosition, endPosition - startPosition));
            edit.Insert(startPosition, suggestion);
            edit.Apply();
            AIProgrammingAssistantPackage._dte.ExecuteCommand("Edit.FormatDocument");
        }
    }
}
