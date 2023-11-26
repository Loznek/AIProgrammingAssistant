using AIProgrammingAssistant.AIConnection;
using AIProgrammingAssistant.Commands.Optimize;
using Community.VisualStudio.Toolkit;
using Community.VisualStudio.Toolkit.DependencyInjection;
using Community.VisualStudio.Toolkit.DependencyInjection.Core;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace AIProgrammingAssistant.Commands.GiveFeedBack
{
    [Command(PackageIds.GiveFeedback)]
    public class GiveFeedback : BaseDICommand
    {
        private readonly IAIFunctions aiApi;
        public GiveFeedback(DIToolkitPackage package, IAIFunctions api) : base(package)
        {
            aiApi = api;
        }
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var activeDocument = await VS.Documents.GetActiveDocumentViewAsync();
            var selectedCode = "";
            selectedCode = activeDocument?.TextView.Selection.SelectedSpans.FirstOrDefault().GetText();
            var originalEnd = activeDocument?.TextView.Selection.SelectedSpans.LastOrDefault().End;

            if (selectedCode != null)
            {
                var lines = selectedCode.Split('\n');
                var lastLine = lines[lines.Length - 1];
                var tabs = lastLine.Count(x => x == ' ');
                var wholeCode = activeDocument?.TextView.TextBuffer.CurrentSnapshot.GetText();
                string suggestion = await aiApi.AskForFeedbackAsync(wholeCode, selectedCode.ToString());
                suggestion = suggestion.Replace("\n", "\n" + new string(' ', Math.Max(tabs - 5, 0)) + "//m> ");
                var point = activeDocument.TextView.Selection.AnchorPoint;
                var edit = activeDocument.TextBuffer.CreateEdit();
                var position = activeDocument?.TextView.Selection.AnchorPoint.Position;
                edit.Insert(originalEnd.Value, suggestion);
                edit.Apply();
            }


         
        }
    }
}
