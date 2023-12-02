﻿using AIProgrammingAssistant.AIConnection;
using AIProgrammingAssistant.Classification;
using AIProgrammingAssistant.Commands.Exceptions;
using AIProgrammingAssistant.Commands.Helpers;
using Azure;
using Community.VisualStudio.Toolkit;
using Community.VisualStudio.Toolkit.DependencyInjection;
using Community.VisualStudio.Toolkit.DependencyInjection.Core;
using EnvDTE;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Linq;
using ServiceProvider = Microsoft.VisualStudio.Shell.ServiceProvider;

namespace AIProgrammingAssistant.Commands.Optimize
{
    [Command(PackageIds.Optimize)]
    public class Optimize : BaseDICommand //BaseCommand<Optimize> 
    {
        private IAIFunctions aiApi;
  
        
        public Optimize(DIToolkitPackage package, IAIFunctions api) : base(package)
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


             
            string optmizedCode = await ApiCallHelper.HandleApiCallAsync(() => aiApi.AskForOptimizedCodeAsync(activeDocumentProperties.WholeCode, activeDocumentProperties.SelectedCode));
            if (optmizedCode == null) return;

            string optimizedCode;
            try
            {
                optimizedCode = await aiApi.AskForOptimizedCodeAsync(activeDocumentProperties.WholeCode, activeDocumentProperties.SelectedCode);
            }
            catch (InvalidKeyException keyException)
            {
                await VS.MessageBox.ShowWarningAsync("AI Programming Assistant Error", keyException.Message);
                TextInputDialog.Show("Worng OpenAI Api key was given", "You can change your API key", "key", out string keyString);
                AIProgrammingAssistantPackage.apiKey = keyString;
                return;
            }
            catch (AIApiException apiException)
            {

                await VS.MessageBox.ShowWarningAsync("AI Programming Assistant Warning", apiException.Message);
                return;
            }

            optimizedCode = optimizedCode.Replace("\n", "\n" + new string(' ', Math.Max(activeDocumentProperties.NumberOfStartingSpaces - 5, 0)) + SuggestionLineSign.optimization + " ");
            activeDocumentProperties.OptimizedEndPosition = DocumentHelper.insertSuggestion(activeDocumentProperties.ActiveDocument, activeDocumentProperties.OriginalEndPosition, optimizedCode);
            optimizedCode = optimizedCode.Replace(SuggestionLineSign.optimization, "    ");

            IVsTextManager2 textManager = (IVsTextManager2)ServiceProvider.GlobalProvider.GetService(typeof(SVsTextManager));
            textManager.GetActiveView2(1, null, (uint)_VIEWFRAMETYPE.vftCodeWindow, out IVsTextView activeView);
            new SimpleSuggestionHandler(activeView, activeDocumentProperties, optimizedCode);

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        }
    }
}
