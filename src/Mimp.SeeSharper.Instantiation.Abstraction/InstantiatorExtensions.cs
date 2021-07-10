using System;

namespace Mimp.SeeSharper.Instantiation.Abstraction
{
    public static class InstantiatorExtensions
    {


        public static T? Instantiate<T>(this IInstantiator instantiator, object? instantiateValues, out object? ignoredInstantiatorValues)
        {
            if (instantiator is null)
                throw new ArgumentNullException(nameof(instantiator));

            return (T?)instantiator.Instantiate(typeof(T), instantiateValues, out ignoredInstantiatorValues);
        }


        public static object? Construct(this IInstantiator instantiator, Type type, object? instantiateValues, out object? ignoredInstantiateValues, object? initializeValues, out object? ignoredInitializeValues)
        {
            if (instantiator is null)
                throw new ArgumentNullException(nameof(instantiator));
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            var instance = instantiator.Instantiate(type, instantiateValues, out ignoredInstantiateValues);
            instantiator.Initialize(instance, initializeValues, out ignoredInitializeValues);
            return instance;
        }

        public static object? Construct(this IInstantiator instantiator, Type type, object? values, out object? ignoredValues)
        {
            if (instantiator is null)
                throw new ArgumentNullException(nameof(instantiator));
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            var instance = instantiator.Instantiate(type, values, out ignoredValues);
            instantiator.Initialize(instance, ignoredValues, out ignoredValues);
            return instance;
        }

        public static object? Construct(this IInstantiator instantiator, Type type, object? values) =>
            instantiator.Construct(type, values, out _);


        public static T? Construct<T>(this IInstantiator instantiator, object? instantiateValues, out object? ignoredInstantiateValues, object? initializeValues, out object? ignoredInitializeValues)
        {
            if (instantiator is null)
                throw new ArgumentNullException(nameof(instantiator));

            return (T?)instantiator.Construct(typeof(T), instantiateValues, out ignoredInstantiateValues, initializeValues, out ignoredInitializeValues);
        }

        public static T? Construct<T>(this IInstantiator instantiator, object? values, out object? ignoredValues)
        {
            if (instantiator is null)
                throw new ArgumentNullException(nameof(instantiator));

            return (T?)instantiator.Construct(typeof(T), values, out ignoredValues);
        }

        public static T? Construct<T>(this IInstantiator instantiator, object? values) =>
            instantiator.Construct<T>(values, out _);


    }
}
