using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.ObjectDescription;
using Mimp.SeeSharper.ObjectDescription.Abstraction;
using Mimp.SeeSharper.Reflection;
using System;

namespace Mimp.SeeSharper.Instantiation
{
    /// <summary>
    /// A <see cref="IInstantiator"/> to instantiate <see cref="Nullable{T}"/>.
    /// </summary>
    public class NullableInstantiator : IInstantiator
    {


        public IInstantiator Instantiator { get; }


        public NullableInstantiator(IInstantiator instantiator)
        {
            Instantiator = instantiator ?? throw new ArgumentNullException(nameof(instantiator));
        }


        public bool Instantiable(Type type, IObjectDescription description)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (description is null)
                throw new ArgumentNullException(nameof(description));

            return type.IsNullable() && Instantiator.Instantiable(type.GetNullableValueType()!, description);
        }


        public object? Instantiate(Type type, IObjectDescription description, out IObjectDescription? ignored)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (description is null)
                throw new ArgumentNullException(nameof(description));
            if (!Instantiable(type, description))
                throw InstantiationException.GetNotMatchingTypeException(this, type, description);

            try
            {
                return Instantiator.Instantiate(type.GetNullableValueType()!, description, out ignored);
            }
            catch
            {
                description = description.IsWrappedValue() ? description.UnwrapValue() : description;
                if (description.IsNullOrEmpty() ||
                    description.HasValue && description.Value is string s && string.IsNullOrWhiteSpace(s))
                {
                    ignored = null;
                    return type.Default();
                }

                throw;
            }
        }


        public object? Initialize(Type type, object? instance, IObjectDescription description, out IObjectDescription? ignored)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (description is null)
                throw new ArgumentNullException(nameof(description));

            if (instance is null)
                return Instantiate(type, description, out ignored);

            return Instantiator.Initialize(type, instance, description, out ignored);
        }


    }
}
