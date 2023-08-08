using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StatimUI
{
    internal readonly struct TextSpan
    {
        public readonly string Text;
        public readonly int Start;
        public readonly int Length;

        public char this[int i]
        {
            get => Text[i + Start];
        }


        public TextSpan Slice(int start, int length)
        {
            return new TextSpan(Text, Start + start, length);
        }

        public TextSpan Slice(int start)
        {
            return new TextSpan(Text, Start + start, Length - start);
        }


        public TextSpan(string text, int start)
        {
            Text = text;
            Start = start;
            Length = text.Length - Start;
        }

        public TextSpan(string text, int start, int length)
        {
            Text = text;
            Start = start;
            Length = length;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Text))
                return string.Empty;

            return Text.Substring(Start, Length);
        }

        public bool StartsWith(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (Text[Start + i] != text[i])
                    return false;
            }

            return true;
        }
    }

    internal class Lexer<T> where T : Enum
    {
        public List<TokenDefinition<T>> TokenDefinitions = new();
        private int pos;
        public string Text;
        public Token<T> Current;
        private T invalid;

        public void Reset(string text)
        {
            Text = text;
            pos = 0;
        }

        internal void MoveNext()
        {
            if (pos >= Text.Length)
            {
                Current = new Token<T>(invalid, string.Empty);
                return;
            }

            while (char.IsWhiteSpace(Text[pos]))
            {
                if (pos >= Text.Length - 1)
                {
                    Current = new Token<T>(invalid, string.Empty);
                    return;
                }
                pos++;
            }

            foreach (var tokenDefinition in TokenDefinitions)
            {
                var match = tokenDefinition.Matcher(new TextSpan(Text, pos));
                if (match.Length > 0)
                {
                    Current = new Token<T>(tokenDefinition.TokenType, match.Content.ToString());

                    pos += match.Length;

                    return;
                }
                
            }

            throw new Exception("No token found. The remainingText is: "  + Text.Substring(pos));
        }

        public Lexer(List<TokenDefinition<T>> tokenDefinitions, T invalidToken)
        {
            TokenDefinitions = tokenDefinitions;
            invalid = invalidToken;
        }
    }

    internal delegate Match Matcher(TextSpan text);

    internal struct Match
    {
        public static Match Emtpy => new Match();

        internal TextSpan Content;
        internal int Length;

        public Match(int length)
        {
            Content = new(string.Empty, 0, 0);
            Length = length;
        }

        public Match(int length, TextSpan content)
        {
            Length = length;
            Content = content;
        }
    }

    internal struct TokenDefinition<T>
    {
        internal Matcher Matcher { get; set; }
        internal T TokenType { get; set; }

        internal TokenDefinition(T type, Matcher matcher)
        {
            TokenType = type;
            Matcher = matcher;
        }
    }

    internal struct Token<T>
    {
        internal T Type { get; set; }
        internal string Content { get; set; }

        internal Token(T type, string content)
        {
            Type = type;
            Content = content;
        }
    }
}
