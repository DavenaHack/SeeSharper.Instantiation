using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.ObjectDescription;
using Mimp.SeeSharper.ObjectDescription.Abstraction;
using Mimp.SeeSharper.Reflection;
using System;
using System.Collections.Generic;

namespace Mimp.SeeSharper.Instantiation
{
    /// <summary>
    /// A <see cref="IInstantiator"/> to instantiate a object with recieving type in instantiate values.
    /// </summary>
    public class TypedInstantiator : IInstantiator
    {


        public IInstantiator InstanceInstantiator { get; }

        public IInstantiator TypeInstantiator { get; }

        public string TypeKey { get; }


        public TypedInstantiator(IInstantiator instanceInstantiator, IInstantiator typeInstantiator, string typeKey)
        {
            InstanceInstantiator = instanceInstantiator ?? throw new ArgumentNullException(nameof(instanceInstantiator));
            TypeInstantiator = typeInstantiator ?? throw new ArgumentNullException(nameof(typeInstantiator));
            if (!TypeInstantiator.Instantiable(typeof(Type), ObjectDescriptions.NullDescription))
                throw new ArgumentException($@"{TypeInstantiator} can't instantiate a instance of type ""{typeof(Type)}""");
            TypeKey = typeKey ?? throw new ArgumentNullException(nameof(typeKey));
        }


        public bool Instantiable(Type type, IObjectDescription description)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (description is null)
                throw new ArgumentNullException(nameof(description));

            if (!description.HasValue)
                foreach (var pair in description.Children)
                    if (string.Equals(pair.Key, TypeKey, StringComparison.InvariantCultureIgnoreCase))
                        try
                        {
                            type = TypeInstantiator.Construct<Type>(pair.Value, out var typeValues) ?? type;
                            description = description.Remove(pair);
                            break;
                        }
                        catch { }
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

            ignored = description;
            try
            {
                if (!ignored.HasValue)
                {
                    foreach (var child in ignored.Children)
                        if (string.Equals(child.Key, TypeKey, StringComparison.InvariantCultureIgnoreCase))
                            try
                            {
                                var t = TypeInstantiator.Construct<Type>(child.Value, out var ignore);
                                if (t is not null)
                                {
                                    if (!t.InheritOrAssignable(type))
                                        throw new InstantiationException(type, child.Value, null, $@"""{t}"" have to be assignable to type ""{type}""");
                                    type = t;
                                }
                                ignored = ignored.Remove(child);
                                if (ignore is not null)
                                    ignored = ignored.Append(child.Key, ignore);
                            }
                            catch (Exception ex)
                            {
                                throw InstantiationException.GetCanNotInstantiateException(type, child.Value, TypeKey, ex);
                            }
                }

                return InstanceInstantiator.Instantiate(type, ignored.Constant(), out ignored);
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

            return InstanceInstantiator.Initialize(type, instance, description, out ignored);
        }


    }
}
