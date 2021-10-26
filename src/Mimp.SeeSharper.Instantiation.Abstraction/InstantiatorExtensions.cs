using Mimp.SeeSharper.ObjectDescription.Abstraction;
using System;

namespace Mimp.SeeSharper.Instantiation.Abstraction
{
    public static class InstantiatorExtensions
    {


        public static object? Instantiate(this IInstantiator instantiator, Type type, IObjectDescription description)
        {
            if (instantiator is null)
                throw new ArgumentNullException(nameof(instantiator));

            var result = instantiator.Instantiate(type, description, out var ignored);
            if (ignored is not null)
                throw InstantiationException.GetUsedNotAllException(type, description, ignored);
            return result;
        }

        public static object? Initialize(this IInstantiator instantiator, Type type, object? instance, IObjectDescription description)
        {
            if (instantiator is null)
                throw new ArgumentNullException(nameof(instantiator));

            var result = instantiator.Initialize(type, instance, description, out var ignored);
            if (ignored is not null)
                throw InstantiationException.GetUsedNotAllException(type, description, ignored);
            return result;
        }


        public static T? Instantiate<T>(this IInstantiator instantiator, IObjectDescription description, out IObjectDescription? ignored)
        {
            if (instantiator is null)
                throw new ArgumentNullException(nameof(instantiator));

            return (T?)instantiator.Instantiate(typeof(T), description, out ignored);
        }

        public static T? Instantiate<T>(this IInstantiator instantiator, IObjectDescription description) =>
            (T?)instantiator.Instantiate(typeof(T), description);


        public static T? Initialize<T>(this IInstantiator instantiator, T? instance, IObjectDescription description, out IObjectDescription? ignored)
        {
            if (instantiator is null)
                throw new ArgumentNullException(nameof(instantiator));

            return (T?)instantiator.Initialize(typeof(T), instance, description, out ignored);
        }

        public static T? Initialize<T>(this IInstantiator instantiator, T? instance, IObjectDescription description) =>
            (T?)instantiator.Initialize(typeof(T), instantiator, description);


        public static object? Construct(this IInstantiator instantiator, Type type,
            IObjectDescription instantiate, out IObjectDescription? ignoredInstantiate,
            IObjectDescription initialize, out IObjectDescription? ignoredInitialize)
        {
            if (instantiator is null)
                throw new ArgumentNullException(nameof(instantiator));

            var instance = instantiator.Instantiate(type, instantiate, out ignoredInstantiate);
            return instantiator.Initialize(type, instance, initialize, out ignoredInitialize);
        }

        public static object? Construct(this IInstantiator instantiator, Type type,
            IObjectDescription instantiate, IObjectDescription initialize)
        {
            var instance = instantiator.Instantiate(type, instantiate);
            return instantiator.Initialize(type, instance, initialize);
        }


        public static T? Construct<T>(this IInstantiator instantiator,
                IObjectDescription instantiate, out IObjectDescription? ignoredInstantiate,
                IObjectDescription initialize, out IObjectDescription? ignoredInitialize
            ) => (T?)instantiator.Construct(typeof(T), instantiate, out ignoredInstantiate, initialize, out ignoredInitialize);

        public static T? Construct<T>(this IInstantiator instantiator,
            IObjectDescription instantiate, IObjectDescription initialize) =>
            (T?)instantiator.Construct(typeof(T), instantiate, initialize);


    }
}
