using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Mimp.SeeSharper.Instantiation
{
    /// <summary>
    /// A <see cref="IInstantiator"/> to instantiate <see cref="ICollection"/> or <see cref="ICollection{T}"/>.
    /// </summary>
    public class CollectionInstantiator : IInstantiator
    {


        public IInstantiator InstanceInstantiator { get; }

        public IInstantiator ValueInstantiator { get; }


        public CollectionInstantiator(IInstantiator instanceInstantiator, IInstantiator valueInstantiator)
        {
            InstanceInstantiator = instanceInstantiator ?? throw new ArgumentNullException(nameof(instanceInstantiator));
            ValueInstantiator = valueInstantiator ?? throw new ArgumentNullException(nameof(valueInstantiator));
        }


        public bool Instantiable(Type type, object? instantiateValues)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            return type == typeof(ICollection)
                || type == typeof(ICollection<>)
                || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICollection<>)
                || type.IsICollection() && InstanceInstantiator.Instantiable(type, instantiateValues);
        }


        public object? Instantiate(Type type, object? instantiateValues, out object? ignoredInstantiateValues)
        {
			if (type is null)
				throw new ArgumentNullException(nameof(type));
			if (!Instantiable(type, instantiateValues))
                throw InstantiationException.GetNotMatchingTypeException(this, type);

            if (type == typeof(ICollection) || type == typeof(ICollection<>) || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICollection<>))
            {
                type = typeof(List<>).MakeGenericType(type.GetICollectionValueType() ?? typeof(object));
                if (EnumerableInstantiator.TryInstantiateEnumerableConstructor(type, instantiateValues, InstantiateValue, out ignoredInstantiateValues, out var inits))
                    return inits;
            }

            var instance = (IEnumerable?)InstanceInstantiator.Instantiate(type, instantiateValues, out ignoredInstantiateValues);
            if (instance is null)
                return null;

            EnumerableInstantiator.InstantiateInstance(type, instance, ignoredInstantiateValues, InstantiateValue, out ignoredInstantiateValues);

            return instance;
        }

        protected virtual object? InstantiateValue(Type type, object? instantiateValues, out object? ignoredInstantiateValues) =>
            ValueInstantiator.Construct(type, instantiateValues, out ignoredInstantiateValues);


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
            EnumerableInstantiator.InitializeInstance((IEnumerable)instance, ignoredInitializeValues, InitializeValue, out ignoredInitializeValues);
        }

        protected virtual object? InitializeValue(Type type, object? instance, object? initializeValues, out object? ignoredInitializeValues)
        {
            if (instance is null)
                return ValueInstantiator.Construct(type, initializeValues, out ignoredInitializeValues);

            ValueInstantiator.Initialize(instance, initializeValues, out ignoredInitializeValues);
            return instance;
        }


    }
}
