using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.ObjectDescription.Abstraction;
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


        public bool Instantiable(Type type, IObjectDescription description)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (description is null)
                throw new ArgumentNullException(nameof(description));

            return type == typeof(ICollection)
                || type == typeof(ICollection<>)
                || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICollection<>)
                || type.IsICollection() && InstanceInstantiator.Instantiable(type, description);
        }


        public object? Instantiate(Type type, IObjectDescription description, out IObjectDescription? ignored)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (description is null)
                throw new ArgumentNullException(nameof(description));
            if (!Instantiable(type, description))
                throw InstantiationException.GetNotMatchingTypeException(this, type, description);

            if (type == typeof(ICollection) || type == typeof(ICollection<>) || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICollection<>))
            {
                type = typeof(List<>).MakeGenericType(type.GetICollectionValueType() ?? typeof(object));
                if (EnumerableInstantiator.TryInstantiateEnumerableConstructor(type, description, InstantiateValue, out ignored, out var inits))
                    return inits;
            }

            return EnumerableInstantiator.Instantiate(type, description, InstanceInstantiator, InstantiateValue, out ignored);
        }

        protected virtual object? InstantiateValue(Type type, IObjectDescription description, out IObjectDescription? ignored) =>
            ValueInstantiator.Construct(type, description, out ignored);


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

            return EnumerableInstantiator.Initialize(type, (IEnumerable)instance, description, InstanceInstantiator, InitializeValue, out ignored);
        }

        protected virtual object? InitializeValue(Type type, object? instance, IObjectDescription description, out IObjectDescription? ignored)
        {
            if (instance is null)
                return ValueInstantiator.Construct(type, description, out ignored);

            return ValueInstantiator.Initialize(type, instance, description, out ignored);
        }


    }
}
