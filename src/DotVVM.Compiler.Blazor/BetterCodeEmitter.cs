using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Framework.Compilation;
using Microsoft.AspNetCore.Blazor.RenderTree;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using DotVVM.Framework.Utils;
using Microsoft.CodeAnalysis;

namespace DotVVM.Compiler.Blazor
{
    public class BetterCodeEmitter : DefaultViewCompilerCodeEmitter
    {
        public ExpressionSyntax TranslateExpression(Expression expression, Func<ParameterExpression, ExpressionSyntax> assignParameter)
        {
            if (expression is ConstantExpression constant)
                return this.EmitValue(constant.Value);
            else if (expression is ParameterExpression p)
                return assignParameter(p);
            else if (expression is MemberExpression member)
                return SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, this.TranslateExpression(member.Expression, assignParameter), SyntaxFactory.IdentifierName(member.Member.Name));
            else if (expression.NodeType == ExpressionType.Convert)
                return SyntaxFactory.CastExpression(
                    this.ParseTypeName(expression.Type),
                    TranslateExpression(((UnaryExpression)expression).Operand, assignParameter));

            // TODO:  just maybe ... we will need a bit more complete translator
            throw new NotSupportedException($"{expression} of type {expression.Type} is not supported.");
        }

        public void InitializeDataContext(Type type)
        {
            this.otherDeclarations.Add(SyntaxFactory.PropertyDeclaration(ParseTypeName(type), SyntaxFactory.Identifier("DataContext")).WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword))).WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.List<AccessorDeclarationSyntax>(new AccessorDeclarationSyntax[] { SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)), SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)) }))));
        }

        protected override IEnumerable<BaseTypeSyntax> GetBuilderBaseTypes() => new BaseTypeSyntax[] {
            SyntaxFactory.SimpleBaseType(ParseTypeName(this.BaseType))
        };

        public Type BaseType { get; set; } = typeof(Microsoft.AspNetCore.Blazor.Components.IComponent);

        protected override IEnumerable<MemberDeclarationSyntax> GetDefaultMemberDeclarations() => new MemberDeclarationSyntax[] {
        };

        protected override ClassDeclarationSyntax ProcessViewBuilderClass(ClassDeclarationSyntax @class, string fileName) => @class;

        public ExpressionSyntax CreateTuple(Type tupleType, params ExpressionSyntax[] items) =>
            // SyntaxFactory.TupleExpression(SyntaxFactory.SeparatedList(
            //     items.Select(i => SyntaxFactory.Argument(i))
            // ));
            this.EmitCreateObjectExpression(
                ParseTypeName(tupleType),
                items
            );

        public void AddClassMember(MemberDeclarationSyntax member)
        {
            this.otherDeclarations.Add(member);
        }


        #region builder methods
        private int sequenceCounter;

        public void EmitAddBuilderAttribute(ExpressionSyntax name, ExpressionSyntax value)
        {
            sequenceCounter++;
            EmitStatement(
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("builder"),
                            SyntaxFactory.IdentifierName(nameof(RenderTreeBuilder.AddAttribute))
                        ),
                        SyntaxFactory.SeparatedList<ArgumentSyntax>(new ArgumentSyntax[]{
                            SyntaxFactory.Argument(this.EmitValue(sequenceCounter)),
                            SyntaxFactory.Argument(name),
                            SyntaxFactory.Argument(value),
                        }).Apply(SyntaxFactory.ArgumentList)
                    )
                )
            );
        }

        public void EmitOpenBuilderElement(ExpressionSyntax name)
        {
            sequenceCounter++;
            EmitStatement(
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("builder"),
                            SyntaxFactory.IdentifierName(nameof(RenderTreeBuilder.OpenElement))
                        ),
                        SyntaxFactory.SeparatedList<ArgumentSyntax>(new ArgumentSyntax[]{
                            SyntaxFactory.Argument(this.EmitValue(sequenceCounter)),
                            SyntaxFactory.Argument(name)
                        }).Apply(SyntaxFactory.ArgumentList)
                    )
                )
            );
        }

        public void EmitAddBuilderText(ExpressionSyntax text)
        {
            sequenceCounter++;
            EmitStatement(
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("builder"),
                            SyntaxFactory.IdentifierName(nameof(RenderTreeBuilder.AddText))
                        ),
                        SyntaxFactory.SeparatedList<ArgumentSyntax>(new ArgumentSyntax[]{
                            SyntaxFactory.Argument(this.EmitValue(sequenceCounter)),
                            SyntaxFactory.Argument(text)
                        }).Apply(SyntaxFactory.ArgumentList)
                    )
                )
            );
        }

        public void EmitOpenBuilderComponent(TypeSyntax type)
        {
            sequenceCounter++;
            EmitStatement(
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("builder"),
                            SyntaxFactory.GenericName(
                                SyntaxFactory.Identifier(nameof(RenderTreeBuilder.OpenComponent)),
                                SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList(type))
                            )
                        ),
                        SyntaxFactory.SeparatedList<ArgumentSyntax>(new ArgumentSyntax[]{
                            SyntaxFactory.Argument(this.EmitValue(sequenceCounter))
                        }).Apply(SyntaxFactory.ArgumentList)
                    )
                )
            );
        }

        public void EmitCloseBuilderComponent()
        {
            EmitStatement(
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("builder"),
                            SyntaxFactory.IdentifierName(nameof(RenderTreeBuilder.CloseComponent))
                        )
                    )
                )
            );
        }
        public void EmitCloseBuilderElement()
        {
            EmitStatement(
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("builder"),
                            SyntaxFactory.IdentifierName(nameof(RenderTreeBuilder.CloseElement))
                        )
                    )
                )
            );
        }
        #endregion
    }
}
