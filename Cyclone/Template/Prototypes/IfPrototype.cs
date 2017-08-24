using Cyclone.Utils;

namespace Cyclone.Template.Prototypes
{
    internal class IfPrototype
    {
        private IfPrototype() { }

        public object Lhs { get; private set; }
        public string Operator { get; private set; }
        public object Rhs { get; private set; }

        internal static IfPrototype Parse<T>(SimpleLexer lexer, T model)
        {
            lexer.ConfirmCurrent("if");
            lexer.ConfirmNext("(");
            var lhs = lexer.Next();
            var op = lexer.Next();
            var rhs = lexer.Next();
            lexer.ConfirmNext(")");

            var lhsValue = TemplateBuilder.GetField(model, lhs);
            var rhsValue = rhs == "null" ? null : TemplateBuilder.GetField(model, rhs);
            return new IfPrototype { Lhs = lhsValue, Operator = op, Rhs = rhsValue };
        }

    }
}