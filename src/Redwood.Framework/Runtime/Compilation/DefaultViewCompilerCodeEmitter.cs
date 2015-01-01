using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Redwood.Framework.Controls;

namespace Redwood.Framework.Runtime.Compilation
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

        private Stack<EmitterMethodInfo> methods = new Stack<EmitterMethodInfo>();
        private List<EmitterMethodInfo> outputMethods = new List<EmitterMethodInfo>();
        public SyntaxTree SyntaxTree { get; private set; }


        private List<Type> usedControlBuilderTypes = new List<Type>();
        public List<Type> UsedControlBuilderTypes
        {
            get { return usedControlBuilderTypes; }
        }


        /// <summary>
        /// Emits the create object expression.
        /// </summary>
        public string EmitCreateObject(Type type, object[] constructorArguments = null)
        {
            if (constructorArguments == null)
            {
                constructorArguments = new object[] { };
            }

            var name = "c" + CurrentControlIndex;
            CurrentControlIndex++;

            CurrentStatements.Add(
                SyntaxFactory.LocalDeclarationStatement(
                    SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var")).WithVariables(
                        SyntaxFactory.VariableDeclarator(name).WithInitializer(
                            SyntaxFactory.EqualsValueClause(
                                SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName(type.FullName)).WithArgumentList(
                                    SyntaxFactory.ArgumentList(
                                        SyntaxFactory.SeparatedList(
                                            constructorArguments.Select(a => SyntaxFactory.Argument(EmitValue(a)))
                                        )
                                    )
                                )
                            )
                        )
                    )
                )
            );

            return name;
        }

        /// <summary>
        /// Emits the control builder invocation.
        /// </summary>
        public string EmitInvokeControlBuilder(Type controlType, Type controlBuilderType)
        {
            usedControlBuilderTypes.Add(controlBuilderType);

            var builderName = "c" + CurrentControlIndex + "_builder";
            var untypedName = "c" + CurrentControlIndex + "_untyped";
            var name = "c" + CurrentControlIndex;
            CurrentControlIndex++;

            CurrentStatements.Add(
                SyntaxFactory.LocalDeclarationStatement(
                    SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var")).WithVariables(
                        SyntaxFactory.VariableDeclarator(builderName).WithInitializer(
                            SyntaxFactory.EqualsValueClause(
                                SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName(controlBuilderType.FullName))
                                    .WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>()))
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
                                        SyntaxFactory.IdentifierName("BuildControl")
                                    )
                                ).WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>()))
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
            throw new NotSupportedException();
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
        public void EmitSetProperty(string controlName, string propertyName, string variableName)
        {
            var valueSyntax = SyntaxFactory.IdentifierName(variableName);
            EmitSetPropertyCore(controlName, propertyName, valueSyntax);
        }

        /// <summary>
        /// Emits the set property statement.
        /// </summary>
        public void EmitSetProperty(string controlName, string propertyName, ExpressionSyntax value)
        {
            EmitSetPropertyCore(controlName, propertyName, value);
        }

        private void EmitSetPropertyCore(string controlName, string propertyName, ExpressionSyntax valueSyntax)
        {
            CurrentStatements.Add(
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.BinaryExpression(
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
        /// Emits the set binding statement.
        /// </summary>
        public void EmitSetBinding(string controlName, Type controlType, string propertyName, string variableName)
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
                            SyntaxFactory.SeparatedList(
                                new[] { 
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.ParseTypeName(controlType.FullName),
                                            SyntaxFactory.IdentifierName(propertyName + "Property")
                                        )
                                    ),
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.IdentifierName(variableName)
                                    )
                                }
                            )
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
            EmitAddHtmlAttribute(controlName, name, EmitStringLiteral(value));
        }

        /// <summary>
        /// Emits the add HTML attribute.
        /// </summary>
        public void EmitAddHtmlAttribute(string controlName, string name, ExpressionSyntax valueSyntax)
        {
            CurrentStatements.Add(
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.BinaryExpression(
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
                    SyntaxFactory.BinaryExpression(
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
            var root = SyntaxFactory.CompilationUnit().WithMembers(
                SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName(namespaceName)).WithMembers(
                    SyntaxFactory.ClassDeclaration(className)
                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                        .WithBaseList(SyntaxFactory.BaseList(
                            SyntaxFactory.SeparatedList(new[] { SyntaxFactory.ParseTypeName(typeof(IControlBuilder).FullName) })
                        ))
                        .WithMembers(
                        SyntaxFactory.List<MemberDeclarationSyntax>(
                            outputMethods.Select(m => 
                                SyntaxFactory.MethodDeclaration(
                                    SyntaxFactory.ParseTypeName(typeof(RedwoodControl).FullName),
                                    m.Name
                                )
                                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                .WithBody(SyntaxFactory.Block(m.Statements))
                            )
                        )
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
        public void PushNewMethod(string name)
        {
            methods.Push(new EmitterMethodInfo()
            {
                Name = name
            });
        }

        /// <summary>
        /// Pops the method.
        /// </summary>
        public void PopMethod()
        {
            outputMethods.Add(methods.Pop());
        }

    }
}
