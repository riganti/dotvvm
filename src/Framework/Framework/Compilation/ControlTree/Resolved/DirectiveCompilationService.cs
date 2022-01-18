using System;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using DotVVM.Framework.Compilation.Binding;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public class DirectiveCompilationService
    {
        private readonly CompiledAssemblyCache compiledAssemblyCache;
        private readonly ExtensionMethodsCache extensionMethodsCache;

        public DirectiveCompilationService(CompiledAssemblyCache compiledAssemblyCache, ExtensionMethodsCache extensionMethodsCache)
        {
            this.compiledAssemblyCache = compiledAssemblyCache;
            this.extensionMethodsCache = extensionMethodsCache;
        }


        public ResolvedTypeDescriptor? ResolveType(DothtmlDirectiveNode directive, BindingParserNode nameSyntax)
        {
            if (CompileDirectiveExpression(directive, nameSyntax) is not StaticClassIdentifierExpression expression)
            {
                directive.AddError($"Could not resolve type '{nameSyntax.ToDisplayString()}'.");
                return null;
            }
            else return new ResolvedTypeDescriptor(expression.Type);
        }

        public Type? ResolveTypeOrNamespace(DothtmlDirectiveNode directive, BindingParserNode nameSyntax)
        {
            var expression = CompileDirectiveExpression(directive, nameSyntax);

            if (expression is UnknownStaticClassIdentifierExpression unknownStaticClassIdentifier)
            {
                var namespaceValid = compiledAssemblyCache.IsAssemblyNamespace(unknownStaticClassIdentifier.Name);

                if (!namespaceValid)
                {
                    directive.AddError($"{nameSyntax.ToDisplayString()} is unknown type or namespace.");
                }

                return null;

            }
            else if (expression is StaticClassIdentifierExpression)
            {
                return expression.Type;
            }

            directive.AddError($"{nameSyntax.ToDisplayString()} is not a type or namespace.");
            return null;
        }

        public object? ResolvePropertyInitializer(DothtmlDirectiveNode directive, Type? propertyType, BindingParserNode? initializer)
        {
            if (initializer == null) { return null; }

            var registry = TypeRegistry.DirectivesDefault(compiledAssemblyCache);

            var visitor = new ExpressionBuildingVisitor(registry, new MemberExpressionFactory(extensionMethodsCache)) {
                ResolveOnlyTypeName = false,
                Scope = null
            };

            var initializerExpression = visitor.Visit(initializer);

            var lambda = Expression.Lambda<Func<object?>>(Expression.Convert(Expression.Block(initializerExpression), typeof(object)));
            var lambdaDelegate = lambda.Compile(true);

            return lambdaDelegate.Invoke() ?? CreateDefaultValue(propertyType);
        }

        private object? CreatePropertyInitializerValue(DothtmlDirectiveNode directive, Type? propertyType, LiteralExpressionBindingParserNode? initializer)
        {
            if (initializer?.Value == null || propertyType == null) { return null; }

            var originalLiteralType = initializer.Value.GetType();

            if (originalLiteralType != typeof(string)) { return initializer.Value; }

            var initializerValueString = initializer.Value.ToString();

            if (propertyType == typeof(char))
            {
                if (initializerValueString.Length != 1)
                {
                    directive.AddError($"Could not convert \"{initializerValueString}\" to char when initializing property {directive.Name}.");
                    return default(char);
                }

                return initializerValueString.Single();
            }

            if (propertyType == typeof(Guid))
            {
                if (Guid.TryParse(initializerValueString, out var guid))
                {
                    return guid;
                }
                directive.AddError($"Could not convert \"{initializerValueString}\" to Guid when initializing property {directive.Name}.");
                return default(Guid);
            }

            return initializerValueString;
        }

        private object? CreateDefaultValue(Type? type)
        {
            if (type != null && type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        private Expression? CompileDirectiveExpression(DothtmlDirectiveNode directive, BindingParserNode expressionSyntax)
        {
            TypeRegistry registry;
            if (expressionSyntax is TypeOrFunctionReferenceBindingParserNode typeOrFunction)
                expressionSyntax = typeOrFunction.ToTypeReference();

            if (expressionSyntax is AssemblyQualifiedNameBindingParserNode assemblyQualifiedName)
            {
                registry = TypeRegistry.DirectivesDefault(compiledAssemblyCache, assemblyQualifiedName.AssemblyName.ToDisplayString());
            }
            else
            {
                registry = TypeRegistry.DirectivesDefault(compiledAssemblyCache);
            }

            var visitor = new ExpressionBuildingVisitor(registry, new MemberExpressionFactory(extensionMethodsCache)) {
                ResolveOnlyTypeName = true,
                Scope = null
            };

            try
            {
                return visitor.Visit(expressionSyntax);
            }
            catch (Exception ex)
            {
                directive.AddError($"{expressionSyntax.ToDisplayString()} is not a valid type or namespace: {ex.Message}");
                return null;
            }
        }
    }
}
