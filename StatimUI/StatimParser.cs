using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StatimUI
{
    public static class StatimParser
    {

        private static Match isChar(ReadOnlySpan<char> span, char c) => span[0] == c ? new Match(1) : Match.Emtpy;

        private static Match IsIdentifier(ReadOnlySpan<char> t)
        {
            var c = t[0];
            if (!char.IsLetter(c) || c == '_')
                return Match.Emtpy;

            for (int i = 1; i < t.Length; i++)
            {
                c = t[i];
                if (!char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '.')
                    return new Match(i, t.Slice(0, i));
            }
            return new Match(t.Length, t);
        }

        private static int Escape(ReadOnlySpan<char> t, int i, char startingBrace)
        {
            var c = t[i];
            if (c == startingBrace || c == '\\' || c == '0' || c == 'a' || c == 'b' || c == 'f' || c == 'n' || c == 'r' || c == 't' || c == 'v')
                return 0;

            if (c == 'u' && HexLength(t, i) >= 4)
                return 4;

            if (c == 'x')
            {
                var xLength = HexLength(t, i);
                if (xLength >= 1 && xLength <= 4)
                    return xLength;
            }

            if (c == 'U' && HexLength(t, i) >= 4)
                return 8;

            throw new Exception("Backslashes must be escaped");
        }

        private static int HexLength(ReadOnlySpan<char> t, int i)
        {
            for (int j = 1; j < t.Length; j++)
            {
                var lowerCase = char.ToLower(t[j + i]);
                if (!char.IsDigit(t[j + i]) && lowerCase != 'a' && lowerCase != 'b' && lowerCase != 'c' && lowerCase != 'd' && lowerCase != 'e' && lowerCase != 'f')
                    return j - 1;
            }

            return t.Length - 1;
        }

        private static Match MatchString(ReadOnlySpan<char> t)
        {
            char c = t[0];
            char startingBrace;
            if (c == '"')
                startingBrace = '"';
            else if (c == '\'')
                startingBrace = '\'';
            else
                return Match.Emtpy;

            bool isEscaped = false;
            for (int i = 1; i < t.Length; i++)
            {
                c = t[i];
                if (!isEscaped && c == startingBrace)
                    return new Match(i + 1, t.Slice(1, i - 1));

                if (isEscaped)
                {
                    isEscaped = false;
                    i += Escape(t, i, startingBrace);
                }
                else if (c == '\\')
                    isEscaped = true;

            }

            throw new Exception("String never ends. Missing " + startingBrace);
        }

        private static Match MatchCurlyContent(ReadOnlySpan<char> t)
        {
            if (t[0] == '{')
            {
                int depthLevel = 0;
                for (int i = 1; i < t.Length; i++)
                {
                    var stringMatch = MatchString(t.Slice(i));
                    if (stringMatch.Length > 0)
                    {
                        i += stringMatch.Length - 1;
                        continue;
                    }

                    var c = t[i];
                    if (c == '{')
                        depthLevel++;
                    else if (c == '}')
                    {
                        if (depthLevel == 0)
                            return new Match(i + 1, t.Slice(1, i - 1));

                        depthLevel--;
                    }
                }
                throw new Exception("A binding must have a closing curly bracket");
            }

            return Match.Emtpy;
        }

        private static Match MatchOneWayBinding(ReadOnlySpan<char> t)
        {
            var c = t[0];
            char startingBrace;
            if (c == '"')
                startingBrace = '"';
            else if (c == '\'')
                startingBrace = '\'';
            else
                return MatchCurlyContent(t);

            var match = MatchCurlyContent(t.Slice(1));
            if (t[match.Length + 1] == startingBrace)
                return new Match(match.Length + 2, match.Content);

            return Match.Emtpy;
        }

        private static Match MatchTwoWayBinding(ReadOnlySpan<char> t)
        {
            var match = MatchOneWayBinding(t);
            if (match.Length == 0)
                return Match.Emtpy;

            if (match.Content.StartsWith("bind "))
                return new Match(match.Length, match.Content.Slice(5));

            return Match.Emtpy;
        }

        internal enum TokenType
        {
            OpenAngleBracket,
            ClosedAngleBracket,
            Slash,
            Equal,

            TwoWayBinding,
            OneWayBinding,
            Identifier,
            String
        }

        public static void Parse(string xml)
        {
            var tokens = new List<TokenDefinition<TokenType>>
            {
                new (TokenType.OpenAngleBracket, t => isChar(t, '<')),
                new (TokenType.ClosedAngleBracket, t => isChar(t, '>')),
                new (TokenType.Slash, t => isChar(t, '/')),
                new (TokenType.Equal, t => isChar(t, '=')),
                new (TokenType.TwoWayBinding, MatchTwoWayBinding),
                new (TokenType.OneWayBinding, MatchOneWayBinding),
                new (TokenType.Identifier, IsIdentifier),
                new (TokenType.String, MatchString),
            };
            var lexer = new Lexer<TokenType>(tokens, xml);

            var watch = Stopwatch.StartNew();
            while (lexer.MoveNext())
            {
                Console.WriteLine(lexer.Current.Type + " " + lexer.Current.Content.ToString());
            }
            watch.Stop();
            Console.WriteLine(watch.ElapsedTicks / 10_000f);
        }
    }
}
