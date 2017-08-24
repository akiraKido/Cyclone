using System;
using System.Collections;
using Cyclone.Utils;

namespace Cyclone.Template.Prototypes
{
    internal struct LoopPrototype
    {
        public IEnumerable IterableObject { get; private set; }
        public string VariableName { get; private set; }

        internal static LoopPrototype Parse<T>( SimpleLexer lexer, T model )
        {
            lexer.ConfirmCurrent( "foreach" );
            lexer.ConfirmNext( "(" );
            lexer.ConfirmNext( "var" );
            string variableName = lexer.Next();
            lexer.ConfirmNext( "in" );
            string fieldName = lexer.Next();
            lexer.ConfirmNext( ")" );

            object iterableObject = TemplateBuilder.GetField( model, fieldName );
            if (!(iterableObject is IEnumerable))
            {
                throw new Exception( $"{fieldName} is not iterable." );
            }

            return new LoopPrototype
            {
                VariableName = variableName,
                IterableObject = (IEnumerable) iterableObject
            };
        }
    }
}