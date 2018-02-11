using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using Microsoft.CodeAnalysis.CSharp;
using System;
using DotVVM.Framework.Binding;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace DotVVM.Compiler
{
    public class CompileTimeCodeEmitter : DefaultViewCompilerCodeEmitter
    {
        private readonly RefObjectSerializer objSerializer;
        private readonly string serializerObjectName;

        public CompileTimeCodeEmitter(RefObjectSerializer objSerializer, string serializerObjectName)
        {
            this.objSerializer = objSerializer;
            this.serializerObjectName = serializerObjectName;
        }

        public override ExpressionSyntax EmitValueReference(object value)
        {
            var field = objSerializer.AddObject(value);
            return SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.ParseTypeName(serializerObjectName),
                SyntaxFactory.IdentifierName(field));
        }
    }
    internal class ViewCompilationResult
    {
        public string BuilderClassName { get; set; }
        public Type ControlType { get; set; }
        public Type DataContextType { get; set; }
        public ResolvedTreeRoot ResolvedTree { get; set; }
    }
}
