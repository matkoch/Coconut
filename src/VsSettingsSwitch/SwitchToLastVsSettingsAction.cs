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
using JetBrains.ActionManagement;
using JetBrains.Application.ComponentModel;
using JetBrains.Application.DataContext;
using JetBrains.ReSharper.Feature.Services.Menu;
using JetBrains.ReSharper.Feature.Services.Navigation.ContextNavigation;
using JetBrains.ReSharper.Feature.Services.Navigation.Requests;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.UI.ActionsRevised;
using JetBrains.Util;

namespace Coconut
{
  [ShellFeaturePart]
  public class CSharpDeclarationSearch : DefaultDeclarationSearch
  {
    public override bool IsContextApplicable (IDataContext dataContext)
    {
      var dte = Shell.Instance.GetComponent<DTE>();
      return dte.Debugger.CurrentMode == dbgDebugMode.dbgBreakMode;
    }

    protected override IEnumerable<DeclaredElementTypeUsageInfo> GetCandidates (IDataContext context, ReferencePreferenceKind kind)
    {
      var candidates = base.GetCandidates(context, kind).ToList();
      if (candidates.Count != 1)
        return candidates;

      System.Diagnostics.Debugger.Launch();
      return candidates;
    }
  }


  [Action("Invest", Id = 6216)]
  public class BlaAction : IExecutableAction, IInsertLast<EditOthersGroup>
  {
    #region IExecutableAction

    public bool Update(IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
    {
      return true;
    }

    public void Execute(IDataContext context, DelegateExecute nextExecute)
    {
      var element = context.GetSelectedTreeNode<IDeclaration>();
      var dte = Shell.Instance.GetComponent<DTE>();
      var locals = dte.Debugger.CurrentStackFrame.Locals;


      var objects = locals.OfType<Expression>().ToArray();

      var names = objects.Select(x => x.Name).ToList();
      var s = objects.Select(x => x.Type).ToList();

      System.Diagnostics.Debugger.Launch();
    }

    #endregion
  }

  [Action ("Switch to last VsSettings", Id = 6215)]
  public class SwitchToLastVsSettingsAction : IExecutableAction, IInsertLast<EditOthersGroup>
  {
    #region IExecutableAction

    public bool Update (IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
    {
      return CoconutUtility.GetLastSwitchedList().Count >= 2;
    }

    public void Execute (IDataContext context, DelegateExecute nextExecute)
    {
      var lastVsSettings = CoconutUtility.GetLastSwitchedList()[1];
      CoconutUtility.SwitchToSetting(context, lastVsSettings);
    }

    #endregion
  }
}