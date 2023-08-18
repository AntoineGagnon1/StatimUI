using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StatimCodeGenerator
{
    public static class Parser
    {

        private static Match isChar(TextSpan span, char c) => span[0] == c ? new Match(1) : Match.Emtpy;

        private static Match IsIdentifier(TextSpan t)
        {
            var c = t[0];
            if (!char.IsLetter(c) || c == '_')
                return Match.Emtpy;

            for (int i = 1; i < t.Length; i++)
            {
                c = t[i];
                if (!char.IsLetterOrDigit(c) && c != '_' && c != '-' && c != '.')
                    return new Match(i, t.Slice(0, i));
            }
            return new Match(t.Length, t);
        }

        private static int Escape(TextSpan t, int i, char startingBrace)
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

        private static int HexLength(TextSpan t, int i)
        {
            for (int j = 1; j < t.Length; j++)
            {
                var lowerCase = char.ToLower(t[j + i]);
                if (!char.IsDigit(t[j + i]) && lowerCase != 'a' && lowerCase != 'b' && lowerCase != 'c' && lowerCase != 'd' && lowerCase != 'e' && lowerCase != 'f')
                    return j - 1;
            }

            return t.Length - 1;
        }

        private static Match MatchString(TextSpan t)
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

        private static Match MatchCurlyContent(TextSpan t)
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
                    // TODO: match comments too because if they contain a { or } you don't want the comment to mess up the depth
                    // TODO: match also char literals
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

        private static Match MatchCode(TextSpan t)
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

        internal enum TokenType
        {
            OpenAngleBracket,
            ClosedAngleBracket,
            Slash,
            Equal,

            Code,
            Identifier,
            String,

            Invalid
        }

        static List<TokenDefinition<TokenType>> tokens = new()
        {
            new (TokenType.OpenAngleBracket, t => isChar(t, '<')),
            new (TokenType.ClosedAngleBracket, t => isChar(t, '>')),
            new (TokenType.Slash, t => isChar(t, '/')),
            new (TokenType.Equal, t => isChar(t, '=')),
            new (TokenType.Code, MatchCode),
            new (TokenType.Identifier, IsIdentifier),
            new (TokenType.String, MatchString),
        };

        static Lexer<TokenType> lexer = new (tokens, TokenType.Invalid);

        private static PropertyType TokenToPropertyType(TokenType token)
        {
            if (token == TokenType.String)
                return PropertyType.Value;
            if (token == TokenType.Code)
                return PropertyType.Binding;

            throw new Exception("Could not parse the property");
        }

        private static PropertySyntax? MatchProperty(Lexer<TokenType> lexer)
        {
            if (lexer.Current.Type == TokenType.Identifier)
            {
                var name = lexer.Current.Content;
                lexer.MoveNext();
                if (lexer.Current.Type == TokenType.Equal)
                {
                    lexer.MoveNext();
                    var type = TokenToPropertyType(lexer.Current.Type);
                    var content = lexer.Current.Content.ToString();
                    lexer.MoveNext();
                    return new PropertySyntax(content, type, name.ToString());
                }

                return new PropertySyntax("true", PropertyType.Value, name.ToString());
            }

            return null;
        }

        private static ComponentSyntax MatchIf(Lexer<TokenType> lexer)
        {
            if (lexer.Current.Type == TokenType.Code)
            {
                var condition = lexer.Current.Content;
                lexer.MoveNext();
                if (lexer.Current.Type == TokenType.ClosedAngleBracket)
                {
                    lexer.MoveNext();
                    var children = MatchChildren(lexer);
                    EnsureClosingTag(lexer);
                    return new ComponentSyntax("if", children, new List<PropertySyntax> { new PropertySyntax(condition, PropertyType.Binding, "Condition") });
                }

                if (lexer.Current.Type == TokenType.Slash)
                    throw new Exception("An if componentcan't be self-closed");

            }
            throw new Exception("An if component must follow this syntax:\n<if {condition}>\n    [children here]\n</if>");
        }

        private static ForEachSyntax MatchForEach(Lexer<TokenType> lexer)
        {
            if (lexer.Current.Type == TokenType.Code)
            {
                var item = lexer.Current.Content;
                lexer.MoveNext();
                if (lexer.Current.Type == TokenType.Identifier && lexer.Current.Content == "in")
                {
                    lexer.MoveNext();
                    if (lexer.Current.Type == TokenType.Code)
                    {
                        var items = lexer.Current.Content;
                        lexer.MoveNext();
                        if (lexer.Current.Type == TokenType.ClosedAngleBracket)
                        {
                            lexer.MoveNext();
                            var children = MatchChildren(lexer);
                            EnsureClosingTag(lexer);
                            return new ForEachSyntax(children, item, items);
                        }

                        if (lexer.Current.Type == TokenType.Slash)
                            throw new Exception("A foreach component can't be self-closed");
                    }
                }
            }
            throw new Exception("A foreach component must follow this syntax:\n<foreach {itemName} in {listName}>\n    [children here]\n</foreach>");
        }

        private static void EnsureClosingTag(Lexer<TokenType> lexer)
        {
            if (lexer.Current.Type == TokenType.Slash)
            {
                lexer.MoveNext();
                if (lexer.Current.Type == TokenType.Identifier)
                {
                    lexer.MoveNext();
                    if (lexer.Current.Type == TokenType.ClosedAngleBracket)
                    {
                        lexer.MoveNext();
                        return;
                    }
                }
            }
            throw new Exception("An opened tag must be closed");
        }

        private static List<ComponentSyntax> MatchChildren(Lexer<TokenType> lexer)
        {
            var components = new List<ComponentSyntax>();
            while (true)
            {
                var component = MatchComponent(lexer);
                if (component == null)
                    break;

                components.Add(component);
            }
            return components;
        }

        private static ComponentSyntax? MatchComponent(Lexer<TokenType> lexer)
        {
            if (lexer.Current.Type == TokenType.OpenAngleBracket)
            {
                lexer.MoveNext();
                if (lexer.Current.Type == TokenType.Identifier)
                {
                    var name = lexer.Current.Content;
                    lexer.MoveNext();

                    if (name == "script")
                        return MatchScript(lexer);

                    if (name == "foreach")
                        return MatchForEach(lexer);

                    if (name == "if")
                        return MatchIf(lexer);

                    var properties = new List<PropertySyntax>();

                    var property = MatchProperty(lexer);
                    while (property != null)
                    {
                        properties.Add(property);

                        property = MatchProperty(lexer);
                    }

                    if (lexer.Current.Type == TokenType.Slash)
                    {
                        lexer.MoveNext();
                        if (lexer.Current.Type == TokenType.ClosedAngleBracket)
                        {
                            lexer.MoveNext();
                            return new ComponentSyntax(name.ToString(), new(), properties);
                        }
                        throw new Exception("A slash must be followed by a closing angle bracket to make a self closing tag");
                    }

                    if (lexer.Current.Type == TokenType.ClosedAngleBracket)
                    {
                        lexer.MoveNext();
                        var children = MatchChildren(lexer);

                        EnsureClosingTag(lexer);

                        return new ComponentSyntax(name.ToString(), children, properties);
                    }

                    if (lexer.Current.Type == TokenType.String || lexer.Current.Type == TokenType.Code || lexer.Current.Type == TokenType.Equal)
                        throw new Exception("Properties must have a name");

                    throw new Exception("A component declaration can contain properties and must end by a closed angle bracket");
                }

                if (lexer.Current.Type == TokenType.Slash)
                    return null; // closing tag

                throw new Exception("An opened angle bracket must be followed by a name to define a component");
            }
            return null;
        }

        private static ScriptSyntax MatchScript(Lexer<TokenType> lexer)
        {
            var properties = new List<PropertySyntax>();

            var property = MatchProperty(lexer);
            while (property != null)
            {
                properties.Add(property);

                property = MatchProperty(lexer);
            }

            if (lexer.Current.Type == TokenType.Slash)
            {
                lexer.MoveNext();
                if (lexer.Current.Type == TokenType.ClosedAngleBracket)
                {
                    lexer.MoveNext();
                    return new ScriptSyntax("", properties);
                }    
            }

            if (lexer.Current.Type == TokenType.ClosedAngleBracket)
            {
                lexer.MoveNext();
                if (lexer.Current.Type == TokenType.Code)
                {
                    var code = lexer.Current.Content;
                    lexer.MoveNext();
                    if (lexer.Current.Type == TokenType.OpenAngleBracket)
                    {
                        lexer.MoveNext();
                        if (lexer.Current.Type == TokenType.Slash)
                        {
                            lexer.MoveNext();
                            if (lexer.Current.Type == TokenType.Identifier && lexer.Current.Content == "script")
                            {
                                lexer.MoveNext();
                                if (lexer.Current.Type == TokenType.ClosedAngleBracket)
                                {
                                    lexer.MoveNext();
                                    return new ScriptSyntax(code, properties);
                                }
                            }
                        }
                    }
                }
            }

            // TODO: better error;
            throw new Exception("invalid script syntax");
        }

        public static ParsingResult Parse(string statim)
        {
            lexer.Reset(statim);
            lexer.MoveNext();
            ScriptSyntax? scriptSyntax = null;
            ComponentSyntax? root = null;
            ComponentSyntax? component = null;

            while ((component = MatchComponent(lexer)) != null)
            {
                if (component is ScriptSyntax script)
                {
                    if (scriptSyntax != null)
                        throw new Exception("A component cannot have more than one script tag");
                    scriptSyntax = script;
                }
                else
                {
                    if (root != null)
                        throw new Exception("A component cannot have more than one root");
                    root = component;
                }
            }
            return new ParsingResult(root, scriptSyntax);
        }
    }


    public enum PropertyType
    {
        Value, Binding
    }

    public class PropertySyntax
    {
        public string Name { get; }
        public string Value { get; }
        public PropertyType Type { get; }

        public PropertySyntax(string value, PropertyType type, string name)
        {
            Value = value;
            Type = type;
            Name = name;
        }
    }

    public class ParsingResult
    {
        public ComponentSyntax? Root { get; }
        public ScriptSyntax? Script { get; }

        public ParsingResult(ComponentSyntax? root, ScriptSyntax? script)
        {
            Root = root;
            Script = script;
        }
    }

    public class ScriptSyntax : ComponentSyntax
    {
        public string Code { get; }

        public ScriptSyntax(string code, List<PropertySyntax> properties) : base("script", new() { }, properties)
        {
            Code = code;
        }
    }

    public class ComponentSyntax
    {
        public List<PropertySyntax> Properties { get; } = new();
        public string Name { get; }
        public List<ComponentSyntax> Slots { get; } = new();

        public ComponentSyntax(string name, List<ComponentSyntax> slots)
        {
            Name = name;
            Slots = slots;
        }

        public ComponentSyntax(string name, List<ComponentSyntax> slots, List<PropertySyntax> properties)
        {
            Name = name;
            Slots = slots;
            Properties = properties;
        }
    }

    public class ForEachSyntax : ComponentSyntax
    {
        public string Item { get; }
        public string Items { get; }

        public ForEachSyntax(List<ComponentSyntax> slots, string item, string items) : base("foreach", slots)
        {
            Item = item;
            Items = items;
        }
    }
}
