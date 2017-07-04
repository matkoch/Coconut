// Copyright 2016, 2015, 2014 Matthias Koch
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
        public static IPsiSourceFile TryGetSourceFile (this FileSystemPath path, ISolution solution)
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