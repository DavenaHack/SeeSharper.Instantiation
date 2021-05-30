﻿using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mimp.SeeSharper.Instantiation
{
    public class UShortInstantiator : IInstantiator
    {


        public bool Instantiable(Type type, object? instantiateValues) 
		{
			if (type is null)
				throw new ArgumentNullException(nameof(type));

			return type.IsAssignableFrom(typeof(ushort));
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
            if (instantiateValues is ushort v)
            {
                ignoredInstantiateValues = null;
                return v;
            }
            if (instantiateValues is string s)
                if (string.IsNullOrWhiteSpace(s) && type.IsNullable())
                {
                    ignoredInstantiateValues = null;
                    return type.Default();
                }
                else
                    try
                    {
                        v = ushort.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
                        ignoredInstantiateValues = null;
                        return v;
                    }
                    catch (Exception ex)
                    {
                        throw InstantiationException.GetCanNotInstantiateExeption(type, instantiateValues, ex);
                    }
			var valueType = instantiateValues.GetType();
            if (valueType.IsNumber())
            {
                ignoredInstantiateValues = null;
                return (ushort)instantiateValues;
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