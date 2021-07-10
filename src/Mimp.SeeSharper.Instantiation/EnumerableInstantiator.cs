using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
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
                if (TryInstantiateEnumerableConstructor(type, instantiateValues, InitializeValues, out ignoredInstantiateValues, out var inits))
                    return inits;
            }

            var instance = (IEnumerable?)InstanceInstantiator.Instantiate(type, instantiateValues, out ignoredInstantiateValues);
            if (instance is null)
                return null;

            InstantiateInstance(type, instance, ignoredInstantiateValues, InitializeValues, out ignoredInstantiateValues);

            return instance;
        }

        protected virtual object? InitializeValues(Type type, object? initializeValues, out object? ignoreInitializeValues) =>
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

            InstanceInstantiator.Initialize(instance, initializeValues, out ignoredInitializeValues);
            InitializeInstance((IEnumerable)instance, ignoredInitializeValues, InitializeValue, out ignoredInitializeValues);
        }

        protected virtual object? InitializeValue(Type type, object? instance, object? initializeValues, out object? ignoredInitializeValues)
        {
            if (instance is null)
                return ValueInstantiator.Construct(type, initializeValues, out ignoredInitializeValues);

            ValueInstantiator.Initialize(instance, initializeValues, out ignoredInitializeValues);
            return instance;
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

            var add = instanceType.GetInstanceMemberInvokeDelegate<Action<object, object?>>(nameof(ICollection<object>.Add), 1);
            var count = instanceType.GetInstanceMemberAccessDelegate<Func<object, int>>(nameof(ICollection.Count));

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


        internal static void InitializeInstance(IEnumerable instance, object? initializeValues, InitializeDelegate initializeValue, out object? ignoredInitializeValues)
        {
            // TODO do some smart initialize actions
            ignoredInitializeValues = initializeValues;
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
