using Mimp.SeeSharper.ObjectDescription.Abstraction;
using System;

namespace Mimp.SeeSharper.Instantiation.Abstraction
{
    public static class InstantiatorExtensions
    {


        public static T? Instantiate<T>(this IInstantiator instantiator, IObjectDescription description, out IObjectDescription? ignored)
        {
            if (instantiator is null)
                throw new ArgumentNullException(nameof(instantiator));

            return (T?)instantiator.Instantiate(typeof(T), description, out ignored);
        }

        public static T? Initialize<T>(this IInstantiator instantiator, T? instance, IObjectDescription description, out IObjectDescription? ignored)
        {
            if (instantiator is null)
                throw new ArgumentNullException(nameof(instantiator));

            return (T?)instantiator.Initialize(typeof(T), instance, description, out ignored);
        }


        public static object? Construct(this IInstantiator instantiator, Type type,
            IObjectDescription instantiate, out IObjectDescription? ignoredInstantiate,
            IObjectDescription initialize, out IObjectDescription? ignoredInitialize)
        {
            if (instantiator is null)
                throw new ArgumentNullException(nameof(instantiator));

            var instance = instantiator.Instantiate(type, instantiate, out ignoredInstantiate);
            return instantiator.Initialize(type, instance, initialize, out ignoredInitialize);
        }


        public static T? Construct<T>(this IInstantiator instantiator,
                IObjectDescription instantiate, out IObjectDescription? ignoredInstantiate,
                IObjectDescription initialize, out IObjectDescription? ignoredInitialize
            ) => (T?)instantiator.Construct(typeof(T), instantiate, out ignoredInstantiate, initialize, out ignoredInitialize);


    }
}
