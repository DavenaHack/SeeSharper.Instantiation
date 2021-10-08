using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.ObjectDescription;
using Mimp.SeeSharper.ObjectDescription.Abstraction;
using System;

namespace Mimp.SeeSharper.Instantiation
{
    /// <summary>
    /// A <see cref="IInstantiator"/> to instantiate classes and structors with a public constructor and set members too.
    /// </summary>
    public class ConstructorMemberInstantiator : ConstructorInstantiator
    {


        public IInstantiator MemberInstantiator => _memberInstantiator.ValueInstantiator;

        public Action<object, string?, object?>? HandleUnknownMember
        {
            get => _memberInstantiator.HandleUnknownMember;
            set => _memberInstantiator.HandleUnknownMember = value;
        }

        public Action<object, string?, Type, object?, Exception>? HandleMemberCanNotSet
        {
            get => _memberInstantiator.HandleMemberCanNotSet;
            set => _memberInstantiator.HandleMemberCanNotSet = value;
        }


        private readonly MemberInstantiator _memberInstantiator;


        public ConstructorMemberInstantiator(
            IInstantiator parameterInstantiator,
            bool tryDefaultInstance,
            bool useDefaultParameter,
            IInstantiator memberInstantiator
            ) : base(parameterInstantiator, tryDefaultInstance, useDefaultParameter)
        {
            _memberInstantiator = new MemberInstantiator(this, memberInstantiator ?? throw new ArgumentNullException(nameof(memberInstantiator)));
        }

        public ConstructorMemberInstantiator(IInstantiator valueInstantiator, bool tryDefaultInstance, bool useDefaultParameter)
            : this(valueInstantiator ?? throw new ArgumentNullException(nameof(valueInstantiator)), tryDefaultInstance, useDefaultParameter, valueInstantiator) { }

        public ConstructorMemberInstantiator(IInstantiator valueInstantiator)
            : this(valueInstantiator, true, true) { }


        protected override void InstantiateInstance(object instance, IObjectDescription description, out IObjectDescription? ignored)
        {
            base.InstantiateInstance(instance, description, out ignored);
            _memberInstantiator.InstantiateInstance(instance, ignored ?? ObjectDescriptions.NullDescription, out ignored);
        }


        public override object? Initialize(Type type, object? instance, IObjectDescription description, out IObjectDescription? ignored)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (description is null)
                throw new ArgumentNullException(nameof(description));

            instance = base.Initialize(type, instance, description, out ignored);
            return _memberInstantiator.Initialize(instance, ignored ?? ObjectDescriptions.NullDescription, out ignored);
        }


    }
}
