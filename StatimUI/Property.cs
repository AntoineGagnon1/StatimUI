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

        public static Property<T> FromBinding(Binding binding)
        {
            if (binding.IsTwoWay)
            {
                return new TwoWayProperty<T>(
                    () => (T)binding.Getter(),
                    value => binding.Setter(value!)
                );
            }

            return new OneWayProperty<T>(() => (T)binding.Getter());
        }


        public override void SetValue(object value)
        {
            Value = (T)Convert.ChangeType(value, typeof(T));
        }

        public override object GetValue()
        {
            // will crash
            return Value!;
        }
    }

    public class ValueProperty<T> : Property<T>
    {

        public ValueProperty(T value)
        {
            _value = value;
        }

        public ValueProperty()
        {
            _value = default!;
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

    public class TwoWayProperty<T> : Property<T>
    {
        private Func<T> getter;

        public TwoWayProperty(Func<T> getter, Action<T> setter)
        {
            this.getter = getter;
            ValueChanged += setter;
        }


        public override T Value
        {
            get => getter();
            set
            {
                var oldValue = Value;
                if (oldValue is null || !oldValue.Equals(value))
                {
                    OnValueChanged(value);
                }
            }
        }
    }

    public class OneWayProperty<T> : Property<T>
    {
        private T lastGetterValue;
        private Func<T> getter;

        public OneWayProperty(Func<T> getter)
        {
            _value = getter();
            lastGetterValue = _value;
            this.getter = getter;
        }

        private T _value;

        public override T Value
        {
            get
            {
                var getterValue = getter();
                if (getterValue is not null && !getterValue.Equals(lastGetterValue))
                {
                    lastGetterValue = getterValue;
                    _value = getterValue;
                }
                return _value;
            }
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
}
