﻿// ReSharper disable All
namespace Gu.Inject.Tests.Types
{
    using System;

    public class StaticConstructor
    {
        private static int n;

        static StaticConstructor()
        {
            n++;
        }

        public StaticConstructor()
        {
            if (n != 1)
            {
                throw new InvalidOperationException($"Static constructor ran {n} times.");
            }
        }
    }
}
