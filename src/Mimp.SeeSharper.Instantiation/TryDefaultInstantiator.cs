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
    /// <see cref="TryDefaultInstantiator"/> try all <see cref="IInstantiator"/>s and if no one can instantiate the object
    /// it will return the default of the object.
    /// </summary>
    public class TryDefaultInstantiator : IInstantiator
    {


        private readonly ICollection<KeyValuePair<object, IInstantiator>> _initializes;


        public ICollection<IInstantiator> Instantiators { get; }


        public TryDefaultInstantiator(IEnumerable<IInstantiator> instantiators)
        {
            Instantiators = instantiators?.ToArray() ?? throw new ArgumentNullException(nameof(instantiators));
            if (Instantiators.Any(i => i is null))
                throw new ArgumentNullException(nameof(instantiators), "At least one instantiator is null");
            _initializes = new List<KeyValuePair<object, IInstantiator>>();
        }

        public TryDefaultInstantiator(Func<IInstantiator, IEnumerable<IInstantiator>> getInstantiators)
        {
            Instantiators = (getInstantiators ?? throw new ArgumentNullException(nameof(getInstantiators)))(this)?.ToArray()
                ?? throw new ArgumentNullException(nameof(getInstantiators), "Return null");
            if (Instantiators.Any(i => i is null))
                throw new ArgumentNullException(nameof(getInstantiators), "At least one instantiator is null");
            _initializes = new List<KeyValuePair<object, IInstantiator>>();
        }

        public TryDefaultInstantiator()
            : this(Array.Empty<IInstantiator>()) { }


        public bool Instantiable(Type type, IObjectDescription description)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (description is null)
                throw new ArgumentNullException(nameof(description));

            foreach (var i in Instantiators)
                if (i.Instantiable(type, description))
                    return true;
            return false;
        }


        public object? Instantiate(Type type, IObjectDescription description, out IObjectDescription? ignored)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (description is null)
                throw new ArgumentNullException(nameof(description));

            foreach (var instantiator in Instantiators)
                if (instantiator.Instantiable(type, description))
                    try
                    {
                        var instance = instantiator.Instantiate(type, description, out ignored);
                        if (instance is not null)
                            _initializes.Add(new KeyValuePair<object, IInstantiator>(instance, instantiator));
                        return instance;
                    }
                    catch { }

            ignored = description.IsNullOrEmpty() ? null : description;
            return type.Default();
        }


        public object? Initialize(Type type, object? instance, IObjectDescription description, out IObjectDescription? ignored)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (description is null)
                throw new ArgumentNullException(nameof(description));

            if (instance is null)
                return Instantiate(type, description, out ignored);

            IInstantiator? instantiator = null;
            lock (_initializes)
            {
                foreach (var pair in _initializes)
                    if (ReferenceEquals(pair.Key, instance))
                    {
                        instantiator = pair.Value;
                        _initializes.Remove(pair);
                        break;
                    }
            }
            if (instantiator is not null)
                return instantiator.Initialize(instance, description, out ignored);

            foreach (var inst in Instantiators)
                if (inst.Instantiable(type, description))
                    try
                    {
                        return inst.Instantiate(type, description, out ignored);
                    }
                    catch { }

            ignored = description.IsNullOrEmpty() ? null : description;
            return instance;
        }


    }
}
