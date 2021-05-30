﻿using Mimp.SeeSharper.Instantiation.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mimp.SeeSharper.Instantiation
{
    public class TryThrowInstantiatorBuilder : IInstantiatorBuilder
    {


        private readonly ISet<Func<IInstantiator, IEnumerable<IInstantiator>>> _getChildren;

        private Func<IInstantiator, IInstantiator>? _getRoot;


        public TryThrowInstantiatorBuilder()
        {
            _getChildren = new HashSet<Func<IInstantiator, IEnumerable<IInstantiator>>>();
        }


        public IInstantiatorBuilder SetRoot(Func<IInstantiator, IInstantiator> setRoot)
        {
            var old = _getRoot;
            _getRoot = old is null ? setRoot
                : root => setRoot(old(root));

            return this;
        }

        public IInstantiatorBuilder Add(Func<IInstantiator, IEnumerable<IInstantiator>> getChildren)
        {
            _getChildren.Add(getChildren ?? throw new ArgumentNullException(nameof(getChildren)));
            return this;
        }

        public IInstantiator Build()
        {
            IInstantiator? root = null;
            new TryThrowInstantiator(r =>
            {
                root = _getRoot is null ? r : _getRoot(r) ?? throw new ArgumentException("SetRoot return null.");
                return _getChildren.SelectMany(getChild => getChild(root) ?? throw new ArgumentException("At least one getChildren has return null."));
            });
            return root!;
        }


    }
}
