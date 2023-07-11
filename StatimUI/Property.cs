using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI
{
    public abstract class Property
    {
        public abstract void SetValue(object value);
        public abstract object GetValue(object value);
    }

    public class Property<T> : Property
    {
        private Func<T> getter;
        private Action<T> setter;

        public Property(Func<T> getter, Action<T> setter) 
        {
            this.getter = getter;
            this.setter = setter;
            HasChanged = false;
        }

        public T Value 
        { 
            get => getter();
            set 
            {
                if(Value is null || !Value.Equals(value))
                {
                    HasChanged = true;
                    setter(value);
                }
            } 
        }

        public static implicit operator T(Property<T> p) => p.Value;

        public bool HasChanged { get; internal set; }

        public override void SetValue(object value)
        {
            // TODO: WILL CRASH
            Value = (T)value;
        }

        public override object GetValue(object value)
        {
            return value;
        }
    }
}
