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

using System.Drawing;
using JetBrains.Annotations;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Occurrences;
using JetBrains.ReSharper.Feature.Services.Presentation;
using JetBrains.ReSharper.Psi;
using JetBrains.UI.PopupMenu;
using JetBrains.UI.Resources;
using JetBrains.UI.RichText;
using JetBrains.Util;

namespace Coconut.Debugging.GotoBreakpoints
{
  [OccurrencePresenter]
  public class BreakpointOccurrencePresenter : IOccurrencePresenter
  {
    private static readonly TextStyle s_grayTextColor = TextStyle.FromForeColor(Color.Gray);

    private static readonly DeclaredElementPresenterStyle s_styleMemberName =
        new DeclaredElementPresenterStyle
        {
          ShowName = NameStyle.SHORT,
          ShowParameterTypes = true,
          ShowParameterNames = false,
          ShowType = TypeStyle.AFTER,
          ShowTypeParameters = TypeParameterStyle.FULL_WITH_VARIANCE
        };

    public bool IsApplicable ([NotNull] IOccurrence occurrence)
    {
      return occurrence is BreakpointOccurrence;
    }

    public bool Present (
      [NotNull] IMenuItemDescriptor descriptor,
      [NotNull] IOccurrence occurrence,
      OccurrencePresentationOptions occurrencePresentationOptions)
    {
      if (!occurrence.IsValid)
        return false;

      var breakpointOccurrence = occurrence as BreakpointOccurrence;
      if (breakpointOccurrence == null)
        return false;

      var breakpoint = breakpointOccurrence.Breakpoint;
      var sourceFile = breakpointOccurrence.SourceFile;
      var declaredElement = breakpointOccurrence.DeclaredElement;

      DeclaredElementPresenterMarking marking;
      var declaredElementText = DeclaredElementMenuItemFormatter.FormatText(declaredElement, sourceFile.PrimaryPsiLanguage, out marking);
      var lineAppendix = new RichText(" (Line " + breakpoint.FunctionLineOffset + ")");
      var fullText = declaredElementText + lineAppendix;
      fullText.SetStyle(s_grayTextColor, fullText.Length - lineAppendix.Length, lineAppendix.Length);

      descriptor.Style = MenuItemStyle.Enabled;
      descriptor.Icon = CommonThemedIcons.Abort.Id;
      descriptor.Text = fullText;
      descriptor.TailGlyph = breakpointOccurrence.GetSolution().NotNull().GetComponent<PsiSourceFilePresentationService>().GetIconId(sourceFile);
      descriptor.ShortcutText = new RichText(sourceFile.DisplayName, s_grayTextColor);

      descriptor.Tooltip = breakpoint.GetLineText(sourceFile);

      return true;
    }

    //[CanBeNull]
    //private IDeclaredElement GetDeclaredElement (BreakpointEnvoy envoy, IPsiSourceFile sourceFile)
    //{
    //  var offset = envoy.GetOffset(sourceFile);
    //  TextControlToPsi.GetDeclaredElements(sourceFile.GetSolution(), sourceFile.Document, offset, );
    //}
  }
}