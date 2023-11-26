using AIProgrammingAssistant.AIConnection;
using Community.VisualStudio.Toolkit;
using Community.VisualStudio.Toolkit.DependencyInjection;
using Community.VisualStudio.Toolkit.DependencyInjection.Core;
using EnvDTE80;
using Microsoft.VisualStudio.PlatformUI;
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
    public class CreateQuery : BaseDICommand
    {
        private readonly IAIFunctions aiApi;
        public CreateQuery(DIToolkitPackage package, IAIFunctions api) : base(package)
        {
            aiApi = api;
        }

        private static DTE2 _dte;

        private static string entitiesFolderPath;
        private static string dataContextFilePath;

        string schema = "";
        string context = "";

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
        
            var activeDocument = await VS.Documents.GetActiveDocumentViewAsync();
            var selectedCode = activeDocument?.TextView.Selection.SelectedSpans.FirstOrDefault();
            if (selectedCode.HasValue)
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                _dte = AIProgrammingAssistantPackage._dte;

                string dalDir="";
                var folderDialog = new FolderBrowserDialog();
                folderDialog.Description = "Select the database model directory!";
                folderDialog.SelectedPath= Directory.GetParent(_dte.Solution.FullName).FullName;
                DialogResult result = folderDialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    dalDir = folderDialog.SelectedPath;
                }

                string dbContextFileName;
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.InitialDirectory = Directory.GetParent(_dte.Solution.FullName).FullName;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    dbContextFileName = openFileDialog.FileName;          
                }
               
                TextInputDialog.Show("Database context file", "Enter the DBContext file name", "DbContext.cs", out dbContextFileName);

                var dir = Directory.GetParent(_dte.Solution.FullName);
                var files = dir.GetFiles("*.cs", SearchOption.AllDirectories);
                var dbContextFile = files.Where(f => f.FullName.Contains(dbContextFileName)).FirstOrDefault();
                var modelFiles = files.Where(f => f.Directory.FullName.Contains(dalDir)).ToList();
                modelFiles.ForEach(f => schema += getTextAsync(f.FullName).Result);
                context += getTextAsync(dbContextFile.FullName).Result;


                string query = await aiApi.AskForQueryAsync(selectedCode.ToString(), context, schema);
                var point = activeDocument.TextView.Selection.AnchorPoint;
                var edit = activeDocument.TextBuffer.CreateEdit();
                var position = activeDocument?.TextView.Selection.AnchorPoint.Position;
                edit.Insert(position.Value, "\n" + query);
                edit.Apply();

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            }
        }

        protected async Task<string> getTextAsync(string file)
        {
            await VS.Documents.OpenViaProjectAsync(file);
            var text = await VS.Documents.GetDocumentViewAsync(file);
            return text?.TextView.TextBuffer.CurrentSnapshot.GetText();
        }

    }
}
