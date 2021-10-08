using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.ObjectDescription;
using Mimp.SeeSharper.ObjectDescription.Abstraction;
using Mimp.SeeSharper.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mimp.SeeSharper.Instantiation
{
    /// <summary>
    /// A <see cref="IInstantiator"/> to instantiate classes or structors with a public constructor.
    /// </summary>
    public class ConstructorInstantiator : IInstantiator
    {


        public IInstantiator ParameterInstantiator { get; }

        public bool TryDefaultInstance { get; }

        public bool UseDefaultParameter { get; }


        public ConstructorInstantiator(IInstantiator parameterInstantiator, bool tryDefaultInstance, bool useDefaultParameter)
        {
            ParameterInstantiator = parameterInstantiator ?? throw new ArgumentNullException(nameof(parameterInstantiator));
            TryDefaultInstance = tryDefaultInstance;
            UseDefaultParameter = useDefaultParameter;
        }

        public ConstructorInstantiator(IInstantiator parameterInstantiator)
            : this(parameterInstantiator, true, true) { }


        public bool Instantiable(Type type, IObjectDescription description)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (description is null)
                throw new ArgumentNullException(nameof(description));

            return type.IsValueType || !type.IsAbstract && type.GetConstructors().Length > 0;
        }


        public object? Instantiate(Type type, IObjectDescription description, out IObjectDescription? ignored)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (description is null)
                throw new ArgumentNullException(nameof(description));
            if (!Instantiable(type, description))
                throw InstantiationException.GetNotMatchingTypeException(this, type, description);

            if (description.IsNull())
                return GetDefault(type, description, out ignored);

            var constructors = GetConstructors(type, description, out ignored);
            var exceptions = new List<Exception>();

            foreach (var constructor in constructors)
                try
                {
                    var instance = constructor.Invoke(GetParameters(type, constructor, ignored ?? ObjectDescriptions.EmptyDescription, out var ignore));
                    InstantiateInstance(instance, ignore ?? ObjectDescriptions.EmptyDescription, out ignore);
                    ignored = ignore;
                    return instance;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }

            try
            {
                var def = GetDefault(type, description, out ignored);
                if (def is null)
                    return def;
                InstantiateInstance(def, ignored ?? ObjectDescriptions.EmptyDescription, out ignored);
                return def;
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }

            if (exceptions.Count > 0)
                throw InstantiationException.GetCanNotInstantiateException(type, description, exceptions);

            throw InstantiationException.GetCanNotInstantiateException(type, description);
        }


        protected virtual object? GetDefault(Type type, IObjectDescription description, out IObjectDescription? ignored)
        {
            var exceptions = new List<Exception>();
            try
            {
                if (TryDefaultInstance)
                    try
                    {
                        foreach (var constructor in GetConstructors(type, ObjectDescriptions.NullDescription, out _))
                            try
                            {
                                return constructor.Invoke(GetParameters(type, constructor, ObjectDescriptions.NullDescription, out _));
                            }
                            catch (Exception ex)
                            {
                                exceptions.Add(new InstantiationException(type, description, null, $"Can't call default constructor {constructor}", ex));
                            }
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(new InstantiationException(type, description, null, $"Can't get default constructor for {type}", ex));
                    }

                return type.Default();
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
                throw new InstantiationException(type, description, null, $"Can't get default for {type}", exceptions);
            }
            finally
            {
                ignored = description.IsNullOrEmpty() ? null : description;
            }
        }


        protected virtual IEnumerable<ConstructorInfo> GetConstructors(Type type, IObjectDescription description, out IObjectDescription? ignored)
        {
            ignored = description;

            var constructors = type.GetConstructors();

            if (description.IsNullOrEmpty())
                return constructors.OrderBy(c => c.GetParameters().Length);

            if (description.HasValue)
                description = description.WrapValue();

            var hasNameMatching = false;
            var matches = constructors.Select(constructor =>
            {
                var nameMatch = 0;
                var noMatch = 0;
                var paras = constructor.GetParameters();
                var usedParas = new HashSet<ParameterInfo>();
                foreach (var p in paras)
                {
                    var nameFound = false;
                    foreach (var pair in description.Children)
                    {
                        if (usedParas.Contains(p))
                            continue;
                        var name = pair.Key;
                        if (string.Equals(p.Name, name, StringComparison.InvariantCultureIgnoreCase))
                        {
                            nameFound = true;
                            usedParas.Add(p);
                            break;
                        }
                    }
                    if (nameFound)
                        nameMatch++;
                    else
                        noMatch++;
                }

                return new
                {
                    Constructor = constructor,
                    NameMatches = nameMatch,
                    NoMatches = noMatch,
                };
            }).ToList();

            if (hasNameMatching) // if initializeValues aren't parameter definition the values should be pass as parameter
                foreach (var c in constructors)
                {
                    var paras = c.GetParameters();
                    if (paras.Length == 1 && description.GetType().InheritOrAssignable(paras[0].ParameterType))
                        return new[] { c };
                }

            matches.Sort((m0, m1) =>
            {
                if (m0.NameMatches < m1.NameMatches)
                    return 1;

                if (m0.NameMatches == m1.NameMatches)
                    if (m0.NoMatches > m1.NoMatches)
                        return 1;
                    else if (m0.NoMatches == m1.NoMatches)
                        return 0;

                return -1;
            });
            return matches.Select(m => m.Constructor).ToArray();
        }


        protected virtual object?[] GetParameters(Type type, ConstructorInfo constructor, IObjectDescription description, out IObjectDescription? ignored)
        {
            object? createParameter(ParameterInfo parameter, IObjectDescription description, out IObjectDescription? ignored)
            {
                object? def = null;
                bool hasDef = false;
                if (UseDefaultParameter && parameter.HasDefaultValue && description.IsNullOrEmpty())
                {
                    def = parameter.DefaultValue;
                    hasDef = true;
                }
                try
                {
                    var result = ConstructConstructorParameter(parameter.ParameterType, description, out ignored);
                    if (hasDef && Equals(result, parameter.ParameterType.Default()))
                    {
                        ignored = null;
                        return def;
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    if (hasDef)
                    {
                        ignored = null;
                        return def;
                    }
                    throw InstantiationException.GetCanNotInstantiateParameterException(type, description, constructor, parameter, ex);
                }
            }

            var parameters = constructor.GetParameters();

            description = description.IsWrappedValue() ? description.UnwrapValue() : description;

            var exceptions = new List<Exception>();

            if (parameters.Length == 1 && description.HasValue)
                try
                {
                    return new[] { createParameter(parameters[0], description, out ignored) };
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }

            if (description.HasValue || description.IsWrappedValue())
                description = description.IsNull() ? ObjectDescriptions.EmptyDescription
                    : description.WrapValue();

            try
            {
                var paras = new object?[parameters.Length];
                ignored = description.Constant();

                for (var i = 0; i < parameters.Length; i++)
                {
                    var param = parameters[i];
                    var name = param.Name;
                    var created = false;
                    foreach (var pair in ignored.Children)
                        if (string.Equals(param.Name, pair.Key, StringComparison.InvariantCultureIgnoreCase))
                        {
                            paras[i] = createParameter(param, pair.Value, out var ignore);
                            ignored = ignored.Remove(pair);
                            if (ignore is not null)
                                ignored = ignored.Append(pair.Key, ignore);
                            created = true;
                            break;
                        }
                    if (!created)
                        paras[i] = createParameter(param, ObjectDescriptions.NullDescription, out _);
                }

                ignored = ignored.IsNullOrEmpty() ? null : ignored.Constant();
                return paras;
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }

            if (parameters.Length == 1)
                try
                {
                    return new[] { createParameter(parameters[0], description, out ignored) };
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }

            throw InstantiationException.GetCanNotInstantiateException(type, description, exceptions);
        }


        protected virtual object? ConstructConstructorParameter(Type type, IObjectDescription description, out IObjectDescription? ignored) =>
            ParameterInstantiator.Construct(type, description, out ignored);

        protected virtual void InstantiateInstance(object instance, IObjectDescription description, out IObjectDescription? ignored)
        {
            ignored = description.IsNullOrEmpty() ? null : description;
        }


        public virtual object? Initialize(Type type, object? instance, IObjectDescription description, out IObjectDescription? ignored)
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
