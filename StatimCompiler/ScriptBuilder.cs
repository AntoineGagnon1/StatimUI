using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatimCodeGenerator
{
    internal class ScriptBuilder
    {
        private StringBuilder builder = new StringBuilder();

        private static int appendWidth = 0;
        public int Capacity => builder.Capacity;
        public int Length => builder.Length;

        [Conditional("DEBUG")]
        public void Indent(int num = 1) => appendWidth += num;

        [Conditional("DEBUG")]
        public void Unindent(int num = 1) => appendWidth -= num;

        [Conditional("DEBUG")]
        private void AddIndent(ref string? str)
        {
            if (str is null)
                return;

            string indentStr = new string(' ', appendWidth * 4);
            str = indentStr + str;
        }


        public void Append(string? str)
        {
            AddIndent(ref str);
            builder.Append(str);
        }

        public void AppendLine(string? str = "")
        {
            AddIndent(ref str);
            builder.AppendLine(str);
        }

        public void AppendLineNoIndent(string? str)
        {
            builder.AppendLine(str);
        }

        public void Clear() => builder.Clear();

        // Returns the new capacity
        public int EnsureCapacity(int capacity) => builder.EnsureCapacity(capacity);

        public char this[int index] { get => builder[index]; set => builder[index] = value; }

        public override string ToString() => builder.ToString();
        public string ToString(int startIndex, int length) => builder.ToString(startIndex, length);
    }
}
