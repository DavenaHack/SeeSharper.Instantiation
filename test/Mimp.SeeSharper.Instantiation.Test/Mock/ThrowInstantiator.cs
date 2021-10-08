using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.ObjectDescription.Abstraction;
using System;

namespace Mimp.SeeSharper.Instantiation.Test.Mock
{
    public class ThrowInstantiator : IInstantiator
    {

        public object? Initialize(Type type, object? instance, IObjectDescription description, out IObjectDescription? ignored)
        {
            ignored = description;
            return instance;
        }

        public bool Instantiable(Type type, IObjectDescription description)
        {
            return true;
        }

        public object? Instantiate(Type type, IObjectDescription description, out IObjectDescription? ignored)
        {
            throw new NotImplementedException();
        }
    }
}
