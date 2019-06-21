﻿namespace Gu.Inject.Analyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal static class KnownSymbol
    {
        internal static readonly QualifiedType Object = new QualifiedType("System.Object", "object");
        internal static readonly QualifiedType Boolean = new QualifiedType("System.Boolean", "bool");
        internal static readonly QualifiedType KernelOfT = new QualifiedType("Gu.Inject.Kernel`1");
    }
}
