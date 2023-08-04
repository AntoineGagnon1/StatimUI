using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI
{
    public interface IStringConverter<T>
    {
        T ToValue(string input);
    }
}
