using Mimp.SeeSharper.Instantiation.Abstraction;
using System;
using System.Collections.Generic;

namespace Mimp.SeeSharper.Instantiation
{
    public static class InstantiatorBuilderExtensions
    {


        public static IInstantiatorBuilder AddPrimitives(this IInstantiatorBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            return builder.Add(AddPrimitives());
        }

        public static Func<IInstantiator, IEnumerable<IInstantiator>> AddPrimitives()
        {
            return _ => new IInstantiator[]
            {
                new BigIntegerInstantiator(),
                new BooleanInstantiator(),
                new ByteInstantiator(),
                new CharInstantiator(),
                new DateTimeInstantiator(),
                new DecimalInstantiator(),
                new DoubleInstantiator(),
                new FloatInstantiator(),
                new IntInstantiator(),
                new LongInstantiator(),
                new SByteInstantiator(),
                new ShortInstantiator(),
                new StringInstantiator(),
                new TimeSpanInstantiator(),
                new UIntInstantiator(),
                new ULongInstantiator(),
                new UShortInstantiator(),
                new EnumInstantiator(),
            };
        }


        public static IInstantiatorBuilder AddEnumerables(
            this IInstantiatorBuilder builder,
            Func<IInstantiator, IInstantiator> instanceInstantiator,
            Func<IInstantiator, IInstantiator> internalInstantiator
        )
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));
            if (instanceInstantiator is null)
                throw new ArgumentNullException(nameof(instanceInstantiator));
            if (internalInstantiator is null)
                throw new ArgumentNullException(nameof(internalInstantiator));

            return builder.Add(AddEnumerables(instanceInstantiator, internalInstantiator));
        }

        public static Func<IInstantiator, IEnumerable<IInstantiator>> AddEnumerables(
            Func<IInstantiator, IInstantiator> instanceInstantiator,
            Func<IInstantiator, IInstantiator> internalInstantiator
        )
        {
            if (instanceInstantiator is null)
                throw new ArgumentNullException(nameof(instanceInstantiator));
            if (internalInstantiator is null)
                throw new ArgumentNullException(nameof(internalInstantiator));

            return root =>
            {
                var instance = instanceInstantiator(root);
                var intern = internalInstantiator(root);

                return new IInstantiator[] {
                    new KeyValuePairInstantiator(intern),
                    new DictionaryInstantiator(instance, intern),
                    new ListInstantiator(instance, intern),
                    new SetInstantiator(instance, intern),
                    new CollectionInstantiator(instance, intern),
                    new EnumerableInstantiator(instance, intern)
                };
            };
        }


        public static IInstantiatorBuilder AddUtilities(
            this IInstantiatorBuilder builder,
            Func<IInstantiator, IInstantiator> instanceInstantiator,
            Func<IInstantiator, IInstantiator> internalInstantiator
        )
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));
            if (instanceInstantiator is null)
                throw new ArgumentNullException(nameof(instanceInstantiator));
            if (internalInstantiator is null)
                throw new ArgumentNullException(nameof(internalInstantiator));

            return builder.Add(AddUtilities(instanceInstantiator, internalInstantiator));
        }

        public static Func<IInstantiator, IEnumerable<IInstantiator>> AddUtilities(
            Func<IInstantiator, IInstantiator> instanceInstantiator,
            Func<IInstantiator, IInstantiator> internalInstantiator
        )
        {
            if (instanceInstantiator is null)
                throw new ArgumentNullException(nameof(instanceInstantiator));
            if (internalInstantiator is null)
                throw new ArgumentNullException(nameof(internalInstantiator));

            return root =>
            {
                var instantiators = new HashSet<IInstantiator>();

                var instance = instanceInstantiator(root);
                var intern = internalInstantiator(root);

                foreach (var i in AddPrimitives()(root))
                    instantiators.Add(i);
                instantiators.Add(new NullableInstantiator(intern));

                foreach (var i in AddEnumerables(_ => instance, _ => intern)(root))
                    instantiators.Add(i);

                instantiators.Add(new MemberInstantiator(instance, intern));

                instantiators.Add(new DefaultInstantiator());

                return instantiators;
            };
        }


    }
}
