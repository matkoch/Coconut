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
using JetBrains.ActionManagement;
using JetBrains.Annotations;
using JetBrains.Application.DataContext;
using JetBrains.ReSharper.Features.Navigation.Features.FindHierarchy;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.DataContext;
using JetBrains.UI.ActionsRevised;

namespace Coconut.DebugNavigation
{
  public class GotoDebugDeclarationAction : IExecutableAction
  {
    public bool Update (IDataContext context, ActionPresentation presentation, [NotNull] DelegateUpdate nextUpdate)
    {
      if (DebuggingHelper.IsDebugging && DebuggingHelper.GetInitializedExpression(context) == null && context.Psi().DeclaredElements.OfType<IOverridableMember>().Any(x => x.IsAbstract))
        return true;

      return nextUpdate();
    }

    public void Execute (IDataContext context, DelegateExecute nextExecute)
    {
      var actionManager = context.GetComponent<IActionManager>();
      actionManager.ExecuteAction<GotoInheritorsAction>();
    }
  }
}