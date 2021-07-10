using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mimp.SeeSharper.Instantiation
{
    /// <summary>
    /// A <see cref="IInstantiator"/> to instantiate <see cref="IDictionary"/> or <see cref="IDictionary{TKey, TValue}"/>.
    /// </summary>
    public class DictionaryInstantiator : IInstantiator
    {


        public IInstantiator InstanceInstantiator { get; }

        public IInstantiator PairInstantiator { get; }


        public DictionaryInstantiator(IInstantiator instanceInstantiator, IInstantiator pairInstantiator)
        {
            InstanceInstantiator = instanceInstantiator ?? throw new ArgumentNullException(nameof(instanceInstantiator));
            PairInstantiator = pairInstantiator ?? throw new ArgumentNullException(nameof(pairInstantiator));
        }

        public DictionaryInstantiator(IInstantiator instanceInstantiator, IInstantiator keyInstantiator, IInstantiator valueInstantiator)
            : this(instanceInstantiator, new KeyValuePairInstantiator(keyInstantiator, valueInstantiator)) { }


        public bool Instantiable(Type type, object? instantiateValues)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            return type == typeof(IDictionary)
                || type == typeof(IDictionary<,>)
                || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>)
                || type.IsIDictionary() && InstanceInstantiator.Instantiable(type, instantiateValues);
        }


        public object? Instantiate(Type type, object? instantiateValues, out object? ignoredInstantiateValues)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (!Instantiable(type, instantiateValues))
                throw InstantiationException.GetNotMatchingTypeException(this, type);

            if (type == typeof(IDictionary) || type == typeof(IDictionary<,>) || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
            {
                type = typeof(Dictionary<,>).MakeGenericType(type.GetIDictionaryKeyValueType()?.ToArray() ?? new[] { typeof(object), typeof(object) });
                if (EnumerableInstantiator.TryInstantiateEnumerableConstructor(type, instantiateValues, InstantiatePair, out ignoredInstantiateValues, out var inits))
                    return inits;
            }

            var instance = (IEnumerable?)InstanceInstantiator.Instantiate(type, instantiateValues, out ignoredInstantiateValues);
            if (instance is null)
                return null;

            EnumerableInstantiator.InstantiateInstance(type, instance, ignoredInstantiateValues, InstantiatePair, out ignoredInstantiateValues);

            return instance;
        }

        protected virtual object? InstantiatePair(Type type, object? instantiateValues, out object? ignoredInstantiateValues) =>
            PairInstantiator.Construct(type, instantiateValues, out ignoredInstantiateValues);


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
                return PairInstantiator.Construct(type, initializeValues, out ignoredInitializeValues);

            PairInstantiator.Initialize(instance, initializeValues, out ignoredInitializeValues);
            return instance;
        }


    }
}
