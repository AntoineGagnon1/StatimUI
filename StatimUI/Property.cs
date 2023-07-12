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
            Value = (T)value;
        }

        public override object GetValue(object value)
        {
            return value;
        }
    }

    public abstract class VariableProperty<T> : Property<T>
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

    public abstract class BindedProperty<T> : Property<T>
    {
        private Func<T> getter;

        public BindedProperty(Func<T> getter, Action<T> setter)
        {
            this.getter = getter;
            ValueChanged += setter;
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
