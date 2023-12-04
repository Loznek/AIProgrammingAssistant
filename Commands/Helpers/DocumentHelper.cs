using AIProgrammingAssistant.Commands.Exceptions;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.PlatformUI;
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
        public static async Task<ActiveDocumentProperties> GetActiveDocumentPropertiesAsync(this DocumentView activeDocument) 
        {
            ActiveDocumentProperties properties = new ActiveDocumentProperties();
            //var activeDocument = await VS.Documents.GetActiveDocumentViewAsync();
            //properties.ActiveDocument = activeDocument;
            var selectedCode = activeDocument?.TextView.Selection.SelectedSpans.FirstOrDefault();


            if (!selectedCode.HasValue || selectedCode.Value.IsEmpty) {
                await VS.MessageBox.ShowWarningAsync("AI Programming Assistant Warning", "No code selected");
                //VS.MessageBox.ShowErrorAsync
                return null;
            };

            properties.SelectedCode = selectedCode?.GetText();
            properties.NumberOfStartingSpaces = (int)(selectedCode?.Start.GetContainingLine().GetText().TakeWhile(c => c == ' ').Count());
            properties.WholeCode = activeDocument?.TextView.TextBuffer.CurrentSnapshot.GetText();
            properties.OriginalStartPosition = (int)(activeDocument?.TextView.Selection.SelectedSpans.FirstOrDefault().Start.Position);
            properties.OriginalEndPosition = (int)(activeDocument?.TextView.Selection.SelectedSpans.LastOrDefault().End);
            return properties;
        }

        public static int InsertSuggestion(this DocumentView activeDocument, int startPosition, string suggestion) {
            var edit = activeDocument.TextBuffer.CreateEdit();
            edit.Insert(startPosition, suggestion);
            edit.Apply();
            return (int)(activeDocument?.TextView.Selection.SelectedSpans.LastOrDefault().End.Position);
        }
   //o> public static int InsertSuggestion(this DocumentView activeDocument, int startPosition, string suggestion) 
   //o> {
   //o>     using (var edit = activeDocument.TextBuffer.CreateEdit())
   //o>     {
   //o>         edit.Insert(startPosition, suggestion);
   //o>         edit.Apply();
   //o>     }
   //o>     return (int)(activeDocument?.TextView.Selection.SelectedSpans.LastOrDefault().End.Position);
   //o> }
   //o> 

        public static void DeleteSuggestion(this DocumentView activeDocument, int startPosition, int endPosition)
        {
            var edit = activeDocument.TextBuffer.CreateEdit();
            edit.Delete(new Span(startPosition, endPosition - startPosition));
            edit.Apply();
            AIProgrammingAssistantPackage.dte.ExecuteCommand("Edit.FormatDocument");
        }

        public static void EnforceSuggestion(this DocumentView activeDocument, int startPosition, int endPosition, string suggestion)
        {
            var edit = activeDocument.TextBuffer.CreateEdit();
            edit.Delete(new Span(startPosition, endPosition - startPosition));
            edit.Insert(startPosition, suggestion);
            edit.Apply();
            AIProgrammingAssistantPackage.dte.ExecuteCommand("Edit.FormatDocument");
        }
  
    }
}
