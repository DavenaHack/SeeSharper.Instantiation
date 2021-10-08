using Mimp.SeeSharper.ObjectDescription.Abstraction;
using System;

namespace Mimp.SeeSharper.Instantiation.Abstraction
{
    /// <summary>
    /// <see cref="IInstantiator"/> instantiate a object.
    /// </summary>
    public interface IInstantiator
    {


        /// <summary>
        /// Check if <see cref="IInstantiator"/> can instantiate a object of <paramref name="type"/> with the <paramref name="description"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool Instantiable(Type type, IObjectDescription description);


        /// <summary>
        /// Instantiate a object of <paramref name="type"/> with <paramref name="description"/>.
        /// If <see cref="IInstantiator"/> can't use all <paramref name="description"/> it will store it to <paramref name="ignored"/>.
        /// If <paramref name="ignored"/> is null it means all of <paramref name="description"/> was used.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="description"></param>
        /// <param name="ignored"></param>
        /// <returns></returns>
        /// <exception cref="InstantiationException"></exception>
        public object? Instantiate(Type type, IObjectDescription description, out IObjectDescription? ignored);


        /// <summary>
        /// Initialize <paramref name="instance"/> with <paramref name="description"/>.
        /// If <see cref="IInstantiator"/> can't use all <paramref name="description"/> it will store it to <paramref name="ignored"/>.
        /// If <paramref name="ignored"/> is null it means all of <paramref name="description"/> was used.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="description"></param>
        /// <param name="ignored"></param>
        /// <returns>if <paramref name="instance"/> wis null it return a new instance otherwise <paramref name="instance"/></returns>
        /// <exception cref="InstantiationException"></exception>
        public object? Initialize(Type type, object? instance, IObjectDescription description, out IObjectDescription? ignored);


    }
}
