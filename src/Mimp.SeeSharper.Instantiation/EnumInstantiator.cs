using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mimp.SeeSharper.Instantiation
{
    public class EnumInstantiator : IInstantiator
    {


        public bool Instantiable(Type type, object? instantiateValues)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            return type.IsEnum || type.IsNullable() && type.GetNullableValueType()!.IsEnum;
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
                if (type.IsNullable())
                    return type.Default();
                var values = type.GetEnumValues();
                return values.Length > 0 ? values.GetValue(0) : type.Default();
            }
            if (instantiateValues is string s)
            {
                ignoredInstantiateValues = null;
                return string.IsNullOrWhiteSpace(s) ? Instantiate(type, null, out ignoredInstantiateValues) : Enum.Parse(type, s, true);
            }
            var valueType = instantiateValues.GetType();
            if (valueType.IsNumber() || valueType.IsEnum)
            {
                ignoredInstantiateValues = null;
                return type.GetCastFunc(type)(instantiateValues);
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
            throw InstantiationException.GetCanNotInstantiateExeption(type, instantiateValues);
        }

        public void Initialize(object? instance, object? initializeValues, out object? ignoredInitializeValues)
        {
            ignoredInitializeValues = initializeValues;
        }


    }
}
