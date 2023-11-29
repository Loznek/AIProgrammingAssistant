using AIProgrammingAssistant.AIConnection;
using AIProgrammingAssistant.Classification;
using AIProgrammingAssistant.Commands.Exceptions;
using AIProgrammingAssistant.Commands.Helpers;
using AIProgrammingAssistant.Commands.Optimize;
using Community.VisualStudio.Toolkit;
using Community.VisualStudio.Toolkit.DependencyInjection;
using Community.VisualStudio.Toolkit.DependencyInjection.Core;
using Microsoft.VisualStudio.PlatformUI;
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
            ActiveDocumentProperties activeDocumentProperties;
            try
            {
                activeDocumentProperties = await DocumentHelper.GetActiveDocumentPropertiesAsync();
            }
            catch (WrongSelectionException ex)
            {
                await VS.MessageBox.ShowWarningAsync("AI Programming Assistant Warning", ex.Message);
                return;
            }

            string[] separator = new string[] { "####" };

            string resultMessage;
            try
            {
                resultMessage = await aiApi.AskForVariableRevisionAsync(activeDocumentProperties.SelectedCode, activeDocumentProperties.WholeCode);
            }
            catch (InvalidKeyException keyException)
            {
                await VS.MessageBox.ShowWarningAsync("AI Programming Assistant Error", keyException.Message);
                string keyString;
                TextInputDialog.Show("Worng OpenAI Api key was given", "You can change your API key", "key", out keyString);
                AIProgrammingAssistantPackage.apiKey = keyString;
                return;

            }
            catch (AIApiException apiException)
            {
                resultMessage = apiException.Message;
                resultMessage = resultMessage.Replace("\n", "\n" + new string(' ', Math.Max(activeDocumentProperties.NumberOfStartingSpaces - 5, 0)) + SuggestionLineSign.message + " ");
                DocumentHelper.insertSuggestion(activeDocumentProperties.ActiveDocument, resultMessage);

                return;
            }

            

            var resultValues = resultMessage.Split(separator, StringSplitOptions.None);
            var message = resultValues[1].Replace("\n", "\n" + new string(' ', Math.Max(activeDocumentProperties.NumberOfStartingSpaces - 5, 0)) + SuggestionLineSign.message + " ");
            var goodCode = resultValues[0].Replace("\n", "\n" + new string(' ', Math.Max(activeDocumentProperties.NumberOfStartingSpaces, 0)));

            activeDocumentProperties.OriginalEndPosition = DocumentHelper.insertSuggestion(activeDocumentProperties.ActiveDocument, message);

            IVsTextManager2 textManager = (IVsTextManager2)ServiceProvider.GlobalProvider.GetService(typeof(SVsTextManager));
            textManager.GetActiveView2(1, null, (uint)_VIEWFRAMETYPE.vftCodeWindow, out IVsTextView activeView);
            new SimpleSuggestionHandler(activeView, activeDocumentProperties, goodCode);

        }
    }
}