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

        /// <summary>
        /// Collects the necessary information from the active document.
        /// </summary>
        public static async Task<ActiveDocumentProperties> GetActiveDocumentPropertiesAsync(this DocumentView activeDocument)
        {
            ActiveDocumentProperties properties = new ActiveDocumentProperties();
            var selectedCode = activeDocument?.TextView.Selection.SelectedSpans.FirstOrDefault();


            if (!selectedCode.HasValue || selectedCode.Value.IsEmpty)
            {
                await VS.MessageBox.ShowWarningAsync("AI Programming Assistant Warning", "No code selected");
                return null;
            };

            properties.SelectedCode = selectedCode?.GetText();
            properties.NumberOfStartingSpaces = (int)(selectedCode?.Start.GetContainingLine().GetText().TakeWhile(c => c == ' ').Count());
            properties.WholeCode = activeDocument?.TextView.TextBuffer.CurrentSnapshot.GetText();
            properties.OriginalStartPosition = (int)(activeDocument?.TextView.Selection.SelectedSpans.FirstOrDefault().Start.Position);
            properties.OriginalEndPosition = (int)(activeDocument?.TextView.Selection.SelectedSpans.LastOrDefault().End);
            return properties;
        }

        /// <summary>
        /// Inserts the suggestion to the active document.
        /// </summary>

        public static int InsertSuggestion(this DocumentView activeDocument, int startPosition, string suggestion)
        {
            using (var edit = activeDocument.TextBuffer.CreateEdit())
            {
                edit.Insert(startPosition, suggestion);
                edit.Apply();
            }
            return (int)(activeDocument?.TextView.Selection.SelectedSpans.LastOrDefault().End.Position);
        }


        /// <summary>
        /// Deletes the suggestion from the active document.
        /// </summary>

        public static void DeleteSuggestion(this DocumentView activeDocument, int startPosition, int endPosition)
        {
            using (var edit = activeDocument.TextBuffer.CreateEdit())
            {
                edit.Delete(new Span(startPosition, endPosition - startPosition));
                edit.Apply();
            }
            AIProgrammingAssistantPackage.dte.ExecuteCommand("Edit.FormatDocument");
        }


        /// <summary>
        /// Replaces the selected code with the suggestion.
        /// </summary>
        public static void EnforceSuggestion(this DocumentView activeDocument, int startPosition, int endPosition, string suggestion)
        {
            using (var edit = activeDocument.TextBuffer.CreateEdit())
            {
                edit.Replace(new Span(startPosition, endPosition - startPosition), suggestion);
                edit.Apply();
            }
            AIProgrammingAssistantPackage.dte.ExecuteCommand("Edit.FormatDocument");
        }


    }
}
