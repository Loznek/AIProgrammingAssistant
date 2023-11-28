using AIProgrammingAssistant.AIConnection;
using AIProgrammingAssistant.Classification;
using AIProgrammingAssistant.Commands.Exceptions;
using AIProgrammingAssistant.Commands.Optimize;
using Community.VisualStudio.Toolkit;
using Community.VisualStudio.Toolkit.DependencyInjection;
using Community.VisualStudio.Toolkit.DependencyInjection.Core;
using Microsoft.VisualStudio.PlatformUI;
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


                string suggestion = "";
                try
                {
                    suggestion = await aiApi.AskForFeedbackAsync(wholeCode, selectedCode.ToString());

                }
                catch (InvalidKeyException keyException)
                {
                    await VS.MessageBox.ShowWarningAsync("AI Programming Assistant Error", keyException.Message);
                        string keyString;
                        TextInputDialog.Show("Worng OpenAI Api key was given", "You can change your API key", "key", out keyString);
                        AIProgrammingAssistantPackage.apiKey = keyString;
                    return;

                }
                catch (AIApiException apiException) {
                    suggestion= apiException.Message;
                    suggestion = suggestion.Replace("\n", "\n" + new string(' ', Math.Max(tabs - 5, 0)) + SuggestionLineSign.message + " ");
                    var point1 = activeDocument.TextView.Selection.AnchorPoint;
                    var edit1 = activeDocument.TextBuffer.CreateEdit();
                    var position1 = activeDocument?.TextView.Selection.AnchorPoint.Position;
                    edit1.Insert(originalEnd.Value, suggestion);
                    edit1.Apply();
                    return;
                }
                




                suggestion = suggestion.Replace("\n", "\n" + new string(' ', Math.Max(tabs - 5, 0)) + SuggestionLineSign.message + " ");
                var point = activeDocument.TextView.Selection.AnchorPoint;
                var edit = activeDocument.TextBuffer.CreateEdit();
                var position = activeDocument?.TextView.Selection.AnchorPoint.Position;
                edit.Insert(originalEnd.Value, suggestion);
                edit.Apply();
            }


         
        }
    }
}
