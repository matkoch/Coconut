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

using EnvDTE;

namespace Coconut.Debugging
{
  public class VsBreakpoint : IBreakpoint
  {
    public VsBreakpoint (Breakpoint breakpoint)
    {
      File = breakpoint.File;
      FileLine = breakpoint.FileLine - 1;
      FileColumn = breakpoint.FileColumn - 1;
      FunctionName = breakpoint.FunctionName;
      FunctionLineOffset = breakpoint.FunctionLineOffset - 1;
      FunctionColumnOffset = breakpoint.FunctionColumnOffset - 1;
    }

    public string File { get; }
    public int FileLine { get; }
    public int FileColumn { get; }

    public string FunctionName { get; }
    public int FunctionLineOffset { get; }
    public int FunctionColumnOffset { get; }

    public bool IsValid ()
    {
      return DebuggingService.IsValid(this);
    }
  }
}