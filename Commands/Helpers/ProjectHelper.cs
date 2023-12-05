using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio;
using Microsoft;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VSLangProj;
using Project = EnvDTE.Project;
using Reference = VSLangProj.Reference;
using Solution = EnvDTE.Solution;

namespace AIProgrammingAssistant.Helpers
{

    /// <summary>
    /// This class is based on the class with the same the in the following project:Mads Kristensen: Add Any File, https://github.com/madskristensen/AddAnyFile
    /// </summary>
    public static class ProjectHelper
    {
        private static readonly DTE2 _dte = AIProgrammingAssistantPackage.dte;

        public static string GetRootFolder(this Project project)
        {
            if (project == null) return null;

            if (project.IsKind("{66A26720-8FB5-11D2-AA7E-00C04F688DDE}")) //ProjectKinds.vsProjectKindSolutionFolder
            {
                return Path.GetDirectoryName(_dte.Solution.FullName);
            }

            if (string.IsNullOrEmpty(project.FullName)) return null;

            string fullPath;

            try
            {
                fullPath = project.Properties.Item("FullPath").Value as string;
            }
            catch (ArgumentException)
            {
                try
                {
                    // MFC projects don't have FullPath, and there seems to be no way to query existence
                    fullPath = project.Properties.Item("ProjectDirectory").Value as string;
                }
                catch (ArgumentException)
                {
                    // Installer projects have a ProjectPath.
                    fullPath = project.Properties.Item("ProjectPath").Value as string;
                }
            }

            if (string.IsNullOrEmpty(fullPath))
            {
                return File.Exists(project.FullName) ? Path.GetDirectoryName(project.FullName) : null;
            }

            if (Directory.Exists(fullPath)) return fullPath;

            if (File.Exists(fullPath)) return Path.GetDirectoryName(fullPath);

            return null;
        }

        public static ProjectItem AddFileToProject(this EnvDTE.Project project, FileInfo file, string itemType = null)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (project.IsKind(ProjectTypes.ASPNET_5, ProjectTypes.SSDT))
            {
                return _dte.Solution.FindProjectItem(file.FullName);
            }

            string root = project.GetRootFolder();

            if (string.IsNullOrEmpty(root) || !file.FullName.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            // Appears to be a bug in project system or VS, so need to make sure the project is aware of the folder structure first,
            // then add the time to that folderStructure (create and/or find)
            // if adding to the root ProjectItem then just do that.
            ProjectItems projectItems;
            if (string.Equals(root.TrimEnd(Path.DirectorySeparatorChar), file.DirectoryName, StringComparison.InvariantCultureIgnoreCase))
            {
                projectItems = project.ProjectItems;
            }
            else
            {
                projectItems = AddFolders(project, file.DirectoryName).ProjectItems;
            }

            ProjectItem item = projectItems.AddFromTemplate(file.FullName, file.Name);
            item.SetItemType(itemType);

            return item;
        }

        public static ProjectItem AddFolders(Project project, string targetFolder)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            List<string> list = new List<string>();
            DirectoryInfo root = new DirectoryInfo(project.GetRootFolder());
            DirectoryInfo target = new DirectoryInfo(targetFolder);

            while (!target.FullName.Equals(root.FullName.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
            {
                list.Add(target.Name);
                target = target.Parent;
            }

            list.Reverse();

            ProjectItem existing = project.ProjectItems.Cast<ProjectItem>().FirstOrDefault(i => i.Name.Equals(list.First(), StringComparison.OrdinalIgnoreCase));
            ProjectItem item = existing ?? project.ProjectItems.AddFolder(list.First());

            foreach (string folder in list.Skip(1))
            {
                existing = item.ProjectItems.Cast<ProjectItem>().FirstOrDefault(i => i.Name.Equals(folder, StringComparison.OrdinalIgnoreCase));
                item = existing ?? item.ProjectItems.AddFolder(folder);
            }

            return item;
        }

        public static void SetItemType(this ProjectItem item, string itemType)
        {
            try
            {
                if (item == null || item.ContainingProject == null) return;


                if (string.IsNullOrEmpty(itemType)
                    || item.ContainingProject.IsKind(ProjectTypes.WEBSITE_PROJECT)
                    || item.ContainingProject.IsKind(ProjectTypes.UNIVERSAL_APP))
                {
                    return;
                }

                item.Properties.Item("ItemType").Value = itemType;
            }
            catch (Exception ex)
            {

            }
        }

        public static bool IsKind(this Project project, params string[] kindGuids)
        {
            foreach (string guid in kindGuids)
            {
                if (project.Kind.Equals(guid, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsKind(this ProjectItem projectItem, params string[] kindGuids)
        {
            foreach (string guid in kindGuids)
            {
                if (projectItem.Kind.Equals(guid, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public static class ProjectTypes
    {
        public const string ASPNET_5 = "{8BB2217D-0F2D-49D1-97BC-3654ED321F3B}";
        public const string DOTNET_Core = "{9A19103F-16F7-4668-BE54-9A1E7A4F7556}";
        public const string WEBSITE_PROJECT = "{E24C65DC-7377-472B-9ABA-BC803B73C61A}";
        public const string UNIVERSAL_APP = "{262852C6-CD72-467D-83FE-5EEB1973A190}";
        public const string NODE_JS = "{9092AA53-FB77-4645-B42D-1CCCA6BD08BD}";
        public const string SSDT = "{00d1a9c2-b5f0-4af3-8072-f6c62b433612}";
    }
}
