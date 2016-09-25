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
using JetBrains.Annotations;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Navigation.NavigationExtensions;
using JetBrains.ReSharper.Psi;
using JetBrains.Util;
using JetBrains.Util.dataStructures.TypedIntrinsics;

namespace Coconut.Debugging
{
  public static class BreakpointExtensions
  {
    [CanBeNull]
    public static IDeclaredElement GetDeclaredElement (this IBreakpoint breakpoint, IPsiSourceFile file)
    {
      var parenthesisIndex = breakpoint.FunctionName.IndexOf(value: '(');
      var functionNameWithoutParameterList = parenthesisIndex != -1 ? breakpoint.FunctionName.Substring(startIndex: 0, length: parenthesisIndex) : breakpoint.FunctionName;
      var lastDotIndex = functionNameWithoutParameterList.LastIndexOf(value: '.');
      var typeName = functionNameWithoutParameterList.Substring(startIndex: 0, length: lastDotIndex);
      var functionName = functionNameWithoutParameterList.Substring(lastDotIndex + 1);
      var isConstructor = typeName.EndsWith("." + functionName);

      var psiServices = file.GetSolution().GetPsiServices();
      var symbolScope = psiServices.Symbols.GetSymbolScope(file.PsiModule, withReferences: true, caseSensitive: true);
      var typeElement = symbolScope.GetTypeElementByCLRName(typeName).NotNull("typeElement != null");
      var candidates = GetCandidateMembers(typeElement, isConstructor, functionName).ToList();
      if (candidates.Count == 1)
        return candidates.Single();

      var parameterOwners = candidates.OfType<IParametersOwner>().ToList();
      var paramterValues = breakpoint.FunctionName.Substring(parenthesisIndex + 1).Trim(')')
          .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
      var parameterTypes = paramterValues.Where((x, i) => i % 2 == 0).ToList();
      var parameterNames = paramterValues.Where((x, i) => i % 2 == 1).ToList();
      candidates = GetCandidateParameterOwners(parameterOwners, parameterTypes, parameterNames).ToList();
      return candidates.SingleOrFirstOrDefaultErr("multiple candidates");
    }

    private static IEnumerable<IDeclaredElement> GetCandidateMembers (ITypeElement typeElement, bool isConstructor, string functionName)
    {
      foreach (var member in typeElement.GetMembers())
      {
        if ((isConstructor && member.ShortName.EndsWith("ctor")) ||
            (!isConstructor && member.ShortName == functionName))
          yield return member;
      }
    }

    private static IEnumerable<IDeclaredElement> GetCandidateParameterOwners (
      IList<IParametersOwner> parameterOwners,
      IList<string> paramterValues,
      IList<string> parameterNames)
    {
      foreach (var parametersOwner in parameterOwners)
      {
        if (parametersOwner.Parameters.Count != paramterValues.Count / 2)
          continue;

        var matches = true;
        for (var i = 0; i < parametersOwner.Parameters.Count; i++)
        {
          // TODO: check for types
          if (parametersOwner.Parameters[i].ShortName == parameterNames[i])
            matches = false;
        }

        if (matches)
          yield return parametersOwner;
      }
    }

    public static bool Navigate(this IBreakpoint breakpoint, IPsiSourceFile sourceFile)
    {
      var offset = sourceFile.Document.GetLineStartOffset((Int32<DocLine>) breakpoint.FileLine) + breakpoint.FileColumn;
      var textRange = new TextRange(offset);
      return sourceFile.Navigate(textRange, activate: true);
    }
    
    public static string GetLineText (this IBreakpoint breakpoint, IPsiSourceFile sourceFile)
    {
      return sourceFile.Document.GetLineText((Int32<DocLine>) breakpoint.FileLine).Trim();
    }
  }
}