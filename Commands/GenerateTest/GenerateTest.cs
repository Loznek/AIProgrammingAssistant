﻿using AIProgrammingAssistant.AIConnection;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using AIProgrammingAssistant.Helpers;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project = EnvDTE.Project;
using Community.VisualStudio.Toolkit;
using Community.VisualStudio.Toolkit.DependencyInjection.Core;
using Community.VisualStudio.Toolkit.DependencyInjection;
using System.Windows.Forms;
using static Azure.Core.HttpHeader;

namespace AIProgrammingAssistant.Commands.GenerateTest
{
    [Command(PackageIds.GenerateTest)]
    public class GenerateTest : BaseDICommand
    {
        private static string testFolderPath;
        

        private readonly IAIFunctions aiApi;
        public GenerateTest(DIToolkitPackage package, IAIFunctions api) : base(package)
        {
            aiApi = api;
        }

        private static DTE2 _dte;

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            
            _dte = AIProgrammingAssistantPackage._dte;
            var activeDocument = await VS.Documents.GetActiveDocumentViewAsync();

            var selectedCode = activeDocument?.TextView.Selection.SelectedSpans.FirstOrDefault();
            var wholeCode = activeDocument?.TextView.TextBuffer.CurrentSnapshot.GetText();
            if (selectedCode.HasValue)
            {
                FileInfo testFile = await AddFileAsync();
                var nameSpace=testFile.DirectoryName.Split('\\').Last();
                string testCode = await aiApi.AskForTestCodeAsync(selectedCode.ToString());

                activeDocument = await VS.Documents.GetActiveDocumentViewAsync();

                var edit = activeDocument.TextBuffer.CreateEdit();
                edit.Insert(0, testCode);
                edit.Apply();

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            }

            await VS.MessageBox.ShowWarningAsync("AIProgrammingAssistant", "Button clicked");
        }

        private async Task<FileInfo> AddFileAsync()
        {
            string testFileName;
            TextInputDialog.Show("Generate testfile", "Enter the name of the testfile ", "Testfile.cs", out testFileName);

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            string testDirectoryPath="";
            var folderDialog = new FolderBrowserDialog();
            folderDialog.Description = "Select the test directory!";
            folderDialog.SelectedPath = Directory.GetParent(_dte.Solution.FullName).FullName;

            DialogResult result = folderDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                testDirectoryPath = folderDialog.SelectedPath;
            }

            FileInfo testFile = new FileInfo(Path.Combine(testDirectoryPath, testFileName));
            
            // Make sure the directory exists before we create the file. Don't use
            // `PackageUtilities.EnsureOutputPath()` because it can silently fail.
            Directory.CreateDirectory(testFile.DirectoryName);


            if (!testFile.Exists)
            {
                Project testProject = _dte.Solution.FindProjectItem(_dte.ActiveDocument.FullName).ContainingProject;
                Projects projects = _dte.Solution.Projects;
                var enumerator = projects.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Project analyzedProject = (Project)enumerator.Current;
                    var root = analyzedProject.GetRootFolder().Substring(1, analyzedProject.GetRootFolder().Length-2);
                    var fname = analyzedProject.FullName;
                    if (testDirectoryPath.Contains(root))
                    {
                       testProject = analyzedProject;
                    }
                }

                await WriteFileAsync(testProject, testFile.FullName);

                ProjectItem item = testProject.AddFileToProject(testFile);
                testProject.ProjectItems.AddFromFile(testFile.FullName);

                ServiceProvider sp = new ServiceProvider((Microsoft.VisualStudio.OLE.Interop.IServiceProvider)_dte);
                VsShellUtilities.OpenDocument(sp, testFile.FullName);
                ExecuteCommandIfAvailable("SolutionExplorer.SyncWithActiveDocument");

                _dte.ActiveDocument.Activate();
                item.Document.Activate();
            }
            return testFile;

        }

        private void ExecuteCommandIfAvailable(string commandName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Command command;

            try
            {
                command = _dte.Commands.Item(commandName);
            }
            catch (ArgumentException)
            {
                // The command does not exist, so we can't execute it.
                return;
            }

            if (command.IsAvailable)
            {
                _dte.ExecuteCommand(commandName);
            }
        }

        private static async Task<int> WriteFileAsync(Project project, string file)
        {
            await WriteToDiskAsync(file, string.Empty);
            return 0;
        }

        private static async Task WriteToDiskAsync(string file, string content)
        {
            using (StreamWriter writer = new StreamWriter(file, false, new UTF8Encoding(true)))
            {
                await writer.WriteAsync(content);
            }
        }
    }
}
