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
using System.Drawing;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.DocumentModel;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Occurrences;
using JetBrains.ReSharper.Feature.Services.Presentation;
using JetBrains.ReSharper.Psi;
using JetBrains.UI.PopupMenu;
using JetBrains.UI.Resources;
using JetBrains.UI.RichText;
using JetBrains.Util;
using JetBrains.Util.dataStructures.TypedIntrinsics;

namespace Coconut.Debugging.GotoBreakpoints
{
  [OccurrencePresenter]
  public class BreakpointOccurrencePresenter : IOccurrencePresenter
  {
    private static readonly TextStyle s_grayTextColor = TextStyle.FromForeColor(Color.Gray);

    private static readonly DeclaredElementPresenterStyle s_styleMemberName =
        new DeclaredElementPresenterStyle
        {
          ShowName = NameStyle.SHORT,
          ShowParameterTypes = true,
          ShowParameterNames = false,
          ShowType = TypeStyle.AFTER,
          ShowTypeParameters = TypeParameterStyle.FULL_WITH_VARIANCE
        };

    public bool IsApplicable ([NotNull] IOccurrence occurrence)
    {
      return occurrence is BreakpointOccurrence;
    }

    public bool Present (
      [NotNull] IMenuItemDescriptor descriptor,
      [NotNull] IOccurrence occurrence,
      OccurrencePresentationOptions occurrencePresentationOptions)
    {
      if (!occurrence.IsValid)
        return false;

      var breakpointOccurrence = occurrence as BreakpointOccurrence;
      if (breakpointOccurrence == null)
        return false;

      var envoy = breakpointOccurrence.Envoy;
      var sourceFile = breakpointOccurrence.SourceFile;
      var declaredElement = GetDeclaredElement(envoy, sourceFile);
      if (declaredElement == null)
        return false;

      DeclaredElementPresenterMarking marking;
      var declaredElementText = DeclaredElementMenuItemFormatter.FormatText(declaredElement, sourceFile.PrimaryPsiLanguage, out marking);
      var lineAppendix = new RichText(" (Line " + envoy.FunctionLineOffset + ")");
      var fullText = declaredElementText + lineAppendix;
      fullText.SetStyle(s_grayTextColor, fullText.Length - lineAppendix.Length, lineAppendix.Length);

      descriptor.Style = MenuItemStyle.Enabled;
      descriptor.Icon = CommonThemedIcons.Abort.Id;
      descriptor.Text = fullText;
      descriptor.TailGlyph = breakpointOccurrence.GetSolution().NotNull().GetComponent<PsiSourceFilePresentationService>().GetIconId(sourceFile);
      descriptor.ShortcutText = new RichText(sourceFile.DisplayName, s_grayTextColor);

      descriptor.Tooltip = sourceFile.Document.GetLineText(((Int32<DocLine>) envoy.FileLine).Minus1()).Trim();

      return true;
    }

    //[CanBeNull]
    //private IDeclaredElement GetDeclaredElement (BreakpointEnvoy envoy, IPsiSourceFile sourceFile)
    //{
    //  var offset = envoy.GetOffset(sourceFile);
    //  TextControlToPsi.GetDeclaredElements(sourceFile.GetSolution(), sourceFile.Document, offset, );
    //}

    [CanBeNull]
    private IDeclaredElement GetDeclaredElement (BreakpointEnvoy envoy, IPsiSourceFile file)
    {
      var parenthesisIndex = envoy.FunctionName.IndexOf('(');
      var functionNameWithoutParameterList = parenthesisIndex != -1 ? envoy.FunctionName.Substring(0, parenthesisIndex) : envoy.FunctionName;
      var lastDotIndex = functionNameWithoutParameterList.LastIndexOf('.');
      var typeName = functionNameWithoutParameterList.Substring(0, lastDotIndex);
      var functionName = functionNameWithoutParameterList.Substring(lastDotIndex + 1);
      var isConstructor = typeName.EndsWith("." + functionName);

      var psiServices = file.GetSolution().GetPsiServices();
      var symbolScope = psiServices.Symbols.GetSymbolScope(file.PsiModule, true, true);
      var typeElement = symbolScope.GetTypeElementByCLRName(typeName).NotNull("typeElement != null");
      var candidates = GetCandidateMembers(typeElement, isConstructor, functionName).ToList();
      if (candidates.Count == 1)
        return candidates.Single();

      var parameterOwners = candidates.OfType<IParametersOwner>().ToList();
      var paramterValues = envoy.FunctionName.Substring(parenthesisIndex + 1).Trim(')')
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
  }
}