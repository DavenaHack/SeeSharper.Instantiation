using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.ObjectDescription;
using Mimp.SeeSharper.ObjectDescription.Abstraction;
using Mimp.SeeSharper.Reflection;
using System;

namespace Mimp.SeeSharper.Instantiation
{
    /// <summary>
    /// A <see cref="IInstantiator"/> to instantiate <see cref="Type"/>.
    /// </summary>
    public class TypeInstantiator : IInstantiator
    {


        public bool Instantiable(Type type, IObjectDescription description)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (description is null)
                throw new ArgumentNullException(nameof(description));

            return type.IsAssignableFrom(typeof(Type));
        }


        public object? Instantiate(Type type, IObjectDescription description, out IObjectDescription? ignored)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (description is null)
                throw new ArgumentNullException(nameof(description));
            if (!Instantiable(type, description))
                throw InstantiationException.GetNotMatchingTypeException(this, type, description);

            var constDesc = description.Constant();
            if (constDesc.HasValue)
            {
                if (constDesc.Value is null)
                {
                    ignored = null;
                    return type.Default();
                }

                if (constDesc.Value is Type v)
                {
                    ignored = null;
                    return v;
                }

                if (constDesc.Value is string s)
                    return InstantiateFromString(type, s, description, out ignored);

            }
            else if (constDesc.IsEmpty())
                try
                {
                    return Instantiate(type, ObjectDescriptions.NullDescription, out ignored);
                }
                catch (Exception ex)
                {
                    throw InstantiationException.GetCanNotInstantiateException(type, description, ex);
                }
            else if (constDesc.IsWrappedValue())
                try
                {
                    return Instantiate(type, constDesc.UnwrapValue(), out ignored);
                }
                catch (Exception ex)
                {
                    throw InstantiationException.GetCanNotInstantiateException(type, description, ex);
                }

            throw InstantiationException.GetCanNotInstantiateException(type, description);
        }

        protected virtual object? InstantiateFromString(Type type, string value, IObjectDescription description, out IObjectDescription? ignored)
        {
            try
            {
                ignored = null;
                return Type.GetType(value, true, true);
            }
            catch (Exception ex)
            {
                throw InstantiationException.GetCanNotInstantiateException(type, description, ex);
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

            ignored = description.IsNullOrEmpty() ? null : description;
            return instance;
        }


    }
}
