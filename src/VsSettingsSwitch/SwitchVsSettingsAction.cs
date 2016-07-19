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
using System.IO;
using System.Linq;
using JetBrains.ActionManagement;
using JetBrains.Application.DataContext;
using JetBrains.ReSharper.Feature.Services.Menu;
using JetBrains.UI.ActionsRevised;
using JetBrains.UI.Controls;
using JetBrains.UI.DataContext;
using JetBrains.UI.PopupMenu;
using JetBrains.UI.Tooltips;
using JetBrains.Util;

namespace VsSettingsSwitch
{
  [Action ("Switch VsSettings", Id = 6214)]
  public class SwitchVsSettingsAction : IExecutableAction, IInsertLast<EditOthersGroup>
  {
    #region IExecutableAction

    public bool Update (IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
    {
      return true;
    }

    public void Execute (IDataContext context, DelegateExecute nextExecute)
    {
      var tooltipManager = context.GetComponent<ITooltipManager>().NotNull();
      var contextSource = context.GetData(UIDataConstants.PopupWindowContextSource).NotNull();
      var jetPopupMenus = context.GetComponent<JetPopupMenus>().NotNull();

      var settingsDirectory = VsSettingsSwitchUtility.GetSettingsDirectory();
      var settingFiles = VsSettingsSwitchUtility.GetSettingsFiles();

      if (settingFiles.IsEmpty())
      {
        tooltipManager.Show($"No VsSettings available in {settingsDirectory}", contextSource);
        return;
      }

      jetPopupMenus.ShowModal(
          JetPopupMenu.ShowWhen.NoItemsBannerIfNoItems,
          (lifetime, menu) =>
          {
            menu.Caption.Value = WindowlessControlAutomation.Create($"Switch to VsSettings from {settingsDirectory}");
            menu.PopupWindowContextSource = contextSource;
            menu.ItemKeys.AddRange(settingFiles);

            menu.DescribeItem.Advise(
                lifetime,
                e =>
                {
                  e.Descriptor.Style = MenuItemStyle.Enabled;
                  e.Descriptor.Text = ((FileInfo) e.Key).Name;
                });

            menu.ItemClicked.Advise(
                lifetime,
                key => VsSettingsSwitchUtility.SwitchToSetting(context, ((FileInfo) key).FullName));
          });
    }

    #endregion
  }
}