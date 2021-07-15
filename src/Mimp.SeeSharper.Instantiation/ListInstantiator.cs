using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mimp.SeeSharper.Instantiation
{
    /// <summary>
    /// A <see cref="IInstantiator"/> to instantiate <see cref="IList"/> or <see cref="IList{T}"/>.
    /// </summary>
    public class ListInstantiator : IInstantiator
    {


        public IInstantiator InstanceInstantiator { get; }

        public IInstantiator ValueInstantiator { get; }


        public ListInstantiator(IInstantiator instanceInstantiator, IInstantiator valueInstantiator)
        {
            InstanceInstantiator = instanceInstantiator ?? throw new ArgumentNullException(nameof(instanceInstantiator));
            ValueInstantiator = valueInstantiator ?? throw new ArgumentNullException(nameof(valueInstantiator));
        }


        public bool Instantiable(Type type, object? instantiateValues)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            return type == typeof(IList)
                || type == typeof(IList<>)
                || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>)
                || type.IsIList() && InstanceInstantiator.Instantiable(type, instantiateValues);
        }


        public object? Instantiate(Type type, object? instantiateValues, out object? ignoredInstantiateValues)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (!Instantiable(type, instantiateValues))
                throw InstantiationException.GetNotMatchingTypeException(this, type);

            if (type == typeof(IList) || type == typeof(IList<>) || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>))
            {
                type = typeof(List<>).MakeGenericType(type.GetIListValueType() ?? typeof(object));
                if (EnumerableInstantiator.TryInstantiateEnumerableConstructor(type, instantiateValues, InstantiateValue, out ignoredInstantiateValues, out var inits))
                    return inits;
            }

            return EnumerableInstantiator.Instantiate(type, instantiateValues, InstanceInstantiator, InstantiateValue, out ignoredInstantiateValues);
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

            EnumerableInstantiator.Initialize(type, (IEnumerable)instance, initializeValues, InstanceInstantiator, InitializeValue, out ignoredInitializeValues);
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
