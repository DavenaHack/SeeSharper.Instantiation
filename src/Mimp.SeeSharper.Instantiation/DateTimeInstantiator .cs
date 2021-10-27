using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.ObjectDescription;
using Mimp.SeeSharper.ObjectDescription.Abstraction;
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


        public bool Instantiable(Type type, IObjectDescription description)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (description is null)
                throw new ArgumentNullException(nameof(description));

            return type.IsAssignableFrom(typeof(DateTime));
        }


        public object? Instantiate(Type type, IObjectDescription description, out IObjectDescription? ignored)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (description is null)
                throw new ArgumentNullException(nameof(description));
            if (!Instantiable(type, description))
                throw InstantiationException.GetNotMatchingTypeException(this, type, description);

            var constDesc = description.Constant();
            if (constDesc.HasValue)
            {
                if (constDesc.Value is null)
                {
                    ignored = null;
                    return type.Default();
                }

                if (constDesc.Value is DateTime v)
                {
                    ignored = null;
                    return v;
                }

                if (constDesc.Value is string s)
                    return InstantiateFromString(type, s, description, out ignored);

                var valueType = constDesc.Value.GetType();
                if (valueType.IsNumber())
                {
                    ignored = null;
                    return new DateTime((long)constDesc.Value);
                }

            }
            else if (constDesc.IsEmpty())
                try
                {
                    return Instantiate(type, ObjectDescriptions.NullDescription, out ignored);
                }
                catch (Exception ex)
                {
                    throw InstantiationException.GetCanNotInstantiateException(type, description, ex);
                }
            else if (constDesc.IsWrappedValue())
                try
                {
                    return Instantiate(type, constDesc.UnwrapValue(), out ignored);
                }
                catch (Exception ex)
                {
                    throw InstantiationException.GetCanNotInstantiateException(type, description, ex);
                }

            throw InstantiationException.GetCanNotInstantiateException(type, description);
        }

        protected virtual object? InstantiateFromString(Type type, string value, IObjectDescription description, out IObjectDescription? ignored)
        {
            if (type.IsNullable()
                && (string.IsNullOrWhiteSpace(value)
                    || NullFormats.Any(format => format.IsMatch(value))))
            {
                ignored = null;
                return type.Default();
            }
            else
                try
                {
                    var result = Formats.Any() ? DateTime.ParseExact(value, (string[])Formats, FormatProvider, DateTimeStyles)
                        : DateTime.Parse(value, FormatProvider, DateTimeStyles);
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
