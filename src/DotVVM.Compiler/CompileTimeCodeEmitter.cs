using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation;
using Microsoft.CodeAnalysis.CSharp;
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
}
