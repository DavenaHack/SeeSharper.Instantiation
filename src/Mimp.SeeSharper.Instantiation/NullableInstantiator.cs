using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mimp.SeeSharper.Instantiation
{
    /// <summary>
    /// A <see cref="IInstantiator"/> to instantiate <see cref="Nullable{T}"/>.
    /// </summary>
    public class NullableInstantiator : IInstantiator
    {


        public IInstantiator Instantiator { get; }


        public NullableInstantiator(IInstantiator instantiator)
        {
            Instantiator = instantiator ?? throw new ArgumentNullException(nameof(instantiator));
        }


        public bool Instantiable(Type type, object? instantiateValues)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            return type.IsNullable() && Instantiator.Instantiable(type.GetNullableValueType()!, instantiateValues);
        }


        public object? Instantiate(Type type, object? instantiateValues, out object? ignoredInstantiateValues)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (!Instantiable(type, instantiateValues))
                throw InstantiationException.GetNotMatchingTypeException(this, type);

            if (instantiateValues is null)
            {
                ignoredInstantiateValues = null;
                return type.Default();
            }

            try
            {
                return Instantiator.Instantiate(type.GetNullableValueType()!, instantiateValues, out ignoredInstantiateValues);
            }
            catch (Exception ex)
            {
                if (instantiateValues is string str && string.IsNullOrWhiteSpace(str)
                    || instantiateValues is IEnumerable<KeyValuePair<string?, object?>> enumerable && (
                        !enumerable.Any()
                        || !enumerable.Skip(1).Any()
                            && string.IsNullOrEmpty(enumerable.First().Key)
                            && enumerable.First().Value is string value && string.IsNullOrWhiteSpace(value)
                    ))
                {
                    ignoredInstantiateValues = null;
                    return type.Default();
                }

                throw InstantiationException.GetCanNotInstantiateException(type, instantiateValues, ex);
            }
        }


        public void Initialize(object? instance, object? initializeValues, out object? ignoredInitializeValues)
        {
            Instantiator.Initialize(instance, initializeValues, out ignoredInitializeValues);
        }


    }
}
