using Mimp.SeeSharper.Instantiation.Abstraction;
using System;

namespace Mimp.SeeSharper.Instantiation
{
    /// <summary>
    /// A <see cref="IInstantiator"/> to instantiate classes and structors with a public constructor and set members too.
    /// </summary>
    public class ConstructorMemberInstantiator : ConstructorInstantiator
    {


        public IInstantiator MemberInstantiator
        {
            get => _memberInstantiator.ValueInstantiator;
        }

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
            IInstantiator memberInstantiator
            ) : base(parameterInstantiator, tryDefaultInstance)
        {
            _memberInstantiator = new MemberInstantiator(this, memberInstantiator ?? throw new ArgumentNullException(nameof(memberInstantiator)));
        }

        public ConstructorMemberInstantiator(IInstantiator valueInstantiator, bool tryDefaultInstance)
            : this(valueInstantiator ?? throw new ArgumentNullException(nameof(valueInstantiator)), tryDefaultInstance, valueInstantiator) { }

        public ConstructorMemberInstantiator(IInstantiator valueInstantiator)
            : this(valueInstantiator, true) { }


        protected override void InstantiateInstance(object instance, object? instantiateValues, out object? ignoredInstantiateValues)
        {
            base.InstantiateInstance(instance, instantiateValues, out ignoredInstantiateValues);
            _memberInstantiator.InstantiateInstance(instance, ignoredInstantiateValues, out ignoredInstantiateValues);
        }


        public override void Initialize(object? instance, object? initializeValues, out object? ignoredInitializeValues)
        {
            base.Initialize(instance, initializeValues, out ignoredInitializeValues);
            _memberInstantiator.Initialize(instance, ignoredInitializeValues, out ignoredInitializeValues);
        }


    }
}
