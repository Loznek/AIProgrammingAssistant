using AIProgrammingAssistant.AIConnection;
using AIProgrammingAssistant.Classification;
using AIProgrammingAssistant.Commands.Exceptions;
using AIProgrammingAssistant.Commands.Helpers;
using AIProgrammingAssistant.Commands.Optimize;
using Community.VisualStudio.Toolkit;
using Community.VisualStudio.Toolkit.DependencyInjection;
using Community.VisualStudio.Toolkit.DependencyInjection.Core;
using EnvDTE80;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AIProgrammingAssistant.Commands.CreateQuery
{
    [Command(PackageIds.CreateQuery)]
    public class CreateQuery :  BaseDICommand  ////BaseCommand<CreateQuery>
    {
        private IAIFunctions aiApi;
        
        
        public CreateQuery(DIToolkitPackage package, IAIFunctions api) : base(package)
        {
            aiApi = api;
        }

        private static DTE2 _dte;
        private static string entitiesFolderPath;
        private static string dbContextFilePath;

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            aiApi = new AzureApi();
            _dte = AIProgrammingAssistantPackage._dte;
            ActiveDocumentProperties activeDocumentProperties;
            try
            {
                activeDocumentProperties = await DocumentHelper.GetActiveDocumentPropertiesAsync();
            }
            catch (WrongSelectionException ex) {
                await VS.MessageBox.ShowWarningAsync("AI Programming Assistant Warning", ex.Message);
                return;
            }
            
            if (entitiesFolderPath == null)
            {
                var folderDialog = new FolderBrowserDialog();
                folderDialog.Description = "Select the database model directory!";
                folderDialog.SelectedPath = Directory.GetParent(_dte.Solution.FullName).FullName;
                DialogResult result = folderDialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    entitiesFolderPath = folderDialog.SelectedPath;
                }
                else return;
            }

            var directory = new DirectoryInfo(entitiesFolderPath);
            var modelFiles = directory.GetFiles("*.cs", SearchOption.AllDirectories).ToList();

            if (dbContextFilePath == null)
            {
                FileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.InitialDirectory = Directory.GetParent(_dte.Solution.FullName).FullName;
                openFileDialog.Title = "Select the database context file! Next time you will be able to use the function!";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    dbContextFilePath = openFileDialog.FileName;
                }
                else return;
            }

            FileInfo dbContextFile = new FileInfo(dbContextFilePath);
 
            string schema = "";
            string context = await dbContextFile.OpenText().ReadToEndAsync();
            foreach (FileInfo file in modelFiles)
            {
                schema += await file.OpenText().ReadToEndAsync();
            }
            List<string> queries = new List<string>();
            List<string> rawQueries;

            try
            {
                rawQueries = await aiApi.AskForQueryAsync(activeDocumentProperties.SelectedCode,activeDocumentProperties.WholeCode, context, schema);
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
            
            rawQueries.ForEach(q => queries.Add(q.Replace("\n", "\n" + new string(' ', Math.Max(activeDocumentProperties.NumberOfStartingSpaces - 5, 0)) + SuggestionLineSign.linq +" ")));
            activeDocumentProperties.OptimizedEndPosition= DocumentHelper.insertSuggestion(activeDocumentProperties.ActiveDocument, activeDocumentProperties.OriginalEndPosition, queries[0]);
         
            IVsTextManager2 textManager = (IVsTextManager2)ServiceProvider.GlobalProvider.GetService(typeof(SVsTextManager));
            textManager.GetActiveView2(1, null, (uint)_VIEWFRAMETYPE.vftCodeWindow, out IVsTextView activeView);
            new CreateQueryHandler(activeView, activeDocumentProperties ,queries);
        }

        protected async Task<string> getTextAsync(string file)
        {
            await VS.Documents.OpenViaProjectAsync(file);
            var text = await VS.Documents.GetDocumentViewAsync(file);
            return text?.TextView.TextBuffer.CurrentSnapshot.GetText();
        }

    }
}
