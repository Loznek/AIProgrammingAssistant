using AIProgrammingAssistant.AIConnection;
using AIProgrammingAssistant.Classification;
using Azure;
using Community.VisualStudio.Toolkit;
using Community.VisualStudio.Toolkit.DependencyInjection;
using Community.VisualStudio.Toolkit.DependencyInjection.Core;
using Community.VisualStudio.Toolkit.DependencyInjection.Microsoft;
using Microsoft.VisualStudio.PlatformUI;
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
                string goodCode = "";
                try
                {
                    goodCode = await aiApi.AskForOptimizedCodeAsync(wholeCode, selectedCode.ToString());
                }
                catch (RequestFailedException ex)
                {
                    await VS.MessageBox.ShowWarningAsync("AI Programming Assistant Error", new string(ex.Message.TakeWhile(c => c != '\r').ToArray()) + " \r\nError code: " + ex.ErrorCode);
                    if (ex.ErrorCode.Equals("invalid_api_key"))
                    {
                        string keyString;
                        TextInputDialog.Show("Api Key", "You can change your API key", "key", out keyString);
                        AIProgrammingAssistantPackage.apiKey = keyString;
                    }
                    return;
                }
                catch (AggregateException ex) {
                    await VS.MessageBox.ShowWarningAsync("AI Programming Assistant Error", ex.InnerException.Message + "\r\nThe problem is might be with your internet connection.");
                    
                };
               
                goodCode = goodCode.Replace("\n", "\n" + new string(' ', Math.Max(tabs - 5, 0)) + SuggestionLineSign.optimization + " ");
                var point = activeDocument.TextView.Selection.AnchorPoint;
                var edit = activeDocument.TextBuffer.CreateEdit();
                var position = activeDocument?.TextView.Selection.AnchorPoint.Position;
                edit.Insert(originalEnd.Value, goodCode);
                edit.Apply();

                var optimizedEnd = activeDocument?.TextView.Selection.SelectedSpans.LastOrDefault().End;

                goodCode = goodCode.Replace(SuggestionLineSign.optimization, "    ");

                IVsTextManager2 textManager = (IVsTextManager2)ServiceProvider.GlobalProvider.GetService(typeof(SVsTextManager));
                textManager.GetActiveView2(1, null, (uint)_VIEWFRAMETYPE.vftCodeWindow, out IVsTextView activeView);
                OptimizeHandler filter = new OptimizeHandler(activeView, activeDocument, (SnapshotPoint)originalStart, (SnapshotPoint)originalEnd, (SnapshotPoint)optimizedEnd, goodCode);

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();


            }

        }
    }
}
