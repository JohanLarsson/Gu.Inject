// ReSharper disable RedundantTypeArgumentsOfMethod
#pragma warning disable IDE0001
namespace Gu.Inject
{
    using System;

    /// <summary>
    /// Rebind extension methods for <see cref="Kernel"/>.
    /// </summary>
    public static class RebindExtensions
    {
        /// <summary>
        /// Check if there is a binding for <paramref name="type"/>.
        /// </summary>
        /// <param name="kernel">The <see cref="Kernel"/>.</param>
        /// <param name="type">The type to resolve.</param>
        /// <returns>True if there is a binding.</returns>
        public static bool HasBinding(this Kernel kernel, Type type)
        {
            if (kernel is null)
            {
                throw new ArgumentNullException(nameof(kernel));
            }

            return kernel.HasBinding(type);
        }

        /// <summary>
        /// Provide an override to the automatic mapping.
        /// </summary>
        /// <param name="kernel">The <see cref="Kernel"/>.</param>
        /// <param name="from">The type to map.</param>
        /// <param name="to">The mapped type.</param>
        /// <returns>The same instance.</returns>
        public static Kernel Rebind(this Kernel kernel, Type from, Type to)
        {
            if (kernel is null)
            {
                throw new ArgumentNullException(nameof(kernel));
            }

            if (from is null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            if (to is null)
            {
                throw new ArgumentNullException(nameof(to));
            }

            kernel.Rebind(from, Binding.Map(to), requireExistingBinding: true);
            return kernel;
        }

        /// <summary>
        /// Use this binding when there are circular dependencies.
        /// This binds an FormatterServices.GetUninitializedObject() that is used when creating the graph.
        /// </summary>
        /// <typeparam name="T">The type to map.</typeparam>
        /// <param name="kernel">The <see cref="Kernel"/>.</param>
        /// <returns>The same instance.</returns>
        public static Kernel RebindUninitialized<T>(this Kernel kernel)
            where T : class
        {
            if (kernel is null)
            {
                throw new ArgumentNullException(nameof(kernel));
            }

            kernel.Rebind(typeof(T), Binding.Uninitialized<T>(), requireExistingBinding: true);
            return kernel;
        }

        /// <summary>
        /// Provide an override to the automatic mapping.
        /// </summary>
        /// <typeparam name="TInterface">The type to map.</typeparam>
        /// <typeparam name="TConcrete">The mapped type.</typeparam>
        /// <param name="kernel">The <see cref="Kernel"/>.</param>
        /// <returns>The same instance.</returns>
        public static Kernel Rebind<TInterface, TConcrete>(this Kernel kernel)
            where TConcrete : TInterface
        {
            if (kernel is null)
            {
                throw new ArgumentNullException(nameof(kernel));
            }

            kernel.Rebind(typeof(TInterface), Binding.Map<TConcrete>(), requireExistingBinding: true);
            return kernel;
        }

        /// <summary>
        /// Provide an override to the automatic mapping.
        /// </summary>
        /// <typeparam name="TInterface1">The first type to map.</typeparam>
        /// <typeparam name="TInterface2">The second type to map.</typeparam>
        /// <typeparam name="TConcrete">The mapped type.</typeparam>
        /// <param name="kernel">The <see cref="Kernel"/>.</param>
        /// <returns>The same instance.</returns>
        public static Kernel Rebind<TInterface1, TInterface2, TConcrete>(this Kernel kernel)
            where TConcrete : TInterface1, TInterface2
        {
            if (kernel is null)
            {
                throw new ArgumentNullException(nameof(kernel));
            }

            kernel.Rebind(typeof(TInterface1), Binding.Map<TConcrete>(), requireExistingBinding: true);
            kernel.Rebind(typeof(TInterface2), Binding.Map<TConcrete>(), requireExistingBinding: true);
            return kernel;
        }

        /// <summary>
        /// Provide an override to the automatic mapping.
        /// </summary>
        /// <typeparam name="TInterface1">The first type to map.</typeparam>
        /// <typeparam name="TInterface2">The second type to map.</typeparam>
        /// <typeparam name="TInterface3">The third type to map.</typeparam>
        /// <typeparam name="TConcrete">The mapped type.</typeparam>
        /// <param name="kernel">The <see cref="Kernel"/>.</param>
        /// <returns>The same instance.</returns>
        public static Kernel Rebind<TInterface1, TInterface2, TInterface3, TConcrete>(this Kernel kernel)
            where TConcrete : TInterface1, TInterface2, TInterface3
        {
            if (kernel is null)
            {
                throw new ArgumentNullException(nameof(kernel));
            }

            kernel.Rebind(typeof(TInterface1), Binding.Map<TConcrete>(), requireExistingBinding: true);
            kernel.Rebind(typeof(TInterface2), Binding.Map<TConcrete>(), requireExistingBinding: true);
            kernel.Rebind(typeof(TInterface3), Binding.Map<TConcrete>(), requireExistingBinding: true);
            return kernel;
        }

        /// <summary>
        /// Provide an override for the automatic mapping.
        /// If the <paramref name="instance"/> implements IDisposable, the responsibility to dispose it remains the caller's, disposing the kernel doesn't do that.
        /// </summary>
        /// <typeparam name="T">The mapped type.</typeparam>
        /// <param name="kernel">The <see cref="Kernel"/>.</param>
        /// <param name="instance">The instance to bind.</param>
        /// <returns>The same instance.</returns>
        public static Kernel Rebind<T>(this Kernel kernel, T instance)
        {
            if (kernel is null)
            {
                throw new ArgumentNullException(nameof(kernel));
            }

            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (typeof(T) == instance.GetType())
            {
                kernel.Rebind(typeof(T), Binding.Instance(instance), requireExistingBinding: true);
            }
            else
            {
                kernel.Rebind(typeof(T), Binding.Mapped(instance), requireExistingBinding: true);
                kernel.Rebind(instance.GetType(), Binding.Instance(instance), requireExistingBinding: false);
            }

            return kernel;
        }

        /// <summary>
        /// Provide an override for the automatic mapping.
        /// If the <paramref name="instance"/> implements IDisposable, the responsibility to dispose it remains the caller's, disposing the kernel doesn't do that.
        /// </summary>
        /// <typeparam name="TInterface">The type to map.</typeparam>
        /// <typeparam name="TConcrete">The mapped type.</typeparam>
        /// <param name="kernel">The <see cref="Kernel"/>.</param>
        /// <param name="instance">The instance to bind.</param>
        /// <returns>The same instance.</returns>
        public static Kernel Rebind<TInterface, TConcrete>(this Kernel kernel, TConcrete instance)
            where TConcrete : TInterface
        {
            if (kernel is null)
            {
                throw new ArgumentNullException(nameof(kernel));
            }

            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            kernel.Rebind<TConcrete>(instance);
            kernel.Rebind(typeof(TInterface), Binding.Mapped(instance), requireExistingBinding: true);
            return kernel;
        }

        /// <summary>
        /// Provide an override for the automatic mapping.
        /// If the <paramref name="instance"/> implements IDisposable, the responsibility to dispose it remains the caller's, disposing the kernel doesn't do that.
        /// </summary>
        /// <typeparam name="TInterface1">The first type to map.</typeparam>
        /// <typeparam name="TInterface2">The second type to map.</typeparam>
        /// <typeparam name="TConcrete">The mapped type.</typeparam>
        /// <param name="kernel">The <see cref="Kernel"/>.</param>
        /// <param name="instance">The instance to bind.</param>
        /// <returns>The same instance.</returns>
        public static Kernel Rebind<TInterface1, TInterface2, TConcrete>(this Kernel kernel, TConcrete instance)
            where TConcrete : TInterface1, TInterface2
        {
            if (kernel is null)
            {
                throw new ArgumentNullException(nameof(kernel));
            }

            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            kernel.Rebind<TConcrete>(instance);
            kernel.Rebind(typeof(TInterface1), Binding.Mapped(instance), requireExistingBinding: true);
            kernel.Rebind(typeof(TInterface2), Binding.Mapped(instance), requireExistingBinding: true);
            return kernel;
        }

        /// <summary>
        /// Provide an override for the automatic mapping.
        /// If the <paramref name="instance"/> implements IDisposable, the responsibility to dispose it remains the caller's, disposing the kernel doesn't do that.
        /// </summary>
        /// <typeparam name="TInterface1">The first type to map.</typeparam>
        /// <typeparam name="TInterface2">The second type to map.</typeparam>
        /// <typeparam name="TInterface3">The third type to map.</typeparam>
        /// <typeparam name="TConcrete">The mapped type.</typeparam>
        /// <param name="kernel">The <see cref="Kernel"/>.</param>
        /// <param name="instance">The instance to bind.</param>
        /// <returns>The same instance.</returns>
        public static Kernel Rebind<TInterface1, TInterface2, TInterface3, TConcrete>(this Kernel kernel, TConcrete instance)
            where TConcrete : TInterface1, TInterface2, TInterface3
        {
            if (kernel is null)
            {
                throw new ArgumentNullException(nameof(kernel));
            }

            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            kernel.Rebind<TConcrete>(instance);
            kernel.Rebind(typeof(TInterface1), Binding.Mapped(instance), requireExistingBinding: true);
            kernel.Rebind(typeof(TInterface2), Binding.Mapped(instance), requireExistingBinding: true);
            kernel.Rebind(typeof(TInterface3), Binding.Mapped(instance), requireExistingBinding: true);
            return kernel;
        }

        /// <summary>
        /// Provide an override for the automatic mapping.
        /// The instance is created lazily by <paramref name="create"/> and is cached for subsequent calls to .Get().
        /// The instance is owned by the kernel, that is, calling .Dispose() on the kernel disposes the instance, if its type implements IDisposable.
        /// </summary>
        /// <typeparam name="T">The mapped type.</typeparam>
        /// <param name="kernel">The <see cref="Kernel"/>.</param>
        /// <param name="create">The factory function used to create the instance.</param>
        /// <returns>The same instance.</returns>
        public static Kernel Rebind<T>(this Kernel kernel, Func<T> create)
        {
            if (kernel is null)
            {
                throw new ArgumentNullException(nameof(kernel));
            }

            if (create is null)
            {
                throw new ArgumentNullException(nameof(create));
            }

            kernel.Rebind(typeof(T), Binding.Func(create), requireExistingBinding: true);
            return kernel;
        }

        /// <summary>
        /// Provide an override for the automatic mapping.
        /// The instance is created lazily by <paramref name="create"/> and is cached for subsequent calls to .Get().
        /// </summary>
        /// <typeparam name="TInterface">The type to map.</typeparam>
        /// <typeparam name="TConcrete">The mapped type.</typeparam>
        /// <param name="kernel">The <see cref="Kernel"/>.</param>
        /// <param name="create">The factory function used to create the instance.</param>
        /// <returns>The same instance.</returns>
        public static Kernel Rebind<TInterface, TConcrete>(this Kernel kernel, Func<TConcrete> create)
            where TConcrete : TInterface
        {
            if (kernel is null)
            {
                throw new ArgumentNullException(nameof(kernel));
            }

            if (create is null)
            {
                throw new ArgumentNullException(nameof(create));
            }

            kernel.Rebind(typeof(TInterface), Binding.Map<TConcrete>(), requireExistingBinding: true);
            kernel.Rebind(typeof(TConcrete), Binding.Func(create), requireExistingBinding: true);
            return kernel;
        }

        /// <summary>
        /// Provide an override for the automatic mapping.
        /// The instance is created lazily by <paramref name="create"/> and is cached for subsequent calls to .Get().
        /// </summary>
        /// <typeparam name="TInterface1">The first type to map.</typeparam>
        /// <typeparam name="TInterface2">The second type to map.</typeparam>
        /// <typeparam name="TConcrete">The mapped type.</typeparam>
        /// <param name="kernel">The <see cref="Kernel"/>.</param>
        /// <param name="create">The factory function used to create the instance.</param>
        /// <returns>The same instance.</returns>
        public static Kernel Rebind<TInterface1, TInterface2, TConcrete>(this Kernel kernel, Func<TConcrete> create)
            where TConcrete : TInterface1, TInterface2
        {
            if (kernel is null)
            {
                throw new ArgumentNullException(nameof(kernel));
            }

            if (create is null)
            {
                throw new ArgumentNullException(nameof(create));
            }

            kernel.Rebind(typeof(TInterface1), Binding.Map<TConcrete>(), requireExistingBinding: true);
            kernel.Rebind(typeof(TInterface2), Binding.Map<TConcrete>(), requireExistingBinding: true);
            kernel.Rebind(typeof(TConcrete), Binding.Func(create), requireExistingBinding: true);
            return kernel;
        }

        /// <summary>
        /// Provide an override for the automatic mapping.
        /// The instance is created lazily by <paramref name="create"/> and is cached for subsequent calls to .Get().
        /// </summary>
        /// <typeparam name="TInterface1">The first type to map.</typeparam>
        /// <typeparam name="TInterface2">The second type to map.</typeparam>
        /// <typeparam name="TInterface3">The third type to map.</typeparam>
        /// <typeparam name="TConcrete">The mapped type.</typeparam>
        /// <param name="kernel">The <see cref="Kernel"/>.</param>
        /// <param name="create">The factory function used to create the instance.</param>
        /// <returns>The same instance.</returns>
        public static Kernel Rebind<TInterface1, TInterface2, TInterface3, TConcrete>(this Kernel kernel, Func<TConcrete> create)
            where TConcrete : TInterface1, TInterface2, TInterface3
        {
            if (kernel is null)
            {
                throw new ArgumentNullException(nameof(kernel));
            }

            if (create is null)
            {
                throw new ArgumentNullException(nameof(create));
            }

            kernel.Rebind(typeof(TInterface1), Binding.Map<TConcrete>(), requireExistingBinding: true);
            kernel.Rebind(typeof(TInterface2), Binding.Map<TConcrete>(), requireExistingBinding: true);
            kernel.Rebind(typeof(TInterface3), Binding.Map<TConcrete>(), requireExistingBinding: true);
            kernel.Rebind(typeof(TConcrete), Binding.Func(create), requireExistingBinding: true);
            return kernel;
        }

        /// <summary>
        /// Provide an override for the automatic mapping.
        /// The kernel will keep <paramref name="create"/> alive until disposed.
        /// <paramref name="create"/> is disposed by the kernel if disposable.
        /// </summary>
        /// <typeparam name="T">The mapped type.</typeparam>
        /// <param name="kernel">The <see cref="Kernel"/>.</param>
        /// <param name="create">The instance to bind.</param>
        /// <returns>The same instance.</returns>
        public static Kernel Rebind<T>(this Kernel kernel, Func<IReadOnlyKernel, T> create)
        {
            if (kernel is null)
            {
                throw new ArgumentNullException(nameof(kernel));
            }

            if (create is null)
            {
                throw new ArgumentNullException(nameof(create));
            }

            kernel.Rebind(typeof(T), Binding.Resolver(create), requireExistingBinding: true);
            return kernel;
        }

        /// <summary>
        /// Provide an override for the automatic mapping.
        /// The kernel will keep <paramref name="create"/> alive until disposed.
        /// <paramref name="create"/> is disposed by the kernel if disposable.
        /// </summary>
        /// <typeparam name="TInterface">The type to map.</typeparam>
        /// <typeparam name="TConcrete">The mapped type.</typeparam>
        /// <param name="kernel">The <see cref="Kernel"/>.</param>
        /// <param name="create">The instance to bind.</param>
        /// <returns>The same instance.</returns>
        public static Kernel Rebind<TInterface, TConcrete>(this Kernel kernel, Func<IReadOnlyKernel, TConcrete> create)
            where TConcrete : TInterface
        {
            if (kernel is null)
            {
                throw new ArgumentNullException(nameof(kernel));
            }

            if (create is null)
            {
                throw new ArgumentNullException(nameof(create));
            }

            kernel.Rebind(typeof(TInterface), Binding.Map<TConcrete>(), requireExistingBinding: true);
            kernel.Rebind(typeof(TConcrete), Binding.Resolver(create), requireExistingBinding: true);
            return kernel;
        }

        /// <summary>
        /// Provide an override for the automatic mapping.
        /// The kernel will keep <paramref name="create"/> alive until disposed.
        /// <paramref name="create"/> is disposed by the kernel if disposable.
        /// </summary>
        /// <typeparam name="TInterface1">The first type to map.</typeparam>
        /// <typeparam name="TInterface2">The second type to map.</typeparam>
        /// <typeparam name="TConcrete">The mapped type.</typeparam>
        /// <param name="kernel">The <see cref="Kernel"/>.</param>
        /// <param name="create">The instance to bind.</param>
        /// <returns>The same instance.</returns>
        public static Kernel Rebind<TInterface1, TInterface2, TConcrete>(this Kernel kernel, Func<IReadOnlyKernel, TConcrete> create)
            where TConcrete : TInterface1, TInterface2
        {
            if (kernel is null)
            {
                throw new ArgumentNullException(nameof(kernel));
            }

            if (create is null)
            {
                throw new ArgumentNullException(nameof(create));
            }

            kernel.Rebind(typeof(TInterface1), Binding.Map<TConcrete>(), requireExistingBinding: true);
            kernel.Rebind(typeof(TInterface2), Binding.Map<TConcrete>(), requireExistingBinding: true);
            kernel.Rebind(typeof(TConcrete), Binding.Resolver(create), requireExistingBinding: true);
            return kernel;
        }

        /// <summary>
        /// Provide an override for the automatic mapping.
        /// The kernel will keep <paramref name="create"/> alive until disposed.
        /// <paramref name="create"/> is disposed by the kernel if disposable.
        /// </summary>
        /// <typeparam name="TInterface1">The first type to map.</typeparam>
        /// <typeparam name="TInterface2">The second type to map.</typeparam>
        /// <typeparam name="TInterface3">The third type to map.</typeparam>
        /// <typeparam name="TConcrete">The mapped type.</typeparam>
        /// <param name="kernel">The <see cref="Kernel"/>.</param>
        /// <param name="create">The instance to bind.</param>
        /// <returns>The same instance.</returns>
        public static Kernel Rebind<TInterface1, TInterface2, TInterface3, TConcrete>(this Kernel kernel, Func<IReadOnlyKernel, TConcrete> create)
            where TConcrete : TInterface1, TInterface2
        {
            if (kernel is null)
            {
                throw new ArgumentNullException(nameof(kernel));
            }

            if (create is null)
            {
                throw new ArgumentNullException(nameof(create));
            }

            kernel.Rebind(typeof(TInterface1), Binding.Map<TConcrete>(), requireExistingBinding: true);
            kernel.Rebind(typeof(TInterface2), Binding.Map<TConcrete>(), requireExistingBinding: true);
            kernel.Rebind(typeof(TInterface3), Binding.Map<TConcrete>(), requireExistingBinding: true);
            kernel.Rebind(typeof(TConcrete), Binding.Resolver(create), requireExistingBinding: true);
            return kernel;
        }
    }
}
