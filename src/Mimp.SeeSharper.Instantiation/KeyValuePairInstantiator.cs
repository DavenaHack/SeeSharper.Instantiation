using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mimp.SeeSharper.Instantiation
{
    /// <summary>
    /// A <see cref="IInstantiator"/> to instantiate <see cref="KeyValuePair{TKey, TValue}"/>.
    /// </summary>
    public class KeyValuePairInstantiator : IInstantiator
    {


        public IInstantiator KeyInstantiator { get; }

        public IInstantiator ValueInstantiator { get; }


        public KeyValuePairInstantiator(IInstantiator keyInstantiator, IInstantiator valueInstantiator)
        {
            KeyInstantiator = keyInstantiator ?? throw new ArgumentNullException(nameof(keyInstantiator));
            ValueInstantiator = valueInstantiator ?? throw new ArgumentNullException(nameof(valueInstantiator));
        }

        public KeyValuePairInstantiator(IInstantiator instantiator)
            : this(instantiator, instantiator) { }


        public bool Instantiable(Type type, object? instantiateValues)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            return type.IsKeyValuePair();
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

            var initType = instantiateValues.GetType();
            dynamic dyn = instantiateValues;
            var types = type.GetKeyValuePairKeyValueType()!.ToArray();
            object? Create(object? key, object? value)
            {
                try
                {
                    key = InstantiateKey(types[0], key, out _);
                }
                catch (Exception ex)
                {
                    throw InstantiationException.GetCanNotInstantiateException(type, instantiateValues, $".{nameof(KeyValuePair<string?, object?>.Key)}", ex);
                }
                try
                {
                    value = InstantiateValue(types[1], value, out _);
                }
                catch (Exception ex)
                {
                    throw InstantiationException.GetCanNotInstantiateException(type, instantiateValues, $".{nameof(KeyValuePair<string?, object?>.Value)}", ex);
                }
                return type.GetNewParameterFunc(types)(new[] { key, value });
            }

            if (initType.IsKeyValuePair())
            {
                var result = Create(dyn.Key, dyn.Value);
                ignoredInstantiateValues = null;
                return result;
            }

            if (instantiateValues is IEnumerable<KeyValuePair<string?, object?>> enumerable)
            {
                var has = false;
                KeyValuePair<string?, object?>? pair = null;
                var hasKey = false;
                var hasValue = false;
                object? key = null;
                object? value = null;

                foreach (var p in enumerable)
                {
                    if (!has && !hasKey && !hasValue && string.IsNullOrEmpty(p.Key))
                    {
                        has = true;
                        pair = p;
                    }
                    else if (!has && !hasKey && string.Equals(p.Key, nameof(KeyValuePair<string?, object?>.Key), StringComparison.InvariantCultureIgnoreCase))
                    {
                        hasKey = true;
                        key = p.Value;
                    }
                    else if (!has && !hasValue && string.Equals(p.Key, nameof(KeyValuePair<string?, object?>.Value), StringComparison.InvariantCultureIgnoreCase))
                    {
                        hasValue = true;
                        value = p.Value;
                    }
                    else
                        throw InstantiationException.GetNoMemberException(type, instantiateValues, p.Key);
                }

                if (has)
                {
                    var result = Create(pair!.Value.Key, pair.Value.Value);
                    ignoredInstantiateValues = null;
                    return result;
                }
                else if (!hasKey && !hasValue)
                    try
                    {
                        return Instantiate(type, null, out ignoredInstantiateValues);
                    }
                    catch (Exception ex)
                    {
                        throw InstantiationException.GetCanNotInstantiateException(type, instantiateValues, ex);
                    }
                else if (hasKey && hasValue)
                {
                    var result = Create(key, value);
                    ignoredInstantiateValues = null;
                    return result;
                }
                else
                    throw new InstantiationException(type, instantiateValues, null, $@"{type} has to have a ""{nameof(KeyValuePair<string?, object?>.Key)}"" and ""{nameof(KeyValuePair<string?, object?>.Value)}"".");
            }

            throw InstantiationException.GetCanNotInstantiateException(type, instantiateValues);
        }

        protected virtual object? InstantiateKey(Type type, object? instantiateValues, out object? ignoredInstantiateValues) =>
            KeyInstantiator.Construct(type, instantiateValues, out ignoredInstantiateValues);

        protected virtual object? InstantiateValue(Type type, object? instantiateValues, out object? ignoredInstantiateValues) =>
            ValueInstantiator.Construct(type, instantiateValues, out ignoredInstantiateValues);


        public void Initialize(object? instance, object? initializeValues, out object? ignoredInitializeValues)
        {
            if (instance is null || initializeValues is null)
            {
                ignoredInitializeValues = initializeValues;
                return;
            }

            var type = instance.GetType();
            if (!Instantiable(type, null))
                throw InstantiationException.GetNotMatchingTypeException(this, type);

            var initType = initializeValues.GetType();
            dynamic dyn = initializeValues;
            var types = type.GetKeyValuePairKeyValueType()!.ToArray();
            void Initialize(object? key, object? value)
            {
                try
                {
                    InitializeKey(types[0], instance, key, out _);
                }
                catch (Exception ex)
                {
                    throw InstantiationException.GetCanNotInstantiateException(type, initializeValues, $".{nameof(KeyValuePair<string?, object?>.Key)}", ex);
                }
                try
                {
                    InitializeValue(types[1], instance, value, out _);
                }
                catch (Exception ex)
                {
                    throw InstantiationException.GetCanNotInstantiateException(type, initializeValues, $".{nameof(KeyValuePair<string?, object?>.Value)}", ex);
                }
            }
            
            if (initType.IsKeyValuePair())
            {
                Initialize(dyn.Key, dyn.Value);
                ignoredInitializeValues = null;
                return;
            }

            if (initializeValues is IEnumerable<KeyValuePair<string?, object?>> enumerable)
            {
                var has = false;
                KeyValuePair<string?, object?>? pair = null;
                var hasKey = false;
                var hasValue = false;
                object? key = null;
                object? value = null;

                foreach (var p in enumerable)
                {
                    if (!has && !hasKey && !hasValue && string.IsNullOrEmpty(p.Key))
                    {
                        has = true;
                        pair = p;
                    }
                    else if (!has && !hasKey && string.Equals(p.Key, nameof(KeyValuePair<string?, object?>.Key), StringComparison.InvariantCultureIgnoreCase))
                    {
                        hasKey = true;
                        key = p.Value;
                    }
                    else if (!has && !hasValue && string.Equals(p.Key, nameof(KeyValuePair<string?, object?>.Value), StringComparison.InvariantCultureIgnoreCase))
                    {
                        hasValue = true;
                        value = p.Value;
                    }
                    else
                        throw InstantiationException.GetNoMemberException(type, initializeValues, p.Key);
                }

                if (has)
                    Initialize(pair!.Value.Key, pair.Value.Value);
                else if (!hasKey || !hasValue)
                    Initialize(key, value);
                ignoredInitializeValues = null;
                return;
            }

            throw InstantiationException.GetCanNotInstantiateException(type, initializeValues);
        }

        protected virtual void InitializeKey(Type type, object? instance, object? initializeValues, out object? ignoredInitializeValues) =>
            KeyInstantiator.Initialize(instance, initializeValues, out ignoredInitializeValues);

        protected virtual void InitializeValue(Type type, object? instance, object? initializeValues, out object? ignoredInitializeValues) =>
            ValueInstantiator.Initialize(instance, initializeValues, out ignoredInitializeValues);


    }
}
