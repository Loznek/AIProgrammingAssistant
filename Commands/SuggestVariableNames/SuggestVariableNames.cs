using AIProgrammingAssistant.AIConnection;
using AIProgrammingAssistant.Commands.Optimize;
using Community.VisualStudio.Toolkit;
using Community.VisualStudio.Toolkit.DependencyInjection;
using Community.VisualStudio.Toolkit.DependencyInjection.Core;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIProgrammingAssistant.Commands.SuggestVariableNames
{

    [Command(PackageIds.SuggestVariableNames)]
    public class SuggestVariableNames : BaseDICommand
    {
        private readonly IAIFunctions aiApi;
        public SuggestVariableNames(DIToolkitPackage package, IAIFunctions api) : base(package)
        {
            aiApi = api;
        }

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var activeDocument = await VS.Documents.GetActiveDocumentViewAsync();

            var selectedCode = activeDocument?.TextView.Selection.SelectedSpans.FirstOrDefault().GetText();
            var originalStart = activeDocument?.TextView.Selection.SelectedSpans.FirstOrDefault().Start;
            var originalEnd = activeDocument?.TextView.Selection.SelectedSpans.LastOrDefault().End;

            if (selectedCode != null)
            {
                var lines = selectedCode.Split('\n');
                var lastLine = lines[lines.Length - 1];
                var tabs = lastLine.Count(x => x == ' ');
                var wholeCode = activeDocument?.TextView.TextBuffer.CurrentSnapshot.GetText();
                string[] separator = new string[] { "####" }; ;
                string resultMessage = await aiApi.AskForVariableRevisionAsync( selectedCode.ToString(), wholeCode);
                var resultValues = resultMessage.Split(separator, System.StringSplitOptions.None);
                var message = resultValues[1].Replace("\n", "\n" + new string(' ', Math.Max(tabs - 5, 0)) + "//m> ");
                var goodCode = resultValues[0].Replace("\n", "\n" + new string(' ', Math.Max(tabs, 0)));
                var point = activeDocument.TextView.Selection.AnchorPoint;
                var edit = activeDocument.TextBuffer.CreateEdit();
                var position = activeDocument?.TextView.Selection.AnchorPoint.Position;
                edit.Insert(originalEnd.Value, message);
                edit.Apply();

                var optimizedEnd = activeDocument?.TextView.Selection.SelectedSpans.LastOrDefault().End;


                IVsTextManager2 textManager = (IVsTextManager2)ServiceProvider.GlobalProvider.GetService(typeof(SVsTextManager));
                textManager.GetActiveView2(1, null, (uint)_VIEWFRAMETYPE.vftCodeWindow, out IVsTextView activeView);
                SuggestVariableNamesHandler filter = new SuggestVariableNamesHandler(activeView, activeDocument, (SnapshotPoint)originalStart, (SnapshotPoint)originalEnd, (SnapshotPoint)optimizedEnd, goodCode);

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            }
        }
    }
}