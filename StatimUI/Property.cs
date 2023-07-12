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
        public abstract object GetValue();
    }

    public abstract class Property<T> : Property
    {
        public event Action<T>? ValueChanged;

        protected void OnValueChanged(T value)
        {
            ValueChanged?.Invoke(value);
        }

        public abstract T Value { get; set; }

        public static implicit operator T(Property<T> p) => p.Value;

        public override void SetValue(object value)
        {
            // TODO: WILL CRASH
            Value = (T)Convert.ChangeType(value, typeof(T));
        }

        public override object GetValue()
        {
            return Value;
        }
    }

    public class VariableProperty<T> : Property<T>
    {

        public VariableProperty(T value)
        {
            _value = value;
        }

        public VariableProperty()
        {
            // TODO: Could crash if class
            _value = default(T);
        }

        T _value;
        public override T Value
        {
            get => _value;
            set
            {
                if (_value is null || !_value.Equals(value))
                {
                    _value = value;
                    OnValueChanged(value);
                }
            }
        }
    }

    public class BindedProperty<T> : Property<T>
    {
        private Func<T> getter;

        public BindedProperty(Func<T> getter, Action<T> setter)
        {
            this.getter = getter;
            ValueChanged += setter;
        }

        public BindedProperty(Func<object> getter, Action<object> setter)
        {
            this.getter = () => (T)Convert.ChangeType(getter(), typeof(T));
            ValueChanged += value => setter(value);
        }

        public override T Value
        {
            get => getter();
            set
            {
                if (Value is null || !Value.Equals(value))
                {
                    OnValueChanged(value);
                }
            }
        }
    }
}
