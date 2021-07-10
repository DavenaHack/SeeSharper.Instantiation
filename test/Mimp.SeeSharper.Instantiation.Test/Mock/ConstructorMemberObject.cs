using System;

namespace Mimp.SeeSharper.Instantiation.Test.Mock
{
    public class ConstructorMemberObject
    {


        public string? Num { get; set; }

        public string? Prop { get; }

        public string? Bar { get; set; }


        public ConstructorMemberObject(string? num, string? bar)
        {
            Num = num;
            Bar = bar;
        }

        public ConstructorMemberObject(string? prop)
        {
            Prop = prop;
        }


        public override bool Equals(object? obj)
        {
            return obj is ConstructorMemberObject @object &&
                   Num == @object.Num &&
                   Prop == @object.Prop &&
                   Bar == @object.Bar;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Num, Prop, Bar);
        }

        
    }
}
