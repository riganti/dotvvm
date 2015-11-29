using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Base;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Dothtml
{
    public class MetadataControlResolver
    {
        private ConcurrentDictionary<string, ControlMetadata> metadata = new ConcurrentDictionary<string, ControlMetadata>(StringComparer.CurrentCultureIgnoreCase);
        private CachedValue<List<CompletionData>> allControls = new CachedValue<List<CompletionData>>();
        private CachedValue<List<AttachedPropertyMetadata>> attachedProperties = new CachedValue<List<AttachedPropertyMetadata>>();
        private CachedValue<List<INamedTypeSymbol>> allClasses = new CachedValue<List<INamedTypeSymbol>>();
        private ControlMetadata htmlGenericControlMetadata;

        public IEnumerable<CompletionData> GetElementNames(DothtmlCompletionContext context, List<string> tagNameHierarchy)
        {
            // get all available allControls
            var controls = allControls.GetOrRetrieve(() => ReloadAllControls(context));

            // get element properties
            var elementProperties = new List<CompletionData>();
            ControlMetadata currentControl;
            ControlPropertyMetadata currentProperty;
            GetElementContext(tagNameHierarchy, out currentControl, out currentProperty);
            if (currentControl != null)
            {
                if (currentProperty == null)
                {
                    elementProperties.AddRange(currentControl.Properties.Where(p => p.IsElement).Select(p => new CompletionData(p.Name)));

                    // get default property
                    var defaultContentProperty = currentControl.GetProperty(currentControl.DefaultContentProperty);
                    if (defaultContentProperty != null)
                    {
                        var filteredControls = GetElementNamesInPropertyContext(defaultContentProperty);
                        return elementProperties.Concat(filteredControls);
                    }
                    else if (!currentControl.AllowContent)
                    {
                        // content is not allowed, return only inner properties
                        return elementProperties;
                    }
                    else
                    {
                        // content is allowed - return all allControls
                        return elementProperties.Concat(controls);
                    }
                }
                else
                {
                    return GetElementNamesInPropertyContext(currentProperty);
                }
            }

            return controls;
        }

        public IEnumerable<CompletionData> GetControlAttributeNames(DothtmlCompletionContext context, List<string> tagNameHierarchy, out bool combineWithHtmlCompletions)
        {
            allControls.GetOrRetrieve(() => ReloadAllControls(context));

            ControlMetadata currentControl;
            ControlPropertyMetadata currentProperty;
            GetElementContext(tagNameHierarchy, out currentControl, out currentProperty);

            combineWithHtmlCompletions = currentControl != null && currentControl.Name == typeof(HtmlGenericControl).Name && currentControl.Namespace == typeof(HtmlGenericControl).Namespace;

            if (currentControl != null && currentProperty == null)
            {
                return currentControl.Properties.Where(p => !p.IsElement).Select(p => new CompletionData(p.Name));
            }
            return Enumerable.Empty<CompletionData>();
        }

        public IEnumerable<CompletionData> GetAttachedPropertyNames(DothtmlCompletionContext context)
        {
            return attachedProperties.GetOrRetrieve(() => ReloadAllAttachedProperties(context)).Select(a => new CompletionData(a.Name));
        }

        public IEnumerable<CompletionData> GetAttachedPropertyValues(DothtmlCompletionContext context, string attachedPropertyName)
        {
            return attachedProperties.GetOrRetrieve(() => ReloadAllAttachedProperties(context))
                .Where(a => a.Name == attachedPropertyName)
                .SelectMany(p => HintPropertyValues(p.Type));
        }

        public IEnumerable<CompletionData> GetControlAttributeValues(DothtmlCompletionContext context, List<string> tagNameHierarchy, string attributeName)
        {
            allControls.GetOrRetrieve(() => ReloadAllControls(context));

            ControlMetadata currentControl;
            ControlPropertyMetadata currentProperty;
            GetElementContext(tagNameHierarchy, out currentControl, out currentProperty);

            if (currentControl != null && currentProperty == null)
            {
                var property = currentControl.Properties.FirstOrDefault(p => p.Name == attributeName);
                if (property != null)
                {
                    // it is a real property
                    return HintPropertyValues(property.Type);
                }
                else
                {
                    // it can be an attached property
                    return GetAttachedPropertyValues(context, attributeName);
                }
            }
            return Enumerable.Empty<CompletionData>();
        }

        public void OnWorkspaceChanged()
        {
            allClasses.ClearCachedValue();
            allControls.ClearCachedValue();
            attachedProperties.ClearCachedValue();
        }

        internal ControlMetadata GetMetadata(string tagName)
        {
            ControlMetadata result = null;
            metadata.TryGetValue(tagName, out result);
            return result;
        }

        internal void GetElementContext(List<string> tagNameHierarchy, out ControlMetadata currentControl, out ControlPropertyMetadata currentProperty)
        {
            currentProperty = null;
            currentControl = null;

            for (int i = 0; i < tagNameHierarchy.Count; i++)
            {
                currentProperty = null;
                currentControl = null;

                var tagName = tagNameHierarchy[i];
                if (metadata.ContainsKey(tagName))
                {
                    // we have found a control
                    currentControl = metadata[tagName];

                    // the next element in the hierarchy might be a property
                    if (i + 1 < tagNameHierarchy.Count)
                    {
                        currentProperty = currentControl.Properties.FirstOrDefault(p => p.Name == tagNameHierarchy[i + 1] && p.IsElement);
                        if (currentProperty != null)
                        {
                            i++;
                        }
                    }
                }
                else
                {
                    // HTML or unknown element
                    currentControl = htmlGenericControlMetadata;
                }
            }
        }

        internal List<CompletionData> ReloadAllControls(DothtmlCompletionContext context)
        {
            // get all possible control symbols
            var allClasses = ReloadAllClasses(context);
            var controlClasses = allClasses
                .Where(c => CompletionHelper.GetBaseTypes(c).Any(t => CheckType(t, typeof(DotvvmBindableObject))))
                .ToList();

            var result = new List<CompletionData>();
            metadata = new ConcurrentDictionary<string, ControlMetadata>(StringComparer.CurrentCultureIgnoreCase);
            htmlGenericControlMetadata = null;

            foreach (var rule in context.Configuration.Markup.Controls)
            {
                string tagName;
                if (!string.IsNullOrEmpty(rule.Src))
                {
                    // markup control
                    tagName = rule.TagPrefix + ":" + rule.TagName;

                    // TODO: parse markup, find base type and extract metadata

                    result.Add(new CompletionData(tagName));
                }
                else
                {
                    // find all classes declared in the project
                    var controls = controlClasses.Where(c => c.ContainingAssembly.Name == rule.Assembly && c.ContainingNamespace.ToDisplayString() == rule.Namespace);
                    foreach (var control in controls)
                    {
                        tagName = rule.TagPrefix + ":" + control.Name;
                        var controlMetadata = GetControlMetadata(control, rule.TagPrefix, control.Name);
                        metadata[tagName] = controlMetadata;
                        result.Add(new CompletionData(tagName));

                        if (CheckType(control, typeof(HtmlGenericControl)))
                        {
                            htmlGenericControlMetadata = controlMetadata;
                        }
                    }
                }
            }

            return result;
        }

        internal List<AttachedPropertyMetadata> ReloadAllAttachedProperties(DothtmlCompletionContext context)
        {
            // get all possible control symbols
            var allClasses = ReloadAllClasses(context);
            var attachedPropertyClasses = allClasses
                .Where(c => c.GetThisAndAllBaseTypes().Any(t => t.GetAttributes().Any(a => CheckType(a.AttributeClass, typeof(ContainsDotvvmPropertiesAttribute)))))
                .ToList();

            // find all attached properties
            var attachedProperties = attachedPropertyClasses
                .SelectMany(c => c.GetMembers().OfType<IFieldSymbol>())
                .Where(f => f.Type.GetThisAndAllBaseTypes().Any(t => CheckType(t, typeof(DotvvmProperty))))
                .Where(f => f.GetAttributes().Any(a => CheckType(a.AttributeClass, typeof(AttachedPropertyAttribute))));

            return attachedProperties
                .Select(f => new AttachedPropertyMetadata()
                {
                    Name = ComposeAttachedPropertyName(f),
                    Type = f.GetAttributes().First(a => CheckType(a.AttributeClass, typeof(AttachedPropertyAttribute)))
                        .ConstructorArguments[0].Value as ITypeSymbol
                })
                .ToList();
        }

        private static bool CheckType(ITypeSymbol symbol, Type type)
        {
            if (symbol is IErrorTypeSymbol || symbol.ContainingNamespace == null || string.IsNullOrWhiteSpace(symbol.Name))
            {
                return false;
            }
            return symbol.ContainingNamespace.ToDisplayString() == type.Namespace && symbol.Name == type.Name;
        }

        private IEnumerable<CompletionData> GetElementNamesInPropertyContext(ControlPropertyMetadata controlProperty)
        {
            IEnumerable<CompletionData> filteredControls;
            if (CheckType(controlProperty.Type, typeof(ITemplate)))
            {
                // all controls can appear in a template
                filteredControls = metadata.Select(c => new CompletionData(c.Key, c.Key));
            }
            else
            {
                // resolve element type
                var iEnumerable = CompletionHelper.FindIEnumerableType(controlProperty.Type);
                var elementType = iEnumerable != null ? iEnumerable.TypeArguments[0] : controlProperty.Type;

                // filter the allControls collection
                var filteredControlMetadata = metadata.Where(m => CompletionHelper.IsAssignable(m.Value.Type, elementType));
                filteredControls = filteredControlMetadata.Select(c => new CompletionData(c.Key, c.Key));
            }
            return filteredControls;
        }

        private IEnumerable<CompletionData> HintPropertyValues(ITypeSymbol propertyType)
        {
            if (propertyType.TypeKind == TypeKind.Enum)
            {
                return propertyType.GetMembers().Where(m => m.Kind == SymbolKind.Field).Select(m => new CompletionData(m.Name));
            }
            if (CheckType(propertyType, typeof(bool)))
            {
                return new[]
                {
                    new CompletionData("false"),
                    new CompletionData("true")
                };
            }
            return Enumerable.Empty<CompletionData>();
        }

        private string ComposeAttachedPropertyName(IFieldSymbol field)
        {
            var name = field.ContainingType.Name + "." + field.Name;
            return name.EndsWith("Property") ? name.Substring(0, name.Length - "Property".Length) : name;
        }

        private List<INamedTypeSymbol> ReloadAllClasses(DothtmlCompletionContext context)
        {
            return allClasses.GetOrRetrieve(() =>
            {
                var syntaxTrees = CompletionHelper.GetSyntaxTrees(context);
                var ownSymbols = syntaxTrees.SelectMany(t => t.Tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()
                    .Select(c => t.SemanticModel.GetDeclaredSymbol(c))).ToList();
                var referencedSymbols = CompletionHelper.GetReferencedSymbols(context);
                return Enumerable.Concat(referencedSymbols, ownSymbols).OfType<INamedTypeSymbol>()
                    .Where(c => c.DeclaredAccessibility == Accessibility.Public && !c.IsAbstract)
                    .ToList();
            });
        }

        private ControlMetadata GetControlMetadata(INamedTypeSymbol control, string tagPrefix, string tagName)
        {
            var attribute = control.GetAttributes().FirstOrDefault(a => CheckType(a.AttributeClass, typeof(ControlMarkupOptionsAttribute)));

            return new ControlMetadata()
            {
                Type = control,
                TagPrefix = tagPrefix,
                TagName = tagName,
                Name = control.Name,
                Namespace = control.ContainingNamespace.ToDisplayString(),
                Properties = CompletionHelper.GetBaseTypes(control).Concat(new[] { control })
                    .SelectMany(c => c.GetMembers().OfType<IPropertySymbol>())
                    .Where(p => p.DeclaredAccessibility == Accessibility.Public)
                    .Where(p => p.GetMethod != null && p.SetMethod != null)
                    .Select(GetPropertyMetadata)
                    .Where(p => p != null)
                    .ToList(),
                AllowContent = attribute?.NamedArguments.Where(a => a.Key == "AllowContent").Select(a => a.Value.Value as bool?).FirstOrDefault() ?? true,
                DefaultContentProperty = attribute?.NamedArguments.Where(a => a.Key == "DefaultContentProperty").Select(a => a.Value.Value as string).FirstOrDefault()
            };
        }

        private ControlPropertyMetadata GetPropertyMetadata(IPropertySymbol property)
        {
            var attribute = property.GetAttributes().FirstOrDefault(a => CheckType(a.AttributeClass, typeof(MarkupOptionsAttribute)));

            var metadata = new ControlPropertyMetadata()
            {
                Type = property.Type,
                IsTemplate = CheckType(property.Type, typeof(ITemplate)) || property.Type.AllInterfaces.Any(i => CheckType(i, typeof(ITemplate))),
                AllowHtmlContent = property.Type.AllInterfaces.Any(i => CheckType(i, typeof(IControlWithHtmlAttributes)))
            };

            if (attribute != null)
            {
                metadata.Name = attribute.NamedArguments.Where(a => a.Key == "Name").Select(a => a.Value.Value as string).FirstOrDefault() ?? property.Name;
                metadata.AllowBinding = attribute.NamedArguments.Where(a => a.Key == "AllowBinding").Select(a => a.Value.Value as bool?).FirstOrDefault() ?? true;
                metadata.AllowHardCodedValue = attribute.NamedArguments.Where(a => a.Key == "AllowHardCodedValue").Select(a => a.Value.Value as bool?).FirstOrDefault() ?? true;

                var mappingMode = (MappingMode)(attribute.NamedArguments.Where(a => a.Key == "MappingMode").Select(a => a.Value.Value as int?).FirstOrDefault() ?? (int)MappingMode.Attribute);
                if (mappingMode == MappingMode.InnerElement)
                {
                    metadata.IsElement = true;
                }
                else if (mappingMode == MappingMode.Exclude)
                {
                    return null;
                }
            }
            else
            {
                metadata.Name = property.Name;
                metadata.AllowBinding = true;
                metadata.AllowHardCodedValue = true;
            }

            if (metadata.IsTemplate)
            {
                metadata.IsElement = true;
            }

            return metadata;
        }
    }
}