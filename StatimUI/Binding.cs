using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI
{
    public struct Binding
    {
        public Func<object> Getter;
        public Action<object>? Setter;

        public bool IsOneWay => Setter == null;

        [MemberNotNullWhen(returnValue: true, nameof(Setter))]
        public bool IsTwoWay => Setter != null;


        public Binding(Func<object> get, Action<object>? set = null)
        {
            Getter = get;
            Setter = set;
        }
    }
}
