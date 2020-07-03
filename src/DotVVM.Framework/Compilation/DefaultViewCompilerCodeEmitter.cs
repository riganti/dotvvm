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
using DotVVM.Framework.Compilation.ControlTree;
using System.Threading;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.Binding;
using Microsoft.Extensions.DependencyInjection;
using DotVVM.Framework.Controls;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace DotVVM.Framework.Compilation
{
    public class DefaultViewCompilerCodeEmitter
    {

        public DefaultViewCompilerCodeEmitter()
        {
            this.valueEmitter = new RoslynValueEmitter(ParseTypeName);
        }

        private int CurrentControlIndex;

        private List<StatementSyntax> CurrentStatements
        {
            get { return methods.Peek().Statements; }
        }

        public const string ControlBuilderFactoryParameterName = "controlBuilderFactory";
        public const string ServiceProviderParameterName = "services";
        public const string BuildTemplateFunctionName = "BuildTemplate";
        protected readonly RoslynValueEmitter valueEmitter;
        private Dictionary<GroupedDotvvmProperty, string> cachedGroupedDotvvmProperties = new Dictionary<GroupedDotvvmProperty, string>();
        private ConcurrentDictionary<(Type obj, string argTypes), string> injectionFactoryCache = new ConcurrentDictionary<(Type obj, string argTypes), string>();
        private Stack<EmitterMethodInfo> methods = new Stack<EmitterMethodInfo>();
        private List<EmitterMethodInfo> outputMethods = new List<EmitterMethodInfo>();
        public SyntaxTree SyntaxTree { get; private set; }
        public Type BuilderDataContextType { get; set; }
        public TypeSyntax ResultControlTypeSyntax { get; set; }

        private ConcurrentDictionary<Assembly, string> usedAssemblies = new ConcurrentDictionary<Assembly, string>();
        private static int assemblyIdCtr = 0;
        public IEnumerable<KeyValuePair<Assembly, string>> UsedAssemblies
        {
            get { return usedAssemblies; }
        }

        public string UseType(Type type)
        {
            if (type == null) return null;
            UseType(type.GetTypeInfo().BaseType);
            return usedAssemblies.GetOrAdd(type.GetTypeInfo().Assembly, _ => "Asm_" + Interlocked.Increment(ref assemblyIdCtr));
        }

        private List<MemberDeclarationSyntax> otherDeclarations = new List<MemberDeclarationSyntax>();

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

        public ExpressionSyntax EmitValue(object value) => valueEmitter.EmitValue(value);

        /// <summary>
        /// Emits the create object expression.
        /// </summary>
        public string EmitCreateObject(Type type, object[] constructorArguments = null)
        {
            if (constructorArguments == null)
            {
                constructorArguments = new object[] { };
            }

            UseType(type);
            return EmitCreateObject(ParseTypeName(type), constructorArguments.Select(EmitValue));
        }

        public ExpressionSyntax InvokeDefaultInjectionFactory(Type objectType, Type[] parameterTypes) =>
            ParseTypeName(typeof(ActivatorUtilities))
            .Apply(a => SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, a, SyntaxFactory.IdentifierName(nameof(ActivatorUtilities.CreateFactory))))
            .Apply(SyntaxFactory.InvocationExpression)
                .WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(new[]
                {
                    EmitValue(objectType).Apply(SyntaxFactory.Argument),
                    EmitValue(parameterTypes).Apply(SyntaxFactory.Argument),
                })));

        public string EmitCustomInjectionFactoryInvocation(Type factoryType, Type controlType) =>
                SyntaxFactory.IdentifierName(ServiceProviderParameterName)
                .Apply(i => SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    i,
                    SyntaxFactory.IdentifierName(nameof(IServiceProvider.GetService))))
                .Apply(SyntaxFactory.InvocationExpression)
                    .WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument
                (EmitValue(factoryType)))))
                .Apply(n => SyntaxFactory.CastExpression(ParseTypeName(factoryType), n))
                .Apply(SyntaxFactory.InvocationExpression)
                    .WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(new[] {
                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName(ServiceProviderParameterName)),
                        SyntaxFactory.Argument(EmitValue(controlType))
                    })))
                .Apply(a => SyntaxFactory.CastExpression(ParseTypeName(controlType), a))
                .Apply(EmitCreateVariable);
        
        public string EmitInjectionFactoryInvocation(
            Type type,
            (Type type, ExpressionSyntax expression)[] arguments,
            Func<Type, Type[], ExpressionSyntax> factoryInvocation) =>
                this.injectionFactoryCache.GetOrAdd((type, string.Join(";", arguments.Select(i => i.type))), _ =>
                {
                    var fieldName = "Obj_" + type.Name + "_Factory_" + otherDeclarations.Count;
                    otherDeclarations.Add(SyntaxFactory.FieldDeclaration(SyntaxFactory.VariableDeclaration(
                            this.ParseTypeName(typeof(ObjectFactory)))
                        .WithVariables(
                            SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(fieldName))
                                .WithInitializer(SyntaxFactory.EqualsValueClause(
                                    factoryInvocation(type, arguments.Select(a => a.type).ToArray())
                                ))
                        )));
                    return fieldName;
                })
                .Apply(SyntaxFactory.IdentifierName)
                .Apply(SyntaxFactory.InvocationExpression)
                    .WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(new[] {
                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName(ServiceProviderParameterName)),
                        SyntaxFactory.Argument(SyntaxFactory.ArrayCreationExpression(
                            SyntaxFactory.ArrayType(ParseTypeName(typeof(object)))
                                .WithRankSpecifiers(SyntaxFactory.SingletonList<ArrayRankSpecifierSyntax>(SyntaxFactory.ArrayRankSpecifier(  SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(SyntaxFactory.OmittedArraySizeExpression())))),
                            SyntaxFactory.InitializerExpression(SyntaxKind.ArrayInitializerExpression,
                                SyntaxFactory.SeparatedList(arguments.Select(a => a.expression)))))
                    })))
                .Apply(a => SyntaxFactory.CastExpression(ParseTypeName(type), a))
                .Apply(EmitCreateVariable);
        
        /// <summary>
        /// Emits the create object expression.
        /// </summary>
        public string EmitCreateObject(TypeSyntax typeSyntax, object[] constructorArguments = null)
        {
            if (constructorArguments == null)
            {
                constructorArguments = new object[] { };
            }

            return EmitCreateObject(typeSyntax, constructorArguments.Select(EmitValue));
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

        public static ExpressionSyntax EmitCreateArray(TypeSyntax elementType, IEnumerable<ExpressionSyntax> values)
        {
            return
                SyntaxFactory.ArrayCreationExpression(
                    SyntaxFactory.ArrayType(
                        elementType,
                        SyntaxFactory.SingletonList(
                            SyntaxFactory.ArrayRankSpecifier(
                                SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                    SyntaxFactory.OmittedArraySizeExpression())))),
                    SyntaxFactory.InitializerExpression(
                        SyntaxKind.ArrayInitializerExpression,
                        SyntaxFactory.SeparatedList(
                            values)));
        }

        public ExpressionSyntax EmitAttributeInitializer(CustomAttributeData attr)
        {
            UseType(attr.AttributeType);
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
            UseType(controlType);

            var builderName = "c" + CurrentControlIndex + "_builder";
            var untypedName = "c" + CurrentControlIndex + "_untyped";
            var name = "c" + CurrentControlIndex;
            CurrentControlIndex++;

            CurrentStatements.Add(
                SyntaxFactory.LocalDeclarationStatement(
                    SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var")).WithVariables(
                        SyntaxFactory.VariableDeclarator(builderName).WithInitializer(
                            SyntaxFactory.EqualsValueClause(
                                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.IdentifierName(ControlBuilderFactoryParameterName),
                                                SyntaxFactory.IdentifierName(nameof(IControlBuilderFactory.GetControlBuilder))
                                            ),
                                            SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[] {
                                                SyntaxFactory.Argument(EmitValue(virtualPath))
                                            }))
                                        ),
                                    SyntaxFactory.IdentifierName("Item2")),
                                SyntaxFactory.IdentifierName("Value"))
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
                                        SyntaxFactory.IdentifierName(nameof(IControlBuilder.BuildControl))
                                    ),
                                    SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[] {
                                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName(ControlBuilderFactoryParameterName)),
                                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName(ServiceProviderParameterName))
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

        public ExpressionSyntax CreateDotvvmPropertyIdentifier(DotvvmProperty property)
        {
            if (property is GroupedDotvvmProperty)
            {
                var gprop = (GroupedDotvvmProperty)property;
                string fieldName;
                if (!cachedGroupedDotvvmProperties.TryGetValue(gprop, out fieldName))
                {
                    fieldName = $"_staticCachedGroupProperty_{cachedGroupedDotvvmProperties.Count}";
                    cachedGroupedDotvvmProperties.Add(gprop, fieldName);
                    otherDeclarations.Add(SyntaxFactory.FieldDeclaration(
                        SyntaxFactory.VariableDeclaration(ParseTypeName(typeof(DotvvmProperty)),
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.VariableDeclarator(fieldName)
                                .WithInitializer(SyntaxFactory.EqualsValueClause(
                                    SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleAssignmentExpression,
                                            SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleAssignmentExpression,
                                                ParseTypeName(gprop.PropertyGroup.DeclaringType),
                                                SyntaxFactory.IdentifierName(gprop.PropertyGroup.DescriptorField.Name)
                                            ),
                                            SyntaxFactory.IdentifierName(nameof(DotvvmPropertyGroup.GetDotvvmProperty))
                                        ),
                                        SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(
                                            SyntaxFactory.Argument(this.EmitValue(gprop.GroupMemberName))
                                        ))
                                    )
                                ))
                            )
                        )
                    ));
                }
                return SyntaxFactory.ParseName(fieldName);
            }
            else
            {
                return SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    ParseTypeName(property.DeclaringType),
                    SyntaxFactory.IdentifierName(property.Name + "Property"));
            }
        }

        private Dictionary<string, List<(DotvvmProperty prop, ExpressionSyntax value)>> controlProperties = new Dictionary<string, List<(DotvvmProperty, ExpressionSyntax)>>();

        public void EmitSetDotvvmProperty(string controlName, DotvvmProperty property, object value) =>
            EmitSetDotvvmProperty(controlName, property, EmitValue(value));

        public void EmitSetDotvvmProperty(string controlName, DotvvmProperty property, ExpressionSyntax value)
        {
            if (!controlProperties.TryGetValue(controlName, out var propertyList))
                throw new Exception($"Can not set property, control {controlName} is not registered");

            UseType(property.DeclaringType);
            UseType(property.PropertyType);

            if (property.IsVirtual)
            {
                var gProperty = property as GroupedDotvvmProperty;
                if (gProperty != null && gProperty.PropertyGroup.PropertyGroupMode == PropertyGroupMode.ValueCollection)
                {
                    EmitAddToDictionary(controlName, property.CastTo<GroupedDotvvmProperty>().PropertyGroup.Name, gProperty.GroupMemberName, value);
                }
                else
                {
                    EmitSetProperty(controlName, property.PropertyInfo.Name, value);
                }
            }
            else
            {
                propertyList.Add((property, value));
            }
        }

        /// Instructs the emitter that this object can receive DotvvmProperties
        /// Note that the properties have to be committed using <see cref="CommitDotvvmProperties(string)" />
        public void RegisterDotvvmProperties(string controlName) =>
            controlProperties.Add(controlName, new List<(DotvvmProperty prop, ExpressionSyntax value)>());

        public void CommitDotvvmProperties(string name)
        {
            var properties = controlProperties[name];
            controlProperties.Remove(name);
            if (properties.Count == 0) return;

            properties.Sort((a, b) => a.prop.FullName.CompareTo(b.prop.FullName));

            var (hashSeed, keys, values) = PropertyImmutableHashtable.CreateTableWithValues(properties.Select(p => p.prop).ToArray(), properties.Select(p => p.value).ToArray());

            var invertedValues = new object[values.Length];
            var successfulInversion = true;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] != null)
                {
                    successfulInversion = successfulInversion && this.valueEmitter.TryInvertExpression(values[i], out invertedValues[i]);
                }
            }

            ExpressionSyntax valueExpr;
            if (successfulInversion)
            {
                valueExpr = valueEmitter.EmitValueReference(invertedValues);
            }
            else
            {
                valueExpr = EmitCreateArray(
                    this.ParseTypeName(typeof(object)),
                    values.Select(v => v ?? this.EmitValue(null))
                );
            }

            var keyExpr = valueEmitter.EmitValueReference(keys);

            // control.MagicSetValue(keys, values, hashSeed)
            CurrentStatements.Add(
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName(name),
                                SyntaxFactory.IdentifierName(nameof(DotvvmBindableObject.MagicSetValue))
                        ),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList(new[] {
                                keyExpr,
                                valueExpr,
                                EmitValue(hashSeed)
                            }.Select(SyntaxFactory.Argument))
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

        /// <summary>
        /// Emits the add HTML attribute.
        /// </summary>
        public void EmitAddToDictionary(string controlName, string propertyName, string key, ExpressionSyntax valueSyntax)
        {
            CurrentStatements.Add(
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.ElementAccessExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName(controlName),
                                SyntaxFactory.IdentifierName(propertyName)
                            ),
                            SyntaxFactory.BracketedArgumentList(
                                SyntaxFactory.SeparatedList(
                                    new[]
                                    {
                                        SyntaxFactory.Argument(EmitValue(key))
                                    }
                                )
                            )
                        ),
                        valueSyntax
                    )
                )
            );
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
                                        SyntaxFactory.Argument(EmitValue(name))
                                    }
                                )
                            )
                        ),
                        EmitValue(value)
                    )
                )
            );
        }

        public string EmitEnsureCollectionInitialized(string parentName, DotvvmProperty property)
        {
            UseType(property.PropertyType);

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
                            new[] { EmitValue($"Property '{ property.FullName }' can't be used as control collection since it is not initialized and does not have setter available for automatic initialization") }
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
                                    SyntaxFactory.SeparatedList(new[] {
                                        SyntaxFactory.Argument(this.CreateDotvvmPropertyIdentifier(property))
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
                                    SyntaxFactory.SeparatedList(new[] {
                                        SyntaxFactory.Argument(this.CreateDotvvmPropertyIdentifier(property)),
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
                                SyntaxFactory.SeparatedList(new[] {
                                    SyntaxFactory.Argument(this.CreateDotvvmPropertyIdentifier(property))
                                })
                            )
                        )
                    )
                );
            }
        }

        public TypeSyntax ParseTypeName(Type type)
        {
            var asmName = UseType(type);
            if (type == typeof(void))
            {
                return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword));
            }
            else if (!type.GetTypeInfo().IsGenericType)
            {
                return SyntaxFactory.ParseTypeName($"{asmName}::{type.FullName.Replace('+', '.')}");
            }
            else
            {
                var fullName = type.GetGenericTypeDefinition().FullName;
                if (fullName.Contains("`"))
                {
                    fullName = fullName.Substring(0, fullName.IndexOf("`", StringComparison.Ordinal));
                }

                var parts = fullName.Split('.');
                NameSyntax identifier = SyntaxFactory.AliasQualifiedName(
                    SyntaxFactory.IdentifierName(asmName),
                    SyntaxFactory.IdentifierName(parts[0]));
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

        Regex allowedCachedExpressions =
            new Regex(@"Asm_\d+::DotVVM.Framework.Compilation\.RoslynValueEmitter\.(_ViewImmutableObjects_ObjArray|_ViewImmutableObjects|_ViewImmutableObjects_PropArray)");

        // private IEnumerable<StatementSyntax> OptimizeMethodBody(List<StatementSyntax> statements)
        // {
        //     var nodes =
        //         statements.SelectMany((s, i) =>
        //             s.DescendantNodes()
        //             .Where(n => (n.IsKind(SyntaxKind.SimpleMemberAccessExpression)) &&
        //                         allowedCachedExpressions.IsMatch(n.ToString()))
        //             .Select(n => (i, n))
        //         )
        //         .GroupBy(n => n.n.ToString())
        //         .Where(g => g.Count() > 2);
        //     foreach (var ng in nodes)
        //     {
                
        //     }
        // }

        /// <summary>
        /// Gets the result syntax tree.
        /// </summary>
        public IEnumerable<SyntaxTree> BuildTree(string namespaceName, string className, string fileName)
        {
            if (controlProperties.FirstOrDefault(c => c.Value.Any()) is var uncommittedControl && uncommittedControl.Value != null)
                throw new Exception($"Control {uncommittedControl.Key} has unresolved properties {String.Join(", ", uncommittedControl.Value.Select(p => p.prop.FullName + " " + p.value))}");

            UseType(BuilderDataContextType);

            var root = SyntaxFactory.CompilationUnit()
                .WithExterns(SyntaxFactory.List(
                    UsedAssemblies.Select(k => SyntaxFactory.ExternAliasDirective(SyntaxFactory.Identifier(k.Value)))
                ))
                .WithMembers(
                SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(namespaceName)).WithMembers(
                    SyntaxFactory.List<MemberDeclarationSyntax>(
                        new[]
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
                                            (QualifiedNameSyntax)ParseTypeName(typeof(LoadControlBuilderAttribute)),
                                            SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(new [] {
                                                SyntaxFactory.AttributeArgument(EmitValue(fileName))
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
                                                    SyntaxFactory.ArrowExpressionClause(SyntaxFactory.TypeOfExpression(ResultControlTypeSyntax)))
                                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                                        }).Concat(otherDeclarations)
                                    )
                                )
                        }
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

        public ParameterSyntax[] EmitControlBuilderParameters()
            => new[]
            {
                EmitParameter(ControlBuilderFactoryParameterName, typeof(IControlBuilderFactory)),
                EmitParameter(ServiceProviderParameterName, typeof(IServiceProvider))
            };

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
            otherDeclarations.Add(
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
