using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.ObjectDescription;
using Mimp.SeeSharper.ObjectDescription.Abstraction;
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


        public bool Instantiable(Type type, IObjectDescription description)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (description is null)
                throw new ArgumentNullException(nameof(description));

            return type.IsKeyValuePair();
        }


        public object? Instantiate(Type type, IObjectDescription description, out IObjectDescription? ignored)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (description is null)
                throw new ArgumentNullException(nameof(description));
            if (!Instantiable(type, description))
                throw InstantiationException.GetNotMatchingTypeException(this, type, description);

            var types = type.GetKeyValuePairKeyValueType()!.ToArray();

            var constDesc = description;
            if (constDesc.HasValue)
            {
                if (constDesc.Value is null)
                {
                    ignored = null;
                    return type.Default();
                }

                var initType = constDesc.Value.GetType();
                dynamic dyn = constDesc.Value;
                if (initType.IsKeyValuePair())
                {
                    var result = Create(ObjectDescriptions.Constant((object?)dyn.Key), out var keyIgnore,
                        ObjectDescriptions.Constant((object?)dyn.Value), out var valueIgnore);

                    ignored = null;
                    if (keyIgnore is not null)
                        ignored = keyIgnore;
                    if (valueIgnore is not null)
                        ignored = ignored?.Concat(valueIgnore).Constant() ?? valueIgnore;

                    return result;
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
            else
            {
                KeyValuePair<string?, IObjectDescription>? key = null;
                KeyValuePair<string?, IObjectDescription>? value = null;

                foreach (var c in constDesc.Children)
                {
                    if (key is null && string.Equals(c.Key, nameof(KeyValuePair<string?, object?>.Key), StringComparison.InvariantCultureIgnoreCase))
                    {
                        key = c;
                    }
                    else if (value is null && string.Equals(c.Key, nameof(KeyValuePair<string?, object?>.Value), StringComparison.InvariantCultureIgnoreCase))
                    {
                        value = c;
                    }
                    else
                        throw InstantiationException.GetNoMemberException(type, description, c.Key);
                }

                if (!key.HasValue && !value.HasValue)
                    try
                    {
                        return Instantiate(type, ObjectDescriptions.NullDescription, out ignored);
                    }
                    catch (Exception ex)
                    {
                        throw InstantiationException.GetCanNotInstantiateException(type, description, ex);
                    }
                else if (key.HasValue && value.HasValue)
                {
                    var result = Create(key.Value.Value, out var keyIgnore, value.Value.Value, out var valueIgnore);

                    ignored = null;
                    if (keyIgnore is not null)
                        ignored = ObjectDescriptions.Constant(key.Value.Key, keyIgnore);
                    if (valueIgnore is not null)
                        ignored = ignored?.Append(value.Value.Key, valueIgnore).Constant()
                            ?? ObjectDescriptions.Constant(value.Value.Key, valueIgnore);

                    return result;
                }
                else
                    throw new InstantiationException(type, description, null,
                        $@"{type} has to have a ""{nameof(KeyValuePair<string?, object?>.Key)}"" "
                        + $@"and ""{nameof(KeyValuePair<string?, object?>.Value)}"".");

            }
            throw InstantiationException.GetCanNotInstantiateException(type, description);


            object? Create(IObjectDescription key, out IObjectDescription? ignoredKey, IObjectDescription value, out IObjectDescription? ignoredValue)
            {
                try
                {
                    object? keyValue;
                    try
                    {
                        keyValue = InstantiateKey(types[0], key, out ignoredKey);
                    }
                    catch (Exception ex)
                    {
                        throw InstantiationException.GetCanNotInstantiateException(types[0], key, $".{nameof(KeyValuePair<string?, object?>.Key)}", ex);
                    }
                    object? valueValue;
                    try
                    {
                        valueValue = InstantiateValue(types[1], value, out ignoredValue);
                    }
                    catch (Exception ex)
                    {
                        throw InstantiationException.GetCanNotInstantiateException(types[1], value, $".{nameof(KeyValuePair<string?, object?>.Value)}", ex);
                    }
                    return type.GetNewParameterFunc(types)(new[] { keyValue, valueValue });
                }
                catch (Exception ex)
                {
                    throw InstantiationException.GetCanNotInstantiateException(type, description, ex);
                }
            }
        }

        protected virtual object? InstantiateKey(Type type, IObjectDescription description, out IObjectDescription? ignored) =>
            KeyInstantiator.Construct(type, description, out ignored);

        protected virtual object? InstantiateValue(Type type, IObjectDescription description, out IObjectDescription? ignored) =>
            ValueInstantiator.Construct(type, description, out ignored);


        public object? Initialize(Type type, object? instance, IObjectDescription description, out IObjectDescription? ignored)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (description is null)
                throw new ArgumentNullException(nameof(description));

            if (instance is null)
                return Instantiate(type, description, out ignored);

            var constDesc = description.Constant();
            type = instance.GetType();
            if (!Instantiable(type, constDesc))
                throw InstantiationException.GetNotMatchingTypeException(this, type, description);

            if (constDesc.IsNullOrEmpty())
            {
                ignored = null;
                return instance;
            }

            var types = type.GetKeyValuePairKeyValueType()!.ToArray();

            if (constDesc.HasValue)
            {
                var initType = constDesc.GetType();
                dynamic pair = constDesc;

                if (initType.IsKeyValuePair())
                {
                    instance = Initialize(instance, ObjectDescriptions.Constant((object?)pair.Key), out var keyIgnore,
                        ObjectDescriptions.Constant((object?)pair.Value), out var valueIgnore);

                    ignored = null;
                    if (keyIgnore is not null)
                        ignored = keyIgnore;
                    if (valueIgnore is not null)
                        ignored = ignored?.Concat(valueIgnore).Constant() ?? valueIgnore;

                    return instance;
                }
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
            else
            {
                KeyValuePair<string?, IObjectDescription>? key = null;
                KeyValuePair<string?, IObjectDescription>? value = null;

                foreach (var c in constDesc.Children)
                {
                    if (key is null && string.Equals(c.Key, nameof(KeyValuePair<string?, object?>.Key), StringComparison.InvariantCultureIgnoreCase))
                    {
                        key = c;
                    }
                    else if (value is null && string.Equals(c.Key, nameof(KeyValuePair<string?, object?>.Value), StringComparison.InvariantCultureIgnoreCase))
                    {
                        value = c;
                    }
                    else
                        throw InstantiationException.GetNoMemberException(type, description, c.Key);
                }

                if (key.HasValue || value.HasValue)
                {
                    instance = Initialize(instance, key.HasValue ? key.Value.Value : null, out var keyIgnore,
                        value.HasValue ? value.Value.Value : null, out var valueIgnore);

                    ignored = null;
                    if (key is not null && keyIgnore is not null)
                        ignored = ObjectDescriptions.Constant(key.Value.Key, keyIgnore);
                    if (value is not null && valueIgnore is not null)
                        ignored = ignored?.Append(value.Value.Key, valueIgnore).Constant()
                            ?? ObjectDescriptions.Constant(value.Value.Key, valueIgnore);

                    return instance;
                }
            }


            throw InstantiationException.GetCanNotInstantiateException(type, description);


            object Initialize(object instance,
                IObjectDescription? key, out IObjectDescription? ignoredKey,
                IObjectDescription? value, out IObjectDescription? ignoredValue)
            {
                dynamic pair = instance;
                try
                {
                    object? keyValue;
                    if (key is null)
                    {
                        keyValue = pair.Key;
                        ignoredKey = null;
                    }
                    else
                        try
                        {
                            keyValue = InitializeKey(types[0], pair.Key, key, out ignoredKey);
                        }
                        catch (Exception ex)
                        {
                            throw InstantiationException.GetCanNotInstantiateException(types[0], key, $".{nameof(KeyValuePair<string?, object?>.Key)}", ex);
                        }
                    object? valueValue;
                    if (value is null)
                    {
                        valueValue = pair.Value;
                        ignoredValue = null;
                    }
                    else
                        try
                        {
                            valueValue = InitializeValue(types[1], pair.Value, value, out ignoredValue);
                        }
                        catch (Exception ex)
                        {
                            throw InstantiationException.GetCanNotInstantiateException(types[1], value, $".{nameof(KeyValuePair<string?, object?>.Value)}", ex);
                        }
                    if (ReferenceEquals(pair.Key, keyValue) && ReferenceEquals(pair.Value, valueValue))
                        return pair;
                    else
                        return type.GetNewParameterFunc(types)(new[] { keyValue, valueValue });
                }
                catch (Exception ex)
                {
                    throw InstantiationException.GetCanNotInstantiateException(type, description, ex);
                }
            }
        }

        protected virtual object? InitializeKey(Type type, object? instance, IObjectDescription description, out IObjectDescription? ignored) =>
            KeyInstantiator.Initialize(instance, description, out ignored);

        protected virtual object? InitializeValue(Type type, object? instance, IObjectDescription description, out IObjectDescription? ignored) =>
            ValueInstantiator.Initialize(instance, description, out ignored);


    }
}
