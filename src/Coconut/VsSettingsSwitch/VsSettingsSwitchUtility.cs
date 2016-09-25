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
using System.Xml.Linq;
using EnvDTE;
using EnvDTE80;
using JetBrains.Annotations;
using JetBrains.Application.DataContext;
using JetBrains.Application.Settings;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.UI.DataContext;
using JetBrains.UI.Tooltips;
using JetBrains.Util;
using Microsoft.Win32;

namespace Coconut.VsSettingsSwitch
{
  public static class VsSettingsSwitchUtility
  {
    private static DTE2 Dte =>
        Shell.Instance.GetComponent<DTE2>();

    private static IContextBoundSettingsStore SettingsStore =>
        Shell.Instance.GetComponent<ISettingsStore>().BindToContextTransient(ContextRange.ApplicationWide);

    private static IEnumerable<Command> GetCommands () => Dte.Commands.OfType<Command>();

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
          //.Where(x => !x.Name.StartsWith("CurrentSettings"))
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

      ResetShortcuts();
      ImportFile(fileName);
      SetLastSwitchedList(fileName);
      tooltipManager.Show($"Switched to '{fileName}'", contextSource);
    }

    private static void ImportFile (string fileName)
    {
      Dte.ExecuteCommand("Tools.ImportandExportSettings", $"/import:\"{fileName}\"");
    }

    public static void ResetShortcuts ()
    {
      foreach (var command in GetCommands())
      {
        var commandName = command.Name;
        if (string.IsNullOrEmpty(commandName))
          continue;

        if (commandName.StartsWith("ReSharper"))
          System.Diagnostics.Debugger.Launch();
        var bindings = KeyboardSchemes.VisualCSharp.TryGetValue(commandName, new object[0]);
        command.Bindings = bindings;
      }
    }

    //private static void GetValue3 ()
    //{
    //  var tempFile = Path.GetTempFileName();
    //  Dte.ExecuteCommand("Tools.ImportandExportSettings", $"/export:\"{tempFile}\"");
    //  var document = XDocument.Load(tempFile);
    //  document.Descendants("UserShortcuts").Remove();
    //  document.Save(tempFile);
    //  Dte.ExecuteCommand("Tools.ImportandExportSettings", $"/reset");
    //  Dte.ExecuteCommand("Tools.ImportandExportSettings", $"/import:\"{tempFile}\"");
    //}

    //private static void GetValue1 ()
    //{
    //  var tempFile = Path.GetTempFileName();
    //  Dte.ExecuteCommand("Tools.ImportandExportSettings", $"/export:\"{tempFile}\"");

    //  var lines = File.ReadAllLines(tempFile);
    //  lines = lines.Select(InvertTag).ToArray();
    //  File.WriteAllLines(tempFile, lines);

    //  ImportFile(tempFile);
    //}

    //private static string InvertTag (string tag)
    //{
    //  if (tag.Contains("<Shortcut"))
    //    return tag.Replace("Shortcut", "RemoveShortcut");
    //  if (tag.Contains("<RemoveShortcut"))
    //    return tag.Replace("RemoveShortcut", "Shortcut");

    //  return tag;
    //}

    //private static void GetValue ()
    //{
    //  var tempFile = @"C:\Users\matthias.koch\Desktop\bla.txt";
    //  using (var file = File.Open(tempFile, FileMode.Create))
    //  using (var writer = new StreamWriter(file))
    //  {
    //    //writer.WriteLine("<UserShortcuts>");
    //    foreach (var c in Dte.Commands.OfType<Command>())
    //    {
    //      if (string.IsNullOrEmpty(c.Name))
    //        continue;
    //      //c.Bindings = new object[0];
    //      //if (string.IsNullOrEmpty(c.Name))
    //      //  continue;

    //      var bindings = (object[]) c.Bindings;
    //      if (bindings.Length == 0)
    //        continue;

    //      var array = bindings.Select(x => '"' + x.ToString() + '"').Join(", ");
    //      var tag = $"{{ \"{c.Name}\", new[] {{ {array} }} }}";
    //      writer.WriteLine(tag);
    //    }
    //    //writer.WriteLine("</UserShortcuts>");
    //  }

    //  //ImportFile(tempFile);
    //}
  }
}