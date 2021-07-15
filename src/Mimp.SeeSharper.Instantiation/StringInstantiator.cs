using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.Reflection;
using System;
using System.Collections.Generic;

namespace Mimp.SeeSharper.Instantiation
{
    /// <summary>
    /// A <see cref="IInstantiator"/> to instantiate <see cref="string"/>.
    /// </summary>
    public class StringInstantiator : IInstantiator
    {


        public bool Instantiable(Type type, object? instantiateValues)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            return type == typeof(string);
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

            if (instantiateValues is string s)
            {
                ignoredInstantiateValues = null;
                return s;
            }

            if (instantiateValues.GetType().IsPrimitive)
            {
                ignoredInstantiateValues = null;
                return instantiateValues.ToString();
            }

            if (instantiateValues is IEnumerable<KeyValuePair<string?, object?>> enumerable)
            {
                var i = 0;
                object? value = null;
                foreach (var pair in enumerable)
                {
                    if (i++ > 1)
                        break;
                    if (!string.IsNullOrEmpty(pair.Key))
                    {
                        i++;
                        break;
                    }
                    value = pair.Value;
                }
                if (i < 2)
                    try
                    {
                        return Instantiate(type, i < 1 ? null : value, out ignoredInstantiateValues);
                    }
                    catch (Exception ex)
                    {
                        throw InstantiationException.GetCanNotInstantiateException(type, instantiateValues, ex);
                    }
            }

            throw InstantiationException.GetCanNotInstantiateException(type, instantiateValues);
        }


        public void Initialize(object? instance, object? initializeValues, out object? ignoredInitializeValues)
        {
            ignoredInitializeValues = initializeValues;
        }


    }
}
