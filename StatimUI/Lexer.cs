using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StatimUI
{
    internal ref struct Lexer<T> where T : Enum
    {
        public List<TokenDefinition<T>> TokenDefinitions = new();
        private ReadOnlySpan<char> remainingText;
        public Token<T> Current;

        internal bool MoveNext()
        {
            if (remainingText.IsWhiteSpace())
                return false;

            int i = 0;
            while (char.IsWhiteSpace(remainingText[i]))
                i++;

            if (i > 0)
                remainingText = remainingText.Slice(i);

            foreach (var tokenDefinition in TokenDefinitions)
            {
                var match = tokenDefinition.Matcher(remainingText);
                if (match.Length > 0)
                {
                    Current = new Token<T>(tokenDefinition.TokenType, match.Content);
                    
                    remainingText = remainingText.Slice(match.Length);

                    return true;
                }
                
            }

            throw new Exception("No token found. The remainingText is: "  + remainingText.ToString());
        }

        public Lexer(List<TokenDefinition<T>> tokenDefinitions, string text)
        {
            TokenDefinitions = tokenDefinitions;
            remainingText = text.AsSpan();
        }
    }

    internal delegate Match Matcher(ReadOnlySpan<char> text);

    internal ref struct Match
    {
        public static Match Emtpy => new Match(0);

        internal ReadOnlySpan<char> Content = ReadOnlySpan<char>.Empty;
        internal int Length;

        public Match(int length)
        {
            Length = length;
        }

        public Match(int length, ReadOnlySpan<char> content)
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

    internal ref struct Token<T>
    {
        internal T Type { get; set; }
        internal ReadOnlySpan<char> Content { get; set; }

        internal Token(T type, ReadOnlySpan<char> content)
        {
            Type = type;
            Content = content;
        }
    }
}
