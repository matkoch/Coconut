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
using System.Collections.Generic;
using System.Linq;
using Coconut.Utilities;
using JetBrains.Annotations;
using JetBrains.Application.ComponentModel;
using JetBrains.ReSharper.Feature.Services.Navigation.Goto.Misc;
using JetBrains.ReSharper.Feature.Services.Navigation.Goto.ProvidersAPI;
using JetBrains.ReSharper.Feature.Services.Occurrences;
using JetBrains.Text;
using JetBrains.UI.Utils;
using JetBrains.Util;

namespace Coconut.Debugging.GotoBreakpoints
{
  [ShellFeaturePart]
  public class GotoBreakpointsProvider : IInstantGotoEverythingProvider
  {
    public bool IsApplicable ([NotNull] INavigationScope scope, [NotNull] GotoContext gotoContext, [NotNull] IdentifierMatcher matcher)
    {
      return true;
    }

    public IEnumerable<Pair<IOccurrence, MatchingInfo>> GetMatchingOccurrences (
      [NotNull] IdentifierMatcher matcher,
      [NotNull] INavigationScope scope,
      [NotNull] GotoContext gotoContext,
      [NotNull] Func<bool> checkForInterrupt)
    {
      var solution = scope.GetSolution();

      var breakpoints = DebuggingService.GetBreakpoints().ToList();
      foreach (var breakpoint in breakpoints)
      {
        var sourceFile = FileSystemPath.Parse(breakpoint.File).TryGetSourceFile(solution);
        if (sourceFile == null)
          continue;

        var breakpointEnvoy = new VsBreakpoint(breakpoint);
        var declaredElement = breakpointEnvoy.GetDeclaredElement(sourceFile);
        if (declaredElement == null)
          continue;

        if (!matcher.Filter.IsNullOrWhitespace() && !matcher.Matches(declaredElement.ShortName))
          continue;

        var matchingInfo = new MatchingInfo(matcher, declaredElement.ShortName, matchingIndiciesAreCorrect: !matcher.Filter.IsNullOrWhitespace());
        var occurrence = (IOccurrence) new BreakpointOccurrence(sourceFile, breakpointEnvoy, declaredElement);
        yield return Pair.Of(occurrence, matchingInfo);
      }
    }
  }
}