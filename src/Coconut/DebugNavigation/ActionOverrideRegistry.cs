using System;
using System.Linq;
using JetBrains.ActionManagement;
using JetBrains.Application;
using JetBrains.ReSharper.Features.Navigation.Features.GoToDeclaration;

namespace Coconut.DebugNavigation
{
  [ShellComponent]
  public class ActionOverrideRegistry
  {
    public ActionOverrideRegistry (IActionManager manager)
    {
      var gotoDeclarationAction = manager.Defs.GetActionDef(typeof(GotoDeclarationAction));
      manager.Handlers.AddHandler(gotoDeclarationAction, new GotoDebugDeclarationAction());
    }
  }
}