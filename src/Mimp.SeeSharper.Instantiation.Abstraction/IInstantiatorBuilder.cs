using System;
using System.Collections.Generic;

namespace Mimp.SeeSharper.Instantiation.Abstraction
{
    public interface IInstantiatorBuilder
    {


        public IInstantiatorBuilder SetRoot(Func<IInstantiator, IInstantiator> setRoot);

        public IInstantiatorBuilder Add(Func<IInstantiator, IEnumerable<IInstantiator>> getChildren);

        public IInstantiator Build();


    }
}
