using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.ObjectDescription;
using Mimp.SeeSharper.ObjectDescription.Abstraction;
using Mimp.SeeSharper.Reflection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Mimp.SeeSharper.Instantiation
{
    /// <summary>
    /// A <see cref="IInstantiator"/> to instantiate <see cref="TimeSpan"/> or <see cref="Nullable{TimeSpan}"/>.
    /// </summary>
    public class TimeSpanInstantiator : IInstantiator
    {


        public IFormatProvider FormatProvider { get; }

        public TimeSpanStyles TimeSpanStyles { get; }

        public IEnumerable<string> Formats { get; }


        public TimeSpanInstantiator(IFormatProvider formatProvider, TimeSpanStyles timeSpanStyles, IEnumerable<string> formats)
        {
            FormatProvider = formatProvider ?? throw new ArgumentNullException(nameof(formatProvider));
            TimeSpanStyles = timeSpanStyles;
            Formats = formats?.ToArray() ?? throw new ArgumentNullException(nameof(formats));
            if (Formats.Any(f => f is null))
                throw new ArgumentNullException(nameof(formats), "At least on format is null.");
        }

        public TimeSpanInstantiator(IFormatProvider formatProvider, TimeSpanStyles timeSpanStyles)
            : this(formatProvider, timeSpanStyles, Array.Empty<string>()) { }

        public TimeSpanInstantiator(IFormatProvider formatProvider)
            : this(formatProvider, TimeSpanStyles.None) { }

        public TimeSpanInstantiator()
            : this(CultureInfo.InvariantCulture) { }


        public bool Instantiable(Type type, IObjectDescription description)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (description is null)
                throw new ArgumentNullException(nameof(description));

            return type.IsAssignableFrom(typeof(TimeSpan));
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

                if (description.Value is TimeSpan v)
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
                    return new TimeSpan((long)description.Value);
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
            if (type.IsNullable() && string.IsNullOrWhiteSpace(value))
            {
                ignored = null;
                return type.Default();
            }
            else
                try
                {
                    var result = Formats.Any() ? TimeSpan.ParseExact(value, (string[])Formats, FormatProvider, TimeSpanStyles)
                        : TimeSpan.Parse(value, FormatProvider);
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
