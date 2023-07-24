using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI
{
    internal class ScriptBuilder
    {
        private StringBuilder builder = new StringBuilder();
#if DEBUG
        private int appendWidth = 0;
#endif
        public int Capacity => builder.Capacity;
        public int Length => builder.Length;

        [Conditional("DEBUG")]
        public void Indent(int num = 1) => appendWidth += num;
        [Conditional("DEBUG")]
        public void Undent(int num = 1) => appendWidth -= num;

        [Conditional("DEBUG")]
        private void AddIndend(ref string? str)
        {
            if (str is null)
                return;

            string indentStr = new string('\t', appendWidth);
            str = str.Replace("\n", "\n" + indentStr);
        }


        public void Append(string? str)
        {
            AddIndend(ref str);
            builder.Append(str);
        }

        public void AppendLine(string? str)
        {
            str += "\n";
            AddIndend(ref str);
            builder.Append(str);
        }

        public void AppendLine()
        {
            string? str = "\n";
            AddIndend(ref str);
            builder.Append(str);
        }

        public void Clear() => builder.Clear();

        // Returns the new capacity
        public int EnsureCapacity(int capacity) => builder.EnsureCapacity(capacity);

        public char this[int index] { get => builder[index]; set => builder[index] = value; }

        public override string ToString() => builder.ToString();
        public string ToString(int startIndex, int length) => builder.ToString(startIndex, length);
    }
}
