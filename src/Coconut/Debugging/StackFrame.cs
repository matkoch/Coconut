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

namespace Coconut.DebugNavigation
{
  public class StackFrame
  {
    public static StackFrame Parse (EnvDTE.StackFrame stackFrame)
    {
      return new StackFrame(
               stackFrame.Module,
               stackFrame.FunctionName,
               stackFrame.ReturnType,
               stackFrame.Arguments.OfType<EnvDTE.Expression>().Select(Expression.Parse).ToList(),
               stackFrame.Locals.OfType<EnvDTE.Expression>().Select(Expression.Parse).ToList());
    }

    public StackFrame (
      string module,
      string function,
      string returnType,
      IReadOnlyCollection<Expression> arguments,
      IReadOnlyCollection<Expression> locals)
    {
      Module = module;
      Function = function;
      ReturnType = returnType;
      Arguments = arguments;
      Locals = locals;
    }

    public string Module { get; }
    public string Function { get; }
    public string ReturnType { get; }

    public IReadOnlyCollection<Expression> Arguments { get; }
    public IReadOnlyCollection<Expression> Locals { get; }

    public override string ToString ()
    {
      return $"{Module}!{Function}";
    }
  }
}