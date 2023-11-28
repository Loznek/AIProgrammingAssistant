using AIProgrammingAssistant.AIConnection;
using AIProgrammingAssistant.Classification;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace AIProgrammingAssistant.Commands.CreateQuery
{
    [Command(PackageIds.CreateQuery)]
    public class CreateQuery : BaseDICommand
    {
        private readonly IAIFunctions aiApi;
        public CreateQuery(DIToolkitPackage package, IAIFunctions api) : base(package)
        {
            aiApi = api;
        }

        private static DTE2 _dte;
        private static string entitiesFolderPath;
        private static string dbContextFilePath;

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {

            var activeDocument = await VS.Documents.GetActiveDocumentViewAsync();
            var selectedCode = activeDocument?.TextView.Selection.SelectedSpans.FirstOrDefault();

            if (!selectedCode.HasValue)
            {
                await VS.MessageBox.ShowWarningAsync("AI Programming Assistant", "Please select a method!");
                return;
            }

            var start = activeDocument?.TextView.Selection.SelectedSpans.FirstOrDefault().Start.GetContainingLine().GetText();
            var numberOfStartingSpaces = start.TakeWhile(c => c == ' ').Count();
            var wholeCode = activeDocument?.TextView.TextBuffer.CurrentSnapshot.GetText();
            var originalStart = activeDocument?.TextView.Selection.SelectedSpans.FirstOrDefault().Start;
            var originalEnd = activeDocument?.TextView.Selection.SelectedSpans.LastOrDefault().End;


           

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            _dte = AIProgrammingAssistantPackage._dte;


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
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.InitialDirectory = Directory.GetParent(_dte.Solution.FullName).FullName;
                openFileDialog.Title = "Select the database context file!";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    dbContextFilePath = openFileDialog.FileName;
                }
                else return;
            }
            FileInfo dbContextFile = new FileInfo(dbContextFilePath);
            string schema = "";
            string context = "";
            modelFiles.ForEach(f => schema += getTextAsync(f.FullName).Result);
            context += await getTextAsync(dbContextFile.FullName);
            List<string> queries = new List<string>();
           
            List<string> rawQueries = await aiApi.AskForQueryAsync(selectedCode?.GetText(), context, schema);
            rawQueries.ForEach(q => queries.Add(q.Replace("\n", "\n" + new string(' ', Math.Max(numberOfStartingSpaces - 5, 0)) + SuggestionLineSign.linq +" ")));



            var point = activeDocument.TextView.Selection.AnchorPoint;
            var edit = activeDocument.TextBuffer.CreateEdit();
            var position = activeDocument?.TextView.Selection.AnchorPoint.Position;
            edit.Insert(position.Value, "\n" + queries[0]);
            edit.Apply();
            var optimizedEnd = activeDocument?.TextView.Selection.SelectedSpans.LastOrDefault().End;


            IVsTextManager2 textManager = (IVsTextManager2)ServiceProvider.GlobalProvider.GetService(typeof(SVsTextManager));
            textManager.GetActiveView2(1, null, (uint)_VIEWFRAMETYPE.vftCodeWindow, out IVsTextView activeView);
            new CreateQueryHandler(activeView, activeDocument, (SnapshotPoint)originalStart, (SnapshotPoint)originalEnd, (SnapshotPoint)optimizedEnd, queries);


            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        }

        protected async Task<string> getTextAsync(string file)
        {
            await VS.Documents.OpenViaProjectAsync(file);
            var text = await VS.Documents.GetDocumentViewAsync(file);
            return text?.TextView.TextBuffer.CurrentSnapshot.GetText();
        }

    }
}
