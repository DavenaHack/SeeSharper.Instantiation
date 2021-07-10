using System;
using System.Collections.Generic;

namespace Mimp.SeeSharper.Instantiation.Abstraction
{
    public interface IInstantiatorBuilder
    {


        /// <summary>
        /// <paramref name="setRoot"/> will pass the current root <see cref="IInstantiator"/>
        /// and set the return to the next root.
        /// </summary>
        /// <param name="setRoot"></param>
        /// <returns></returns>
        public IInstantiatorBuilder SetRoot(Func<IInstantiator, IInstantiator> setRoot);


        /// <summary>
        /// <paramref name="getChildren"/> will pass the root and add the <see cref="IInstantiator"/>s
        /// to the children.
        /// </summary>
        /// <param name="getChildren"></param>
        /// <returns></returns>
        public IInstantiatorBuilder Add(Func<IInstantiator, IEnumerable<IInstantiator>> getChildren);


        /// <summary>
        /// Build a <see cref="IInstantiator"/> with configured root and children
        /// </summary>
        /// <returns></returns>
        /// <seealso cref="SetRoot(Func{IInstantiator, IInstantiator})"/>
        /// <seealso cref="Add(Func{IInstantiator, IEnumerable{IInstantiator}})"/>
        public IInstantiator Build();


    }
}
