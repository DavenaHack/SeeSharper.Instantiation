using Mimp.SeeSharper.Instantiation.Abstraction;
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
            if (!TypeInstantiator.Instantiable(typeof(Type), null))
                throw new ArgumentException($@"{TypeInstantiator} can't instantiate a instance of type ""{typeof(Type)}""");
            TypeKey = typeKey ?? throw new ArgumentNullException(nameof(typeKey));
        }


        public bool Instantiable(Type type, object? instantiateValues)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            if (instantiateValues is IEnumerable<KeyValuePair<string?, object?>> keyValues)
                foreach (var pair in keyValues)
                    if (string.Equals(pair.Key, TypeKey, StringComparison.InvariantCultureIgnoreCase))
                        try
                        {
                            var t = (Type?)TypeInstantiator.Instantiate(typeof(Type), pair.Value, out var typeValues);
                            if (t is not null)
                            {
                                TypeInstantiator.Initialize(t, typeValues, out typeValues);
                                type = t;
                            }
                            break;
                        }
                        catch { }
            return InstanceInstantiator.Instantiable(type, instantiateValues);
        }

        public object? Instantiate(Type type, object? instantiateValues, out object? ignoredInstantiateValues)
        {
			if (type is null)
				throw new ArgumentNullException(nameof(type));
			if (!Instantiable(type, instantiateValues))
                throw InstantiationException.GetNotMatchingTypeException(this, type);

            if (instantiateValues is IEnumerable<KeyValuePair<string?, object?>> keyValues)
            {
                var ignoreValues = new List<KeyValuePair<string?, object?>>();
                foreach (var pair in keyValues)
                    if (string.Equals(pair.Key, TypeKey, StringComparison.InvariantCultureIgnoreCase))
                    {
                        try
                        {
                            var t = (Type?)TypeInstantiator.Instantiate(typeof(Type), pair.Value, out var typeValues);
                            if (t is not null)
                            {
                                TypeInstantiator.Initialize(t, typeValues, out typeValues);
                                if (!t.InheritOrAssignable(type))
                                    throw new InstantiationException(type, instantiateValues, null, $@"""{t}"" from {nameof(TypeKey)} have to be assignable to type ""{type}""");
                                type = t;
                            }
                        }
                        catch (Exception ex)
                        {
                            throw InstantiationException.GetCanNotInstantiateException(type, instantiateValues, TypeKey, ex);
                        }
                    }
                    else
                        ignoreValues.Add(pair);
                instantiateValues = ignoreValues;
            }

            return InstanceInstantiator.Instantiate(type, instantiateValues, out ignoredInstantiateValues);
        }

        public void Initialize(object? instance, object? initializeValues, out object? ignoredInitializeValues)
        {
            InstanceInstantiator.Initialize(instance, initializeValues, out ignoredInitializeValues);
        }


    }
}
