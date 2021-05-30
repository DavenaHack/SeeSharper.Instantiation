using System;
using System.Linq;

namespace Mimp.SeeSharper.Instantiation.Abstraction
{
    public static class InstantiatorBuilderExtensions
    {


        public static IInstantiatorBuilder Add(this IInstantiatorBuilder builder, params IInstantiator[] instantiators)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));
            if (instantiators is null)
                throw new ArgumentNullException(nameof(instantiators));
            if (instantiators.Any(i => i is null))
                throw new ArgumentNullException(nameof(instantiators), "At least one instantiator is null.");

            return builder.Add(_ => instantiators);
        }

        public static IInstantiatorBuilder Add(this IInstantiatorBuilder builder, Func<IInstantiator, IInstantiator> getChild)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));
            if (getChild is null)
                throw new ArgumentNullException(nameof(getChild));

            return builder.Add(root => new[] { getChild(root) });
        }


    }
}
