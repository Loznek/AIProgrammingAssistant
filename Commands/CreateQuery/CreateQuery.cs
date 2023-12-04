﻿using AIProgrammingAssistant.AIConnection;
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AIProgrammingAssistant.Commands.CreateQuery
{
    [Command(PackageIds.CreateQuery)]
    public class CreateQuery :  BaseDICommand
    {
        private IAIFunctions aiApi;
        private static string entitiesFolderPath;
        private static string dbContextFilePath;


        public CreateQuery(DIToolkitPackage package, IAIFunctions api) : base(package)
        {
            aiApi = api;
        }

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
           
            DTE2 _dte = AIProgrammingAssistantPackage.dte;
            DocumentView activeDocument = await VS.Documents.GetActiveDocumentViewAsync();
            ActiveDocumentProperties activeDocumentProperties = await activeDocument.GetActiveDocumentPropertiesAsync();
            if (activeDocumentProperties == null) return;

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
            List<string> rawQueries = await ApiCallHelper.HandleApiCallAsync(() => aiApi.AskForQueryAsync(activeDocumentProperties.SelectedCode, activeDocumentProperties.WholeCode, context, schema));
            if (rawQueries == null) return;

            rawQueries.ForEach(q => queries.Add(q.Replace("\n", "\n" + new string(' ', Math.Max(activeDocumentProperties.NumberOfStartingSpaces - 5, 0)) + SuggestionLineSign.linq +" ")));
            activeDocumentProperties.SuggestionEndPosition= activeDocument.InsertSuggestion(activeDocumentProperties.OriginalEndPosition, queries[0]);

           

            IVsTextManager2 textManager = ServiceProvider.GlobalProvider.GetService(typeof(SVsTextManager)) as IVsTextManager2;
            textManager.GetActiveView2(1, null, (uint)_VIEWFRAMETYPE.vftCodeWindow, out IVsTextView activeView);
            new CreateQueryHandler(activeView, activeDocument, activeDocumentProperties ,queries);
        }

    }
}
