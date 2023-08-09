using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text;

namespace StatimUI
{
    public static class Extensions
    {
        public static IEnumerable<T> OptimizedReverse<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable is IList<T> list)
                return ReverseList(list);
            
            if (enumerable is LinkedList<T> linkedList)
                return ReverseLinkedList(linkedList);

            return enumerable.Reverse();
        }

        public static IEnumerable<T> ReverseList<T>(this IList<T> list)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                yield return list[i];
            }
        }

        public static IEnumerable<T> ReverseLinkedList<T>(this LinkedList<T> list)
        {
            var el = list.Last;
            while (el != null)
            {
                yield return el.Value;
                el = el.Previous;
            }
        }

        public static Vector2 AddX(this Vector2 vec, float x) => vec + new Vector2(x, 0f);
        public static Vector2 AddY(this Vector2 vec, float y) => vec + new Vector2(0f, y);

        public static string KebabToPascalCase(this string text)
        {
            StringBuilder builder = new();

            bool nextUpperCase = false;
            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];

                if (i == 0)
                {
                    builder.Append(char.ToUpper(c));
                    continue;
                }

                if (c == '-')
                {
                    nextUpperCase = true;
                    continue;
                }

                builder.Append(nextUpperCase ? char.ToUpper(c) : c);
                nextUpperCase = false;
            }

            return builder.ToString();
        }
    }
}
