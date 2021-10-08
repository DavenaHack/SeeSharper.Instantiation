﻿using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.ObjectDescription;
using Mimp.SeeSharper.ObjectDescription.Abstraction;
using Mimp.SeeSharper.Reflection;
using System;
using System.Globalization;

namespace Mimp.SeeSharper.Instantiation
{
    /// <summary>
    /// A <see cref="IInstantiator"/> to instantiate <see cref="float"/> or <see cref="Nullable{float}"/>.
    /// </summary>
    public class FloatInstantiator : IInstantiator
    {


        public IFormatProvider FormatProvider { get; }

        public NumberStyles NumberStyles { get; }


        public FloatInstantiator(IFormatProvider formatProvider, NumberStyles numberStyles)
        {
            FormatProvider = formatProvider ?? throw new ArgumentNullException(nameof(formatProvider));
            NumberStyles = numberStyles;
        }

        public FloatInstantiator(IFormatProvider formatProvider)
            : this(formatProvider, NumberStyles.AllowThousands | NumberStyles.Float) { }

        public FloatInstantiator()
            : this(CultureInfo.InvariantCulture) { }


        public bool Instantiable(Type type, IObjectDescription description)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (description is null)
                throw new ArgumentNullException(nameof(description));

            return type.IsAssignableFrom(typeof(float));
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
                    return type.Default();
                }

                if (description.Value is float v)
                {
                    ignored = null;
                    return v;
                }

                if (description.Value is string s)
                    return InstantiateFromString(type, s, description, out ignored);

                var valueType = description.Value.GetType();
                if (valueType.IsNumber())
                {
                    ignored = null;
                    return (float)description.Value;
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

        protected virtual object? InstantiateFromString(Type type, string value, IObjectDescription description, out IObjectDescription? ignored)
        {
            if (string.IsNullOrWhiteSpace(value) && type.IsNullable())
            {
                ignored = null;
                return type.Default();
            }
            else
                try
                {
                    var result = float.Parse(value, NumberStyles, FormatProvider);
                    ignored = null;
                    return result;
                }
                catch (Exception ex)
                {
                    throw InstantiationException.GetCanNotInstantiateException(type, description, ex);
                }
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
