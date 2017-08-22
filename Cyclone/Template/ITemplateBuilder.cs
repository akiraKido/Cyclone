namespace Cyclone.Template
{
    public interface ITemplateBuilder
    {
        string Build<T>( string template, T model );
    }
}
