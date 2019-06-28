﻿namespace Gu.Inject.Analyzers.Tests.GuInj001AddAutoBindTests
{
    using Gu.Inject.Analyzers.NodeAnalyzers;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new ObjectCreationAnalyzer();

        [Test]
        public static void WhenNotContainer()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Text;

    public class C
    {
        public C()
        {
            var x = new StringBuilder();
        }
    }
}";
            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void WhenContainerAndCallingAutoBind()
        {
            var autoBindCode = @"
namespace RoslynSandbox
{
    using Gu.Inject;

    public static class ContainerExtensions
    {
        public static Container<C> AutoBind(this Container<C> container)
        { 
            container.Bind(() => new C());
            return container;
        }
    }
}";
            var testCode = @"
namespace RoslynSandbox
{
    using Gu.Inject;

    public class C
    {
        public C()
        {
            var x = new Container<C>().AutoBind();
        }
    }
}";
            RoslynAssert.Valid(Analyzer, autoBindCode, testCode);
        }

        [Test]
        public static void WhenContainerAndCallingBindAndAutoBind()
        {
            var autoBindCode = @"
namespace RoslynSandbox
{
    using Gu.Inject;

    public static class ContainerExtensions
    {
        public static Container<C> AutoBind(this Container<C> container)
        { 
            container.Bind(() => new C());
            return container;
        }
    }
}";
            var testCode = @"
namespace RoslynSandbox
{
    using Gu.Inject;

    public class C
    {
        public C()
        {
            var x = new Container<C>().Bind(() => new C()).AutoBind();
        }
    }
}";
            RoslynAssert.Valid(Analyzer, autoBindCode, testCode);
        }

        [Test]
        public static void WhenContainerOfObject()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using Gu.Inject;

    public class C
    {
        public C()
        {
            var x = new Container<object>();
        }
    }
}";
            RoslynAssert.Valid(Analyzer, testCode);
        }
    }
}
