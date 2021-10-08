using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.ObjectDescription;
using Mimp.SeeSharper.ObjectDescription.Abstraction;
using Mimp.SeeSharper.Reflection;
using System;
using System.Collections.Generic;
using System.Dynamic;
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


        public bool Instantiable(Type type, IObjectDescription description)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (description is null)
                throw new ArgumentNullException(nameof(description));

            return InstanceInstantiator.Instantiable(type, description);
        }


        public object? Instantiate(Type type, IObjectDescription description, out IObjectDescription? ignored)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (description is null)
                throw new ArgumentNullException(nameof(description));
            if (!Instantiable(type, description))
                throw InstantiationException.GetNotMatchingTypeException(this, type, description);

            var instance = InstanceInstantiator.Instantiate(type, description, out ignored);
            if (instance is null)
                return null;

            return InstantiateInstance(instance, ignored ?? ObjectDescriptions.NullDescription, out ignored);
        }

        protected internal virtual object InstantiateInstance(object instance, IObjectDescription description, out IObjectDescription? ignored)
        {
            InitMembers(instance, description,
                (instance, property, description) =>
                {
                    property.SetValue(instance, InstantiateMember(property.PropertyType, description, out var ignore));
                    return ignore;
                },
                (instance, field, description) =>
                {
                    field.SetValue(instance, InstantiateMember(field.FieldType, description, out var ignore));
                    return ignore;
                },
                (instance, name, description) =>
                {
                    instance.GetType().GetDynamicInstanceMemberAssignAction(name)(instance, InstantiateMember(typeof(object), description, out var ignore));
                    return ignore;
                }, out ignored);
            return instance;
        }

        protected virtual object? InstantiateMember(Type type, IObjectDescription description, out IObjectDescription? ignored) =>
            ValueInstantiator.Construct(type, description, out ignored);


        public virtual object? Initialize(Type type, object? instance, IObjectDescription description, out IObjectDescription? ignored)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (description is null)
                throw new ArgumentNullException(nameof(description));

            if (instance is null)
                return Instantiate(type, description, out ignored);

            type = instance.GetType();
            if (!Instantiable(type, description))
                throw InstantiationException.GetNotMatchingTypeException(this, type, description);

            InitMembers(instance, description,
                (instance, property, description) =>
                {
                    var old = property.GetValue(instance);
                    var value = InitializeMember(property.PropertyType, old, description, out var ignore);
                    if (!ReferenceEquals(old, value))
                        property.SetValue(instance, value);
                    return ignore;
                },
                (instance, field, desciption) =>
                {
                    var old = field.GetValue(instance);
                    var value = InitializeMember(field.FieldType, old, description, out var ignore);
                    if (!ReferenceEquals(value, old))
                        field.SetValue(instance, value);
                    return ignore;
                },
                (instance, name, description) =>
                {
                    object? old = null;
                    try
                    {
                        old = instance.GetType().GetDynamicInstanceMemberAccessFunc(name)(instance);
                    }
                    catch { }
                    var value = InitializeMember(typeof(object), old, description, out var ignore);
                    if (!ReferenceEquals(value, old))
                        instance.GetType().GetDynamicInstanceMemberAssignAction(name)(instance, value);
                    return ignore;
                }, out ignored);

            return instance;
        }

        protected virtual object? InitializeMember(Type type, object? instance, IObjectDescription description, out IObjectDescription? ignored)
        {
            if (instance is null)
                return ValueInstantiator.Construct(type, description, out ignored);

            return ValueInstantiator.Initialize(instance, description, out ignored);
        }


        protected void InitMembers(
            object instance,
            IObjectDescription description,
            Func<object, PropertyInfo, IObjectDescription, IObjectDescription?> setProperty,
            Func<object, FieldInfo, IObjectDescription, IObjectDescription?> setField,
            Func<object, string, IObjectDescription, IObjectDescription?> setDynamic,
            out IObjectDescription? ignored
        )
        {
            if (instance is null || description.HasValue || description.IsWrappedValue())
            {
                ignored = description.IsNullOrEmpty() ? null : description;
                return;
            }

            var ignoredValues = description;

            var type = instance.GetType();

            var handleSet = HandleMemberCanNotSet;
            var handleUnknown = HandleUnknownMember;

            var properties = type.GetProperties();
            var fields = type.GetFields();

            foreach (var child in description.Children!)
                try
                {
                    var name = child.Key;
                    var desc = child.Value;
                    var used = false;
                    if (name is not null)
                    {
                        void trySetValue(Func<IObjectDescription?> set, Type memberType)
                        {
                            try
                            {
                                var ignore = set();

                                ignoredValues = ignoredValues.Remove(child);
                                if (ignore is not null)
                                    ignoredValues = ignoredValues.Append(name, ignore);

                                used = true;
                            }
                            catch (Exception ex)
                            {
                                if (handleSet is null)
                                    throw InstantiationException.GetCanNotInstantiateException(memberType, desc, $".{name}", ex);
                                else
                                    try
                                    {
                                        handleSet.Invoke(instance, name, memberType, desc, ex);
                                    }
                                    catch (Exception hex)
                                    {
                                        throw InstantiationException.GetCanNotInstantiateException(memberType, desc, $".{name}", hex);
                                    }
                            }
                        }

                        foreach (var prop in properties)
                            if (string.Equals(name, prop.Name, StringComparison.InvariantCultureIgnoreCase))
                                trySetValue(() => setProperty(instance, prop, desc), prop.PropertyType);
                        if (used)
                            continue;

                        foreach (var field in fields)
                            if (string.Equals(name, field.Name, StringComparison.InvariantCultureIgnoreCase))
                                trySetValue(() => setField(instance, field, desc), field.FieldType);
                        if (used)
                            continue;

                        if (instance is IDynamicMetaObjectProvider provider)
                        {
                            foreach (var m in provider.GetMetaObject(Expression.Constant(instance)).GetDynamicMemberNames())
                                if (string.Equals(name, m, StringComparison.InvariantCultureIgnoreCase))
                                    trySetValue(() => setDynamic(instance, m, desc), typeof(object));
                            if (used)
                                continue;

                            trySetValue(() => setDynamic(instance, name, desc), typeof(object));
                            if (used)
                                continue;
                        }
                    }

                    if (!used)
                        if (handleUnknown is null)
                            throw InstantiationException.GetNoMemberException(typeof(object), desc, name);
                        else
                            try
                            {
                                handleUnknown.Invoke(instance, name, desc);
                            }
                            catch (Exception ex)
                            {
                                throw InstantiationException.GetCanNotInstantiateException(typeof(object), desc, $".{name}", ex);
                            }
                }
                catch (Exception ex)
                {
                    throw InstantiationException.GetCanNotInstantiateException(type, description, ex);
              }

            ignored = ignoredValues.IsNullOrEmpty() ? null : ignoredValues.Constant();
        }


    }
}
