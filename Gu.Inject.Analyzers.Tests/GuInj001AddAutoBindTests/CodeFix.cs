﻿namespace Gu.Inject.Analyzers.Tests.GuInj001AddAutoBindTests
{
    using Gu.Inject.Analyzers.CodeFixes;
    using Gu.Inject.Analyzers.NodeAnalyzers;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new ObjectCreationAnalyzer();
        private static readonly CodeFixProvider Fix = new AutoBindFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(GuInj001AddAutoBind.Descriptor);

        [Test]
        public static void MessageWhenExtensionMethodExists()
        {
            var autoBindCode = @"
namespace RoslynSandbox
{
    using Gu.Inject;

    [System.Runtime.CompilerServices.CompilerGenerated]
    public static class KernelExtensions
    {
        public static Kernel<C> AutoBind(this Kernel<C> kernel)
        { 
            kernel.Bind(() => new C());
            return kernel;
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
            var x = ↓new Kernel<C>();
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Use AutoBind() to wire up bindings."), autoBindCode, testCode);
        }

        [Test]
        public static void WhenExtensionMethodExists()
        {
            var autoBindCode = @"
namespace RoslynSandbox
{
    using Gu.Inject;

    [System.Runtime.CompilerServices.CompilerGenerated]
    public static class KernelExtensions
    {
        public static Kernel<C> AutoBind(this Kernel<C> kernel)
        { 
            kernel.Bind(() => new C());
            return kernel;
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
            var x = ↓new Kernel<C>();
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using Gu.Inject;

    public class C
    {
        public C()
        {
            var x = new Kernel<C>().AutoBind();
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { autoBindCode, testCode }, fixedCode);
        }

        [Test]
        public static void WhenNoExtensionMethod()
        {
            var autoBindCode = @"
namespace RoslynSandbox
{
    using Gu.Inject;

    [System.Runtime.CompilerServices.CompilerGenerated]
    public static class KernelExtensions
    {
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
            var x = ↓new Kernel<C>();
        }
    }
}";

            var fixedAutoBindCode = @"
namespace RoslynSandbox
{
    using Gu.Inject;

    [System.Runtime.CompilerServices.CompilerGenerated]
    public static class KernelExtensions
    {
        public static Kernel<C> AutoBind(this Kernel<C> kernel)
        { 
            kernel.Bind(() => new C());
            return kernel;
        }
    }
}";

            var fixedTestCode = @"
namespace RoslynSandbox
{
    using Gu.Inject;

    public class C
    {
        public C()
        {
            var x = new Kernel<C>().AutoBind();
        }
    }
}";

            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { autoBindCode, testCode }, new[] { fixedAutoBindCode, fixedTestCode });
        }

        [Test]
        public static void WhenCallingBind()
        {
            var autoBindCode = @"
namespace RoslynSandbox
{
    using Gu.Inject;

    [System.Runtime.CompilerServices.CompilerGenerated]
    public static class KernelExtensions
    {
        public static Kernel<C> AutoBind(this Kernel<C> kernel)
        { 
            kernel.Bind(() => new C());
            return kernel;
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
            var x = ↓new Kernel<C>().Bind(() => new C());
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, autoBindCode, testCode);
        }
    }
}
