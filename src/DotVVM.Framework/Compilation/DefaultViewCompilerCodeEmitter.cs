using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotVVM.Framework.Compilation
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
        public string ResultControlType { get; set; }

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
        public string EmitCreateObject(Type type, object[] constructorArguments = null)
        {
            if (constructorArguments == null)
            {
                constructorArguments = new object[] { };
            }

            UsedAssemblies.Add(type.GetTypeInfo().Assembly);
            return EmitCreateObject(ParseTypeName(type), constructorArguments.Select(EmitValue));
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

            return EmitCreateObject(SyntaxFactory.ParseTypeName(typeName), constructorArguments.Select(EmitValue));
        }


        private string EmitCreateObject(TypeSyntax type, IEnumerable<ExpressionSyntax> arguments)
        {
            return EmitCreateVariable(
                EmitCreateObjectExpression(type, arguments)
            );
        }

        public ExpressionSyntax CreateObjectExpression(Type type, IEnumerable<ExpressionSyntax> arguments)
        {
            return EmitCreateObjectExpression(ParseTypeName(type), arguments);
        }

        private ExpressionSyntax EmitCreateObjectExpression(TypeSyntax type, IEnumerable<ExpressionSyntax> arguments)
        {
            return SyntaxFactory.ObjectCreationExpression(type).WithArgumentList(
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList(
                        arguments.Select(SyntaxFactory.Argument)
                    )
                )
            );
        }

        public ExpressionSyntax EmitAttributeInitializer(CustomAttributeData attr)
        {
            UsedAssemblies.Add(attr.AttributeType.GetTypeInfo().Assembly);
            return SyntaxFactory.ObjectCreationExpression(
                ParseTypeName(attr.AttributeType),
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList(
                        attr.ConstructorArguments.Select(a => SyntaxFactory.Argument(EmitValue(a.Value)))
                    )
                ),
                SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression,
                    SyntaxFactory.SeparatedList(
                        attr.NamedArguments.Select(np =>
                             (ExpressionSyntax)SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName(np.MemberName),
                                EmitValue(np.TypedValue.Value)
                            )
                        )
                    )
                )
            );
        }

        /// <summary>
        /// Emits the control builder invocation.
        /// </summary>
        public string EmitInvokeControlBuilder(Type controlType, string virtualPath)
        {
            UsedAssemblies.Add(controlType.GetTypeInfo().Assembly);

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
                                SyntaxFactory.CastExpression(ParseTypeName(controlType),
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
                return SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
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
                return EmitStandardNumericLiteral((int)value);
            }
            if (value is long)
            {
                return EmitStandardNumericLiteral((long)value);
            }
            if (value is ulong)
            {
                return EmitStandardNumericLiteral((ulong)value);
            }
            if (value is uint)
            {
                return EmitStandardNumericLiteral((uint)value);
            }
            if (value is decimal)
            {
                return EmitStandardNumericLiteral((decimal)value);
            }
            if (value is float)
            {
                return EmitStandardNumericLiteral((float)value);
            }
            if (value is double)
            {
                return EmitStandardNumericLiteral((double)value);
            }
            if (value is Type)
            {
                UsedAssemblies.Add((value as Type).GetTypeInfo().Assembly);
                return SyntaxFactory.TypeOfExpression(ParseTypeName((value as Type)));
            }

            var type = value.GetType();


            if (ReflectionUtils.IsNumericType(type))
            {
                return EmitStrangeIntegerValue(Convert.ToInt64(value), type);
            }

            if (type.GetTypeInfo().IsEnum)
            {
                UsedAssemblies.Add(type.GetTypeInfo().Assembly);
                return
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ParseTypeName(type),
                        SyntaxFactory.IdentifierName(value.ToString())
                    );
            }
            if (type.IsArray || typeof(IEnumerable).IsAssignableFrom(type))
            {
                return EmitCreateArray(ReflectionUtils.GetEnumerableType(type), (IEnumerable)value);
            }
            throw new NotSupportedException($"Emiting value of type '{value.GetType().FullName}' is not supported.");
        }

        public ExpressionSyntax EmitCreateArray(Type arrayType, IEnumerable values)
        {
            return SyntaxFactory.ArrayCreationExpression(
                                    SyntaxFactory.ArrayType(
                                        ParseTypeName(arrayType),
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
        /// Emits the code that adds the specified value as a child item in the collection.
        /// </summary>
        public void EmitAddCollectionItem(string controlName, string variableName, string collectionPropertyName = "Children")
        {
            ExpressionSyntax collectionExpression;
            if (string.IsNullOrEmpty(collectionPropertyName))
            {
                collectionExpression = SyntaxFactory.IdentifierName(controlName);
            }
            else
            {
                collectionExpression = SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName(controlName),
                    SyntaxFactory.IdentifierName(collectionPropertyName)
                );
            }

            CurrentStatements.Add(
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            collectionExpression,
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

        public void EmitSetDotvvmProperty(string controlName, DotvvmProperty property, object value) =>
            EmitSetDotvvmProperty(controlName, property, EmitValue(value));
        public void EmitSetDotvvmProperty(string controlName, DotvvmProperty property, ExpressionSyntax value)
        {
            UsedAssemblies.Add(property.DeclaringType.GetTypeInfo().Assembly);
            UsedAssemblies.Add(property.PropertyType.GetTypeInfo().Assembly);

            if (property.IsVirtual)
            {
                EmitSetProperty(controlName, property.PropertyInfo.Name, EmitValue(value));
            }
            else
            {
                CurrentStatements.Add(
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.ParseName(property.DescriptorFullName),
                                SyntaxFactory.IdentifierName("SetValue")
                            ),
                            SyntaxFactory.ArgumentList(
                                SyntaxFactory.SeparatedList(
                                    new[] {
                                    SyntaxFactory.Argument(SyntaxFactory.IdentifierName(controlName)),
                                    SyntaxFactory.Argument(value)
                                    }
                                )
                            )
                        )
                    )
                );
            }
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

        private LiteralExpressionSyntax EmitStandardNumericLiteral(int value)
        {
            return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));
        }

        private LiteralExpressionSyntax EmitStandardNumericLiteral(long value)
        {
            return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));
        }

        private LiteralExpressionSyntax EmitStandardNumericLiteral(ulong value)
        {
            return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));
        }

        private LiteralExpressionSyntax EmitStandardNumericLiteral(uint value)
        {
            return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));
        }

        private LiteralExpressionSyntax EmitStandardNumericLiteral(decimal value)
        {
            return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));
        }

        private LiteralExpressionSyntax EmitStandardNumericLiteral(float value)
        {
            return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));
        }

        private LiteralExpressionSyntax EmitStandardNumericLiteral(double value)
        {
            return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));
        }

        private ExpressionSyntax EmitStrangeIntegerValue(long value, Type type)
        {
            return SyntaxFactory.CastExpression(this.ParseTypeName(type), SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value)));
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

        public string EmitEnsureCollectionInitialized(string parentName, DotvvmProperty property)
        {
            UsedAssemblies.Add(property.PropertyType.GetTypeInfo().Assembly);

            if (property.IsVirtual)
            {
                StatementSyntax initializer;
                if (property.PropertyInfo.SetMethod != null)
                {
                    initializer = SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.IdentifierName(parentName),
                                    SyntaxFactory.IdentifierName(property.Name)
                                ),
                                SyntaxFactory.ObjectCreationExpression(ParseTypeName(property.PropertyType))
                                    .WithArgumentList(
                                        SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new ArgumentSyntax[] { }))
                                    )
                            )
                        );
                }
                else
                {
                    initializer = SyntaxFactory.ThrowStatement(
                        CreateObjectExpression(typeof(InvalidOperationException),
                            new[] { EmitStringLiteral($"Property '{ property.FullName }' can't be used as control collection since it is not initialized and does not have setter available for automatic initialization") }
                        )
                    );
                }
                CurrentStatements.Add(
                    SyntaxFactory.IfStatement(
                        SyntaxFactory.BinaryExpression(
                            SyntaxKind.EqualsExpression,
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName(parentName),
                                SyntaxFactory.IdentifierName(property.Name)
                            ),
                            SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
                        ),
                        initializer
                    )
                );

                return EmitCreateVariable(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(parentName),
                        SyntaxFactory.IdentifierName(property.Name)
                    )
                );
            }
            else
            {
                CurrentStatements.Add(
                    SyntaxFactory.IfStatement(
                        SyntaxFactory.BinaryExpression(
                            SyntaxKind.EqualsExpression,
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.IdentifierName(parentName),
                                    SyntaxFactory.IdentifierName("GetValue")
                                ),
                                SyntaxFactory.ArgumentList(
                                    SyntaxFactory.SeparatedList(new[]
                                    {
                                        SyntaxFactory.Argument(SyntaxFactory.ParseName(property.DescriptorFullName))
                                    })
                                )
                            ),
                            SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
                        ),
                        SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.IdentifierName(parentName),
                                    SyntaxFactory.IdentifierName("SetValue")
                                ),
                                SyntaxFactory.ArgumentList(
                                    SyntaxFactory.SeparatedList(new[]
                                    {
                                        SyntaxFactory.Argument(SyntaxFactory.ParseName(property.DescriptorFullName)),
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.ObjectCreationExpression(ParseTypeName(property.PropertyType))
                                                .WithArgumentList(
                                                    SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new ArgumentSyntax[] { }))
                                                )
                                        )
                                    })
                                )
                            )
                        )
                    )
                );
                return EmitCreateVariable(
                    SyntaxFactory.CastExpression(
                        ParseTypeName(property.PropertyType),
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName(parentName),
                                SyntaxFactory.IdentifierName("GetValue")
                            ),
                            SyntaxFactory.ArgumentList(
                                SyntaxFactory.SeparatedList(new[]
                                {
                                    SyntaxFactory.Argument(SyntaxFactory.ParseName(property.DescriptorFullName))
                                })
                            )
                        )
                    )
                );
            }
        }

        private TypeSyntax ParseTypeName(Type type)
        {
            if(type == typeof(void))
            {
                return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword));
            }
            else if (!type.GetTypeInfo().IsGenericType)
            {
                return SyntaxFactory.ParseTypeName(type.FullName);
            }
            else
            {
                var fullName = type.GetGenericTypeDefinition().FullName;
                if (fullName.Contains("`"))
                {
                    fullName = fullName.Substring(0, fullName.IndexOf("`"));
                }

                var parts = fullName.Split('.');
                NameSyntax identifier = SyntaxFactory.IdentifierName(parts[0]);
                for (var i = 1; i < parts.Length - 1; i++)
                {
                    identifier = SyntaxFactory.QualifiedName(identifier, SyntaxFactory.IdentifierName(parts[i]));
                }

                var typeArguments = type.GetGenericArguments().Select(ParseTypeName);
                return SyntaxFactory.QualifiedName(identifier,
                    SyntaxFactory.GenericName(
                        SyntaxFactory.Identifier(parts[parts.Length - 1]),
                        SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SeparatedList(typeArguments.ToArray())
                        )
                    )
                );
            }
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
        public IEnumerable<SyntaxTree> BuildTree(string namespaceName, string className, string fileName)
        {
            UsedAssemblies.Add(BuilderDataContextType.GetTypeInfo().Assembly);

            var root = SyntaxFactory.CompilationUnit().WithMembers(
                SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(namespaceName)).WithMembers(
                    SyntaxFactory.List<MemberDeclarationSyntax>(
                        otherClassDeclarations.Concat(new[]
                        {
                            SyntaxFactory.ClassDeclaration(className)
                                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                .WithBaseList(SyntaxFactory.BaseList(
                                    SyntaxFactory.SeparatedList(new BaseTypeSyntax[]
                                    {
                                        SyntaxFactory.SimpleBaseType(
                                            ParseTypeName(typeof(IControlBuilder))
                                        )
                                    })
                                ))
                                .AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(new [] {
                                        SyntaxFactory.Attribute(
                                            SyntaxFactory.ParseName(typeof(LoadControlBuilderAttribute).FullName),
                                            SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(new [] {
                                                SyntaxFactory.AttributeArgument(EmitStringLiteral(fileName))
                                            }))
                                        )
                                    })))
                                .WithMembers(
                                SyntaxFactory.List<MemberDeclarationSyntax>(
                                    outputMethods.Select<EmitterMethodInfo, MemberDeclarationSyntax>(m =>
                                        SyntaxFactory.MethodDeclaration(
                                            m.ReturnType,
                                            m.Name)
                                            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                            .WithParameterList(m.Parameters)
                                            .WithBody(SyntaxFactory.Block(m.Statements))
                                        ).Concat(new [] {
                                            SyntaxFactory.PropertyDeclaration(ParseTypeName(typeof(Type)), nameof(IControlBuilder.DataContextType))
                                                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                                .WithExpressionBody(
                                                    SyntaxFactory.ArrowExpressionClause(SyntaxFactory.TypeOfExpression(ParseTypeName(BuilderDataContextType))))
                                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                                            SyntaxFactory.PropertyDeclaration(ParseTypeName(typeof(Type)), nameof(IControlBuilder.ControlType))
                                                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                                .WithExpressionBody(
                                                    SyntaxFactory.ArrowExpressionClause(SyntaxFactory.TypeOfExpression(SyntaxFactory.ParseTypeName(ResultControlType))))
                                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                                        })
                                    )
                                )
                        })
                    )
                )
            );

            // WORKAROUND: serializing and parsing the tree is necessary here because Roslyn throws compilation errors when pass the original tree which uses markup controls (they reference in-memory assemblies)
            // the trees are the same (root2.GetChanges(root) returns empty collection) but without serialization and parsing it does not work
            //SyntaxTree = CSharpSyntaxTree.ParseText(root.ToString());
            //SyntaxTree = root.SyntaxTree;
            return new[] { root.SyntaxTree };
        }


        public ParameterSyntax EmitParameter(string name, Type type)
        =>
            SyntaxFactory.Parameter(
                SyntaxFactory.Identifier(name)
            )
            .WithType(ParseTypeName(type));

        public ParameterSyntax EmitControlBuilderParameter()
            => EmitParameter(ControlBuilderFactoryParameterName, typeof(IControlBuilderFactory));

        /// <summary>
        /// Pushes the new method.
        /// </summary>
        public void PushNewMethod(string name, Type returnType, params ParameterSyntax[] parameters)
        {
            var emitterMethodInfo = new EmitterMethodInfo(ParseTypeName(returnType), parameters) { Name = name };
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
                            SyntaxFactory.SimpleBaseType(ParseTypeName(baseType))
                        })
                    )
                )
            );
        }

    }
}
