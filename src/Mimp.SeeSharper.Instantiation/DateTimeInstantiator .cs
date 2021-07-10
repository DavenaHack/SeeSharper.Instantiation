using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.Reflection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mimp.SeeSharper.Instantiation
{
    /// <summary>
    /// A <see cref="IInstantiator"/> to instantiate <see cref="DateTime"/> or <see cref="Nullable{DateTime}"/>.
    /// </summary>
    public class DateTimeInstantiator : IInstantiator
    {


        public static readonly IEnumerable<Regex> DefaultNullFormats = new[] {
            new Regex(@"^\s*0000-00-00(?:T00:00:00(?:\.0+)?)?")
        };


        public IFormatProvider FormatProvider { get; }

        public DateTimeStyles DateTimeStyles { get; }

        public IEnumerable<string> Formats { get; }

        public IEnumerable<Regex> NullFormats { get; }


        public DateTimeInstantiator(IFormatProvider formatProvider, DateTimeStyles dateTimeStyles, IEnumerable<string> formats, IEnumerable<Regex> nullFormats)
        {
            FormatProvider = formatProvider ?? throw new ArgumentNullException(nameof(formatProvider));
            DateTimeStyles = dateTimeStyles;
            Formats = formats?.ToArray() ?? throw new ArgumentNullException(nameof(formats));
            if (Formats.Any(f => f is null))
                throw new ArgumentNullException(nameof(formats), "At least on format is null.");
            NullFormats = nullFormats?.ToArray() ?? throw new ArgumentNullException(nameof(nullFormats));
            if (NullFormats.Any(f => f is null))
                throw new ArgumentNullException(nameof(nullFormats), "At least on nullformat is null.");
        }

        public DateTimeInstantiator(IFormatProvider formatProvider, DateTimeStyles dateTimeStyles)
            : this(formatProvider, dateTimeStyles, Array.Empty<string>(), Array.Empty<Regex>()) { }

        public DateTimeInstantiator(IFormatProvider formatProvider)
            : this(formatProvider, DateTimeStyles.None) { }

        public DateTimeInstantiator()
            : this(CultureInfo.InvariantCulture, DateTimeStyles.None, Array.Empty<string>(), DefaultNullFormats) { }


        public bool Instantiable(Type type, object? instantiateValues)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            return type.IsAssignableFrom(typeof(DateTime));
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

            if (instantiateValues is DateTime v)
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
                return new DateTime((long)instantiateValues);
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
            if (type.IsNullable() && (
                string.IsNullOrWhiteSpace(value)
                || NullFormats.Any(format => format.IsMatch(value))
                ))
            {
                ignoredInstantiateValues = null;
                return type.Default();
            }
            else
                try
                {
                    var result = Formats.Any() ? DateTime.ParseExact(value, (string[])Formats, FormatProvider, DateTimeStyles)
                        : DateTime.Parse(value, FormatProvider, DateTimeStyles);
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
