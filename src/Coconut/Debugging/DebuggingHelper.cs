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
using Coconut.Debugging.StackFrameActions;
using EnvDTE;
using JetBrains.Annotations;
using JetBrains.Application.DataContext;
using JetBrains.ProjectModel.DataContext;
using JetBrains.ReSharper.Feature.Services.Util;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.DataContext;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.TextControl.DataContext;
using JetBrains.Util;

namespace Coconut.Debugging
{
  /// <summary>
  /// Helper class around the <see cref="Debugger"/> interface 
  /// </summary>
  public static class DebuggingHelper
  {
    private const string c_null = "null";

    public static Debugger Debugger { get; } = Shell.Instance.GetComponent<DTE>().Debugger;

    public static bool IsDebugging => Debugger.CurrentMode == dbgDebugMode.dbgBreakMode;

    [CanBeNull]
    public static EnvDTE.Expression GetInitializedExpression (IDataContext context)
    {
      if (context.Psi().DeclaredElements.IsEmpty())
        return null;

      var expressionText = GetEvaluableExpressionText(context);
      if (expressionText == null)
        return null;

      var expression = Debugger.GetExpression(expressionText);
      if (!expression.IsValidValue || expression.Value == c_null)
        return null;

      return expression;
    }

    [CanBeNull]
    private static string GetEvaluableExpressionText (IDataContext context)
    {
      var solution = context.GetData(ProjectModelDataConstants.SOLUTION);
      var textControl = context.GetData(TextControlDataConstants.TEXT_CONTROL).NotNull();

      var referenceExpression = TextControlToPsi.GetElementFromCaretPosition<IReferenceExpression>(solution, textControl);
      var qualifierExpression = referenceExpression?.QualifierExpression;
      if (!(qualifierExpression is IReferenceExpression) && !(qualifierExpression is ILiteralExpression))
        return null;

      return qualifierExpression.GetText();
    }

    public static void ChangeStackFrame (StackFrameMovement movement)
    {
      var allStackFrames = Debugger.CurrentThread.StackFrames.OfType<EnvDTE.StackFrame>().ToList();
      var currentPosition = allStackFrames.IndexOf(Debugger.CurrentStackFrame);
      currentPosition = Math.Min(Math.Max(currentPosition + (int) movement, 0), allStackFrames.Count - 1);
      Debugger.CurrentStackFrame = allStackFrames[currentPosition];
    }
  }
}