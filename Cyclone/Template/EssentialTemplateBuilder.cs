using System;
using System.Collections;
using Cyclone.Utils;

namespace Cyclone.Template
{
    internal static class CharReaderExtensions
    {
        internal static string GetUntilCode(this CharReader reader)
        {
            return reader.TakeUntil((current, peek) => current == '{' && (peek == '{' || peek == '%'));
        }

        internal static string GetCode(this CharReader reader)
        {
            reader.SkipWhile(c => c == '{' || c == '%');
            var code = reader.TakeUntil((current, peek) => (current == '}' || current == '%') && peek == '}').Trim();
            reader.SkipWhile(c => c == '}' || c == '%');
            return code.Trim();
        }
    }

    internal struct LoopPrototype
    {
        public LoopPrototype(string variableName, IEnumerable iterableObject)
        {
            IterableObject = iterableObject;
            VariableName = variableName;
        }

        public IEnumerable IterableObject { get; }
        public string VariableName { get; }
    }

    public sealed class EssentialTemplateBuilder : TemplateBuilder
    {
        internal static readonly EssentialTemplateBuilder Instance = new EssentialTemplateBuilder();
        private EssentialTemplateBuilder() { }

        private static readonly char BracketBegin = '(';
        private static readonly char BracketEnd = ')';

        public override string Build<T>(string template, T model)
        {
            // If no models exist, just give back the template.
            if (model == null) return template;

            var result = string.Empty;
            var reader = new CharReader(template);

            while (reader.HasNext)
            {
                result += reader.GetUntilCode();
                string codeComponent = reader.GetCode();
                if (!string.IsNullOrWhiteSpace(codeComponent))
                {
                    var codeReader = new CharReader(codeComponent);
                    var command = codeReader.PeekWhile(char.IsLetterOrDigit);
                    switch (command)
                    {
                        case "foreach":
                            var loopPrototype = GetLoopPrototype(codeReader, model);
                            var loopTemplate = GetLoopTemplate(reader);
                            result += BuildLoop(loopPrototype, loopTemplate, model);
                            break;
                        default:
                            result += GetField(model, command).ToString();
                            break;
                    }
                }
            }

            return result;
        }

        private static LoopPrototype GetLoopPrototype<T>(CharReader reader, T model)
        {
            reader.Eat("foreach");
            reader.Eat(BracketBegin);
            reader.Eat("var");
            string variableName = reader.TakeWhile(char.IsLetterOrDigit);
            reader.Eat("in");
            string fieldName = reader.TakeWhile(char.IsLetterOrDigit);
            reader.Eat(BracketEnd);

            object iterableObject = GetField(model, fieldName);
            if (!(iterableObject is IEnumerable))
            {
                throw new Exception($"{fieldName} is not iterable.");
            }
            return new LoopPrototype(variableName, (IEnumerable)iterableObject);
        }

        private static string GetLoopTemplate(CharReader reader)
        {
            var result = string.Empty;
            while (true)
            {
                result += reader.GetUntilCode();
                var loopCode = reader.GetCode();
                if (loopCode == "end") break;
                if (string.IsNullOrWhiteSpace(loopCode)) throw new Exception("Nothing found in bracket. You might be missing {{ end }} for a loop.");
                result += $"{{{{ {loopCode} }}}}";
            }
            return result;
        }

        private static string BuildLoop<T>(LoopPrototype prototype, string loopTemplate, T model)
        {
            var result = string.Empty;
            var template = new CharReader(loopTemplate);
            foreach (var current in prototype.IterableObject)
            {
                while (template.HasNext)
                {
                    result += template.GetUntilCode();
                    string inLoopCode = template.GetCode();
                    if (string.IsNullOrWhiteSpace(inLoopCode)) { break; }
                    result += inLoopCode == prototype.VariableName
                        ? current.ToString()
                        : GetField(model, inLoopCode).ToString();
                }
                template.Reset();
            }
            return result;
        }

    }
}
