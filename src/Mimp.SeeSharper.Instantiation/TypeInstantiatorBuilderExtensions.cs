using Mimp.SeeSharper.Instantiation.Abstraction;
using System;

namespace Mimp.SeeSharper.Instantiation
{
    public static class TypeInstantiatorBuilderExtensions
    {


        public static IInstantiatorBuilder SetTypedRoot(this IInstantiatorBuilder builder, IInstantiator typeInstantiator, string typeKey)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            return builder.SetRoot(root => new TypedInstantiator(root, typeInstantiator, typeKey));
        }


    }
}
