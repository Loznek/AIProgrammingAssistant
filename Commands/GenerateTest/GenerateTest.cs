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

namespace AIProgrammingAssistant.Commands.GenerateTest
{
    [Command(PackageIds.GenerateTest)]
    public class GenerateTest : BaseDICommand
    {
        private readonly IAIFunctions aiApi;
        public GenerateTest(DIToolkitPackage package, IAIFunctions api) : base(package)
        {
            aiApi = api;
        }

        private static DTE2 _dte;

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            _dte = AIProgrammingAssistantPackage._dte;
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

            TestFileInfo testFileInfo = new TestFileInfo();
            string testFileName;
            TextInputDialog.Show("Generate testfile", "Enter the name of the testfile ", "Testfile.cs", out testFileName);
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

            FileInfo testFile = new FileInfo(Path.Combine(testDirectoryPath, testFileName));

            // Make sure the directory exists before we create the file. Don't use
            // `PackageUtilities.EnsureOutputPath()` because it can silently fail.
            Directory.CreateDirectory(testFile.DirectoryName);


            if (testFile.Exists)
            {
                await VS.MessageBox.ShowWarningAsync("AI Programming Assistant Error", "A tesfile already exists with the given name!");
                return;
            }

            Project testProject = _dte.Solution.FindProjectItem(_dte.ActiveDocument.FullName).ContainingProject;
            Projects projects = _dte.Solution.Projects;
            var enumerator = projects.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Project analyzedProject = (Project)enumerator.Current;
                var root = analyzedProject.GetRootFolder().Substring(0, analyzedProject.GetRootFolder().Length - 1);


                if (testDirectoryPath.Contains(root))
                {
                    testProject = analyzedProject;
                    testFileInfo.NameSpace = analyzedProject.Name + testDirectoryPath.Replace(root, "").Replace("\\\\", ".");
                }
            }

            string testCode;
            try
            {
               testCode = await aiApi.AskForTestCodeAsync(activeDocumentProperties.SelectedCode, activeDocumentProperties.WholeCode, testFileInfo.NameSpace, testFileInfo.ClassName);
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
                var exceptionMessage = apiException.Message;
                exceptionMessage = exceptionMessage.Replace("\n", "\n" + new string(' ', Math.Max(activeDocumentProperties.NumberOfStartingSpaces - 5, 0)) + SuggestionLineSign.message + " ");
                DocumentHelper.insertSuggestion(activeDocumentProperties.ActiveDocument, exceptionMessage);
                return;
            }
            

            await WriteFileAsync(testProject, testFile.FullName);

            ProjectItem item = testProject.AddFileToProject(testFile);
            testProject.ProjectItems.AddFromFile(testFile.FullName);

            ServiceProvider sp = new ServiceProvider((Microsoft.VisualStudio.OLE.Interop.IServiceProvider)_dte);
            VsShellUtilities.OpenDocument(sp, testFile.FullName);
            //ExecuteCommandIfAvailable("SolutionExplorer.SyncWithActiveDocument");
            _dte.ExecuteCommand("SolutionExplorer.SyncWithActiveDocument");
            _dte.ActiveDocument.Activate();
            item.Document.Activate();

            DocumentHelper.insertSuggestion(activeDocumentProperties.ActiveDocument, testCode);
            _dte.ExecuteCommand("ProjectandSolutionContextMenus.Project.SyncNamespaces");
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
