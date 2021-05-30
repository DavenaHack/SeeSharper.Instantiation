using System;

namespace Mimp.SeeSharper.Instantiation.Abstraction
{
    public interface IInstantiator
    {


        public bool Instantiable(Type type, object? instantiateValues);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="instantiateValues"></param>
        /// <param name="ignoredInstantiateValues"></param>
        /// <returns></returns>
        /// <exception cref="InitializeException"></exception>
        public object? Instantiate(Type type, object? instantiateValues, out object? ignoredInstantiateValues);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="initializeValues"></param>
        /// <param name="ignoredInitializeValues"></param>
        /// <returns></returns>
        /// <exception cref="InitializeException"></exception>
        public void Initialize(object? instance, object? initializeValues, out object? ignoredInitializeValues);


    }
}
