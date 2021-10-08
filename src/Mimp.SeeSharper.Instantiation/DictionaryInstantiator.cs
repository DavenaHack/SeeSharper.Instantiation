using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.ObjectDescription.Abstraction;
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


        public bool Instantiable(Type type, IObjectDescription description)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (description is null)
                throw new ArgumentNullException(nameof(description));

            return type == typeof(IDictionary)
                || type == typeof(IDictionary<,>)
                || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>)
                || type.IsIDictionary() && InstanceInstantiator.Instantiable(type, description);
        }


        public object? Instantiate(Type type, IObjectDescription description, out IObjectDescription? ignored)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (description is null)
                throw new ArgumentNullException(nameof(description));
            if (!Instantiable(type, description))
                throw InstantiationException.GetNotMatchingTypeException(this, type, description);

            if (type == typeof(IDictionary) || type == typeof(IDictionary<,>) || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
            {
                type = typeof(Dictionary<,>).MakeGenericType(type.GetIDictionaryKeyValueType()?.ToArray() ?? new[] { typeof(object), typeof(object) });
                if (EnumerableInstantiator.TryInstantiateEnumerableConstructor(type, description, InstantiatePair, out ignored, out var inits))
                    return inits;
            }

            return EnumerableInstantiator.Instantiate(type, description, InstanceInstantiator, InstantiatePair, out ignored);
        }

        protected virtual object? InstantiatePair(Type type, IObjectDescription description, out IObjectDescription? ignored) =>
            PairInstantiator.Construct(type, description, out ignored);
        

        public object? Initialize(Type type, object? instance, IObjectDescription description, out IObjectDescription? ignored)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (description is null)
                throw new ArgumentNullException(nameof(description));

            if (instance is null)
                return Instantiate(type, description, out ignored);

            type = instance.GetType();
            if (!Instantiable(type, description))
                throw InstantiationException.GetNotMatchingTypeException(this, type, description);

            return EnumerableInstantiator.Initialize(type, (IEnumerable)instance, description, InstanceInstantiator, InitializePair, out ignored);
        }

        protected virtual object? InitializePair(Type type, object? instance, IObjectDescription description, out IObjectDescription? ignored)
        {
            if (instance is null)
                return PairInstantiator.Construct(type, description, out ignored);

            return PairInstantiator.Initialize(instance, description, out ignored);
        }


    }
}
