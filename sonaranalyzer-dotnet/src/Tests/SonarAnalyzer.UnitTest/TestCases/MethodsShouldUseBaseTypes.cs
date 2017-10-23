using System;
using System.Collections.Generic;

namespace Something
{
    internal interface IFoo
    {
        bool IsFoo { get; }
    }

    public class Foo : IFoo
    {
        public bool IsFoo { get; set; }
    }
}

// Test that rule doesn't suggest base with inconsistent accessibility
namespace Test_25
{
    public class Bar
    {
        public void MethodOne(Something.Foo f)
        {
            var x = f.IsFoo;
        }

        protected void MethodTwo(Something.Foo f)
        {
            var x = f.IsFoo;
        }

        private void MethodThree(Something.Foo f) // Noncompliant
        {
            var x = f.IsFoo;
        }

        internal void MethodFour(Something.Foo f) // Noncompliant
        {
            var x = f.IsFoo;
        }
    }
}