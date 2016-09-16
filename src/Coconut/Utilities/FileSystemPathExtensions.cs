using System;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Modules.ExternalFileModules;
using JetBrains.Util;

namespace Coconut.Utilities
{
  public static class FileSystemPathExtensions
  {
    [CanBeNull]
    public static IPsiSourceFile TryGetSourceFile(this FileSystemPath path, ISolution solution)
    {
      var projectFile = solution.FindProjectItemsByLocation(path).OfType<IProjectFile>().FirstOrDefault();
      if (projectFile != null)
        return projectFile.ToSourceFile();

      var contentFilesModuleFactory = solution.GetComponent<ContentFilesModuleFactory>();
      IPsiSourceFile psiSourceFile;
      if (contentFilesModuleFactory.PsiModule.SourceFilesMap.TryGetValue(path, out psiSourceFile))
        return psiSourceFile;

      return null;
    }
  }
}