using AIProgrammingAssistant.AIConnection;
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
using AIProgrammingAssistant.Commands.Helpers;
using AIProgrammingAssistant.Commands.Exceptions;
using AIProgrammingAssistant.Classification;
using System.Diagnostics;

namespace AIProgrammingAssistant.Commands.GenerateTest
{
    [Command(PackageIds.GenerateTest)]
    public class GenerateTest : BaseDICommand
    {
        private IAIFunctions aiApi;

        public GenerateTest(DIToolkitPackage package, IAIFunctions api) : base(package)
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

            TestFileInfo testFileInfo = new TestFileInfo();
            string testFileName;
            bool userInsertedFileName=TextInputDialog.Show("Generate testfile", "Enter the name of the testfile ", "Testfile.cs", out testFileName);
            if (!userInsertedFileName) return;
            testFileInfo.ClassName = testFileName.Substring(0, testFileName.Length - 3);


            string testDirectoryPath = "";
            var folderDialog = new FolderBrowserDialog();
            folderDialog.Description = "Select the test directory!";
            folderDialog.SelectedPath = Directory.GetParent(_dte.Solution.FullName).FullName;

            DialogResult result = folderDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                testDirectoryPath = folderDialog.SelectedPath;
            }
            else return;

            FileInfo testFile = new FileInfo(Path.Combine(testDirectoryPath, testFileName));

            // Make sure the directory exists before we create the file. Don't use
            // `PackageUtilities.EnsureOutputPath()` because it can silently fail.
            Directory.CreateDirectory(testFile.DirectoryName);


            if (testFile.Exists)
            {
                await VS.MessageBox.ShowWarningAsync("AI Programming Assistant Error", "A testfile already exists with the given name!");
                return;
            }

            Project testProject = _dte.Solution.FindProjectItem(_dte.ActiveDocument.FullName).ContainingProject;
            Projects projects = _dte.Solution.Projects;
            var enumerator = projects.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Project analyzedProject = (Project)enumerator.Current;
                if (analyzedProject.GetRootFolder() != null)
                {
                    var root = analyzedProject.GetRootFolder().Substring(0, analyzedProject.GetRootFolder().Length - 1);
                    if (testDirectoryPath.Contains(root))
                    {
                        testProject = analyzedProject;
                        testFileInfo.NameSpace = analyzedProject.Name + testDirectoryPath.Replace(root, "").Replace("\\", ".");
                    }
                }

            }


            string testCode = await ApiCallHelper.HandleApiCallAsync(() => aiApi.AskForTestCodeAsync(activeDocumentProperties.SelectedCode, activeDocumentProperties.WholeCode, testFileInfo.NameSpace, testFileInfo.ClassName));
            if (testCode == null) return;


            await WriteFileAsync(testProject, testFile.FullName);

            ProjectItem item = testProject.AddFileToProject(testFile);
            testProject.ProjectItems.AddFromFile(testFile.FullName);

            ServiceProvider sp = new ServiceProvider((Microsoft.VisualStudio.OLE.Interop.IServiceProvider)_dte);
            VsShellUtilities.OpenDocument(sp, testFile.FullName);
            _dte.ExecuteCommand("SolutionExplorer.SyncWithActiveDocument");
            _dte.ActiveDocument.Activate();
            item.Document.Activate();
            activeDocument = await VS.Documents.GetActiveDocumentViewAsync();
            activeDocument.InsertSuggestion(0, testCode);
            // _dte.ExecuteCommand("ProjectandSolutionContextMenus.Project.SyncNamespaces");
            _dte.ExecuteCommand("Edit.FormatDocument");
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
