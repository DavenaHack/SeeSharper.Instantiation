using System;

namespace Mimp.SeeSharper.Instantiation.Abstraction
{
    /// <summary>
    /// <see cref="IInstantiator"/> instantiate a object.
    /// </summary>
    public interface IInstantiator
    {


        /// <summary>
        /// Check if <see cref="IInstantiator"/> can instantiate a object of <paramref name="type"/> with the <paramref name="instantiateValues"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="instantiateValues"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool Instantiable(Type type, object? instantiateValues);


        /// <summary>
        /// Instantiate a object of <paramref name="type"/> with <paramref name="instantiateValues"/>.
        /// If <see cref="IInstantiator"/> can't use all <paramref name="instantiateValues"/> it will store it to <paramref name="ignoredInstantiateValues"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="instantiateValues"></param>
        /// <param name="ignoredInstantiateValues"></param>
        /// <returns></returns>
        /// <exception cref="InstantiationException"></exception>
        public object? Instantiate(Type type, object? instantiateValues, out object? ignoredInstantiateValues);


        /// <summary>
        /// Initialize <paramref name="instance"/> with <paramref name="initializeValues"/>.
        /// If <see cref="IInstantiator"/> can't use all <paramref name="initializeValues"/> it will store it to <paramref name="ignoredInitializeValues"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="initializeValues"></param>
        /// <param name="ignoredInitializeValues"></param>
        /// <returns></returns>
        /// <exception cref="InstantiationException"></exception>
        public void Initialize(object? instance, object? initializeValues, out object? ignoredInitializeValues);


    }
}
