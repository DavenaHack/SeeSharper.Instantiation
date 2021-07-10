using Mimp.SeeSharper.Instantiation.Abstraction;
using System;

namespace Mimp.SeeSharper.Instantiation.Test.Mock
{
    public class ThrowInstantiator : IInstantiator
    {

        public void Initialize(object? instance, object? initializeValues, out object? ignoredInitializeValues)
        {
            ignoredInitializeValues = initializeValues;
        }

        public bool Instantiable(System.Type type, object? instantiateValues)
        {
            return true;
        }

        public object? Instantiate(System.Type type, object? instantiateValues, out object? ignoredInstantiateValues)
        {
            throw new NotImplementedException();
        }
    }
}
