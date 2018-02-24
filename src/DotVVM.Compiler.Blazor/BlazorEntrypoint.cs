using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Reflection;
using Microsoft.AspNetCore.Blazor.RenderTree;
using DotVVM.Framework.Blazor;
using Microsoft.AspNetCore.Blazor.Browser.Rendering;

namespace DotVVM.Compiler.Blazor
{
    class BlazorEntrypoint
    {
        // static CompilationUnitSyntax CreateProgramClass(NameSyntax ns, ExpressionSyntax launchMethod)
        // {
        //     return SyntaxFactory.CompilationUnit() .WithMembers( SyntaxFactory.SingletonList<MemberDeclarationSyntax>( SyntaxFactory.NamespaceDeclaration(ns) .WithMembers( SyntaxFactory.SingletonList<MemberDeclarationSyntax>( SyntaxFactory.ClassDeclaration("Program") .WithModifiers( SyntaxFactory.TokenList( SyntaxFactory.Token(SyntaxKind.PublicKeyword) ) ) .WithMembers( SyntaxFactory.SingletonList<MemberDeclarationSyntax>(  ) ) ) ) ) );
        // }

        static MethodInfo FindBlazorEntrypoint(Assembly assembly)
        {
            var mainType = assembly.GetType($"{assembly.GetName().Name}.BlazorProgram");
            return mainType?.GetMethod("Main", new [] { typeof(string[]) });
        }

        static BetterCodeEmitter CreateRootComponent(string ns, (string fileName, ViewCompilationResult compilationResult)[] pages)
        {
            var emitter = new BetterCodeEmitter();
            emitter.BaseType = typeof(DotvvmRootBlazorComponent);

            // emitter.PushNewMethod("BuildRenderTree", typeof(void),
            //     emitter.EmitParameter("builder", typeof(RenderTreeBuilder))
            // );
            // emitter.PopMethod();

            var pageRenders = pages.Select((p, index) => {
                var methodName = $"BuildRenderTree_{index}";
                emitter.PushNewStaticMethod(methodName, typeof(void),
                    emitter.EmitParameter("builder", typeof(RenderTreeBuilder))
                );
                emitter.EmitOpenBuilderComponent(SyntaxFactory.ParseTypeName($"global::{p.compilationResult.BuilderClassName}"));
                emitter.EmitCloseBuilderComponent();
                emitter.PopMethod();
                return methodName;
            }).ToArray();

            ExpressionSyntax createArrayItem(int index)
            {
                return emitter.CreateTuple(
                    typeof(FileNameAndRenderFunctionTuple),
                    emitter.EmitValue(pages[index].fileName),
                    emitter.CreateObjectExpression(typeof(RenderFunctionDelegate), new [] {
                        SyntaxFactory.IdentifierName(pageRenders[index])
                    })
                );
            }

            var baseCtorArgument = SyntaxFactory.ArrayCreationExpression(
                ((ArrayTypeSyntax)emitter.ParseTypeName(typeof(FileNameAndRenderFunctionTuple[]))),
                SyntaxFactory.InitializerExpression(SyntaxKind.ArrayInitializerExpression, SyntaxFactory.SeparatedList(
                    Enumerable.Range(0, pages.Length).Select(createArrayItem)
                ))
            );
            // var baseCtorArgument = SyntaxFactory.ParseExpression("null");
            emitter.AddClassMember(
                SyntaxFactory.ConstructorDeclaration("Program")
                .WithBody(SyntaxFactory.Block())
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithInitializer(
                    SyntaxFactory.ConstructorInitializer(
                        SyntaxKind.BaseConstructorInitializer,
                        SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                            SyntaxFactory.Argument(baseCtorArgument)
                        ))
                    )
                )
            );
            emitter.PushNewStaticMethod("Main", typeof(void), SyntaxFactory.Parameter(SyntaxFactory.Identifier("args")).WithType(emitter.ParseTypeName(typeof(string[]))));
            var renderer = emitter.EmitCreateObject(typeof(BrowserRenderer));
            emitter.EmitStatement(SyntaxFactory.ParseStatement($"{renderer}.AddComponent<Program>(\"app\");"));
            emitter.PopMethod();
            // emitter.AddClassMember(
            //     SyntaxFactory.MethodDeclaration( SyntaxFactory.PredefinedType( SyntaxFactory.Token(SyntaxKind.VoidKeyword) ), SyntaxFactory.Identifier("Main") ) .WithModifiers( SyntaxFactory.TokenList( SyntaxFactory.Token(SyntaxKind.PublicKeyword) ) ) .WithParameterList( SyntaxFactory.ParameterList( SyntaxFactory.SingletonSeparatedList<ParameterSyntax>( SyntaxFactory.Parameter( SyntaxFactory.Identifier("args") ) .WithType( SyntaxFactory.ArrayType( SyntaxFactory.PredefinedType( SyntaxFactory.Token(SyntaxKind.StringKeyword) ) ) .WithRankSpecifiers( SyntaxFactory.SingletonList<ArrayRankSpecifierSyntax>( SyntaxFactory.ArrayRankSpecifier( SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>( SyntaxFactory.OmittedArraySizeExpression() ) ) ) ) ) ) ) ) .WithBody( SyntaxFactory.Block( SyntaxFactory.SingletonList<StatementSyntax>( SyntaxFactory.ExpressionStatement( SyntaxFactory.InvocationExpression(launchMethod) .WithArgumentList( SyntaxFactory.ArgumentList( SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>( SyntaxFactory.Argument( SyntaxFactory.IdentifierName("args") ) ) ) ) ) ) ) ));

            return emitter;
        }

        public static BetterCodeEmitter CreateEntryPoint(
            string assemblyName,
            (string fileName, ViewCompilationResult)[] pages)
        {
            return CreateRootComponent(assemblyName, pages);
        }
    }
}
