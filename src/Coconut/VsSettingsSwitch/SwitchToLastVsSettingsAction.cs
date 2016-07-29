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
using JetBrains.Application.DataContext;
using JetBrains.ReSharper.Feature.Services.Menu;
using JetBrains.UI.ActionsRevised;

namespace Coconut.VsSettingsSwitch
{
  [Action ("Switch to last VsSettings", Id = 6215)]
  public class SwitchToLastVsSettingsAction : IExecutableAction, IInsertLast<EditOthersGroup>
  {
    #region IExecutableAction

    public bool Update (IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
    {
      return VsSettingsSwitchUtility.GetLastSwitchedList().Count >= 2;
    }

    public void Execute (IDataContext context, DelegateExecute nextExecute)
    {
      var lastVsSettings = VsSettingsSwitchUtility.GetLastSwitchedList()[1];
      VsSettingsSwitchUtility.SwitchToSetting(context, lastVsSettings);
    }

    #endregion
  }
}