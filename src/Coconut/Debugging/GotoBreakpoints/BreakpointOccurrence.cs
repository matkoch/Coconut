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
using JetBrains.Annotations;
using JetBrains.IDE;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Occurrences;
using JetBrains.ReSharper.Psi;
using JetBrains.UI.PopupWindowManager;

namespace Coconut.Debugging.GotoBreakpoints
{
  public class BreakpointOccurrence : IOccurrence
  {
    public BreakpointOccurrence (IPsiSourceFile sourceFile, IBreakpoint breakpoint, IDeclaredElement declaredElement)
    {
      SourceFile = sourceFile;
      Breakpoint = breakpoint;
      DeclaredElement = declaredElement;
    }

    public IPsiSourceFile SourceFile { get; }

    public IBreakpoint Breakpoint { get; }

    public IDeclaredElement DeclaredElement { get; }

    public OccurrenceType OccurrenceType => OccurrenceType.Occurrence;

    public bool IsValid => SourceFile.IsValid() && Breakpoint.IsValid();

    public OccurrencePresentationOptions PresentationOptions { get; set; }

    public ISolution GetSolution ()
    {
      return SourceFile.GetSolution();
    }

    public bool Navigate (
      [NotNull] ISolution solution,
      [NotNull] PopupWindowContextSource windowContext,
      bool transferFocus,
      TabOptions tabOptions = TabOptions.Default)
    {
      return Breakpoint.Navigate(SourceFile);
    }

    public string DumpToString ()
    {
      return "[Breakpoint] " + Breakpoint.File + ", line " + Breakpoint.FileLine + ", column " + Breakpoint.FileColumn;
    }
  }
}