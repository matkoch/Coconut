using System;
using System.Linq;

namespace Coconut.DebugNavigation
{
  public class Expression
  {
    public static Expression Parse(EnvDTE.Expression expression)
    {
      return new Expression(expression.Name, expression.Type, expression.Value);
    }

    public Expression (string name, string type, string value)
    {
      Name = name;
      Type = type;
      Value = value;
    }

    public string Name { get; }
    public string Type { get; }
    public string Value { get; }

    public override string ToString ()
    {
      return Name;
    }
  }
}