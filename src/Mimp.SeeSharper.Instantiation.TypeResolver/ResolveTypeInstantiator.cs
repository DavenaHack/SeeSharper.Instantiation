using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.ObjectDescription.Abstraction;
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


        protected override object? InstantiateFromString(Type type, string value, IObjectDescription description, out IObjectDescription? ignored)
        {
            try
            {
                ignored = null;
                return Resolver.ResolveSingle(value);
            }
            catch (Exception ex)
            {
                throw InstantiationException.GetCanNotInstantiateException(type, description, ex);
            }
        }


    }
}
