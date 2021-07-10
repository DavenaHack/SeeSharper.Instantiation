using Mimp.SeeSharper.Instantiation.Abstraction;
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


        public ConstructorInstantiator(IInstantiator parameterInstantiator, bool tryDefaultInstance)
        {
            ParameterInstantiator = parameterInstantiator ?? throw new ArgumentNullException(nameof(parameterInstantiator));
            TryDefaultInstance = tryDefaultInstance;
        }

        public ConstructorInstantiator(IInstantiator parameterInstantiator)
            : this(parameterInstantiator, true) { }


        public virtual bool Instantiable(Type type, object? instantiateValues)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            return type.IsValueType || !type.IsAbstract && type.GetConstructors().Length > 0;
        }


        public virtual object? Instantiate(Type type, object? instantiateValues, out object? ignoredInstantiateValues)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (!Instantiable(type, instantiateValues))
                throw InstantiationException.GetNotMatchingTypeException(this, type);

            if (instantiateValues is null)
            {
                ignoredInstantiateValues = null;
                return GetDefault(type);
            }

            var constructors = GetConstructors(type, instantiateValues, out var ignoredValues);
            var exceptions = new List<Exception>();

            foreach (var constructor in constructors)
            {
                try
                {
                    var parameters = GetParameters(type, constructor, ignoredValues, out var values);
                    var instance = constructor.Invoke(parameters);
                    InstantiateInstance(instance, values, out values);
                    ignoredInstantiateValues = values;
                    return instance;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }
            if (exceptions.Count > 0)
                throw InstantiationException.GetCanNotInstantiateException(type, instantiateValues, exceptions);

            if (type.IsValueType)
            {
                ignoredInstantiateValues = instantiateValues;
                return GetDefault(type);
            }

            throw InstantiationException.GetCanNotInstantiateException(type, instantiateValues);
        }


        protected virtual object? GetDefault(Type type)
        {
            if (TryDefaultInstance)
                try
                {
                    foreach (var constructor in GetConstructors(type, null, out _))
                        try
                        {
                            return constructor.Invoke(GetParameters(type, constructor, null, out _));
                        }
                        catch { }
                }
                catch { }

            return type.Default();
        }


        protected virtual IEnumerable<ConstructorInfo> GetConstructors(Type type, object? instantiateValues, out object? ignoredInstantiateValues)
        {
            ignoredInstantiateValues = instantiateValues;

            var constructors = type.GetConstructors();

            if (instantiateValues is null)
                return constructors.OrderBy(c => c.GetParameters().Length);

            if (instantiateValues is not IEnumerable<KeyValuePair<string?, object?>> parameters)
                parameters = new[] { new KeyValuePair<string?, object?>(null, instantiateValues) };

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
                    foreach (var pair in parameters)
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
                    if (paras.Length == 1 && instantiateValues.GetType().InheritOrAssignable(paras[0].ParameterType))
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

        protected virtual object?[] GetParameters(Type type, ConstructorInfo constructor, object? instantiateValues, out object? ignoredInstantiateValues)
        {
            object? createParameter(ParameterInfo parameter, object? values, out object? ignoredValues)
            {
                object? def = null;
                bool hasDef = false;
                if (parameter.HasDefaultValue && (values is null || values is IEnumerable<KeyValuePair<string?, object?>> keyValues && !keyValues.Any()))
                {
                    def = parameter.DefaultValue;
                    hasDef = true;
                }
                try
                {
                    var result = ConstructConstructorParameter(parameter.ParameterType, values, out ignoredValues);
                    if (hasDef && Equals(result, parameter.ParameterType.Default()))
                        return def;
                    return result;
                }
                catch (Exception ex)
                {
                    if (hasDef)
                    {
                        ignoredValues = null;
                        return def;
                    }
                    throw InstantiationException.GetCanNotInstantiateParameterException(type, values, constructor, parameter, ex);
                }
            }

            var parameters = constructor.GetParameters();

            if (instantiateValues is null)
                instantiateValues = Array.Empty<KeyValuePair<string?, object?>>();

            if (instantiateValues is IEnumerable<KeyValuePair<string?, object?>> values)
            {
                if (parameters.Length == 1)
                {
                    var p = parameters[0];
                    foreach (var pair in values)
                        if (string.Equals(p.Name, pair.Key, StringComparison.InvariantCultureIgnoreCase))
                        {
                            ignoredInstantiateValues = values.Where(p => p.Key != pair.Key).ToArray();
                            return new[] { createParameter(p, pair.Value, out _) };
                        }
                    return new[] { createParameter(p, instantiateValues, out ignoredInstantiateValues) };
                }

                var paras = new object?[parameters.Length];
                var ignoredValues = values.ToList();
                for (var i = 0; i < parameters.Length; i++)
                {
                    var p = parameters[i];
                    var created = false;
                    foreach (var pair in values)
                        if (string.Equals(p.Name, pair.Key, StringComparison.InvariantCultureIgnoreCase))
                        {
                            paras[i] = createParameter(p, pair.Value, out _);
                            ignoredValues.Remove(pair);
                            created = true;
                            break;
                        }
                    if (!created)
                        paras[i] = createParameter(p, null, out _);
                }

                ignoredInstantiateValues = ignoredValues;
                return paras;
            }
            else if (parameters.Length == 1)
                return new[] { createParameter(parameters[0], instantiateValues, out ignoredInstantiateValues) };

            throw InstantiationException.GetCanNotInstantiateException(type, instantiateValues);
        }

        protected virtual object? ConstructConstructorParameter(Type type, object? instantiateValues, out object? ignoredInstantiateValues) =>
            ParameterInstantiator.Construct(type, instantiateValues, out ignoredInstantiateValues);


        protected virtual void InstantiateInstance(object instance, object? instantiateValues, out object? ignoredInstantiateValues)
        {
            ignoredInstantiateValues = instantiateValues;
        }


        public virtual void Initialize(object? instance, object? initializeValues, out object? ignoredInitializeValues)
        {
            ignoredInitializeValues = initializeValues;
        }


    }
}
