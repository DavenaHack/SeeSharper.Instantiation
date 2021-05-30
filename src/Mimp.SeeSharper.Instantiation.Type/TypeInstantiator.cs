using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.Reflection;
using Mimp.SeeSharper.TypeResolver.Abstraction;
using System;

namespace Mimp.SeeSharper.Instantiation.Type
{
    public class TypeInstantiator : IInstantiator
    {


        public ITypeResolver Resolver { get; }


        public TypeInstantiator(ITypeResolver resolver)
        {
            Resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        }


        public bool Instantiable(System.Type type, object? instantiateValues)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            return type.IsAssignableFrom(typeof(System.Type));
        }

        public object? Instantiate(System.Type type, object? instantiateValues, out object? ignoredInstantiateValues)
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
            if (instantiateValues is System.Type t)
            {
                ignoredInstantiateValues = null;
                return t;
            }
            if (instantiateValues is string s)
                try
                {
                    ignoredInstantiateValues = null;
                    return Resolver.ResolveRequired(s);
                }
                catch (Exception ex)
                {
                    throw InstantiationException.GetCanNotInstantiateExeption(type, instantiateValues, ex);
                }
            throw InstantiationException.GetCanNotInstantiateExeption(type, instantiateValues);
        }

        public void Initialize(object? instance, object? initializeValues, out object? ignoredInitializeValues)
        {
            ignoredInitializeValues = initializeValues;
        }


    }
}
