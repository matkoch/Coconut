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
using EnvDTE80;
using JetBrains.ActionManagement;
using JetBrains.Application.DataContext;
using JetBrains.Application.Progress;
using JetBrains.ReSharper.Feature.Services.Menu;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.UI.ActionsRevised;
using JetBrains.UI.ActionSystem.ShortcutManager;
using JetBrains.VsIntegration.Shell.ActionManagement;

namespace Coconut.VsSettingsSwitch
{
  [Action ("Reset Keyboard Settings", Id = 6216)]
  public class ResetKeyboardSettingsAction : IExecutableAction, IInsertLast<EditOthersGroup>, INeedMainThreadToUpdateAction
  {
    public bool Update (IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
    {
      return true;
    }

    public void Execute (IDataContext context, DelegateExecute nextExecute)
    {
      System.Diagnostics.Debugger.Launch();
      //VsSettingsSwitchUtility.ResetShortcuts();
      var dte = Shell.Instance.GetComponent<DTE2>();
      var property = dte.Properties["Environment", "Keyboard"];
      var bla = property.Item("SchemeName").Value;
      property.Item("SchemeName").Value = "(Default)";
    }
  }

  public abstract class ApplyKeyboardSchemeActionBase : IExecutableAction, IInsertLast<EditOthersGroup>, INeedMainThreadToUpdateAction
  {
    private readonly ShortcutScheme myScheme;

    protected ApplyKeyboardSchemeActionBase (ShortcutScheme scheme)
    {
      myScheme = scheme;
    }

    public bool Update (IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
    {
      return true;
    }

    public void Execute (IDataContext context, DelegateExecute nextExecute)
    {
      VsSettingsSwitchUtility.ResetShortcuts();
      var shortcutManager = context.GetComponent<VsShortcutManager>();
      shortcutManager.ApplyShortcutScheme(myScheme, NullProgressIndicator.Instance);
    }
  }

  [Action ("Apply VisualStudio keyboard scheme", Id = 6217)]
  public class ApplyVsKeyboardSchemeAction : ApplyKeyboardSchemeActionBase
  {
    public ApplyVsKeyboardSchemeAction ()
      : base(ShortcutScheme.VS)
    {
    }
  }

  [Action ("Apply IDEA keyboard scheme", Id = 6218)]
  public class ApplyIdeaKeyboardSchemeAction : ApplyKeyboardSchemeActionBase
  {
    public ApplyIdeaKeyboardSchemeAction ()
      : base(ShortcutScheme.Idea)
    {
    }
  }
}