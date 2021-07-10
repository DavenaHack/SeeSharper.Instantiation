using Mimp.SeeSharper.Instantiation.Abstraction;
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


        public bool Instantiable(Type type, object? instantiateValues)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            return type.IsAssignableFrom(typeof(TimeSpan));
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

            if (instantiateValues is TimeSpan v)
            {
                ignoredInstantiateValues = null;
                return v;
            }

            if (instantiateValues is string s)
                return InstantiateFromString(type, s, instantiateValues, out ignoredInstantiateValues);

            var valueType = instantiateValues.GetType();
            if (valueType.IsNumber())
            {
                ignoredInstantiateValues = null;
                return new TimeSpan((long)instantiateValues);
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

        protected virtual object? InstantiateFromString(Type type, string value, object instantiateValues, out object? ignoredInstantiateValues)
        {
            if (type.IsNullable() && string.IsNullOrWhiteSpace(value))
            {
                ignoredInstantiateValues = null;
                return type.Default();
            }
            else
                try
                {
                    var result = Formats.Any() ? TimeSpan.ParseExact(value, (string[])Formats, FormatProvider, TimeSpanStyles)
                        : TimeSpan.Parse(value, FormatProvider);
                    ignoredInstantiateValues = null;
                    return result;
                }
                catch (Exception ex)
                {
                    throw InstantiationException.GetCanNotInstantiateException(type, instantiateValues, ex);
                }
        }


        public void Initialize(object? instance, object? initializeValues, out object? ignoredInitializeValues)
        {
            ignoredInitializeValues = initializeValues;
        }


    }
}
