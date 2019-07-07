﻿namespace Gu.Inject.Analyzers.Tests.GuInj001MissingBindingTests
{
    using System;
    using Gu.Inject.Analyzers.CodeFixes;
    using Gu.Inject.Analyzers.NodeAnalyzers;
    using Gu.Inject.Tests.Types;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new ObjectCreationAnalyzer();
        private static readonly CodeFixProvider Fix = new BindFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(GuInj001MissingBinding.Descriptor);

        [Test]
        public static void Message()
        {
            var before = @"
namespace RoslynSandbox
{
    using Gu.Inject;

    public class C
    {
        public C()
        {
            var x = ↓new Container<C>();
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Provide binding for the type C."), before);

            var after = @"
namespace RoslynSandbox
{
    using Gu.Inject;

    public class C
    {
        public C()
        {
            var x = new Container<C>()
                .Bind(() => new C());
        }
    }
}";

            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Generate binding.");
        }

        [Test]
        public static void FullyQualified()
        {
            var before = @"
namespace RoslynSandbox
{
    public class C
    {
        public C()
        {
            var container = ↓new Gu.Inject.Container<Gu.Inject.Tests.Types.Foo>();
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    public class C
    {
        public C()
        {
            var container = new Gu.Inject.Container<Gu.Inject.Tests.Types.Foo>()
                .Bind(() => new Gu.Inject.Tests.Types.Bar())
                .Bind(x => new Gu.Inject.Tests.Types.Foo(x.Get<Gu.Inject.Tests.Types.Bar>()));
        }
    }
}";

            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void WhenUsingExists()
        {
            var before = @"
namespace RoslynSandbox
{
    using Gu.Inject;
    using Gu.Inject.Tests.Types;

    public class C
    {
        public C()
        {
            var container = ↓new Container<Foo>();
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using Gu.Inject;
    using Gu.Inject.Tests.Types;

    public class C
    {
        public C()
        {
            var container = new Container<Foo>()
                .Bind(() => new Bar())
                .Bind(x => new Foo(x.Get<Bar>()));
        }
    }
}";

            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void WhenCallingBind()
        {
            var before = @"
namespace RoslynSandbox
{
    using Gu.Inject;
    using Gu.Inject.Tests.Types;

    public class C
    {
        public C()
        {
            var container = ↓new Container<Foo>()
                .Bind(() => new Bar());
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using Gu.Inject;
    using Gu.Inject.Tests.Types;

    public class C
    {
        public C()
        {
            var container = new Container<Foo>()
                .Bind(() => new Bar())
                .Bind(x => x.Get<Foo>());
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase(typeof(DefaultCtor))]
        [TestCase(typeof(ParameterlessCtor))]
        public static void SingleParameterlessCtor(Type type)
        {
            var before = @"
namespace RoslynSandbox
{
    using Gu.Inject;
    using Gu.Inject.Tests.Types;

    public class C
    {
        public C()
        {
            var container = ↓new Container<DefaultCtor>();
        }
    }
}".AssertReplace("DefaultCtor", type.Name);

            var after = @"
namespace RoslynSandbox
{
    using Gu.Inject;
    using Gu.Inject.Tests.Types;

    public class C
    {
        public C()
        {
            var container = new Container<DefaultCtor>()
                .Bind(() => new DefaultCtor());
        }
    }
}".AssertReplace("DefaultCtor", type.Name);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase(typeof(SingletonField))]
        [TestCase(typeof(SingletonProperty))]
        public static void Singleton(Type type)
        {
            var before = @"
namespace RoslynSandbox
{
    using Gu.Inject;
    using Gu.Inject.Tests.Types;

    public class C
    {
        public C()
        {
            var x = ↓new Container<SingletonField>();
        }
    }
}".AssertReplace("SingletonField", type.Name);

            var after = @"
namespace RoslynSandbox
{
    using Gu.Inject;
    using Gu.Inject.Tests.Types;

    public class C
    {
        public C()
        {
            var x = new Container<SingletonField>()
                .Bind(() => SingletonField.Instance);
        }
    }
}".AssertReplace("SingletonField", type.Name);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase(typeof(ParameterlessFactoryMethod))]
        public static void ParameterlessFactoryMethod(Type type)
        {
            var before = @"
namespace RoslynSandbox
{
    using Gu.Inject;
    using Gu.Inject.Tests.Types;

    public class C
    {
        public C()
        {
            var container = ↓new Container<ParameterlessFactoryMethod>();
        }
    }
}".AssertReplace("ParameterlessFactoryMethod", type.Name);

            var after = @"
namespace RoslynSandbox
{
    using Gu.Inject;
    using Gu.Inject.Tests.Types;

    public class C
    {
        public C()
        {
            var container = new Container<ParameterlessFactoryMethod>()
                .Bind(() => ParameterlessFactoryMethod.Create());
        }
    }
}".AssertReplace("ParameterlessFactoryMethod", type.Name);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase(typeof(FactoryMethod))]
        public static void FactoryMethod(Type type)
        {
            var before = @"
namespace RoslynSandbox
{
    using Gu.Inject;
    using Gu.Inject.Tests.Types;

    public class C
    {
        public C()
        {
            var container = ↓new Container<FactoryMethod>();
        }
    }
}".AssertReplace("FactoryMethod", type.Name);

            var after = @"
namespace RoslynSandbox
{
    using Gu.Inject;
    using Gu.Inject.Tests.Types;

    public class C
    {
        public C()
        {
            var container = new Container<FactoryMethod>()
                .Bind(() => new Bar())
                .Bind(x => FactoryMethod.Create(x.Get<Bar>()));
        }
    }
}".AssertReplace("FactoryMethod", type.Name);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void ContainingType()
        {
            var before = @"
namespace RoslynSandbox
{
    using Gu.Inject;

    public class C
    {
        public C()
        {
            var x = ↓new Container<C>();
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using Gu.Inject;

    public class C
    {
        public C()
        {
            var x = new Container<C>()
                .Bind(() => new C());
        }
    }
}";

            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        // [Explicit("Extremely slow, maybe due to the formatter?")]
        [Test]
        public static void Graph50()
        {
            var before = @"
namespace RoslynSandbox
{
    using Gu.Inject;
    using Gu.Inject.Tests.Types;

    public class C
    {
        public C()
        {
            var container = ↓new Container<Graph50.Node1>();
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using Gu.Inject;
    using Gu.Inject.Tests.Types;

    public class C
    {
        public C()
        {
            var container = new Container<Graph50.Node1>()
                .Bind(x => new Graph50.Node1(x.Get<Graph50.Node2>(), x.Get<Graph50.Node7>(), x.Get<Graph50.Node10>(), x.Get<Graph50.Node16>(), x.Get<Graph50.Node18>(), x.Get<Graph50.Node24>(), x.Get<Graph50.Node26>(), x.Get<Graph50.Node27>(), x.Get<Graph50.Node29>(), x.Get<Graph50.Node32>()))
                .Bind(x => new Graph50.Node2(x.Get<Graph50.Node4>(), x.Get<Graph50.Node8>(), x.Get<Graph50.Node16>(), x.Get<Graph50.Node48>()))
                .Bind(x => new Graph50.Node4(x.Get<Graph50.Node8>(), x.Get<Graph50.Node32>(), x.Get<Graph50.Node36>()))
                .Bind(() => new Graph50.Node8())
                .Bind(() => new Graph50.Node32())
                .Bind(() => new Graph50.Node36())
                .Bind(() => new Graph50.Node16())
                .Bind(() => new Graph50.Node48())
                .Bind(x => new Graph50.Node7(x.Get<Graph50.Node35>(), x.Get<Graph50.Node49>()))
                .Bind(() => new Graph50.Node35())
                .Bind(() => new Graph50.Node49())
                .Bind(() => new Graph50.Node10())
                .Bind(() => new Graph50.Node18())
                .Bind(x => new Graph50.Node24(x.Get<Graph50.Node48>()))
                .Bind(() => new Graph50.Node26())
                .Bind(() => new Graph50.Node27())
                .Bind(() => new Graph50.Node29());
        }
    }
}";

            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Explicit("Extremely slow, maybe due to the formatter?")]
        [Test]
        public static void Graph500()
        {
            var before = @"
namespace RoslynSandbox
{
    using Gu.Inject;
    using Gu.Inject.Tests.Types;

    public class C
    {
        public C()
        {
            var container = ↓new Container<Graph500.Node1>();
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using Gu.Inject;
    using Gu.Inject.Tests.Types;

    public class C
    {
        public C()
        {
            var container = new Container<Graph500.Node1>()
                .Bind(x => new Graph500.Node1(x.Get<Graph500.Node7>(), x.Get<Graph500.Node8>(), x.Get<Graph500.Node20>(), x.Get<Graph500.Node26>(), x.Get<Graph500.Node29>(), x.Get<Graph500.Node34>(), x.Get<Graph500.Node37>(), x.Get<Graph500.Node49>(), x.Get<Graph500.Node50>(), x.Get<Graph500.Node57>(), x.Get<Graph500.Node60>(), x.Get<Graph500.Node63>(), x.Get<Graph500.Node72>(), x.Get<Graph500.Node79>(), x.Get<Graph500.Node83>(), x.Get<Graph500.Node93>(), x.Get<Graph500.Node96>(), x.Get<Graph500.Node101>(), x.Get<Graph500.Node109>(), x.Get<Graph500.Node113>(), x.Get<Graph500.Node116>(), x.Get<Graph500.Node118>(), x.Get<Graph500.Node121>(), x.Get<Graph500.Node124>(), x.Get<Graph500.Node127>(), x.Get<Graph500.Node135>(), x.Get<Graph500.Node136>(), x.Get<Graph500.Node144>(), x.Get<Graph500.Node154>(), x.Get<Graph500.Node156>(), x.Get<Graph500.Node162>(), x.Get<Graph500.Node167>(), x.Get<Graph500.Node179>(), x.Get<Graph500.Node180>(), x.Get<Graph500.Node183>(), x.Get<Graph500.Node188>(), x.Get<Graph500.Node193>(), x.Get<Graph500.Node200>(), x.Get<Graph500.Node202>(), x.Get<Graph500.Node207>(), x.Get<Graph500.Node216>(), x.Get<Graph500.Node217>(), x.Get<Graph500.Node219>(), x.Get<Graph500.Node221>(), x.Get<Graph500.Node225>(), x.Get<Graph500.Node226>(), x.Get<Graph500.Node230>(), x.Get<Graph500.Node233>(), x.Get<Graph500.Node234>(), x.Get<Graph500.Node240>(), x.Get<Graph500.Node244>(), x.Get<Graph500.Node245>(), x.Get<Graph500.Node252>(), x.Get<Graph500.Node254>(), x.Get<Graph500.Node259>(), x.Get<Graph500.Node261>(), x.Get<Graph500.Node263>(), x.Get<Graph500.Node268>(), x.Get<Graph500.Node277>(), x.Get<Graph500.Node282>(), x.Get<Graph500.Node287>(), x.Get<Graph500.Node288>(), x.Get<Graph500.Node294>(), x.Get<Graph500.Node295>(), x.Get<Graph500.Node300>(), x.Get<Graph500.Node305>(), x.Get<Graph500.Node310>(), x.Get<Graph500.Node318>(), x.Get<Graph500.Node324>(), x.Get<Graph500.Node328>(), x.Get<Graph500.Node334>(), x.Get<Graph500.Node338>(), x.Get<Graph500.Node340>(), x.Get<Graph500.Node344>(), x.Get<Graph500.Node346>(), x.Get<Graph500.Node348>(), x.Get<Graph500.Node349>(), x.Get<Graph500.Node352>(), x.Get<Graph500.Node359>(), x.Get<Graph500.Node363>(), x.Get<Graph500.Node375>(), x.Get<Graph500.Node381>(), x.Get<Graph500.Node383>(), x.Get<Graph500.Node395>(), x.Get<Graph500.Node408>(), x.Get<Graph500.Node411>(), x.Get<Graph500.Node412>(), x.Get<Graph500.Node415>(), x.Get<Graph500.Node423>(), x.Get<Graph500.Node431>(), x.Get<Graph500.Node432>(), x.Get<Graph500.Node437>(), x.Get<Graph500.Node440>(), x.Get<Graph500.Node444>(), x.Get<Graph500.Node448>(), x.Get<Graph500.Node461>(), x.Get<Graph500.Node466>(), x.Get<Graph500.Node472>(), x.Get<Graph500.Node476>(), x.Get<Graph500.Node485>(), x.Get<Graph500.Node490>(), x.Get<Graph500.Node498>()))
                .Bind(x => new Graph500.Node7(x.Get<Graph500.Node35>(), x.Get<Graph500.Node63>(), x.Get<Graph500.Node77>(), x.Get<Graph500.Node105>(), x.Get<Graph500.Node210>(), x.Get<Graph500.Node217>(), x.Get<Graph500.Node329>(), x.Get<Graph500.Node378>(), x.Get<Graph500.Node420>(), x.Get<Graph500.Node441>(), x.Get<Graph500.Node448>(), x.Get<Graph500.Node455>()))
                .Bind(x => new Graph500.Node35(x.Get<Graph500.Node175>(), x.Get<Graph500.Node210>(), x.Get<Graph500.Node280>(), x.Get<Graph500.Node350>(), x.Get<Graph500.Node455>()))
                .Bind(() => new Graph500.Node175())
                .Bind(() => new Graph500.Node210())
                .Bind(() => new Graph500.Node280())
                .Bind(() => new Graph500.Node350())
                .Bind(() => new Graph500.Node455())
                .Bind(x => new Graph500.Node63(x.Get<Graph500.Node126>()))
                .Bind(() => new Graph500.Node126())
                .Bind(x => new Graph500.Node77(x.Get<Graph500.Node308>(), x.Get<Graph500.Node385>(), x.Get<Graph500.Node462>()))
                .Bind(() => new Graph500.Node308())
                .Bind(() => new Graph500.Node385())
                .Bind(() => new Graph500.Node462())
                .Bind(() => new Graph500.Node105())
                .Bind(() => new Graph500.Node217())
                .Bind(() => new Graph500.Node329())
                .Bind(() => new Graph500.Node378())
                .Bind(() => new Graph500.Node420())
                .Bind(() => new Graph500.Node441())
                .Bind(() => new Graph500.Node448())
                .Bind(x => new Graph500.Node8(x.Get<Graph500.Node40>(), x.Get<Graph500.Node168>(), x.Get<Graph500.Node216>(), x.Get<Graph500.Node232>(), x.Get<Graph500.Node248>(), x.Get<Graph500.Node264>(), x.Get<Graph500.Node272>(), x.Get<Graph500.Node304>(), x.Get<Graph500.Node312>(), x.Get<Graph500.Node456>(), x.Get<Graph500.Node464>()))
                .Bind(x => new Graph500.Node40(x.Get<Graph500.Node80>(), x.Get<Graph500.Node160>(), x.Get<Graph500.Node200>(), x.Get<Graph500.Node360>(), x.Get<Graph500.Node480>()))
                .Bind(() => new Graph500.Node80())
                .Bind(() => new Graph500.Node160())
                .Bind(x => new Graph500.Node200(x.Get<Graph500.Node400>()))
                .Bind(() => new Graph500.Node400())
                .Bind(() => new Graph500.Node360())
                .Bind(() => new Graph500.Node480())
                .Bind(() => new Graph500.Node168())
                .Bind(x => new Graph500.Node216(x.Get<Graph500.Node432>()))
                .Bind(() => new Graph500.Node432())
                .Bind(() => new Graph500.Node232())
                .Bind(() => new Graph500.Node248())
                .Bind(() => new Graph500.Node264())
                .Bind(() => new Graph500.Node272())
                .Bind(() => new Graph500.Node304())
                .Bind(() => new Graph500.Node312())
                .Bind(() => new Graph500.Node456())
                .Bind(() => new Graph500.Node464())
                .Bind(x => new Graph500.Node20(x.Get<Graph500.Node60>(), x.Get<Graph500.Node240>(), x.Get<Graph500.Node260>(), x.Get<Graph500.Node380>()))
                .Bind(x => new Graph500.Node60(x.Get<Graph500.Node480>()))
                .Bind(() => new Graph500.Node240())
                .Bind(() => new Graph500.Node260())
                .Bind(() => new Graph500.Node380())
                .Bind(x => new Graph500.Node26(x.Get<Graph500.Node104>(), x.Get<Graph500.Node260>(), x.Get<Graph500.Node338>(), x.Get<Graph500.Node364>(), x.Get<Graph500.Node390>()))
                .Bind(() => new Graph500.Node104())
                .Bind(() => new Graph500.Node338())
                .Bind(() => new Graph500.Node364())
                .Bind(() => new Graph500.Node390())
                .Bind(x => new Graph500.Node29(x.Get<Graph500.Node261>(), x.Get<Graph500.Node406>()))
                .Bind(() => new Graph500.Node261())
                .Bind(() => new Graph500.Node406())
                .Bind(x => new Graph500.Node34(x.Get<Graph500.Node68>(), x.Get<Graph500.Node340>(), x.Get<Graph500.Node476>()))
                .Bind(x => new Graph500.Node68(x.Get<Graph500.Node136>(), x.Get<Graph500.Node476>()))
                .Bind(() => new Graph500.Node136())
                .Bind(() => new Graph500.Node476())
                .Bind(() => new Graph500.Node340())
                .Bind(x => new Graph500.Node37(x.Get<Graph500.Node74>(), x.Get<Graph500.Node370>()))
                .Bind(x => new Graph500.Node74(x.Get<Graph500.Node222>(), x.Get<Graph500.Node370>()))
                .Bind(() => new Graph500.Node222())
                .Bind(() => new Graph500.Node370())
                .Bind(x => new Graph500.Node49(x.Get<Graph500.Node147>(), x.Get<Graph500.Node196>(), x.Get<Graph500.Node441>(), x.Get<Graph500.Node490>()))
                .Bind(() => new Graph500.Node147())
                .Bind(() => new Graph500.Node196())
                .Bind(() => new Graph500.Node490())
                .Bind(() => new Graph500.Node50())
                .Bind(x => new Graph500.Node57(x.Get<Graph500.Node285>()))
                .Bind(() => new Graph500.Node285())
                .Bind(x => new Graph500.Node72(x.Get<Graph500.Node144>(), x.Get<Graph500.Node288>()))
                .Bind(() => new Graph500.Node144())
                .Bind(() => new Graph500.Node288())
                .Bind(x => new Graph500.Node79(x.Get<Graph500.Node237>()))
                .Bind(() => new Graph500.Node237())
                .Bind(x => new Graph500.Node83(x.Get<Graph500.Node249>(), x.Get<Graph500.Node498>()))
                .Bind(() => new Graph500.Node249())
                .Bind(() => new Graph500.Node498())
                .Bind(() => new Graph500.Node93())
                .Bind(() => new Graph500.Node96())
                .Bind(() => new Graph500.Node101())
                .Bind(x => new Graph500.Node109(x.Get<Graph500.Node327>(), x.Get<Graph500.Node436>()))
                .Bind(() => new Graph500.Node327())
                .Bind(() => new Graph500.Node436())
                .Bind(x => new Graph500.Node113(x.Get<Graph500.Node226>()))
                .Bind(x => new Graph500.Node226(x.Get<Graph500.Node452>()))
                .Bind(() => new Graph500.Node452())
                .Bind(x => new Graph500.Node116(x.Get<Graph500.Node348>()))
                .Bind(() => new Graph500.Node348())
                .Bind(x => new Graph500.Node118(x.Get<Graph500.Node236>()))
                .Bind(x => new Graph500.Node236(x.Get<Graph500.Node472>()))
                .Bind(() => new Graph500.Node472())
                .Bind(x => new Graph500.Node121(x.Get<Graph500.Node363>()))
                .Bind(() => new Graph500.Node363())
                .Bind(x => new Graph500.Node124(x.Get<Graph500.Node248>(), x.Get<Graph500.Node496>()))
                .Bind(() => new Graph500.Node496())
                .Bind(() => new Graph500.Node127())
                .Bind(() => new Graph500.Node135())
                .Bind(() => new Graph500.Node154())
                .Bind(() => new Graph500.Node156())
                .Bind(() => new Graph500.Node162())
                .Bind(() => new Graph500.Node167())
                .Bind(() => new Graph500.Node179())
                .Bind(() => new Graph500.Node180())
                .Bind(() => new Graph500.Node183())
                .Bind(() => new Graph500.Node188())
                .Bind(x => new Graph500.Node193(x.Get<Graph500.Node386>()))
                .Bind(() => new Graph500.Node386())
                .Bind(() => new Graph500.Node202())
                .Bind(x => new Graph500.Node207(x.Get<Graph500.Node414>()))
                .Bind(() => new Graph500.Node414())
                .Bind(() => new Graph500.Node219())
                .Bind(x => new Graph500.Node221(x.Get<Graph500.Node442>()))
                .Bind(() => new Graph500.Node442())
                .Bind(() => new Graph500.Node225())
                .Bind(() => new Graph500.Node230())
                .Bind(() => new Graph500.Node233())
                .Bind(() => new Graph500.Node234())
                .Bind(() => new Graph500.Node244())
                .Bind(x => new Graph500.Node245(x.Get<Graph500.Node490>()))
                .Bind(() => new Graph500.Node252())
                .Bind(() => new Graph500.Node254())
                .Bind(() => new Graph500.Node259())
                .Bind(() => new Graph500.Node263())
                .Bind(() => new Graph500.Node268())
                .Bind(() => new Graph500.Node277())
                .Bind(() => new Graph500.Node282())
                .Bind(() => new Graph500.Node287())
                .Bind(() => new Graph500.Node294())
                .Bind(() => new Graph500.Node295())
                .Bind(() => new Graph500.Node300())
                .Bind(() => new Graph500.Node305())
                .Bind(() => new Graph500.Node310())
                .Bind(() => new Graph500.Node318())
                .Bind(() => new Graph500.Node324())
                .Bind(() => new Graph500.Node328())
                .Bind(() => new Graph500.Node334())
                .Bind(() => new Graph500.Node344())
                .Bind(() => new Graph500.Node346())
                .Bind(() => new Graph500.Node349())
                .Bind(() => new Graph500.Node352())
                .Bind(() => new Graph500.Node359())
                .Bind(() => new Graph500.Node375())
                .Bind(() => new Graph500.Node381())
                .Bind(() => new Graph500.Node383())
                .Bind(() => new Graph500.Node395())
                .Bind(() => new Graph500.Node408())
                .Bind(() => new Graph500.Node411())
                .Bind(() => new Graph500.Node412())
                .Bind(() => new Graph500.Node415())
                .Bind(() => new Graph500.Node423())
                .Bind(() => new Graph500.Node431())
                .Bind(() => new Graph500.Node437())
                .Bind(() => new Graph500.Node440())
                .Bind(() => new Graph500.Node444())
                .Bind(() => new Graph500.Node461())
                .Bind(() => new Graph500.Node466())
                .Bind(() => new Graph500.Node485());
        }
    }
}";

            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Explicit("Maybe we should offer to call the extension method?")]
        [Test]
        public static void WhenExtensionMethodExists()
        {
            var autoBindCode = @"
namespace RoslynSandbox
{
    public static class ContainerExtensions
    {
        public static Gu.Inject.Container<C> BindC(this Gu.Inject.Container<C> container) => container
            .Bind(() => new C());
    }
}";

            var before = @"
namespace RoslynSandbox
{
    using Gu.Inject;

    public class C
    {
        public C()
        {
            var container = ↓new Container<C>();
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using Gu.Inject;

    public class C
    {
        public C()
        {
            var container = new Container<C>().BindC();
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { autoBindCode, before }, before);
        }

        [Explicit("Maybe we should offer to call the extension method?")]
        [Test]
        public static void WhenExtensionMethodExistsInDifferentNamespace()
        {
            var autoBindCode = @"
namespace RoslynSandbox.Extensions
{
    /// <summary>
    /// Extension methods for <see cref=""Gu.Inject.Container{T}"" />.
    /// This file is generated by Gu.Inject.Analyzers.
    /// </summary>
    // <auto-generated/>
    public static class ContainerExtensions
    {
        /// <summary>
        /// Adds generated bindings for the graph where <see cref=""C""/> is root.
        /// This method is generated by Gu.Inject.Analyzers.
        /// </summary>
        /// <param name=""container"">The <see cref=""Gu.Inject.Container{C}""/>.</param>
        public static Gu.Inject.Container<C> BindC(this Gu.Inject.Container<C> container) => container
            .Bind(() => new C());
    }
}";

            var before = @"
namespace RoslynSandbox
{
    using Gu.Inject;

    public class C
    {
        public C()
        {
            var x = ↓new Container<C>();
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using Gu.Inject;
    using RoslynSandbox.Extensions;

    public class C
    {
        public C()
        {
            var x = new Container<C>().BindC();
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { autoBindCode, before }, after);
        }

        [TestCase(typeof(TwoConstructors))]
        [TestCase(typeof(ParamsCtor))]
        [TestCase(typeof(IntCtor))]
        [TestCase(typeof(DoubletonField))]
        [TestCase(typeof(DoubletonProperty))]
        public static void Ignore(Type type)
        {
            var before = @"
namespace RoslynSandbox
{
    using Gu.Inject;
    using Gu.Inject.Tests.Types;

    public class C
    {
        public C()
        {
            var x = ↓new Container<TwoConstructors>();
        }
    }
}".AssertReplace("TwoConstructors", type.Name);

            var after = @"
namespace RoslynSandbox
{
    using Gu.Inject;
    using Gu.Inject.Tests.Types;

    public class C
    {
        public C()
        {
            var x = new Container<TwoConstructors>().AutoBind();
        }
    }
}".AssertReplace("TwoConstructors", type.Name);

            RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
