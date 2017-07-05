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

using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.Tools.NuGet;
using Nuke.Core;
using Nuke.Core.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;
using static Nuke.Common.Tools.NuGet.NuGetTasks;

class DefaultBuild : GitHubBuild
{
    public static void Main () => Execute<DefaultBuild>(x => x.Compile);

    Target Clean => _ => _
            .Executes(
                () => DeleteDirectories(GlobDirectories(SolutionDirectory, "**/bin", "**/obj")),
                () => PrepareCleanDirectory(OutputDirectory));

    Target Restore => _ => _
            .DependsOn(Clean)
            .Executes(() => NuGetRestore(SolutionFile));

    Target Compile => _ => _
            .DependsOn(Restore)
            .Executes(() => MSBuild(s => DefaultSettings.MSBuildCompile));

    Target Pack => _ => _
            .DependsOn(Compile)
            .Executes(() => GlobFiles(RootDirectory, "/nuspec/*.nuspec")
                    .ForEach(x => NuGetPack(x, s => DefaultSettings.NuGetPack
                                .SetBasePath(SolutionDirectory))));
}
