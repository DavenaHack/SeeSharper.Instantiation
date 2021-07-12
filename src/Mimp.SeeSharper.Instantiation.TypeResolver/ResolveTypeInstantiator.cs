using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.TypeResolver.Abstraction;
using System;

namespace Mimp.SeeSharper.Instantiation.TypeResolver
{
    public class ResolveTypeInstantiator : TypeInstantiator
    {


        public ITypeResolver Resolver { get; }


        public ResolveTypeInstantiator(ITypeResolver resolver)
            : base()
        {
            Resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        }


        protected override object? InstantiateFromString(Type type, string value, object instantiateValues, out object? ignoredInstantiateValues)
        {
            try
            {
                var result = Resolver.ResolveSingle(value);
                ignoredInstantiateValues = null;
                return result;
            }
            catch (Exception ex)
            {
                throw InstantiationException.GetCanNotInstantiateException(type, instantiateValues, ex);
            }
        }


    }
}
