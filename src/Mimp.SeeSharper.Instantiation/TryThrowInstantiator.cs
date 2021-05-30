using Mimp.SeeSharper.Instantiation.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mimp.SeeSharper.Instantiation
{
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


        public bool Instantiable(Type type, object? instantiateValues)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            foreach (var i in Instantiators)
                if (i.Instantiable(type, instantiateValues))
                    return true;
            return false;
        }


        public object? Instantiate(Type type, object? instantiateValues, out object? ignoredInstantiateValues)
        {
            var exceptions = new List<Exception>();
            foreach (var instantiator in Instantiators)
                if (instantiator.Instantiable(type, instantiateValues))
                    try
                    {
                        var instance = instantiator.Instantiate(type, instantiateValues, out var ignoreValues);
                        ignoredInstantiateValues = ignoreValues;
                        if (instance is not null)
                            _initializes.Add(new KeyValuePair<object, IInstantiator>(instance, instantiator));
                        return instance;
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
            throw InstantiationException.GetCanNotInstantiateExeption(type, instantiateValues, exceptions);
        }

        public void Initialize(object? instance, object? initializeValues, out object? ignoredInitializeValues)
        {
            if (instance is null)
            {
                ignoredInitializeValues = initializeValues;
                return;
            }

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

            if (instantiator is null)
                throw new InvalidOperationException($@"""{instance}"" isn't instantiate from ""{this}""");

            instantiator.Initialize(instance, initializeValues, out ignoredInitializeValues);
        }


    }
}
