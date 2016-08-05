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
using EnvDTE;
using JetBrains.Annotations;
using JetBrains.Application.ComponentModel;
using JetBrains.Application.DataContext;
using JetBrains.ProjectModel.DataContext;
using JetBrains.ReSharper.Feature.Services.Navigation.ContextNavigation;
using JetBrains.ReSharper.Feature.Services.Util;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.DataContext;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.TextControl.DataContext;
using JetBrains.Util;

namespace Coconut.DebugNavigation
{
  // TODO: C#6 + Nullability showcase
  [ShellFeaturePart]
  public class DebugDeclarationSearch : DefaultDeclarationSearch
  {
    private Debugger Debugger { get; } = Shell.Instance.GetComponent<DTE>().Debugger;

    public override bool IsContextApplicable ([NotNull] IDataContext dataContext)
    {
      return Debugger.CurrentMode == dbgDebugMode.dbgBreakMode;
    }

    protected override IEnumerable<DeclaredElementTypeUsageInfo> GetCandidates ([NotNull] IDataContext context, ReferencePreferenceKind kind)
    {
      var actualDeclaredElement = GetActualDeclaredElement(context);
      if (actualDeclaredElement != null)
        return new[] { new DeclaredElementTypeUsageInfo(actualDeclaredElement, true) };

      //var selectedDeclaredElement = psiContext.DeclaredElements.Single() as ITypeMember;
      //if (selectedDeclaredElement != null)
      //  return new[] { new DeclaredElementTypeUsageInfo(selectedDeclaredElement, false) };

      return EmptyList<DeclaredElementTypeUsageInfo>.InstanceList;
    }

    //protected override SearchDeclarationsRequest GetDeclarationSearchRequest (DeclaredElementTypeUsageInfo elementInfo, Func<bool> checkCancelled)
    //{
    //  return elementInfo.IsDeclaration
    //      ? base.GetDeclarationSearchRequest(elementInfo, checkCancelled)
    //      : new MimicSearchRequest(elementInfo.DeclaredElement);
    //}

    [CanBeNull]
    private ITypeMember GetActualDeclaredElement (IDataContext context)
    {
      var psiContext = context.Psi();
      if (psiContext.DeclaredElements.IsEmpty())
        return null;

      var expressionString = GetExpressionString(context);
      if (expressionString == null)
        return null;

      var expression = Debugger.GetExpression(expressionString);
      if (!expression.IsValidValue)
        return null;

      var type = GetTypeFullName(expression);
      return GetDeclaredElement(psiContext, type);
    }

    [CanBeNull]
    private static string GetExpressionString (IDataContext context)
    {
      var solution = context.GetData(ProjectModelDataConstants.SOLUTION);
      var textControl = context.GetData(TextControlDataConstants.TEXT_CONTROL).NotNull();

      var referenceExpression = TextControlToPsi.GetElementFromCaretPosition<IReferenceExpression>(solution, textControl);
      var qualifierExpression = referenceExpression?.QualifierExpression as IReferenceExpression;
      var declaredElement = qualifierExpression?.Reference.Resolve().DeclaredElement;

      var modifiersOwner = declaredElement as IModifiersOwner;
      if (modifiersOwner != null)
      {
        if (!modifiersOwner.IsStatic)
          return declaredElement.ShortName;
        return DeclaredElementPresenter.Format(
            declaredElement.PresentationLanguage,
            new DeclaredElementPresenterStyle(NameStyle.QUALIFIED),
            declaredElement);
      }

      if (declaredElement is IVariableDeclaration || declaredElement is IParameter)
        return declaredElement.ShortName;

      return null;
    }

    private static string GetTypeFullName (Expression expression)
    {
      var type = expression.NotNull().Type;
      type = type.Substring(type.IndexOf('{') + 1);
      type = type.Substring(0, type.Length - 1);
      return type;
    }

    [CanBeNull]
    private static ITypeMember GetDeclaredElement (PsiContext psiContext, string actualType)
    {
      var psiModule = psiContext.SourceFile.NotNull().PsiModule;
      var psiServices = psiContext.Services.NotNull();
      var symbolScope = psiServices.Symbols.GetSymbolScope(psiModule, true, true);

      var declaredElement = (ITypeElement) symbolScope.GetElementsByQualifiedName(actualType).SingleOrDefault();
      if (declaredElement == null)
        return null;

      var shortName = psiContext.DeclaredElements.Single().ShortName;
      return declaredElement.GetMembers().FirstOrDefault(x => x.ShortName == shortName);
    }
  }
}