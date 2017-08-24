namespace Cyclone.Template.Contexts
{
    internal class LoopContext : Context
    {
        public LoopContext( object current, string variableName )
        {
            Current = current;
            VariableName = variableName;
        }

        public object Current { get; }
        public string VariableName { get; }
    }
}