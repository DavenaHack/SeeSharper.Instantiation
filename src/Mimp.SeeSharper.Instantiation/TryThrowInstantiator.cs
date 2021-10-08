using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.ObjectDescription.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mimp.SeeSharper.Instantiation
{
    /// <summary>
    /// <see cref="TryThrowInstantiator"/> try all <see cref="IInstantiator"/>s and if no one can instantiate the object
    /// it will throw a exception.
    /// </summary>
    public class TryThrowInstantiator : IInstantiator
    {


        private readonly ICollection<KeyValuePair<object, IInstantiator>> _initializes;


        public ICollection<IInstantiator> Instantiators { get; }


        public TryThrowInstantiator(IEnumerable<IInstantiator> instantiators)
        {
            Instantiators = instantiators?.ToArray() ?? throw new ArgumentNullException(nameof(instantiators));
            if (Instantiators.Any(i => i is null))
                throw new ArgumentNullException(nameof(instantiators), "At least one instantiator is null");
            _initializes = new List<KeyValuePair<object, IInstantiator>>();
        }

        public TryThrowInstantiator(Func<IInstantiator, IEnumerable<IInstantiator>> getInstantiators)
        {
            Instantiators = (getInstantiators ?? throw new ArgumentNullException(nameof(getInstantiators)))(this)?.ToArray()
                ?? throw new ArgumentNullException(nameof(getInstantiators), "Return null");
            if (Instantiators.Any(i => i is null))
                throw new ArgumentNullException(nameof(getInstantiators), "At least one instantiator is null");
            _initializes = new List<KeyValuePair<object, IInstantiator>>();
        }

        public TryThrowInstantiator()
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

            var exceptions = new List<Exception>();
            foreach (var instantiator in Instantiators)
                if (instantiator.Instantiable(type, description))
                    try
                    {
                        var instance = instantiator.Instantiate(type, description, out ignored);
                        if (instance is not null)
                            _initializes.Add(new KeyValuePair<object, IInstantiator>(instance, instantiator));
                        return instance;
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }

            throw InstantiationException.GetCanNotInstantiateException(type, description, exceptions);
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

            var exceptions = new List<Exception>();
            foreach (var inst in Instantiators)
                if (inst.Instantiable(type, description))
                    try
                    {
                        return inst.Instantiate(type, description, out ignored);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }

            throw InstantiationException.GetCanNotInstantiateException(type, description, exceptions);
        }


    }
}
