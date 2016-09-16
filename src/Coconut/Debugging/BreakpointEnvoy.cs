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
using System.Text;
using EnvDTE;
using JetBrains.Annotations;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Psi;
using JetBrains.Util;
using JetBrains.Util.dataStructures.TypedIntrinsics;

namespace Coconut.Debugging
{
  public class BreakpointEnvoy
  {
    public BreakpointEnvoy (Breakpoint breakpoint)
    {
      File = breakpoint.File;
      FileLine = breakpoint.FileLine;
      FileColumn = breakpoint.FileColumn;
      FunctionName = breakpoint.FunctionName;
      FunctionLineOffset = breakpoint.FunctionLineOffset;
      FunctionColumnOffset = breakpoint.FunctionColumnOffset;
      ConditionType = breakpoint.ConditionType;
      Condition = breakpoint.Condition;
      Name = breakpoint.Name;
      Tag = breakpoint.Tag;
      CurrentHits = breakpoint.CurrentHits;
    }

    public string File { get; }
    public int FileLine { get; }
    public int FileColumn { get; }

    public int FunctionLineOffset { get; }
    public string FunctionName { get; }
    public int FunctionColumnOffset { get; }

    public dbgBreakpointConditionType ConditionType { get; }
    public string Condition { get; }
    public string Name { get; }
    public string Tag { get; }
    public int CurrentHits { get; }

    [CanBeNull]
    public Breakpoint GetBreakpoint ()
    {
      var breakpoints = DebuggingHelper.Debugger.Breakpoints.OfType<Breakpoint>();
      foreach (var breakpoint in breakpoints)
      {
        if (breakpoint.File == File &&
            breakpoint.FileLine == FileLine &&
            breakpoint.FileColumn == FileColumn)
        {
          Assertion.Assert(breakpoint.Name == Name, "breakpoint.Name == Name");
          Assertion.Assert(breakpoint.FunctionName == FunctionName, "breakpoint.FunctionName == FunctionName");
          Assertion.Assert(breakpoint.FunctionLineOffset == FunctionLineOffset, "breakpoint.FunctionLineOffset == FunctionLineOffset");
          Assertion.Assert(breakpoint.FunctionColumnOffset == FunctionColumnOffset, "breakpoint.FunctionColumnOffset == FunctionColumnOffset");

          return breakpoint;
        }
      }

      return null;
    }

    public bool IsValid ()
    {
      return GetBreakpoint() != null;
    }

    public int GetOffset (IPsiSourceFile file)
    {
      return file.Document.GetLineStartOffset(((Int32<DocLine>) FileLine).Minus1()) + FileColumn - 1;
    }

    public override string ToString ()
    {
      return new StringBuilder().Append("[Breakpoint] ")
          .Append(File)
          .Append(" (")
          .Append(FileLine)
          .Append(", ")
          .Append(FileColumn)
          .Append(")")
          .ToString();
    }
  }
}