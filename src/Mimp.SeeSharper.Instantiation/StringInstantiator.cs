using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mimp.SeeSharper.Instantiation
{
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
            if (instantiateValues is IEnumerable<KeyValuePair<string?, object?>> keyValue && keyValue.Count() == 1)
            {
                var p = keyValue.First();
                if (string.IsNullOrWhiteSpace(p.Key))
                    try
                    {
                        return Instantiate(type, p.Value, out ignoredInstantiateValues);
                    }
                    catch (Exception ex)
                    {
                        throw InstantiationException.GetCanNotInstantiateExeption(type, instantiateValues, ex);
                    }
            }
            ignoredInstantiateValues = null;
            return instantiateValues.ToString();
        }

        public void Initialize(object? instance, object? initializeValues, out object? ignoredInitializeValues)
        {
            ignoredInitializeValues = initializeValues;
        }


    }
}
