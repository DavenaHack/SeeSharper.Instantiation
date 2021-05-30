using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.TypeResolver.Abstraction;
using System;

namespace Mimp.SeeSharper.Instantiation.Type
{
    public static class TypeInstantiatorBuilderExtensions
    {


        public static IInstantiatorBuilder SetTypedRoot(this IInstantiatorBuilder builder, ITypeResolver resolver, string typeKey)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            return builder.SetRoot(root => new TypedInstantiator(root, new TypeInstantiator(resolver), typeKey));
        }


        public static IInstantiatorBuilder AddType(this IInstantiatorBuilder builder, ITypeResolver resolver)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            return builder.Add(root => new[] { new TypeInstantiator(resolver) });
        }


    }
}
