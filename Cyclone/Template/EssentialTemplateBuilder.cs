using System;
using System.Linq;
using Cyclone.Template.Contexts;
using Cyclone.Template.Prototypes;
using Cyclone.Utils;

namespace Cyclone.Template
{
    public sealed class EssentialTemplateBuilder : TemplateBuilder
    {
        private static readonly string[] Reserved = {"foreach", "if", "end"};

        private Context _currentContext = null;

        public override string Build<T>( string template, T model )
        {
            // If no models exist, just give back the template.
            if (model == null) return template;

            var result = string.Empty;
            var reader = new CharReader(template);

            while (reader.HasNext)
            {
                result += reader.GetUntilCode();
                string codeComponent = reader.GetCode();
                if (!string.IsNullOrWhiteSpace( codeComponent ))
                {
                    var lexer = new SimpleLexer(codeComponent);
                    var command = lexer.Next();
                    if (Reserved.Contains( command ))
                    {
                        switch (command)
                        {
                            case "foreach":
                                var loopPrototype = LoopPrototype.Parse(lexer, model);
                                var loopTemplate = GetBlock(reader);
                                result += EvaluateLoop( loopPrototype, loopTemplate, model );
                                break;
                            case "if":
                                var ifPrototype = IfPrototype.Parse( lexer, model );
                                var ifBlock = GetBlock( reader );
                                result += EvalueateIf( ifPrototype, ifBlock, model );
                                break;
                            default:
                                throw new Exception( $"Invalid command: {command}" );
                        }
                    }
                    else
                    {
                        switch (_currentContext)
                        {
                            case LoopContext loopContext:
                                result += command == loopContext.VariableName
                                    ? loopContext.Current.ToString()
                                    : GetFieldAsString( model, command );
                                break;
                            default:
                                result += GetFieldAsString( model, command );
                                break;
                        }
                    }
                }
            }

            return result;
        }

        private static string GetFieldAsString<T>( T model, string fieldName )
        {
            var fieldValue = GetField( model, fieldName );
            return fieldValue == null ? string.Empty : fieldValue.ToString();
        }

        private static string GetBlock( CharReader reader )
        {
            var block = string.Empty;
            int indent = 0;
            while (true)
            {
                block += reader.GetUntilCode();
                var code = reader.GetCode();
                var command = code.TakeWhile( char.IsLetterOrDigit ).Aggregate( string.Empty, ( s, c ) => s + c );
                if (Reserved.Contains( command ) && command != "end") { indent++; }
                if (code == "end")
                {
                    if (indent > 0) indent--;
                    else break;
                }
                if ( string.IsNullOrWhiteSpace( code ) )
                    { throw new Exception( "Nothing found in bracket, or you may be missing an {{ end }}." ); }
                block += $"{{{{ {code} }}}}";
            }
            return block;
        }

        private string EvaluateLoop<T>( LoopPrototype prototype, string loopTemplate, T model )
        {
            var result = string.Empty;
            foreach (var current in prototype.IterableObject)
            {
                _currentContext = new LoopContext( current, prototype.VariableName );
                result += Build( loopTemplate, model );
            }
            _currentContext = null;
            return result;
        }

        private string EvalueateIf<T>( IfPrototype prototype, string block, T model )
        {
            bool isEvaluateBlock;
            switch (prototype.Operator)
            {
                case "==":
                    isEvaluateBlock = prototype.Lhs == prototype.Rhs;
                    break;
                case "!=":
                    isEvaluateBlock = prototype.Lhs != prototype.Rhs;
                    break;
                default:
                    throw new Exception( $"{prototype.Operator} is not a valid operator." );
            }

            return isEvaluateBlock ? Build( block, model ) : string.Empty;
        }
    }
}
