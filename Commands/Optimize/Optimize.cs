using AIProgrammingAssistant.AIConnection;
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
using System.Diagnostics;
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


        /// <summary>
        /// Executes the Optimize command when the menu item is clicked.
        /// Get the selected code and send it to the API. 
        /// Then insert the suggestion into the active document.
        /// </summary>
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            DocumentView activeDocument = await VS.Documents.GetActiveDocumentViewAsync();
            ActiveDocumentProperties activeDocumentProperties = await activeDocument.GetActiveDocumentPropertiesAsync();
            if (activeDocumentProperties == null) return;

            string optimizedCode = await ApiCallHelper.HandleApiCallAsync(() => aiApi.AskForOptimizedCodeAsync(activeDocumentProperties.WholeCode, activeDocumentProperties.SelectedCode));
            if (optimizedCode == null) return;
            optimizedCode = optimizedCode.Replace("\n", "\n" + new string(' ', Math.Max(activeDocumentProperties.NumberOfStartingSpaces - 5, 0)) + SuggestionLineSign.optimization + " ");
            activeDocumentProperties.SuggestionEndPosition = activeDocument.InsertSuggestion(activeDocumentProperties.OriginalEndPosition, optimizedCode);
            optimizedCode = optimizedCode.Replace(SuggestionLineSign.optimization, "    ");


            IVsTextManager2 textManager = ServiceProvider.GlobalProvider.GetService(typeof(SVsTextManager)) as IVsTextManager2;
            textManager.GetActiveView2(1, null, (uint)_VIEWFRAMETYPE.vftCodeWindow, out IVsTextView activeView);
            new SimpleSuggestionHandler(activeView,activeDocument, activeDocumentProperties, optimizedCode);

        }
    }
}
