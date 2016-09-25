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
  /// Service around the <see cref="Debugger"/> interface .
  /// </summary>
  public static class DebuggingService
  {
    private const string c_null = "null";

    private static Debugger s_debugger;

    [CanBeNull]
    private static Debugger Debugger => s_debugger = s_debugger ?? Shell.Instance.TryGetComponent<DTE>()?.Debugger;

    public static bool IsDebugging => Debugger?.CurrentMode == dbgDebugMode.dbgBreakMode;

    public static IEnumerable<Breakpoint> GetBreakpoints() => Debugger?.Breakpoints.OfType<Breakpoint>() ?? EmptyList<Breakpoint>.InstanceList;

    public static IEnumerable<StackFrame> GetStackFrames() => Debugger?.CurrentThread.StackFrames.OfType<StackFrame>() ?? EmptyList<StackFrame>.InstanceList;

    [CanBeNull]
    public static Expression GetInitializedExpression (IDataContext context)
    {
      Assertion.Assert(Debugger != null, "Debugger != null");

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
      Assertion.Assert(Debugger != null, "Debugger != null");

      var allStackFrames = GetStackFrames().ToList();
      var currentPosition = allStackFrames.IndexOf(Debugger.CurrentStackFrame);
      currentPosition = Math.Min(Math.Max(currentPosition + (int) movement, 0), allStackFrames.Count - 1);
      Debugger.CurrentStackFrame = allStackFrames[currentPosition];
    }

    public static bool IsValid (IBreakpoint breakpoint)
    {
      foreach (var vsBreakpoint in GetBreakpoints())
      {
        if (vsBreakpoint.File != breakpoint.File || 
          vsBreakpoint.FileLine != breakpoint.FileLine + 1 ||
          vsBreakpoint.FileColumn != breakpoint.FileColumn + 1)
          continue;

        Assertion.Assert(vsBreakpoint.FunctionName == breakpoint.FunctionName, "vsBreakpoint.FunctionName == breakpoint.FunctionName");
        Assertion.Assert(vsBreakpoint.FunctionLineOffset == breakpoint.FunctionLineOffset + 1, "vsBreakpoint.FunctionLineOffset == breakpoint.FunctionLineOffset + 1");
        Assertion.Assert(vsBreakpoint.FunctionColumnOffset == breakpoint.FunctionColumnOffset + 1, "vsBreakpoint.FunctionColumnOffset == breakpoint.FunctionColumnOffset + 1");

        return true;
      }

      return false;
    }
  }
}