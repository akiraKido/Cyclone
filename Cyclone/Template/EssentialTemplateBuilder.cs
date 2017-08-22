using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Cyclone.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Emit;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Cyclone.Template
{
    public abstract class TemplateGenerator
    {
        public abstract string Generate<T>(T model);

        protected static object GetField<T>(T obj, string field)
        {
            var target = Expression.Parameter(typeof(object), "target");

            var lambda = _cache.GetOrAdd($"{nameof(T)}_{field}",
                Expression.Lambda<Func<object, object>>
                (
                    Expression.PropertyOrField
                    (
                        Expression.Convert(target, typeof(T)),
                        field
                    ),
                    target
                ).Compile());
            return lambda(obj);
        }
        private static readonly ConcurrentDictionary<string, Func<object, object>> _cache = new ConcurrentDictionary<string, Func<object, object>>();
    }

    public sealed class EssentialTemplateBuilder : ITemplateBuilder
    {
        internal static readonly EssentialTemplateBuilder Instance = new EssentialTemplateBuilder();
        private EssentialTemplateBuilder() { }

        private static readonly string AssemblyDirectory = Path.GetDirectoryName(typeof(object).Assembly.Location);
        private static readonly MetadataReference[] References = {
            MetadataReference.CreateFromFile( typeof(object).Assembly.Location ),
            MetadataReference.CreateFromFile( Path.Combine( AssemblyDirectory, "netstandard.dll" ) ),
            MetadataReference.CreateFromFile( Path.Combine( AssemblyDirectory, "System.dll" ) ),
            MetadataReference.CreateFromFile( Path.Combine( AssemblyDirectory, "System.Runtime.dll" ) ),
            MetadataReference.CreateFromFile( Path.Combine( AssemblyDirectory, "System.Core.dll" ) ),
            MetadataReference.CreateFromFile( typeof(TemplateGenerator).Assembly.Location ),
        };

        private static readonly object LockObject = new object();
        private static int index = 0;

        public string Build<T>(string template, T model)
        {
            var hash = template.GenerateHash();

            TemplateGenerator templateGenerator = Cache.GetOrAdd(hash, _ =>
            {
                lock (LockObject)
                {
                    var reader = new CharReader(template);

                    var workspace = new AdhocWorkspace();
                    var generator = SyntaxGenerator.GetGenerator(workspace, "C#");

                    var resultDeclaration = ParseStatement("string result = string.Empty;");

                    var statements = new List<SyntaxNode> {
                        resultDeclaration,
                    };

                    while (reader.HasNext)
                    {
                        var line = reader.TakeUntil((c, next) => c == '{' && (next == '{' || next == '%'));
                        line = line.Replace("\"", "\"\"");
                        statements.Add(ParseStatement($"result += @\"{line}\";"));
                        reader.SkipWhile(c => c == '{' || c == '%');
                        string code = reader.TakeUntil((c, next) => (c == '}' || c == '%') && next == '}').Trim();
                        if (!string.IsNullOrWhiteSpace(code))
                        {
                            statements.Add(ParseStatement($"result += GetField(model, \"{code}\");"));
                        }
                        reader.SkipWhile(c => c == '}' || c == '%');
                    }

                    statements.Add(ParseStatement("return result;"));

                    var generateMethod = generator.MethodDeclaration
                    (
                        "Generate",
                        typeParameters: new[] { "T" },
                        modifiers: DeclarationModifiers.Override,
                        parameters: new[] { generator.ParameterDeclaration("model", ParseTypeName("T")) },
                        returnType: ParseTypeName("string"),
                        accessibility: Accessibility.Public, statements: statements
                    );

                    var className = $"TemplateClass{index++}";

                    var classDefinition = generator.ClassDeclaration
                    (
                        className,
                        accessibility: Accessibility.Public,
                        interfaceTypes: new[] { ParseTypeName($"{nameof(Cyclone)}.{nameof(Template)}.{nameof(TemplateGenerator)}") },
                        members: new[] { generateMethod }
                    );


                    var nameSpace = generator.NamespaceDeclaration(nameof(Cyclone), classDefinition);

                    var node = generator.CompilationUnit(nameSpace).NormalizeWhitespace();

                    var compilation = CSharpCompilation.Create(
                        "InMemory",
                        new[] { node.SyntaxTree },
                        references: References,
                        options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, usings: new[] { "Cyclone.Template", "System" }));

                    using (var memoryStream = new MemoryStream())
                    {
                        EmitResult emitResult = compilation.Emit(memoryStream);
                        if (emitResult.Success)
                        {
                            memoryStream.Seek(0, SeekOrigin.Begin);
                            var asm = Assembly.Load(memoryStream.ToArray());
                            Type _class = asm.GetType(className);
                            TemplateGenerator g = (TemplateGenerator)Activator.CreateInstance(_class);
                            return g;
                        }

                        IEnumerable<Diagnostic> failures = emitResult.Diagnostics.Where(diagnostic =>
                            diagnostic.IsWarningAsError ||
                            diagnostic.Severity == DiagnosticSeverity.Error);

                        foreach (Diagnostic diagnostic in failures)
                        {
                            Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                        }
                        return null;
                    }
                }
            });

            return templateGenerator.Generate(new { hoge = "test" }); ;
        }

        private static readonly ConcurrentDictionary<string, TemplateGenerator> Cache = new ConcurrentDictionary<string, TemplateGenerator>();
    }
}
