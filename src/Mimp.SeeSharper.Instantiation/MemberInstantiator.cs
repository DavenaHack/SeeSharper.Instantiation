using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.Reflection;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mimp.SeeSharper.Instantiation
{
    /// <summary>
    /// A <see cref="IInstantiator"/> to instantiate the members of a object.
    /// </summary>
    public class MemberInstantiator : IInstantiator
    {


        public IInstantiator InstanceInstantiator { get; }

        public IInstantiator ValueInstantiator { get; }


        public Action<object, string?, object?>? HandleUnknownMember { get; set; }

        public Action<object, string?, Type, object?, Exception>? HandleMemberCanNotSet { get; set; }


        public MemberInstantiator(IInstantiator instanceInstantiator, IInstantiator valueInstantiator)
        {
            InstanceInstantiator = instanceInstantiator ?? throw new ArgumentNullException(nameof(instanceInstantiator));
            ValueInstantiator = valueInstantiator ?? throw new ArgumentNullException(nameof(valueInstantiator));
        }


        public virtual bool Instantiable(Type type, object? instantiateValues)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            return InstanceInstantiator.Instantiable(type, instantiateValues);
        }


        public virtual object? Instantiate(Type type, object? instantiateValues, out object? ignoredInstantiateValues)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (!Instantiable(type, instantiateValues))
                throw InstantiationException.GetNotMatchingTypeException(this, type);

            var instance = InstanceInstantiator.Instantiate(type, instantiateValues, out ignoredInstantiateValues);
            if (instance is null)
                return null;

            InstantiateInstance(instance, ignoredInstantiateValues, out ignoredInstantiateValues);
            
            return instance;
        }

        protected internal virtual void InstantiateInstance(object instance, object? instantiateValues, out object? ignoredInitializeValues) =>
            SetMembers(instance, instantiateValues,
                (instance, property, value) => property.SetValue(instance, InstantiateMember(property.PropertyType, value, out _)),
                (instance, field, value) => field.SetValue(instance, InstantiateMember(field.FieldType, value, out _)),
                (instance, name, value) => instance.GetType().GetDynamicInstanceMemberAssignAction(name)(instance, InstantiateMember(typeof(object), value, out _)),
                out ignoredInitializeValues);

        protected virtual object? InstantiateMember(Type type, object? instantiateValues, out object? ignoredInstantiateValues) =>
            ValueInstantiator.Construct(type, instantiateValues, out ignoredInstantiateValues);


        public virtual void Initialize(object? instance, object? initializeValues, out object? ignoredInitializeValues)
        {
            if (instance is null || initializeValues is null)
            {
                ignoredInitializeValues = initializeValues;
                return;
            }

            SetMembers(instance, initializeValues,
                (instance, property, value) =>
                {
                    var old = property.GetValue(instance);
                    value = InitializeMember(property.PropertyType, old, value, out _);
                    property.SetValue(instance, value);
                },
                (instance, field, value) =>
                {
                    var old = field.GetValue(instance);
                    value = InitializeMember(field.FieldType, old, value, out _);
                    if (!ReferenceEquals(value, old))
                        field.SetValue(instance, value);
                },
                (instance, name, value) =>
                {
                    object? old = null;
                    try
                    {
                        old = instance.GetType().GetDynamicInstanceMemberAccessFunc(name)(instance);
                    }
                    catch { }
                    value = InitializeMember(typeof(object), old, value, out _);
                    if (!ReferenceEquals(value, old))
                        instance.GetType().GetDynamicInstanceMemberAssignAction(name)(instance, value);
                }, out ignoredInitializeValues);
        }

        protected virtual object? InitializeMember(Type type, object? instance, object? initializeValues, out object? ignoredInitializeValues)
        {
            if (instance is null)
                return ValueInstantiator.Construct(type, initializeValues, out ignoredInitializeValues);

            ValueInstantiator.Initialize(instance, initializeValues, out ignoredInitializeValues);
            return instance;
        }


        private void SetMembers(
            object instance,
            object? instantiateValues,
            Action<object, PropertyInfo, object?> setProperty,
            Action<object, FieldInfo, object?> setField,
            Action<object, string, object?> setDynamic,
            out object? ignoredInstantiateValues
        )
        {
            if (instance is null || instantiateValues is not IEnumerable<KeyValuePair<string?, object?>> values)
            {
                ignoredInstantiateValues = instantiateValues;
                return;
            }

            var type = instance.GetType();
            var props = type.GetProperties();

            var ignoredValues = values.ToList();
            var handleSet = HandleMemberCanNotSet;
            var handleUnknown = HandleUnknownMember;

            var properties = type.GetProperties();
            var fields = type.GetFields();

            foreach (var pair in values)
            {
                var name = pair.Key;
                var value = pair.Value;
                var used = false;
                if (name is not null)
                {
                    void trySetValue(Action set, Type memberType)
                    {
                        try
                        {
                            set();
                        }
                        catch (Exception ex)
                        {
                            if (handleSet is null)
                                throw InstantiationException.GetCanNotInstantiateException(type, instantiateValues, $".{name}", ex);
                            else
                                try
                                {
                                    handleSet.Invoke(instance, name, memberType, value, ex);
                                }
                                catch (Exception hex)
                                {
                                    throw InstantiationException.GetCanNotInstantiateException(type, instantiateValues, $".{name}", hex);
                                }
                        }
                        ignoredValues.Remove(pair);
                        used = true;
                    }

                    foreach (var prop in properties)
                        if (string.Equals(name, prop.Name, StringComparison.InvariantCultureIgnoreCase))
                            trySetValue(() => setProperty(instance, prop, value), prop.PropertyType);
                    if (used)
                        continue;

                    foreach (var field in fields)
                        if (string.Equals(name, field.Name, StringComparison.InvariantCultureIgnoreCase))
                            trySetValue(() => setField(instance, field, value), field.FieldType);
                    if (used)
                        continue;

                    if (instance is IDynamicMetaObjectProvider provider)
                    {
                        foreach (var m in provider.GetMetaObject(Expression.Constant(instance)).GetDynamicMemberNames())
                            if (string.Equals(name, m, StringComparison.InvariantCultureIgnoreCase))
                                trySetValue(() => setDynamic(instance, m, value), typeof(object));
                        if (used)
                            continue;

                        trySetValue(() => setDynamic(instance, name, value), typeof(object));
                        if (used)
                            continue;
                    }
                }

                if (!used)
                    if (handleUnknown is null)
                        throw InstantiationException.GetNoMemberException(type, instantiateValues, name);
                    else
                        try
                        {
                            handleUnknown.Invoke(instance, name, value);
                        }
                        catch (Exception ex)
                        {
                            throw InstantiationException.GetCanNotInstantiateException(type, instantiateValues, $".{name}", ex);
                        }
            }

            ignoredInstantiateValues = ignoredValues;
        }


    }
}
