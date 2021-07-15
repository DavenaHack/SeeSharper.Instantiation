using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mimp.SeeSharper.Instantiation
{
    /// <summary>
    /// A <see cref="IInstantiator"/> to instantiate <see cref="IEnumerable"/> or <see cref="IEnumerable{T}"/>.
    /// </summary>
    public class EnumerableInstantiator : IInstantiator
    {


        public IInstantiator InstanceInstantiator { get; }

        public IInstantiator ValueInstantiator { get; }


        public EnumerableInstantiator(IInstantiator instanceInstantiator, IInstantiator valueInstantiator)
        {
            InstanceInstantiator = instanceInstantiator ?? throw new ArgumentNullException(nameof(instanceInstantiator));
            ValueInstantiator = valueInstantiator ?? throw new ArgumentNullException(nameof(valueInstantiator));
        }


        public bool Instantiable(Type type, object? instantiateValues)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            return type == typeof(IEnumerable)
                || type == typeof(IEnumerable<>)
                || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                || type.IsIEnumerable() && InstanceInstantiator.Instantiable(type, instantiateValues);
        }


        public object? Instantiate(Type type, object? instantiateValues, out object? ignoredInstantiateValues)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (!Instantiable(type, instantiateValues))
                throw InstantiationException.GetNotMatchingTypeException(this, type);

            if (type == typeof(IEnumerable) || type == typeof(IEnumerable<>) || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                type = typeof(List<>).MakeGenericType(type.GetIEnumerableValueType() ?? typeof(object));
                if (TryInstantiateEnumerableConstructor(type, instantiateValues, InstantiateValue, out ignoredInstantiateValues, out var inits))
                    return inits;
            }

            return Instantiate(type, instantiateValues, InstanceInstantiator, InstantiateValue, out ignoredInstantiateValues);
        }

        protected virtual object? InstantiateValue(Type type, object? initializeValues, out object? ignoreInitializeValues) =>
            ValueInstantiator.Construct(type, initializeValues, out ignoreInitializeValues);


        public void Initialize(object? instance, object? initializeValues, out object? ignoredInitializeValues)
        {
            if (instance is null)
            {
                ignoredInitializeValues = initializeValues;
                return;
            }

            var type = instance.GetType();
            if (!Instantiable(type, null))
                throw InstantiationException.GetNotMatchingTypeException(this, type);

            Initialize(type, (IEnumerable)instance, initializeValues, InstanceInstantiator, InitializeValue, out ignoredInitializeValues);
        }

        protected virtual object? InitializeValue(Type type, object? instance, object? initializeValues, out object? ignoredInitializeValues)
        {
            if (instance is null)
                return ValueInstantiator.Construct(type, initializeValues, out ignoredInitializeValues);

            ValueInstantiator.Initialize(instance, initializeValues, out ignoredInitializeValues);
            return instance;
        }


        internal static bool TryInstantiateEnumerableConstructor(Type type, object? instantiateValues, InstantiateDelegate instantiateValue, out object? ignoredInstantiateValues, out IEnumerable? instance)
        {
            if (instantiateValues is null)
            {
                instance = null;
                ignoredInstantiateValues = instantiateValues;
                return false;
            }
            var enumerable = WrapInstantiateValues(type, instantiateValues, out var wrapped);

            var valueType = type.GetIEnumerableValueType() ?? typeof(object);
            ConstructorInfo? constructor = null;
            foreach (var c in type.GetConstructors())
            {
                var ps = c.GetParameters();
                if (ps.Length == 1)
                {
                    var p = ps[0];
                    var t = p.ParameterType;
                    if ((t == typeof(IEnumerable) || t == typeof(IEnumerable<>) || t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        && valueType == (t.GetIEnumerableValueType() ?? typeof(object)))
                    {
                        constructor = c;
                        break;
                    }
                }
            }
            if (constructor is null)
            {
                instance = null;
                ignoredInstantiateValues = instantiateValues;
                return false;
            }

            var tempList = new List<object?>();
            try
            {
                foreach (var v in UnwrapKeyValuePair(enumerable, valueType))
                    try
                    {
                        tempList.Add(instantiateValue(valueType, v, out _));
                    }
                    catch (Exception ex)
                    {
                        throw InstantiationException.GetCanNotInstantiateException(type, instantiateValues, $"[{tempList.Count}]", ex);
                    }
            }
            catch (Exception ex)
            {
                if (wrapped)
                    throw InstantiationException.GetCanNotInstantiateException(type, instantiateValues, ex);
                try
                {
                    tempList = new List<object?>
                    {
                        instantiateValue(valueType, instantiateValues, out _)
                    };
                }
                catch (Exception sex)
                {
                    throw InstantiationException.GetCanNotInstantiateException(type, instantiateValues, new Exception[] {
                        ex,
                        InstantiationException.GetCanNotInstantiateException(type, instantiateValues, "[0]", sex)
                    });
                }
            }

            var value = (IList)typeof(List<>).MakeGenericType(valueType).New();
            foreach (var t in tempList)
                value.Add(t);

            instance = (IEnumerable)constructor.GetParamsFunc()(value);
            ignoredInstantiateValues = null;
            return true;
        }


        internal static object? Instantiate(Type type, object? instantiateValues, IInstantiator instanceInstantiator, InstantiateDelegate instantiateValue, out object? ignoredInstantiateValues)
        {
            try
            {
                var instance = (IEnumerable?)instanceInstantiator.Instantiate(type, instantiateValues, out ignoredInstantiateValues);
                if (instance is null)
                    return null;

                InstantiateInstance(type, instance, ignoredInstantiateValues, instantiateValue, out ignoredInstantiateValues);

                return instance;
            }
            catch (Exception ex)
            {
                if (!SeperateInstantiateValues(type, instantiateValues, out ignoredInstantiateValues, out var elements))
                    throw InstantiationException.GetCanNotInstantiateException(type, instantiateValues, ex);

                try
                {
                    var instance = (IEnumerable?)instanceInstantiator.Instantiate(type, ignoredInstantiateValues, out ignoredInstantiateValues);
                    if (instance is null)
                        return null;

                    if (ignoredInstantiateValues is not null
                        && ignoredInstantiateValues is IEnumerable ie && ie.Cast<object>().Any())
                        throw new InstantiationException(type, instantiateValues, null, $"{instanceInstantiator} don't use {ie}");

                    InstantiateInstance(type, instance, elements, instantiateValue, out ignoredInstantiateValues);

                    return instance;
                }
                catch (Exception sex)
                {
                    throw InstantiationException.GetCanNotInstantiateException(type, instantiateValues, new Exception[] { ex, sex });
                }
            }
        }

        internal static void InstantiateInstance(Type type, IEnumerable instance, object? instantiateValues, InstantiateDelegate instantiateValue, out object? ignoredInstantiateValues)
        {
            var instanceType = instance.GetType();
            if (!instanceType.IsICollection())
            {
                ignoredInstantiateValues = instantiateValues;
                return;
            }
            var enumerable = WrapInstantiateValues(type, instantiateValues, out var wrapped);

            var add = instanceType.GetICollectionType().GetInstanceMemberInvokeDelegate<Action<object, object?>>(nameof(ICollection<object>.Add), 1);
            var count = instanceType.GetICollectionType().GetInstanceMemberAccessDelegate<Func<object, int>>(nameof(ICollection.Count));

            var valueType = instance.GetType().GetIEnumerableValueType()!;
            var tempList = new List<object?>();
            try
            {
                foreach (var e in UnwrapKeyValuePair(enumerable, valueType))
                    try
                    {
                        tempList.Add(instantiateValue(valueType, e, out _));
                    }
                    catch (Exception ex)
                    {
                        throw InstantiationException.GetCanNotInstantiateException(type, instantiateValues, $"[{count(instance) + tempList.Count}]", ex);
                    }
            }
            catch (Exception ex)
            {
                if (wrapped)
                    throw InstantiationException.GetCanNotInstantiateException(type, instantiateValues, ex);
                try
                {
                    tempList = new List<object?>
                    {
                        instantiateValue(valueType, instantiateValues, out _)
                    };
                }
                catch (Exception sex)
                {
                    throw InstantiationException.GetCanNotInstantiateException(type, instantiateValues, new Exception[] {
                        ex,
                        InstantiationException.GetCanNotInstantiateException(type, instantiateValues,  $"[{count(instance)}]", sex)
                    });
                }
            }

            foreach (var t in tempList)
                add(instance, t);

            ignoredInstantiateValues = null;
        }


        internal static void Initialize(Type type, IEnumerable instance, object? initializeValues, IInstantiator instanceInstantiator, InitializeDelegate initializeValue, out object? ignoredInitializeValues)
        {
            try
            {
                instanceInstantiator.Initialize(instance, initializeValues, out ignoredInitializeValues);
                InitializeInstance(instance, ignoredInitializeValues, initializeValue, out ignoredInitializeValues);
            }
            catch (Exception ex)
            {
                if (!SeperateInstantiateValues(type, initializeValues, out ignoredInitializeValues, out var elements))
                    throw InstantiationException.GetCanNotInstantiateException(type, initializeValues, ex);

                try
                {
                    instanceInstantiator.Initialize(instance, ignoredInitializeValues, out ignoredInitializeValues);

                    if (ignoredInitializeValues is not null
                        && ignoredInitializeValues is IEnumerable ie && ie.Cast<object>().Any())
                        throw new InstantiationException(type, initializeValues, null, $"{instanceInstantiator} don't use {ie}");

                    InitializeInstance(instance, elements, initializeValue, out ignoredInitializeValues);
                }
                catch (Exception sex)
                {
                    throw InstantiationException.GetCanNotInstantiateException(type, initializeValues, new Exception[] { ex, sex });
                }
            }
        }

        internal static void InitializeInstance(IEnumerable instance, object? initializeValues, InitializeDelegate initializeValue, out object? ignoredInitializeValues)
        {
            var type = instance.GetType();
            var enumerable = WrapInstantiateValues(type, initializeValues, out var wrapped);

            var valueType = instance.GetType().GetIEnumerableValueType()!;
            try
            {
                var enumerator = UnwrapKeyValuePair(enumerable, valueType).GetEnumerator();
                var instanceEnumerator = instance.GetEnumerator();
                while (enumerator.MoveNext() && instanceEnumerator.MoveNext())
                    initializeValue(valueType, instanceEnumerator.Current, enumerator.Current, out _);

                if (!type.IsICollection())
                {
                    if (!enumerator.MoveNext())
                        ignoredInitializeValues = null;
                    else
                    {
                        IList ignoredValues = enumerator is IEnumerator<KeyValuePair<string?, object?>>
                            ? new List<KeyValuePair<string?, object?>>() : new List<object?>();
                        do
                            ignoredValues.Add(enumerator.Current);
                        while (enumerator.MoveNext());
                        ignoredInitializeValues = ignoredValues;
                    }
                    return;
                }

                var add = type.GetICollectionType().GetInstanceMemberInvokeDelegate<Action<object, object?>>(nameof(ICollection<object>.Add), 1);
                var count = type.GetICollectionType().GetInstanceMemberAccessDelegate<Func<object, int>>(nameof(ICollection.Count));

                while (enumerator.MoveNext())
                    try
                    {
                        add(instance, initializeValue(valueType, null, enumerator.Current, out _));
                    }
                    catch (Exception ex)
                    {
                        throw InstantiationException.GetCanNotInstantiateException(type, initializeValues, $"[{count(instance)}]", ex);
                    }
            }
            catch (Exception ex)
            {
                throw InstantiationException.GetCanNotInstantiateException(type, initializeValues, ex);
            }

            ignoredInitializeValues = null;
        }


        internal static bool SeperateInstantiateValues(Type type, object? instantiateValues, out object? instanceValues, out IEnumerable? enumerable)
        {
            if (instantiateValues is not IEnumerable enumValues)
            {
                instanceValues = instantiateValues;
                enumerable = null;
                return false;
            }

            if (enumValues is not IEnumerable<KeyValuePair<string?, object?>> keyValues)
            {
                instanceValues = Array.Empty<KeyValuePair<string?, object?>>();
                enumerable = enumValues;
                return true;
            }

            var values = new List<KeyValuePair<string?, object?>>();
            var elements = new List<KeyValuePair<string?, object?>>();

            foreach (var pair in keyValues)
                if (!int.TryParse(pair.Key, out _))
                    values.Add(pair);
                else
                    elements.Add(pair);

            instanceValues = values;
            enumerable = elements;
            return elements.Count > 0;
        }

        internal static IEnumerable WrapInstantiateValues(Type type, object? instantiateValues, out bool wrapped)
        {
            wrapped = false;
            if (instantiateValues is not IEnumerable enumerable || instantiateValues.GetType().IsString() && !type.Inherit(typeof(IEnumerable<char>)))
            {
                enumerable = new[] { new KeyValuePair<string?, object?>("0", instantiateValues) };
                wrapped = true;
            }
            return enumerable;
        }

        internal static IEnumerable UnwrapKeyValuePair(IEnumerable enumerable, Type valueType)
        {
            if (enumerable is IEnumerable<KeyValuePair<string?, object?>> keyValues)
            {
                var unwrap = new List<object?>();
                foreach (var pair in keyValues)
                {
                    if (!int.TryParse(pair.Key, out var i))
                        return enumerable;
                    if (i < unwrap.Count)
                        unwrap[i] = pair.Value;
                    else
                    {
                        for (var j = unwrap.Count; j < i; j++)
                            unwrap.Add(null);
                        unwrap.Add(pair.Value);
                    }

                    if (!valueType.IsKeyValuePair())
                        continue;
                    // check if value type is KeyValuePair check underlying object is a KeyValuePair otherwise current KeyValuePair is target
                    if (pair.Value is not IEnumerable<KeyValuePair<string?, object?>> pairValues)
                        return enumerable;
                    var hasKey = false;
                    var hasValue = false;
                    foreach (var p in pairValues)
                    {
                        if (!hasKey && string.Equals(p.Key, nameof(KeyValuePair<string?, object?>.Key), StringComparison.InvariantCultureIgnoreCase))
                            hasKey = true;
                        else if (!hasValue && string.Equals(p.Key, nameof(KeyValuePair<string?, object?>.Value), StringComparison.InvariantCultureIgnoreCase))
                            hasValue = true;
                        else
                            return enumerable;
                    }
                    if (!hasKey || !hasValue)
                        return enumerable;
                }
                enumerable = unwrap;
            }
            return enumerable;
        }


        internal delegate object? InstantiateDelegate(Type type, object? instantiateValues, out object? ignoredInstantiateValues);

        internal delegate object? InitializeDelegate(Type type, object? instance, object? initializeValues, out object? ignoredInitializeValues);


    }
}
