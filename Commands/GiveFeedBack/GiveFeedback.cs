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
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace AIProgrammingAssistant.Commands.GiveFeedBack
{
    [Command(PackageIds.GiveFeedback)]
    public class GiveFeedback : BaseDICommand //BaseCommand<GiveFeedback> 
    {
        private IAIFunctions aiApi;

        
        public GiveFeedback(DIToolkitPackage package, IAIFunctions api) : base(package)
        {
            aiApi = api;
        }
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
          
            ActiveDocumentProperties activeDocumentProperties = await DocumentHelper.GetActiveDocumentPropertiesAsync();
            if (activeDocumentProperties == null) return;

            string feedback = await ApiCallHelper.HandleApiCallAsync(() => aiApi.AskForFeedbackAsync(activeDocumentProperties.WholeCode, activeDocumentProperties.SelectedCode));
            if (feedback == null) return;

            feedback = feedback.Replace("\n", "\n" + new string(' ', Math.Max(activeDocumentProperties.NumberOfStartingSpaces - 5, 0)) + SuggestionLineSign.message + " ");
            DocumentHelper.insertSuggestion(activeDocumentProperties.ActiveDocument, activeDocumentProperties.OriginalEndPosition, feedback);

        }



    }
}

