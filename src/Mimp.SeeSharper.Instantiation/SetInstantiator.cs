using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Mimp.SeeSharper.Instantiation
{
    /// <summary>
    /// A <see cref="IInstantiator"/> to instantiate <see cref="ISet{T}"/>.
    /// </summary>
    public class SetInstantiator : IInstantiator
    {


        public IInstantiator InstanceInstantiator { get; }

        public IInstantiator ValueInstantiator { get; }


        public SetInstantiator(IInstantiator instanceInstantiator, IInstantiator valueInstantiator)
        {
            InstanceInstantiator = instanceInstantiator ?? throw new ArgumentNullException(nameof(instanceInstantiator));
            ValueInstantiator = valueInstantiator ?? throw new ArgumentNullException(nameof(valueInstantiator));
        }


        public bool Instantiable(Type type, object? instantiateValues)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            return type == typeof(ISet<>)
                || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ISet<>)
                || type.IsISet() && InstanceInstantiator.Instantiable(type, instantiateValues);
        }


        public object? Instantiate(Type type, object? instantiateValues, out object? ignoredInstantiateValues)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (!Instantiable(type, instantiateValues))
                throw InstantiationException.GetNotMatchingTypeException(this, type);

            if (type == typeof(ISet<>) || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ISet<>))
            {
                type = typeof(HashSet<>).MakeGenericType(type.GetISetValueType() ?? typeof(object));
                if (EnumerableInstantiator.TryInstantiateEnumerableConstructor(type, instantiateValues, InstantiateValue, out ignoredInstantiateValues, out var inits))
                    return inits;
            }

            var instance = (IEnumerable?)InstanceInstantiator.Instantiate(type, instantiateValues, out ignoredInstantiateValues);
            if (instance is null)
                return null;

            EnumerableInstantiator.InstantiateInstance(type, instance, ignoredInstantiateValues, InstantiateValue, out ignoredInstantiateValues);

            return instance;
        }

        protected virtual object? InstantiateValue(Type type, object? initializeValues, out object? ignoreInitializeValues) =>
            ValueInstantiator.Instantiate(type, initializeValues, out ignoreInitializeValues);


        public void Initialize(object? instance, object? initializeValues, out object? ignoredInitializeValues)
        {
            if (instance is null)
            {
                ignoredInitializeValues = initializeValues;
                return;
            }

            var type = instance.GetType();
            if (!Instantiable(type, null))
                throw InstantiationException.GetNotMatchingTypeException(this, type);

            InstanceInstantiator.Initialize(instance, initializeValues, out ignoredInitializeValues);
            EnumerableInstantiator.InitializeInstance((IEnumerable)instance, ignoredInitializeValues, InitializePair, out ignoredInitializeValues);
        }

        protected virtual object? InitializePair(Type type, object? instance, object? initializeValues, out object? ignoredInitializeValues)
        {
            if (instance is null)
                return ValueInstantiator.Construct(type, initializeValues, out ignoredInitializeValues);

            ValueInstantiator.Initialize(instance, initializeValues, out ignoredInitializeValues);
            return instance;
        }


    }
}
