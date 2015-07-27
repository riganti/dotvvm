using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using DotVVM.Framework.Controls;
using System.Collections;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Runtime.Compilation
{
    public class DefaultViewCompilerCodeEmitter
    {

        private int CurrentControlIndex
        {
            get { return methods.Peek().ControlIndex; }
            set { methods.Peek().ControlIndex = value; }
        }

        private List<StatementSyntax> CurrentStatements
        {
            get { return methods.Peek().Statements; }
        }

        public const string ControlBuilderFactoryParameterName = "controlBuilderFactory";
        public const string BuildControlFunctionName = nameof(IControlBuilder.BuildControl);
        public const string BuildTemplateFunctionName = "BuildTemplate";
        public const string GetControlBuilderFunctionName = nameof(IControlBuilderFactory.GetControlBuilder);
        public const string DataContextTypePropertyName = nameof(IControlBuilder.DataContextType);


        private Stack<EmitterMethodInfo> methods = new Stack<EmitterMethodInfo>();
        private List<EmitterMethodInfo> outputMethods = new List<EmitterMethodInfo>();
        public SyntaxTree SyntaxTree { get; private set; }
        public Type BuilderDataContextType { get; set; }


        private List<Type> usedControlBuilderTypes = new List<Type>();
        public List<Type> UsedControlBuilderTypes
        {
            get { return usedControlBuilderTypes; }
        }

        private HashSet<Assembly> usedAssemblies = new HashSet<Assembly>();
        public HashSet<Assembly> UsedAssemblies
        {
            get { return usedAssemblies; }
        }


        private List<ClassDeclarationSyntax> otherClassDeclarations = new List<ClassDeclarationSyntax>();

        public string EmitCreateVariable(ExpressionSyntax expression)
        {
            var name = "c" + CurrentControlIndex;
            CurrentControlIndex++;

            CurrentStatements.Add(
                SyntaxFactory.LocalDeclarationStatement(
                    SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var")).WithVariables(
                        SyntaxFactory.VariableDeclarator(name).WithInitializer(
                            SyntaxFactory.EqualsValueClause(expression)
                        )
                    )
                )
            );
            return name;
        }

        /// <summary>
        /// Emits the create object expression.
        /// </summary>
        public string EmitCreateObject(string typeName, object[] constructorArguments = null)
        {
            if (constructorArguments == null)
            {
                constructorArguments = new object[] { };
            }

            return EmitCreateVariable(
                SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName(typeName)).WithArgumentList(
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList(
                            constructorArguments.Select(a => SyntaxFactory.Argument(EmitValue(a)))
                        )
                    )
                )
            );
        }

        public ExpressionSyntax CreateObject(string typeName, IEnumerable<ExpressionSyntax> arguments)
        {
            return SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName(typeName)).WithArgumentList(
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList(
                        arguments.Select(a => SyntaxFactory.Argument(a))
                    )
                )
            );
        }

        public ExpressionSyntax CreateObject(Type type, IEnumerable<ExpressionSyntax> arguments)
        {
            UsedAssemblies.Add(type.Assembly);
            return CreateObject(type.FullName, arguments);
        }


        /// <summary>
        /// Emits the create object expression.
        /// </summary>
        public string EmitCreateObject(Type type, object[] constructorArguments = null)
        {
            usedAssemblies.Add(type.Assembly);
            return EmitCreateObject(type.FullName, constructorArguments);
        }

        /// <summary>
        /// Emits the control builder invocation.
        /// </summary>
        public string EmitInvokeControlBuilder(Type controlType, string virtualPath)
        {
            usedControlBuilderTypes.Add(controlType);

            var builderName = "c" + CurrentControlIndex + "_builder";
            var untypedName = "c" + CurrentControlIndex + "_untyped";
            var name = "c" + CurrentControlIndex;
            CurrentControlIndex++;

            CurrentStatements.Add(
                SyntaxFactory.LocalDeclarationStatement(
                    SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var")).WithVariables(
                        SyntaxFactory.VariableDeclarator(builderName).WithInitializer(
                            SyntaxFactory.EqualsValueClause(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.IdentifierName(ControlBuilderFactoryParameterName),
                                        SyntaxFactory.IdentifierName(GetControlBuilderFunctionName)
                                    ),
                                    SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[] {
                                        SyntaxFactory.Argument(EmitStringLiteral(virtualPath))
                                    }))
                                )
                            )
                        )
                    )
                )
            );
            CurrentStatements.Add(
                SyntaxFactory.LocalDeclarationStatement(
                    SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var")).WithVariables(
                        SyntaxFactory.VariableDeclarator(untypedName).WithInitializer(
                            SyntaxFactory.EqualsValueClause(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.IdentifierName(builderName),
                                        SyntaxFactory.IdentifierName(BuildControlFunctionName)
                                    ),
                                    SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[] {
                                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName(ControlBuilderFactoryParameterName))
                                    }))
                                )
                            )
                        )
                    )
                )
            );
            CurrentStatements.Add(
                SyntaxFactory.LocalDeclarationStatement(
                    SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var")).WithVariables(
                        SyntaxFactory.VariableDeclarator(name).WithInitializer(
                            SyntaxFactory.EqualsValueClause(
                                SyntaxFactory.CastExpression(SyntaxFactory.ParseTypeName(controlType.FullName),
                                    SyntaxFactory.IdentifierName(untypedName)
                                )
                            )
                        )
                    )
                )
            );

            return name;
        }

        /// <summary>
        /// Emits the value.
        /// </summary>
        public ExpressionSyntax EmitValue(object value)
        {
            if (value == null)
            {
                return EmitIdentifier("null");
            }
            if (value is string)
            {
                return EmitStringLiteral((string)value);
            }
            if (value is bool)
            {
                return EmitBooleanLiteral((bool)value);
            }
            if (value is int)
            {
                return EmitIntegerLiteral((int)value);
            }

            var type = value.GetType();
            if (type.IsEnum)
            {
                return
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.ParseTypeName(type.FullName),
                        SyntaxFactory.IdentifierName(value.ToString())
                    );
            }
            if (type.IsArray || typeof(IEnumerable).IsAssignableFrom(type))
            {
                return EmitCreateArray(ReflectionUtils.GetEnumerableType(type), (IEnumerable)value);
            }
            throw new NotSupportedException();
        }

        public ExpressionSyntax EmitCreateArray(Type arrayType, IEnumerable values)
        {
            return SyntaxFactory.ArrayCreationExpression(
                                    SyntaxFactory.ArrayType(
                                        SyntaxFactory.ParseTypeName(arrayType.FullName),
                                        SyntaxFactory.SingletonList(
                                            SyntaxFactory.ArrayRankSpecifier(
                                                SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                                    SyntaxFactory.OmittedArraySizeExpression())))),
                                    SyntaxFactory.InitializerExpression(
                                        SyntaxKind.ArrayInitializerExpression,
                                        SyntaxFactory.SeparatedList(
                                            values.Cast<object>().Select(EmitValue))));
        }

        /// <summary>
        /// Emits the boolean literal.
        /// </summary>
        private ExpressionSyntax EmitBooleanLiteral(bool value)
        {
            return SyntaxFactory.LiteralExpression(value ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression);
        }

        /// <summary>
        /// Emits the set property statement.
        /// </summary>
        public void EmitSetProperty(string controlName, string propertyName, ExpressionSyntax valueSyntax)
        {
            CurrentStatements.Add(
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(controlName),
                            SyntaxFactory.IdentifierName(propertyName)
                        ),
                    valueSyntax
                    )
                )
            );
        }

        /// <summary>
        /// Emits the set property statement.
        /// </summary>
        public void EmitSetValue(string controlName, string propertyName, string variableName)
        {
            var valueSyntax = SyntaxFactory.IdentifierName(variableName);
            EmitSetValue(controlName, propertyName, valueSyntax);
        }

        /// <summary>
        /// Emits the set property statement.
        /// </summary>
        public void EmitSetValue(string controlName, string propertyName, ExpressionSyntax valueSyntax)
        {
            CurrentStatements.Add(
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(controlName),
                            SyntaxFactory.IdentifierName("SetValue")
                        ),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList(new[]
                            {
                                SyntaxFactory.Argument(SyntaxFactory.ParseName(propertyName)),
                                SyntaxFactory.Argument(valueSyntax)
                            })
                        )
                    )
                )
            );
        }

        /// <summary>
        /// Emits the set binding statement.
        /// </summary>
        public void EmitSetBinding(string controlName, string propertyName, ExpressionSyntax binding)
        {
            CurrentStatements.Add(
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(controlName),
                            SyntaxFactory.IdentifierName("SetBinding")
                        ),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList(new[] {
                                SyntaxFactory.Argument(SyntaxFactory.IdentifierName(propertyName)),
                                SyntaxFactory.Argument(binding)
                            })
                        )
                    )
                )
            );
        }

        /// <summary>
        /// Emits the set attached property.
        /// </summary>
        public void EmitSetAttachedProperty(string controlName, string propertyType, string propertyName, object value)
        {
            CurrentStatements.Add(
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(controlName),
                            SyntaxFactory.IdentifierName("SetValue")
                        ),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList(
                                new[] {
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.ParseTypeName(propertyType),
                                            SyntaxFactory.IdentifierName(propertyName + "Property")
                                        )
                                    ),
                                    SyntaxFactory.Argument(
                                        EmitValue(value)
                                    )
                                }
                            )
                        )
                    )
                )
            );
        }

        /// <summary>
        /// Emits the code that adds the specified value as a child item in the collection.
        /// </summary>
        public void EmitAddCollectionItem(string controlName, string variableName, string collectionPropertyName = "Children")
        {
            CurrentStatements.Add(
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName(controlName),
                                SyntaxFactory.IdentifierName(collectionPropertyName)
                            ),
                            SyntaxFactory.IdentifierName("Add")
                        )
                    ).WithArgumentList(
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList(
                                new[]
                                {
                                    SyntaxFactory.Argument(SyntaxFactory.IdentifierName(variableName))
                                }
                            )
                        )
                    )
                )
            );
        }

        /// <summary>
        /// Emits the add HTML attribute.
        /// </summary>
        public void EmitAddHtmlAttribute(string controlName, string name, string value)
        {
            EmitAddHtmlAttribute(controlName, name, EmitValue(value));
        }

        /// <summary>
        /// Emits the add HTML attribute.
        /// </summary>
        public void EmitAddHtmlAttribute(string controlName, string name, ExpressionSyntax valueSyntax)
        {
            CurrentStatements.Add(
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.ElementAccessExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName(controlName),
                                SyntaxFactory.IdentifierName("Attributes")
                            ),
                            SyntaxFactory.BracketedArgumentList(
                                SyntaxFactory.SeparatedList(
                                    new[]
                                    {
                                        SyntaxFactory.Argument(EmitStringLiteral(name))
                                    }
                                )
                            )
                        ),
                        valueSyntax
                    )
                )
            );
        }

        private LiteralExpressionSyntax EmitStringLiteral(string value)
        {
            return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(value));
        }

        private LiteralExpressionSyntax EmitIntegerLiteral(int value)
        {
            return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(value));
        }

        /// <summary>
        /// Emits the identifier.
        /// </summary>
        public NameSyntax EmitIdentifier(string identifier)
        {
            return SyntaxFactory.IdentifierName(identifier);
        }

        /// <summary>
        /// Emits the add directive.
        /// </summary>
        public void EmitAddDirective(string controlName, string name, string value)
        {
            CurrentStatements.Add(
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.ElementAccessExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName(controlName),
                                SyntaxFactory.IdentifierName("Directives")
                            ),
                            SyntaxFactory.BracketedArgumentList(
                                SyntaxFactory.SeparatedList(
                                    new[]
                                    {
                                        SyntaxFactory.Argument(EmitStringLiteral(name))
                                    }
                                )
                            )
                        ),
                        EmitStringLiteral(value)
                    )
                )
            );
        }

        /// <summary>
        /// Emits the return clause.
        /// </summary>
        public void EmitReturnClause(string variableName)
        {
            CurrentStatements.Add(SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName(variableName)));
        }

        /// <summary>
        /// Gets the result syntax tree.
        /// </summary>
        public IEnumerable<SyntaxTree> BuildTree(string namespaceName, string className)
        {
            UsedAssemblies.Add(BuilderDataContextType.Assembly);
            var root = SyntaxFactory.CompilationUnit().WithMembers(
                SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName(namespaceName)).WithMembers(
                    SyntaxFactory.List<MemberDeclarationSyntax>(
                        otherClassDeclarations.Concat(new[]
                        {
                            SyntaxFactory.ClassDeclaration(className)
                                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                .WithBaseList(SyntaxFactory.BaseList(
                                    SyntaxFactory.SeparatedList(new BaseTypeSyntax[]
                                    {
                                        SyntaxFactory.SimpleBaseType(
                                            SyntaxFactory.ParseTypeName(typeof(IControlBuilder).FullName)
                                        )
                                    })
                                ))
                                .WithMembers(
                                SyntaxFactory.List<MemberDeclarationSyntax>(
                                    outputMethods.Select<EmitterMethodInfo, MemberDeclarationSyntax>(m =>
                                        SyntaxFactory.MethodDeclaration(
                                            SyntaxFactory.ParseTypeName(typeof(DotvvmControl).FullName),
                                            m.Name)
                                            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                            .WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(new[] {
                                                SyntaxFactory.Parameter(
                                                    SyntaxFactory.Identifier(ControlBuilderFactoryParameterName)
                                                )
                                                .WithType(SyntaxFactory.ParseTypeName(typeof(IControlBuilderFactory).FullName))
                                            })))
                                            .WithBody(SyntaxFactory.Block(m.Statements))
                                        ).Concat(new [] { SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("System.Type"), nameof(IControlBuilder.DataContextType))
                                            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                            .WithExpressionBody(
                                                SyntaxFactory.ArrowExpressionClause(SyntaxFactory.TypeOfExpression(SyntaxFactory.ParseTypeName(BuilderDataContextType.FullName))))
                                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                                        })
                                    )
                                )
                        })
                    )
                )
            ).NormalizeWhitespace();

            // WORKAROUND: serializing and parsing the tree is necessary here because Roslyn throws compilation errors when pass the original tree which uses markup controls (they reference in-memory assemblies)
            // the trees are the same (root2.GetChanges(root) returns empty collection) but without serialization and parsing it does not work
            SyntaxTree = CSharpSyntaxTree.ParseText(root.ToString());
            return new[] { SyntaxTree };
        }


        /// <summary>
        /// Pushes the new method.
        /// </summary>
        public void PushNewMethod(string name, params ParameterSyntax[] parameters)
        {
            var emitterMethodInfo = new EmitterMethodInfo() { Name = name };
            methods.Push(emitterMethodInfo);
        }

        /// <summary>
        /// Pops the method.
        /// </summary>
        public void PopMethod()
        {
            outputMethods.Add(methods.Pop());
        }


        /// <summary>
        /// Emits the control class.
        /// </summary>
        public void EmitControlClass(Type baseType, string className)
        {
            otherClassDeclarations.Add(
                SyntaxFactory.ClassDeclaration(className)
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                    .WithBaseList(SyntaxFactory.BaseList(SyntaxFactory.SeparatedList<BaseTypeSyntax>(new[]
                            {
                        SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(baseType.ToString()))
                            })))
                );
        }
    }
}
