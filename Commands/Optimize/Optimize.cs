using AIProgrammingAssistant.AIConnection;
using Community.VisualStudio.Toolkit;
using Community.VisualStudio.Toolkit.DependencyInjection;
using Community.VisualStudio.Toolkit.DependencyInjection.Core;
using Community.VisualStudio.Toolkit.DependencyInjection.Microsoft;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIProgrammingAssistant.Commands.Optimize
{
    [Command(PackageIds.Optimize)]
    public class Optimize : BaseDICommand
    {
        private readonly IAIFunctions aiApi;
        public Optimize(DIToolkitPackage package, IAIFunctions api) : base(package)
        {
            aiApi = api;
        }

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var activeDocument = await VS.Documents.GetActiveDocumentViewAsync();

            var selectedCode = "";
            var spans = activeDocument?.TextView.Selection.SelectedSpans;
            var originalStart = activeDocument?.TextView.Selection.SelectedSpans.FirstOrDefault().Start;
            var originalEnd = activeDocument?.TextView.Selection.SelectedSpans.LastOrDefault().End;

            foreach (var selected in spans)
            {
                selectedCode += selected.GetText();
            }
            if (selectedCode != null)
            {
                var lines = selectedCode.Split('\n');
                var lastLine = lines[lines.Length - 1];
                var tabs = lastLine.Count(x => x == ' ');
                var wholeCode = activeDocument?.TextView.TextBuffer.CurrentSnapshot.GetText();
                string goodCode = await aiApi.AskForOptimizedCodeAsync(wholeCode, selectedCode.ToString());
                goodCode = goodCode.Replace("\n", "\n" + new string(' ', Math.Max(tabs - 5, 0)) + "//o> ");
                var point = activeDocument.TextView.Selection.AnchorPoint;
                var edit = activeDocument.TextBuffer.CreateEdit();
                var position = activeDocument?.TextView.Selection.AnchorPoint.Position;
                edit.Insert(originalEnd.Value, goodCode);
                edit.Apply();

                var optimizedEnd = activeDocument?.TextView.Selection.SelectedSpans.LastOrDefault().End;

                goodCode = goodCode.Replace("//o> ", "     ");

                var span = new Span();
                IVsTextManager2 textManager = (IVsTextManager2)ServiceProvider.GlobalProvider.GetService(typeof(SVsTextManager));
                textManager.GetActiveView2(1, null, (uint)_VIEWFRAMETYPE.vftCodeWindow, out IVsTextView activeView);
                OptimizeHandler filter = new OptimizeHandler(activeView, activeDocument, (SnapshotPoint)originalStart, (SnapshotPoint)originalEnd, (SnapshotPoint)optimizedEnd, goodCode);
                activeView.AddCommandFilter(filter, out _);

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();


            }

        }
    }
}
