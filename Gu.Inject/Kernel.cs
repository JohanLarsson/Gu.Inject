﻿namespace Gu.Inject
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// A factory for resolving object graphs.
    /// </summary>
    public sealed class Kernel : IDisposable
    {
        private readonly ConcurrentDictionary<Type, object> map = new ConcurrentDictionary<Type, object>();
        private readonly ConcurrentDictionary<Type, Type> bindings = new ConcurrentDictionary<Type, Type>();
        private readonly HashSet<Type> resolved = new HashSet<Type>();

        private bool disposed;

        /// <summary>
        /// This notifies before creating an instance of a type.
        /// </summary>
        public event EventHandler<Type> Resolving;

        /// <summary>
        /// Provide an override to the automatic mapping.
        /// </summary>
        /// <typeparam name="TInterface">The type to map.</typeparam>
        /// <typeparam name="TConcrete">The mapped type.</typeparam>
        public void Bind<TInterface, TConcrete>()
            where TConcrete : TInterface
        {
            this.Bind(typeof(TInterface), typeof(TConcrete));
        }

        /// <summary>
        /// Provide an override to the automatic mapping.
        /// </summary>
        /// <typeparam name="TInterface1">The first type to map.</typeparam>
        /// <typeparam name="TInterface2">The second type to map.</typeparam>
        /// <typeparam name="TConcrete">The mapped type.</typeparam>
        public void Bind<TInterface1, TInterface2, TConcrete>()
            where TConcrete : TInterface1, TInterface2
        {
            this.Bind(typeof(TInterface1), typeof(TConcrete));
            this.Bind(typeof(TInterface2), typeof(TConcrete));
        }

        /// <summary>
        /// Provide an override to the automatic mapping.
        /// </summary>
        /// <param name="from">The type to map.</param>
        /// <param name="to">The mapped type.</param>
        public void Bind(Type from, Type to)
        {
            if (this.map.Count != 0)
            {
                throw new InvalidOperationException("Bind not allowed after Get.");
            }

            this.bindings.AddOrUpdate(
                from,
                t => to,
                (t1, t2) => throw new InvalidOperationException($"{t1.PrettyName()} already has a binding to {t2.PrettyName()}"));
        }

        /// <summary>
        /// Provide an override to a mapping.
        /// </summary>
        /// <typeparam name="TInterface">The type to map.</typeparam>
        /// <typeparam name="TConcrete">The mapped type</typeparam>
        public void ReBind<TInterface, TConcrete>()
            where TConcrete : TInterface
        {
            this.ReBind(typeof(TInterface), typeof(TConcrete));
        }

        /// <summary>
        /// Provide an override to a mapping.
        /// </summary>
        /// <param name="from">The type to map.</param>
        /// <param name="to">The mapped type.</param>
        public void ReBind(Type from, Type to)
        {
            if (this.map.Count != 0)
            {
                throw new InvalidOperationException("ReBind not allowed after Get.");
            }

            this.bindings.AddOrUpdate(
                from,
                t => throw new InvalidOperationException($"{t.PrettyName()} does not have a binding."),
                (t1, t2) => to);
        }

        /// <summary>
        /// Get the singleton instance of <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type to resolve.</typeparam>
        /// <returns>The singleton instance of <typeparamref name="T"/></returns>
        public T Get<T>()
            where T : class
        {
            if (!TypeMap.IsInitialized)
            {
                TypeMap.Initialize(Assembly.GetCallingAssembly());
            }

            return (T)this.Get(typeof(T));
        }

        /// <summary>
        /// Get the singleton instance of <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to resolve.</param>
        /// <returns>The singleton instance of <paramref name="type"/>.</returns>
        public object Get(Type type)
        {
            this.ThrowIfDisposed();
            if (!TypeMap.IsInitialized)
            {
                TypeMap.Initialize(Assembly.GetCallingAssembly());
            }

            return this.map.GetOrAdd(type, this.Resolve);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            foreach (var kvp in this.map)
            {
                (kvp.Value as IDisposable)?.Dispose();
            }

            this.map.Clear();
        }

        private object Resolve(Type type)
        {
            if (this.bindings.TryGetValue(type, out Type bound))
            {
                return this.map.GetOrAdd(bound, this.Resolve);
            }

            if (type.IsInterface || type.IsAbstract)
            {
                var mapped = TypeMap.GetMapped(type);
                if (mapped.Count == 0)
                {
                    throw new NoBindingException(type, mapped);
                }

                if (mapped.Count > 1)
                {
                    throw new AmbiguousBindingException(type, mapped);
                }

                if (mapped[0].IsGenericType && !type.IsGenericType)
                {
                    throw new AmbiguousGenericBindingException(type, mapped);
                }

                return this.map.GetOrAdd(mapped[0], this.Resolve);
            }

            if (!this.resolved.Add(type))
            {
                throw new CircularDependencyException(type);
            }

            var info = Ctor.GetInfo(type);
            this.Resolving?.Invoke(this, type);
            try
            {
                if (info.ParameterTypes.Count == 0)
                {
                    return info.CreateInstance(null);
                }

                var args = new object[info.ParameterTypes.Count];
                for (var i = 0; i < info.ParameterTypes.Count; i++)
                {
                    args[i] = this.map.GetOrAdd(info.ParameterTypes[i], this.Resolve);
                }

                return info.CreateInstance(args);
            }
            catch (ResolveException e)
            {
                throw new ResolveException(type, e);
            }
        }

        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }
    }
}
