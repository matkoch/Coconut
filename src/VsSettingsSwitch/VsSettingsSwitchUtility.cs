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
using System.IO;
using System.Linq;
using EnvDTE80;
using JetBrains.Annotations;
using JetBrains.Application.DataContext;
using JetBrains.Application.Settings;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.UI.DataContext;
using JetBrains.UI.Tooltips;
using JetBrains.Util;
using Microsoft.Win32;

namespace VsSettingsSwitch
{
  public static class VsSettingsSwitchUtility
  {
    private static DTE2 Dte =>
        Shell.Instance.GetComponent<DTE2>();

    private static IContextBoundSettingsStore SettingsStore =>
        Shell.Instance.GetComponent<ISettingsStore>().BindToContextTransient(ContextRange.ApplicationWide);

    public static string GetSettingsDirectory ()
    {
      var vsVersion = Dte.Version;
      var registryKey = Registry.CurrentUser.OpenSubKey($"Software\\Microsoft\\VisualStudio\\{vsVersion}").NotNull();
      var visualStudioDirectory = registryKey.GetValue("VisualStudioLocation").ToString();
      return Path.Combine(visualStudioDirectory, "Settings");
    }

    public static List<FileInfo> GetSettingsFiles ([CanBeNull] string settingsDirectory = null)
    {
      settingsDirectory = settingsDirectory ?? GetSettingsDirectory();

      var lastImportedSetting = GetLastSwitchedList();
      return Directory.GetFiles(settingsDirectory, "*.vssettings").Select(x => new FileInfo(x))
          .Where(x => !x.Name.StartsWith("CurrentSettings"))
          .OrderByDescending(x => x.FullName != lastImportedSetting.FirstOrDefault())
          .ThenBy(x => lastImportedSetting.IndexOf(x.FullName)).ToList();
    }

    public static List<string> GetLastSwitchedList ()
    {
      var settingsStore = Shell.Instance.GetComponent<ISettingsStore>().BindToContextTransient(ContextRange.ApplicationWide);
      return settingsStore.GetValue((VsSettingsSwitchSettingsKey y) => y.LastSwitchedList).Split('|').ToList();
    }

    public static void SetLastSwitchedList (string fileName)
    {
      var lastSwitchedList = GetLastSwitchedList();
      lastSwitchedList.Remove(fileName);
      lastSwitchedList.Insert(0, fileName);

      SettingsStore.SetValue((VsSettingsSwitchSettingsKey s) => s.LastSwitchedList, lastSwitchedList.Join("|"));
    }

    public static void SwitchToSetting (IDataContext context, string fileName)
    {
      var tooltipManager = context.GetComponent<ITooltipManager>().NotNull();
      var contextSource = context.GetData(UIDataConstants.PopupWindowContextSource).NotNull();

      Dte.ExecuteCommand("Tools.ImportandExportSettings", $"/import:\"{fileName}\"");
      SetLastSwitchedList(fileName);
      tooltipManager.Show($"Switched to '{fileName}'", contextSource);
    }
  }
}