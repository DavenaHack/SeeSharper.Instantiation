using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.Reflection;
using System;

namespace Mimp.SeeSharper.Instantiation
{
    public class DefaultInstantiator : IInstantiator
    {


        public bool Instantiable(Type type, object? instantiateValues)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            return instantiateValues is null
                || type.Default() == instantiateValues;
        }


        public object? Instantiate(Type type, object? instantiateValues, out object? ignoredInstantiateValues)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (!Instantiable(type, instantiateValues))
                throw InstantiationException.GetNotMatchingTypeException(this, type);

            ignoredInstantiateValues = null;
            return type.Default();
        }


        public void Initialize(object? instance, object? initializeValues, out object? ignoredInitializeValues)
        {
            ignoredInitializeValues = initializeValues;
        }


    }
}
