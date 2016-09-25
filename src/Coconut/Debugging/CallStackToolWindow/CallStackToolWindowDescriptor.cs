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
using JetBrains.Application;
using JetBrains.Application.DataContext;
using JetBrains.ReSharper.Feature.Services.Resources;
using JetBrains.UI.ActionsRevised;
using JetBrains.UI.MenuGroups;
using JetBrains.UI.ToolWindowManagement;

namespace Coconut.Debugging.CallStackToolWindow
{
  [ToolWindowDescriptor (
     ProductNeutralId = "CallStack",
     Text = "CallStack",
     VisibilityPersistenceScope = ToolWindowVisibilityPersistenceScope.Solution,
     Type = ToolWindowType.MultiInstance,
     //Icon = typeof(ServicesThemedIcons.TypeHierarchyToolWindow),
     InitialDocking = ToolWindowInitialDocking.Floating,
     InitialWidth = 474,
     InitialHeight = 357
   )]
  public class CallStackToolWindowDescriptor : ToolWindowDescriptor
  {
    public CallStackToolWindowDescriptor (IApplicationHost host)
      : base(host)
    {
    }
  }

  [Action (
     Text: "&CallStack",
     Icon = typeof(ServicesThemedIcons.TypeHierarchyToolWindow),
     Id = 2)]
  public class CallStackWindowActionHandler
      : ActivateToolWindowActionHandler<CallStackToolWindowDescriptor>, IInsertFirst<WindowsMenu>
  {
    public override bool Update (IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
    {
      return DebuggingService.IsDebugging;
    }
  }
}