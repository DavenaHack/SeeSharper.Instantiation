using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.TypeResolver.Abstraction;
using System;

namespace Mimp.SeeSharper.Instantiation.TypeResolver
{
    public static class TypeInstantiatorBuilderExtensions
    {


        public static IInstantiatorBuilder SetTypedRoot(this IInstantiatorBuilder builder, ITypeResolver resolver, string typeKey)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            return builder.SetRoot(root => new TypedInstantiator(root, new ResolveTypeInstantiator(resolver), typeKey));
        }


        public static IInstantiatorBuilder AddTypeResolver(this IInstantiatorBuilder builder, ITypeResolver resolver)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            return builder.Add(root => new[] { new ResolveTypeInstantiator(resolver) });
        }


    }
}
