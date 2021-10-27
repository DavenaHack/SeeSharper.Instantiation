using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.ObjectDescription;
using Mimp.SeeSharper.ObjectDescription.Abstraction;
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


        public bool Instantiable(Type type, IObjectDescription description)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (description is null)
                throw new ArgumentNullException(nameof(description));

            return type == typeof(IEnumerable)
                || type == typeof(IEnumerable<>)
                || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                || type.IsIEnumerable() && InstanceInstantiator.Instantiable(type, description);
        }


        public object? Instantiate(Type type, IObjectDescription description, out IObjectDescription? ignored)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (description is null)
                throw new ArgumentNullException(nameof(description));
            if (!Instantiable(type, description))
                throw InstantiationException.GetNotMatchingTypeException(this, type, description);

            if (type == typeof(IEnumerable) || type == typeof(IEnumerable<>) || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                type = typeof(List<>).MakeGenericType(type.GetIEnumerableValueType() ?? typeof(object));
                if (TryInstantiateEnumerableConstructor(type, description, InstantiateValue, out ignored, out var inits))
                    return inits;
            }

            return Instantiate(type, description, InstanceInstantiator, InstantiateValue, out ignored);
        }

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

            type = instance.GetType();
            if (!Instantiable(type, description))
                throw InstantiationException.GetNotMatchingTypeException(this, type, description);

            return Initialize(type, (IEnumerable)instance, description, InstanceInstantiator, InitializeValue, out ignored);
        }

        protected virtual object? InitializeValue(Type type, object? instance, IObjectDescription description, out IObjectDescription? ignored)
        {
            if (instance is null)
                return ValueInstantiator.Construct(type, description, out ignored);

            return ValueInstantiator.Initialize(type, instance, description, out ignored);
        }


        internal static bool TryInstantiateEnumerableConstructor(Type type,
            IObjectDescription description, InstantiateDelegate instantiateValue,
            out IObjectDescription? ignored, out IEnumerable? instance)
        {
            var constDesc = description.Constant();
            if (constDesc.IsNull())
            {
                instance = null;
                ignored = null;
                return true;
            }
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
                ignored = description;
                return false;
            }

            var exceptions = new List<Exception>();
            try
            {
                if (SeperateMembersAndElements(constDesc, out var members, out var elements)
                    && members.IsNullOrEmpty())
                    return Create(valueType, elements, out ignored, out instance);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
            try
            {
                if (SeperateMembersAndElements(UnifyEnumerable(constDesc), out var members, out var elements)
                    && members.IsNullOrEmpty())
                    return Create(valueType, elements, out ignored, out instance);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }

            throw InstantiationException.GetCanNotInstantiateException(type, description, exceptions);

            bool Create(Type valueType, IObjectDescription elements, out IObjectDescription? ignored, out IEnumerable instance)
            {
                ignored = elements;
                var tempList = new List<object?>();
                try
                {
                    foreach (var e in elements.Children)
                        try
                        {
                            tempList.Add(instantiateValue(valueType, e.Value, out var ig));
                            ignored = ignored.Remove(e);
                            if (ig is not null)
                                ignored = ignored.Append(e.Key, ig);
                        }
                        catch (Exception ex)
                        {
                            throw InstantiationException.GetCanNotInstantiateException(type, e.Value.Children.First().Value, $"[{tempList.Count}]", ex);
                        }

                    var value = (IList)typeof(List<>).MakeGenericType(valueType).New();
                    foreach (var t in tempList)
                        value.Add(t);

                    ignored = ignored.IsNullOrEmpty() ? null : ignored.Constant();
                    instance = (IEnumerable)constructor.GetParamsFunc()(value);
                    return true;
                }
                catch (Exception ex)
                {
                    throw InstantiationException.GetCanNotInstantiateException(type, description, ex);
                }
            }
        }


        internal static object? Instantiate(Type type, IObjectDescription description,
            IInstantiator instanceInstantiator, InstantiateDelegate instantiateValue, out IObjectDescription? ignored)
        {
            var constDesc = description.Constant();
            Exception? originEx = null;
            if (SeperateMembersAndElements(constDesc, out var members, out var elements))
                try
                {
                    var instance = (IEnumerable?)instanceInstantiator.Instantiate(type, members, out ignored);
                    if (instance is null)
                    {
                        ignored = ignored?.Concat(elements) ?? (elements.IsNullOrEmpty() ? null : elements);
                        return null;
                    }

                    InstantiateInstance(instance, elements, out var ignore);
                    if (ignore is not null)
                        ignored = ignored?.Concat(ignore) ?? ignore;

                    return instance;
                }
                catch (Exception ex)
                {
                    originEx = ex;
                }

            try
            {
                return (IEnumerable?)instanceInstantiator.Instantiate(type, constDesc, out ignored);
            }
            catch (Exception ex)
            {
                throw InstantiationException.GetCanNotInstantiateException(type, description, originEx is null ? new[] { ex } : new[] { originEx, ex });
            }

            void InstantiateInstance(IEnumerable instance, IObjectDescription elements, out IObjectDescription? ignored)
            {
                var instanceType = instance.GetType();
                if (!instanceType.IsICollection())
                {
                    ignored = elements;
                    return;
                }
                elements = UnifyEnumerable(elements);

                var add = instanceType.GetICollectionType().GetInstanceMemberInvokeDelegate<Action<object, object?>>(nameof(ICollection<object>.Add), 1);
                var count = instanceType.GetICollectionType().GetInstanceMemberAccessDelegate<Func<object, int>>(nameof(ICollection.Count));

                var valueType = instanceType.GetIEnumerableValueType()!;
                var tempList = new List<object?>();
                ignored = elements;
                try
                {
                    foreach (var e in elements.Children)
                        try
                        {
                            tempList.Add(instantiateValue(valueType, e.Value, out var ig));
                            ignored = ignored.Remove(e);
                            if (ig is not null)
                                ignored = ignored.Append(e.Key, ig);
                        }
                        catch (Exception ex)
                        {
                            throw InstantiationException.GetCanNotInstantiateException(type, elements, $"[{count(instance) + tempList.Count}]", ex);
                        }
                }
                catch (Exception ex)
                {
                    throw InstantiationException.GetCanNotInstantiateException(type, elements, ex);
                }

                foreach (var t in tempList)
                    add(instance, t);

                ignored = ignored.IsNullOrEmpty() ? null : ignored.Constant();
            }
        }


        internal static IEnumerable Initialize(Type type, IEnumerable instance, IObjectDescription description,
            IInstantiator instanceInstantiator, InitializeDelegate initializeValue, out IObjectDescription? ignored)
        {
            var constDesc = description.Constant();
            Exception? originEx = null;
            if (SeperateMembersAndElements(constDesc, out var members, out var elements))
                try
                {
                    instance = (IEnumerable)instanceInstantiator.Initialize(type, instance, members, out ignored)!;

                    instance = InitializeInstance(instance, elements, out var ignore);
                    if (ignore is not null)
                        ignored = ignored?.Concat(ignore) ?? ignore;

                    return instance;
                }
                catch (Exception ex)
                {
                    originEx = ex;
                }

            try
            {
                return (IEnumerable)instanceInstantiator.Initialize(type, instance, constDesc, out ignored)!;
            }
            catch (Exception ex)
            {
                throw InstantiationException.GetCanNotInstantiateException(type, description, originEx is null ? new[] { ex } : new[] { originEx, ex });
            }

            IEnumerable InitializeInstance(IEnumerable instance, IObjectDescription elements, out IObjectDescription? ignored)
            {
                var valueType = instance.GetType().GetIEnumerableValueType()!;


                var add = type.IsICollection() ? type.GetICollectionType().GetInstanceMemberInvokeDelegate<Action<object, object?>>(nameof(ICollection<object>.Add), 1) : null;
                var remove = type.IsICollection() ? type.GetICollectionType().GetInstanceMemberInvokeDelegate<Func<object, object?, bool>>(nameof(ICollection<object>.Remove), 1) : null;
                var insert = type.IsIList() ? type.GetIListType().GetInstanceMemberInvokeDelegate<Action<object, int, object?>>(nameof(IList.Insert), 2) : null;

                ignored = elements;
                try
                {
                    var enumerator = elements.Children.GetEnumerator();
                    var instanceEnumerator = instance.GetEnumerator();
                    while (enumerator.MoveNext() && instanceEnumerator.MoveNext())
                        try
                        {
                            var value = initializeValue(valueType, instanceEnumerator.Current, enumerator.Current.Value, out var ig);

                            ignored = ignored.Remove(enumerator.Current);
                            if (ig is not null)
                                ignored = ignored.Append(enumerator.Current.Key, ig);

                            if (!ReferenceEquals(value, instanceEnumerator.Current))
                                if (insert is not null)
                                    insert(instance, int.Parse(enumerator.Current.Key!), value);
                                else if (remove is not null && add is not null)
                                {
                                    remove(instance, instanceEnumerator.Current);
                                    add(instance, value);
                                }
                                else
                                    ignored = ignored.Append(enumerator.Current); // can't added
                        }
                        catch (Exception ex)
                        {
                            throw InstantiationException.GetCanNotInstantiateException(type, elements, $"[{enumerator.Current.Key}]", ex);
                        }


                    while (enumerator.MoveNext())
                        if (add is not null)
                            try
                            {
                                add(instance, initializeValue(valueType, null, enumerator.Current.Value, out var ig));

                                ignored = ignored.Remove(enumerator.Current);
                                if (ig is not null)
                                    ignored = ignored.Append(enumerator.Current.Key, ig);
                            }
                            catch (Exception ex)
                            {
                                throw InstantiationException.GetCanNotInstantiateException(type, elements, $"[{enumerator.Current.Key}]", ex);
                            }
                }
                catch (Exception ex)
                {
                    throw InstantiationException.GetCanNotInstantiateException(type, elements, ex);
                }

                ignored = ignored.IsNullOrEmpty() ? null : ignored.Constant();
                return instance;
            }
        }


        private static bool SeperateMembersAndElements(IObjectDescription description, out IObjectDescription members, out IObjectDescription elements)
        {
            if (description.HasValue)
            {
                members = description;
                elements = ObjectDescriptions.EmptyDescription;
                return false;
            }

            var ms = new List<KeyValuePair<string?, IObjectDescription>>();
            var es = new List<KeyValuePair<int, IObjectDescription>>();

            foreach (var pair in description.Children)
                if (int.TryParse(pair.Key, out var i))
                    es.Add(new KeyValuePair<int, IObjectDescription>(i, pair.Value));
                else
                    ms.Add(pair);

            members = new ConstantObjectDescription(ms);
            var filledElements = new List<KeyValuePair<string?, IObjectDescription>>();
            foreach (var e in es.OrderBy(e => e.Key))
            {
                for (var i = filledElements.Count; i < e.Key; i++)
                    filledElements.Add(new KeyValuePair<string?, IObjectDescription>(i.ToString(), ObjectDescriptions.NullDescription));
                filledElements.Add(new KeyValuePair<string?, IObjectDescription>(e.Key.ToString(), e.Value));
            }
            elements = new ConstantObjectDescription(filledElements);

            return true;
        }

        private static IObjectDescription UnifyEnumerable(IObjectDescription description)
        {
            return ObjectDescriptions.Constant("0", description.IsWrappedValue() ? description.UnwrapValue() : description);
        }


        internal delegate object? InstantiateDelegate(Type type, IObjectDescription description, out IObjectDescription? ignored);

        internal delegate object? InitializeDelegate(Type type, object? instance, IObjectDescription description, out IObjectDescription? ignored);


    }
}
