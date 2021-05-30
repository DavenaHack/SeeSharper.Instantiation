using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.Reflection;
using System;

namespace Mimp.SeeSharper.Instantiation
{
    public class NullableInstantiator : IInstantiator
    {


        public IInstantiator Instantiator { get; }


        public NullableInstantiator(IInstantiator instantiator)
        {
            Instantiator = instantiator ?? throw new ArgumentNullException(nameof(instantiator));
        }


        public bool Instantiable(Type type, object? instantiateValues)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            return type.IsNullable() && Instantiator.Instantiable(type.GetNullableValueType()!, instantiateValues);
        }

        public object? Instantiate(Type type, object? instantiateValues, out object? ignoredInstantiateValues)
        {
			if (type is null)
				throw new ArgumentNullException(nameof(type));
			if (!Instantiable(type, instantiateValues))
                throw InstantiationException.GetNotMatchingTypeException(this, type);

            if (instantiateValues is null)
            {
                ignoredInstantiateValues = null;
                return type.Default();
            }
            try
            {
                return Instantiator.Instantiate(type.GetNullableValueType()!, instantiateValues, out ignoredInstantiateValues);
            }
            catch (Exception ex)
            {
                if (instantiateValues is string s && string.IsNullOrWhiteSpace(s))
                {
                    ignoredInstantiateValues = null;
                    return type.Default();
                }
                throw InstantiationException.GetCanNotInstantiateExeption(type, instantiateValues, ex);
            }
        }

        public void Initialize(object? instance, object? initializeValues, out object? ignoredInitializeValues)
        {
            Instantiator.Initialize(instance, initializeValues, out ignoredInitializeValues);
        }


    }
}
