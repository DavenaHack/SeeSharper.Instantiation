using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.ObjectDescription;
using Mimp.SeeSharper.ObjectDescription.Abstraction;
using Mimp.SeeSharper.Reflection;
using System;
using System.Collections.Generic;

namespace Mimp.SeeSharper.Instantiation
{
    /// <summary>
    /// A <see cref="IInstantiator"/> to instantiate <see cref="Enum"/> or <see cref="Nullable{Enum}"/>.
    /// </summary>
    public class EnumInstantiator : IInstantiator
    {


        public bool Instantiable(Type type, IObjectDescription description)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (description is null)
                throw new ArgumentNullException(nameof(description));

            return type.IsEnum || type.IsNullable() && type.GetNullableValueType()!.IsEnum;
        }


        public object? Instantiate(Type type, IObjectDescription description, out IObjectDescription? ignored)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (description is null)
                throw new ArgumentNullException(nameof(description));
            if (!Instantiable(type, description))
                throw InstantiationException.GetNotMatchingTypeException(this, type, description);

            if (description.HasValue)
            {
                if (description.Value is null)
                {
                    ignored = null;
                    if (type.IsNullable())
                        return type.Default();
                    var values = type.GetEnumValues();
                    return values.Length > 0 ? values.GetValue(0) : type.Default();
                }

                if (description.Value is string s)
                    try
                    {
                        ignored = null;
                        return string.IsNullOrWhiteSpace(s) ? Instantiate(type, ObjectDescriptions.NullDescription, out ignored)
                            : Enum.Parse(type, s, true);
                    }
                    catch (Exception ex)
                    {
                        throw InstantiationException.GetCanNotInstantiateException(type, description, ex);
                    }

                var valueType = description.Value.GetType();
                if (valueType.IsNumber() || valueType.IsEnum)
                {
                    ignored = null;
                    return type.GetCastFunc(type)(description.Value);
                }

            }
            else if (description.IsEmpty())
                try
                {
                    return Instantiate(type, ObjectDescriptions.NullDescription, out ignored);
                }
                catch (Exception ex)
                {
                    throw InstantiationException.GetCanNotInstantiateException(type, description, ex);
                }
            else if (description.IsWrappedValue())
                try
                {
                    return Instantiate(type, description.UnwrapValue(), out ignored);
                }
                catch (Exception ex)
                {
                    throw InstantiationException.GetCanNotInstantiateException(type, description, ex);
                }

            throw InstantiationException.GetCanNotInstantiateException(type, description);
        }


        public object? Initialize(Type type, object? instance, IObjectDescription description, out IObjectDescription? ignored)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (description is null)
                throw new ArgumentNullException(nameof(description));

            if (instance is null)
                return Instantiate(type, description, out ignored);

            ignored = description.IsNullOrEmpty() ? null : description;
            return instance;
        }


    }
}
