using System;

namespace Coconut.Debug
{
    using System.Linq;

    internal class Program
    {
        private static void Main (string[] args)
        {
            IA a = new Foo();
            Console.WriteLine();
            a.Muh();
        }
    }

    public interface IA
    {
        void Muh ();
    }

    public class Foo : IA
    {
        public void Muh ()
        {
        }
    }

    public class Bar : IA
    {
        public void Muh ()
        {
        }
    }
}
