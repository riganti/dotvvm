using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Redwood.Framework.Controls;
using Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml.Base;

namespace Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml
{
    public class MetadataControlResolver
    {
        private ConcurrentDictionary<string, ControlMetadata> metadata = new ConcurrentDictionary<string, ControlMetadata>();
        private CachedValue<List<CompletionData>> allControls = new CachedValue<List<CompletionData>>();


        public IEnumerable<CompletionData> GetElementNames(RwHtmlCompletionContext context, List<string> tagNameHierarchy)
        {
            var controls = allControls.GetOrRetrieve(() => ReloadAllControls(context));

            ControlMetadata currentControl;
            ControlPropertyMetadata currentProperty;
            GetElementContext(tagNameHierarchy, out currentControl, out currentProperty);

            if (currentControl != null && currentProperty == null && currentControl.Properties.Any(p => p.IsElement))
            {
                return currentControl.Properties.Where(p => p.IsElement).Select(p => new CompletionData(p.Name));
            }
            return controls;
        }

        public IEnumerable<CompletionData> GetControlAttributeNames(RwHtmlCompletionContext context, List<string> tagNameHierarchy)
        {
            allControls.GetOrRetrieve(() => ReloadAllControls(context));

            ControlMetadata currentControl;
            ControlPropertyMetadata currentProperty;
            GetElementContext(tagNameHierarchy, out currentControl, out currentProperty);

            if (currentControl != null && currentProperty == null)
            {
                return currentControl.Properties.Where(p => !p.IsElement).Select(p => new CompletionData(p.Name));
            }
            return Enumerable.Empty<CompletionData>();
        }

        public IEnumerable<CompletionData> GetControlAttributeValues(RwHtmlCompletionContext context, List<string> tagNameHierarchy, string attributeName)
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
                    return HintPropertyValues(property);
                }
            }
            return Enumerable.Empty<CompletionData>();
        }

        internal ControlMetadata GetMetadata(string tagName)
        {
            return metadata[tagName];
        }

        private IEnumerable<CompletionData> HintPropertyValues(ControlPropertyMetadata property)
        {
            if (property.Type.TypeKind == TypeKind.Enum)
            {
                return property.Type.GetMembers().Where(m => m.Kind == SymbolKind.Field).Select(m => new CompletionData(m.Name));
            }
            if (CheckType((INamedTypeSymbol)property.Type, typeof(bool)))
            {
                return new[]
                {
                    new CompletionData("false"),
                    new CompletionData("true")
                };
            }
            return Enumerable.Empty<CompletionData>();
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
                    // HTML or unknown element, skip it
                }
            }
        }


        internal List<CompletionData> ReloadAllControls(RwHtmlCompletionContext context)
        {
            // get all possible control symbols
            var syntaxTrees = CompletionHelper.GetSyntaxTrees(context);
            var ownSymbols = syntaxTrees.SelectMany(t => t.Tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()
                .Select(c => t.SemanticModel.GetDeclaredSymbol(c))).ToList();
            var referencedSymbols = CompletionHelper.GetReferencedSymbols(context);

            var allClasses = Enumerable.Concat(referencedSymbols, ownSymbols).OfType<INamedTypeSymbol>()
                .Where(c => c.DeclaredAccessibility == Accessibility.Public && !c.IsAbstract)
                .Where(c => CompletionHelper.GetBaseTypes(c).Any(t => CheckType(t, typeof(RedwoodControl))))
                .ToList();

            var result = new List<CompletionData>();
            metadata = new ConcurrentDictionary<string, ControlMetadata>();

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
                    var controls = allClasses.Where(c => c.ContainingAssembly.Name == rule.Assembly && c.ContainingNamespace.ToDisplayString() == rule.Namespace);
                    foreach (var control in controls)
                    {
                        tagName = rule.TagPrefix + ":" + control.Name;
                        metadata[tagName] = GetControlMetadata(control);
                        result.Add(new CompletionData(tagName));
                    }
                }
            }

            return result;
        }

        private ControlMetadata GetControlMetadata(INamedTypeSymbol control)
        {
            return new ControlMetadata()
            {
                Name = control.Name, 
                Namespace = control.ContainingNamespace.Name,
                Properties = CompletionHelper.GetBaseTypes(control).Concat(new[] { control })
                    .SelectMany(c => c.GetMembers().OfType<IPropertySymbol>())
                    .Where(p => p.DeclaredAccessibility == Accessibility.Public)
                    .Where(p => p.GetMethod != null && p.SetMethod != null)
                    .Select(GetPropertyMetadata)
                    .ToList()
            };
        }

        private ControlPropertyMetadata GetPropertyMetadata(IPropertySymbol property)
        {
            var attribute = property.GetAttributes().FirstOrDefault(a => CheckType(a.AttributeClass, typeof(MarkupOptionsAttribute)));
            
            var metadata = new ControlPropertyMetadata()
            {
                Type = property.Type,
                IsTemplate = CheckType((INamedTypeSymbol)property.Type, typeof(ITemplate)) || property.Type.AllInterfaces.Any(i => CheckType(i, typeof(ITemplate))),
                AllowHtmlContent = property.Type.AllInterfaces.Any(i => CheckType(i, typeof(IControlWithHtmlAttributes)))
            };

            if (attribute != null)
            {
                metadata.Name = attribute.NamedArguments.Where(a => a.Key == "Name").Select(a => a.Value.Value as string).FirstOrDefault() ?? property.Name;
                metadata.AllowBinding = attribute.NamedArguments.Where(a => a.Key == "AllowBinding").Select(a => a.Value.Value as bool?).FirstOrDefault() ?? true;
                metadata.AllowHardCodedValue = attribute.NamedArguments.Where(a => a.Key == "AllowHardCodedValue").Select(a => a.Value.Value as bool?).FirstOrDefault() ?? true;

                var mappingMode = (MappingMode)(attribute.NamedArguments.Where(a => a.Key == "MappingMode").Select(a => a.Value.Value as int?).FirstOrDefault() ?? 0);
                if (mappingMode == MappingMode.InnerElement || mappingMode == MappingMode.Content)
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

        private static bool CheckType(INamedTypeSymbol symbol, Type type)
        {
            return symbol.ContainingNamespace.ToDisplayString() == type.Namespace && symbol.Name == type.Name;
        }


        public void OnWorkspaceChanged()
        {
            allControls.ClearCachedValue();
        }
    }
}
