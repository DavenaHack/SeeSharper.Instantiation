using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.ObjectDescription;
using Mimp.SeeSharper.ObjectDescription.Abstraction;
using System;

namespace Mimp.SeeSharper.Instantiation
{
    public static class InstantiatorExtensions
    {


        public static object? Construct(this IInstantiator instantiator, Type type, IObjectDescription description, out IObjectDescription? ignored)
        {
            if (instantiator is null)
                throw new ArgumentNullException(nameof(instantiator));
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            var instance = instantiator.Instantiate(type, description, out ignored);
            return instantiator.Initialize(type, instance, ignored ?? ObjectDescriptions.NullDescription, out ignored);
        }

        public static object? Construct(this IInstantiator instantiator, Type type, IObjectDescription description)
        {
            var result = instantiator.Construct(type, description, out var ignored);
            if (ignored is not null)
                throw InstantiationException.GetUsedNotAllException(type, description, ignored);
            return result;
        }


        public static T? Construct<T>(this IInstantiator instantiator, IObjectDescription description) =>
            (T?)instantiator.Construct(typeof(T), description);

        public static T? Construct<T>(this IInstantiator instantiator, IObjectDescription description, out IObjectDescription? ignored) =>
            (T?)instantiator.Construct(typeof(T), description, out ignored);


    }
}
