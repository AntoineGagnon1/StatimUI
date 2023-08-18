using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI
{
    public abstract class Property<T>
    {
        public abstract ref T Value { get; }

        public static implicit operator T(Property<T> property) => property.Value;

        // These methods are used in the code gen to be able to type infer without finding the type in the syntax tree
        // For exemple, the code will look like that: Property = Property.ToOneWayProperty(() => some_variable);
        // instead of Property = new OneWayProperty<Unknown_Type_Here>(() => some_variable)
        public ValueProperty<T> ToValueProperty(string input) => ValueProperty<T>.FromString(input);
        public OneWayProperty<T> ToBinding(OneWayGetter<T> getter) => new OneWayProperty<T>(getter);
        public TwoWayProperty<T> ToBinding(TwoWayGetter<T> getter) => new TwoWayProperty<T>(getter);
    }

    public class ValueProperty<T> : Property<T>
    {
        public static IStringConverter<T>? StringConverter;
        public static ValueProperty<T> FromString(string input)
        {
            if (StringConverter != null)
                return new ValueProperty<T>(StringConverter.ToValue(input));

            if (typeof(T).IsEnum)
                return new ValueProperty<T>((T)Enum.Parse(typeof(T), input));

            return new ValueProperty<T>((T)Convert.ChangeType(input, typeof(T)));
        }

        public ValueProperty(T value)
        {
            _value = value;
        }

        public ValueProperty()
        {
            _value = default!;
        }
        
        T _value;
        public override ref T Value => ref _value;
    }

    public delegate ref T TwoWayGetter<T>();

    public class TwoWayProperty<T> : Property<T>
    {
        private TwoWayGetter<T> getter;

        public TwoWayProperty(TwoWayGetter<T> getter)
        {
            this.getter = getter;
        }


        public override ref T Value => ref getter();
    }

    public delegate T OneWayGetter<T>();

    public class OneWayProperty<T> : Property<T>
    {
        private T lastGetterValue;
        private OneWayGetter<T> getter;

        public OneWayProperty(OneWayGetter<T> getter)
        {
            _value = getter();
            lastGetterValue = _value;
            this.getter = getter;
        }

        private T _value;
        public override ref T Value
        {
            get
            {
                var getterValue = getter();
                if (getterValue is not null && !getterValue.Equals(lastGetterValue))
                {
                    lastGetterValue = getterValue;
                    _value = getterValue;
                }
                return ref _value;
            }
        }
    }
}
